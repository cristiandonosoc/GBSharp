using System.Threading;
using GBSharp.Cartridge;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using GBSharp.VideoSpace;
using System.Diagnostics;
using System.Collections.Generic;

namespace GBSharp
{
  public class GameBoy : IGameBoy
  {
    internal static double targetFramerate = 60.0; // This is not the real gameboy framerate, but it's a nice number.
    internal static double stopwatchTicksPerFrame = Stopwatch.Frequency / targetFramerate;
    internal static double stopwatchTicksPerMs = Stopwatch.Frequency / 1000.0;

    internal static double targetMillisecondsPerTick = 0.0002384185791015625; // It is know that this is 2^-22.
    internal static int ticksPerMillisecond = 4194; // Actually it's 4194,304

    public event Action StepCompleted;
    public event Action FrameCompleted;

    private CPUSpace.CPU cpu;
    private CPUSpace.InterruptController interruptController;
    private MemorySpace.Memory memory;
    private Cartridge.Cartridge cartridge;
    private VideoSpace.Display display;
    private AudioSpace.APU apu;
    private bool run;
    private bool frameReady;
    private bool inBreakpoint;
    private Thread clockThread;
    private ManualResetEventSlim manualResetEvent;
    private Keypad buttons;
    private Disassembler disassembler;

    private Stopwatch stopwatch;
    private long tickCounter;
    private long stepCounter;

    public bool ReleaseButtons { get; set; }

#if TIMING
    private long[] timingSamples;

    private int frameCounter = 0;
    private int sampleCounter = 0;
    private int maxSamples = 60 * 100;
    private int sampleAmount = 4;

    private Stopwatch swCPU;
    private Stopwatch swDisplay;
    private Stopwatch swBlit;
    private Stopwatch swClockMem;
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

      this.disassembler = new Disassembler(cpu, memory);

      this.buttons = Keypad.None;
      this.manualResetEvent = new ManualResetEventSlim(false);
      this.clockThread = new Thread(new ThreadStart(this.ThreadedRun));

      // Events
      this.cpu.BreakpointFound += BreakpointHandler;
      this.cpu.InterruptHappened += InterruptHandler;
      this.display.FrameReady += FrameReadyHandler;

      this.inBreakpoint = false;
      this.ReleaseButtons = true;

#if TIMING
      this.timingSamples = new long[sampleAmount * maxSamples];

      this.swCPU = new Stopwatch();
      this.swDisplay = new Stopwatch();
      this.swBlit = new Stopwatch();
      this.swClockMem = new Stopwatch();
#endif
    }


    public ICPU CPU
    {
      get { return cpu; }
    }

    public IMemory Memory
    {
      get { return memory; }
    }

    public ICartridge Cartridge
    {
      get { return cartridge; }
    }

    public IDisplay Display
    {
      get { return display; }
    }

    public IAPU APU
    {
      get { return apu; }
    }

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
    public void LoadCartridge(byte[] cartridgeData)
    {
      if (this.run) { Stop(); }
      this.cartridge = new Cartridge.Cartridge();
      this.cartridge.Load(cartridgeData);
      // We create the MemoryHandler according to the data
      // from the cartridge and set it to the memory.
      // From this point onwards, all the access to memory
      // are done throught the MemoryHandler
      this.memory.SetMemoryHandler(
        GBSharp.MemorySpace.MemoryHandlers.
        MemoryHandlerFactory.CreateMemoryHandler(this));
    }

    /// <summary>
    /// Runs the simulation for the smallest amount of time possible.
    /// This should be 1 whole instruction, arbitrary machine and clock cycles.
    /// </summary>
    public void Step(bool ignoreBreakpoints)
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

