using System.Threading;
using GBSharp.Cartridge;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using GBSharp.VideoSpace;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace GBSharp
{
  public class GameBoy : IGameBoy, IDisposable
  {
    internal static double targetFramerate = 60.0; // This is not the real gameboy framerate, but it's a nice number.
    internal static double stopwatchTicksPerFrame = Stopwatch.Frequency / targetFramerate;
    internal static double stopwatchTicksPerMs = Stopwatch.Frequency / 1000.0;

    internal static double targetMillisecondsPerTick = 0.0002384185791015625; // It is know that this is 2^-22.
    internal static int ticksPerMillisecond = 4194; // Actually it's 4194,304

    public event Action StepCompleted;
    public event Action FrameCompleted;
    public event Action PauseRequested;
    public event Action StopRequested;

    private CPUSpace.CPU cpu;
    private CPUSpace.InterruptController interruptController;
    private MemorySpace.Memory memory;

    internal string CartridgeDirectory { get; private set; }
    internal string CartridgeFilename { get; private set; }
    private Cartridge.Cartridge cartridge;

    private VideoSpace.Display display;
    private AudioSpace.APU apu;
    private SerialSpace.SerialController serial;
    private bool pause;
    private bool run;
    private bool frameReady;
    private bool inBreakpoint;
    private Thread gameLoopThread;

    private ManualResetEvent pauseEvent;
    private ManualResetEventSlim manualResetEvent;

    private Keypad buttons;
    private Disassembler disassembler;

    private Stopwatch stopwatch;
    private long tickCounter;
    private long stepCounter;

    public bool ReleaseButtons { get; set; }

    private int frameCounter = 0;
    private long totalFrameTicks = 0;
    public double FPS { get; private set; }


#if TIMING
    private long[] timingSamples;

    private int timingFrameCounter = 0;
    private int sampleCounter = 0;
    private int maxSamples = 60 * 100;
    private int sampleAmount = 5;

    private Stopwatch swCPU;
    private Stopwatch swDisplay;
    private Stopwatch swBlit;
    private Stopwatch swClockMem;
    public static Stopwatch swBeginInvoke = new Stopwatch();
#endif

    /// <summary>
    /// Class constructor.
    /// Initializes cpu and memory.
    /// </summary>
    public GameBoy()
    {
      this.run = false;
      this.stopwatch = new Stopwatch();

      this.memory = new MemorySpace.Memory();
      this.cpu = new CPUSpace.CPU(this.memory);
      this.interruptController = this.cpu.interruptController;
      this.display = new Display(this.interruptController, this.memory);
      this.apu = new AudioSpace.APU(this.memory, 44000, 2, 2);
      this.serial = new SerialSpace.SerialController(this.interruptController, this.memory);
      this.disassembler = new Disassembler(cpu, memory);

      // Events
      this.cpu.BreakpointFound += BreakpointHandler;
      this.cpu.InterruptHappened += InterruptHandler;
      this.display.FrameReady += FrameReadyHandler;

      InternalReset(false);
    }

    /// <summary>
    /// Wrapper for the outside world
    /// </summary>
    public void Reset()
    {
      InternalReset(true);
    }

    public void InternalReset(bool resetComponents)
    {
      // NOTE(Cristian): It's responsability of the view (or the calling audio code) to
      //                 handling and re-hooking correctly the audio on reset
      if(this.run) { this.Stop(); }

      if(resetComponents)
      {
        this.memory.Reset();
        this.cpu.Reset();
        this.interruptController.Reset();
        this.display.Reset();
        this.apu.Reset();
        this.disassembler.Reset();
        //this.serial = new SerialSpace.SerialController(this.interruptController, this.memory);
      }

      // We re-hook information
      if(this.cartridge != null)
      {
        this.apu.CartridgeFilename = this.CartridgeFilename; 
        this.memory.SetMemoryHandler(GBSharp.MemorySpace.MemoryHandlers.
                                     MemoryHandlerFactory.CreateMemoryHandler(this));
      }

      this.buttons = Keypad.None;
      this.pauseEvent = new ManualResetEvent(true);
      this.manualResetEvent = new ManualResetEventSlim(false);

      this.inBreakpoint = false;
      this.ReleaseButtons = true;

      var disDef = display.GetDisplayDefinition();

#if TIMING
      this.timingSamples = new long[sampleAmount * maxSamples];

      this.swCPU = new Stopwatch();
      this.swDisplay = new Stopwatch();

      this.swBlit = new Stopwatch();
      this.swClockMem = new Stopwatch();
#endif
    }



    #region INTERFACE GETTERS

    public ICPU CPU { get { return cpu; } }
    public IMemory Memory { get { return memory; } }
    public ICartridge Cartridge { get { return cartridge; } }
    public IDisplay Display { get { return display; } }
    public IAPU APU { get { return apu; } }
    public IDisassembler Disassembler { get { return disassembler; } }

    #endregion

    /// <summary>
    /// Provides access to the interrupt controller to every component connected to the gameboy, so interrupts
    /// can be requested from memory handlers, the display controller, serial ports, etc.
    /// If an interrupt can be notified from the memory handler (on write), then direct access from another component
    /// to the interrupt controller is not recommended and the memory.Write() call should be used instead.
    /// </summary>
    internal CPUSpace.InterruptController InterruptController
    {
      get { return interruptController; }
    }

    /// <summary>
    /// Connects to the gameboy a new cartridge with the given contents.
    /// </summary>
    /// <param name="cartridgeData"></param>
    public void LoadCartridge(string cartridgeFullFilename, byte[] cartridgeData)
    {
      if (this.run) { Reset(); }
      this.cartridge = new Cartridge.Cartridge();
      this.cartridge.Load(cartridgeData);

      this.CartridgeFilename = Path.GetFileNameWithoutExtension(cartridgeFullFilename);
      this.CartridgeDirectory = Path.GetDirectoryName(cartridgeFullFilename);
      this.apu.CartridgeFilename = this.CartridgeFilename; 
      // We create the MemoryHandler according to the data
      // from the cartridge and set it to the memory.
      // From this point onwards, all the access to memory
      // are done throught the MemoryHandler
      this.memory.SetMemoryHandler(GBSharp.MemorySpace.MemoryHandlers.
                                   MemoryHandlerFactory.CreateMemoryHandler(this));
    }

    public void Step(bool ignoreBreakpoints)
    {
      Pause();
      MachineStep(ignoreBreakpoints);
    }

    /// <summary>
    /// Runs the simulation for the smallest amount of time possible.
    /// This should be 1 whole instruction, arbitrary machine and clock cycles.
    /// </summary>
    private void MachineStep(bool ignoreBreakpoints)
    {
      // NOTE(Cristian): if inBreakpoint is true, this is the first step since a
      //                 breakpoint. This next step must ignore the breakpoint
      //                 or it will stop with itself.
      bool ignoreNextStep = false;
      if (inBreakpoint)
      {
        inBreakpoint = false;
        ignoreNextStep = true;
      }

#if TIMING 
      swCPU.Start();
      byte ticks = this.cpu.Step(ignoreNextStep || ignoreBreakpoints);
      swCPU.Stop();
#else
      byte ticks = this.cpu.Step(ignoreNextStep || ignoreBreakpoints);
#endif

#if TIMING
      swClockMem.Start();
      // NOTE(Cristian): If the CPU is halted, the hardware carry on
      if (cpu.halted) { ticks = 4; }
      this.cpu.UpdateClockAndTimers(ticks);
      this.memory.Step(ticks);
      swClockMem.Stop();
#else
      // NOTE(Cristian): If the CPU is halted, the hardware carry on
      if (cpu.halted) { ticks = 4; }
      this.cpu.UpdateClockAndTimers(ticks);
      this.memory.Step(ticks);
#endif

#if TIMING
      swDisplay.Start();
      this.display.Step(ticks);
      swDisplay.Stop();
#else
      this.display.Step(ticks);
#endif
      this.tickCounter += ticks;
      this.stepCounter++;

      this.serial.Step(ticks);

      // NOTE(Cristian): Check if something still needs this
      // NotifyStepCompleted();
    }

    /// <summary>
    /// Steps the Gameboy until a frame is ready
    /// </summary>
    private void CalculateFrame()
    {
      display.StartFrame();
      while ((this.run) && (!this.frameReady))
      {
        MachineStep(false);
      }
      display.EndFrame();
    }

    /// <summary>
    /// Attempts to run until the heat death of the universe.
    /// </summary>
    public void Run()
    {
      // We first try to unpause
      if (this.pause)
      {
        this.pauseEvent.Set();
        this.pause = false;
      }

      // NOTE(cdonoso): If we're running, we shouldn't restart. Or should we?
      if (this.run) { return; }
      if (this.cartridge == null) { return; }

      this.run = true;
      this.stepCounter = 0;
      this.tickCounter = 0;
      this.stopwatch.Restart();
      this.manualResetEvent.Set();

      if(gameLoopThread == null)
      {
        // This is the case if the gameboy is started of has been stoped
        this.gameLoopThread = new Thread(new ThreadStart(this.ThreadedRun));
        this.gameLoopThread.Start();
      }
    }

    /// <summary>
    /// Pauses the simulation until Run is called again.
    /// </summary>
    public void Pause()
    {
      if (!this.run) { return; }
      if (this.pause) { return; }
      this.pause = true;
      this.stopwatch.Stop();
      this.pauseEvent.Reset();
    }

    /// <summary>
    /// Cancels the current simulation. The internal state of the cpu and memory is discarded.
    /// </summary>
    public void Stop()
    {
      if (!this.run) { return; }
      this.run = false;
      this.stopwatch.Stop();

      // We unpause if needed
      this.manualResetEvent.Set();
      this.pauseEvent.Set();

      this.gameLoopThread.Join();
      // We get rid of the current thread (a new will be created on restart)
      this.gameLoopThread = null;
      StopRequested();
    }

    /// <summary>
    /// Method that is going to be running in a separate thread, calling Step() forever.
    /// </summary>Infinite
    private void ThreadedRun()
    {
      long drama = 0;
      while (this.run)
      {
        // We check if the thread has been paused
        if (this.pause)
        {
          PauseRequested();
          this.pauseEvent.WaitOne(Timeout.Infinite);
        }

        CalculateFrame();

        // Check timing issues
        long ellapsedStopwatchTicks = this.stopwatch.ElapsedTicks;
        ellapsedStopwatchTicks += drama;

        // Should we sleep?
        if (ellapsedStopwatchTicks < stopwatchTicksPerFrame)
        {
          this.manualResetEvent.Reset();
          int timeToWait = (int)(/* 0.5 + */1000.0 * (stopwatchTicksPerFrame - ellapsedStopwatchTicks) / Stopwatch.Frequency);
          this.manualResetEvent.Wait(timeToWait);
          this.manualResetEvent.Set();
        }

        long finalTicks = this.stopwatch.ElapsedTicks;
        while (finalTicks < stopwatchTicksPerFrame)
        {
          // NOTE(Cristian): Sometimes the thread would be trapped here because when
          //                 the process close signal gets, the timers stop working and
          //                 the finalTicks variable would never update.
          if ((!this.run) || (this.pause)) { break; }
          finalTicks = this.stopwatch.ElapsedTicks;
        }

        drama = finalTicks - (long)stopwatchTicksPerFrame;

        // We calculate how many FPS we're giving
        totalFrameTicks += finalTicks;
        ++frameCounter;
        if (frameCounter >= 30)
        {
          FPS = Math.Round(60 * (double)(30 * stopwatchTicksPerFrame) / (double)totalFrameTicks);
          frameCounter = 0;
          totalFrameTicks = 0;
        }


#if TIMING
        if (sampleCounter < maxSamples)
        {
          int index = sampleCounter * sampleAmount;
          timingSamples[index + 0] = swCPU.ElapsedTicks;
          timingSamples[index + 1] = swClockMem.ElapsedTicks;
          timingSamples[index + 2] = swDisplay.ElapsedTicks;
          timingSamples[index + 3] = swBlit.ElapsedTicks;
          timingSamples[index + 4] = swBeginInvoke.ElapsedTicks;
          ++sampleCounter;
        }
        ++timingFrameCounter;

        swCPU.Reset();
        swClockMem.Reset();
        swDisplay.Reset();
        swBlit.Reset();
        swBeginInvoke.Reset();
#endif

        this.stopwatch.Restart();
        this.tickCounter = 0;
        this.stepCounter = 0;
        this.frameReady = false;

        // We see if the APU needs to close things this frame
        apu.EndFrame();

        // Finally here we trigger the notification
        NotifyFrameCompleted();

      }
    }

    /// <summary>
    /// Debugger. Handles the cpu.BreakPointFound event.
    /// </summary>
    private void BreakpointHandler()
    {
      inBreakpoint = true;
      Pause();
    }

    /// <summary>
    /// Debugger. Handles the cpu.InterruptHappened event.
    /// </summary>
    private void InterruptHandler(Interrupts interrupt)
    {
      inBreakpoint = true;
      Pause();
    }

    /// <summary>
    /// Handles a new frame from the display.
    /// </summary>
    private void FrameReadyHandler()
    {
      frameReady = true;
    }

    /// <summary>
    /// Call this method when a button is pressed in the user interface.
    /// </summary>
    /// <param name="button">The button that was pressed. It can be a combination of buttons too.</param>
    public void PressButton(Keypad button)
    {
      this.buttons |= button;
      this.interruptController.UpdateKeypadState(this.buttons);
      if (cpu.stopped) { cpu.stopped = false; }
    }

    /// <summary>
    /// Call this method when a button is released in the user interface.
    /// </summary>
    /// <param name="button">The button that was released. It can be a combination of buttons too.</param>
    public void ReleaseButton(Keypad button)
    {
      if (ReleaseButtons)
      {
        this.buttons &= ~button;
        this.interruptController.UpdateKeypadState(this.buttons);
      }
    }

    /// <summary>
    /// Notifies subscribers that a step has been completed.
    /// </summary>
    private void NotifyStepCompleted()
    {
      if (StepCompleted != null)
      {
        StepCompleted();
      }
    }

    /// <summary>
    /// Notifies subscribers that a new frame has been completed.
    /// </summary>
    private void NotifyFrameCompleted()
    {
      if (FrameCompleted != null)
      {
#if TIMING
        swBlit.Start();
        display.CopyTargets();
        swBlit.Stop();
#else
        display.CopyTargets();
#endif

        FrameCompleted();
      }
    }

    public Dictionary<MMR, ushort> GetRegisterDic()
    {
      Dictionary<MMR, ushort> registerDic = new Dictionary<MMR, ushort>();
      foreach (MMR registerEnum in Enum.GetValues(typeof(MMR)))
      {
        registerDic.Add(registerEnum, memory.LowLevelRead((ushort)registerEnum));
      }
      return registerDic;
    }

    public void Dispose()
    {
      apu.Dispose();
      memory.Dispose();
    }

    ~GameBoy()
    {
#if TIMING
      using (var file = new System.IO.StreamWriter("timing.csv", false))
      {
        // We write the total timing information
        file.WriteLine("{0}", (int)stopwatchTicksPerFrame);

        // We write the header
        file.WriteLine("{0},{1},{2},{3},{4}", "CPU", "Clock & Mem", "Display", "Blit", "BeginInvoke");

        // We write the data
        for (int i = 0; i < sampleCounter; ++i)
        {
          int index = i * sampleAmount;
          file.WriteLine("{0},{1},{2},{3},{4}",
                         timingSamples[index + 0],
                         timingSamples[index + 1],
                         timingSamples[index + 2],
                         timingSamples[index + 3],
                         timingSamples[index + 4]);
        }
      }
#endif
    }
  }
}