      //NotifyStepCompleted();
    }

    /// <summary>
    /// Attempts to run until the heat death of the universe.
    /// </summary>
    public void Run()
    {
      this.manualResetEvent.Set();
      // NOTE(cdonoso): If we're running, we shouldn't restart. Or should we?
      if (this.run) { return; }
      if (this.cartridge == null) { return; }
      this.run = true;
      this.stepCounter = 0;
      this.tickCounter = 0;
      this.stopwatch.Restart();
      this.clockThread.Start();
    }

    /// <summary>
    /// Pauses the simulation until Run is called again.
    /// </summary>
    public void Pause()
    {
      this.stopwatch.Stop();
      // Do not change this.run to false or simulation will be stopped!
      this.manualResetEvent.Reset();
    }

    /// <summary>
    /// Cancels the current simulation. The internal state of the cpu and memory is discarded.
    /// </summary>
    public void Stop()
    {
      if (!this.run) { return; }
      this.stopwatch.Stop();
      this.run = false; // Allow the thread to exit.
      this.manualResetEvent.Set(); // Unlock the thread to make that happen.

      // Dispose CPU and memory, create a new one and load rom again?
      //throw new NotImplementedException();
    }

    /// <summary>
    /// Method that is going to be running in a separate thread, calling Step() forever.
    /// </summary>
    private void ThreadedRun()
    {
      while (this.run)
      {
        this.manualResetEvent.Wait(); // Wait for pauses.
        this.Step(false);

        // Check timing issues
        if (this.frameReady)
        {
          long ellapsedStopwatchTicks = this.stopwatch.ElapsedTicks;

          // Should we sleep?
          if (ellapsedStopwatchTicks < stopwatchTicksPerFrame)
          {
            this.manualResetEvent.Reset();
            int timeToWait = (int)(/*0.5 + */1000.0 * (stopwatchTicksPerFrame - ellapsedStopwatchTicks) / Stopwatch.Frequency);
            this.manualResetEvent.Wait(timeToWait);
            this.manualResetEvent.Set();
          }
#if TIMING
          if (sampleCounter < maxSamples)
          {
            int index = sampleCounter * sampleAmount;
            timingSamples[index + 0] = swCPU.ElapsedTicks;
            timingSamples[index + 1] = swClockMem.ElapsedTicks;
            timingSamples[index + 2] = swDisplay.ElapsedTicks;
            timingSamples[index + 3] = swBlit.ElapsedTicks;
            ++sampleCounter;
          }
          ++frameCounter;

          swCPU.Reset();
          swClockMem.Reset();
          swDisplay.Reset();
          swBlit.Reset();
#endif

          this.stopwatch.Restart();
          this.tickCounter = 0;
          this.stepCounter = 0;
          this.frameReady = false;

          // Finally here we trigger the notification
#if TIMING
          swBlit.Start();
          NotifyFrameCompleted();
          swBlit.Stop();
#else
          NotifyFrameCompleted();
#endif

        }
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
#warning TODO (wooo): Receive the frame here and trigger a new event instead of accessing directly to the display from the view.
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

    public IEnumerable<IInstruction>
    Disassamble(ushort startAddress, bool permissive = true)
    {
      return disassembler.Disassamble(startAddress, permissive);
    }

    ~GameBoy()
    {
#if TIMING
      using (var file = new System.IO.StreamWriter("timing.log", false))
      {
        // We write the total timing information
        file.WriteLine("{0}", (int)stopwatchTicksPerFrame);

        // We write the header
        file.WriteLine("{0},{1},{2},{3}", "CPU", "Clock & Mem", "Display", "Blit");

        // We write the data
        for (int i = 0; i < sampleCounter; ++i)
        {
          int index = i * sampleAmount;
          //long total = timingSamples[index + 2] +
          //             timingSamples[index + 3] +
          //             timingSamples[index + 4];
          //long rest = timingSamples[index + 1] - total;
          ////file.WriteLine("Frame {0}: {1}/{2} --> CPU: {3} ({4:N2}%), Display: {5} ({6:N2}%), Blit: {7} ({8:N2}%), Other: {9} ({10:N2}%)",
          file.WriteLine("{0},{1},{2},{3}",
                         timingSamples[index + 0],
                         timingSamples[index + 1],
                         timingSamples[index + 2],
                         timingSamples[index + 3]);
        }
      }
#endif
    }
  }
}
