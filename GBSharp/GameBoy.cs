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
    public event Action StepFinished;

    private CPUSpace.CPU cpu;
    private CPUSpace.InterruptController interruptController;
    private MemorySpace.Memory memory;
    private Cartridge.Cartridge cartridge;
    private VideoSpace.Display display;
    private AudioSpace.APU apu;
    private bool run;
    private bool inBreakpoint;
    private Thread clockThread;
    private ManualResetEventSlim manualResetEvent;
    private Keypad buttons;
    private Disassembler disassembler;
    
    private Stopwatch stopwatch;
    private long tickCounter;
    private long stepCounter;
    private const int stepCheck = 5000; // ~ 5ms. Steps ellapsed between timer checks.
    private const double targetSecondsPerTick = 0.0000002384185791015625; // It is know that this is 2^-22.
    private const int minimumSleep = 5; // Used to avoid sleeping intervals that are too short.

    public bool ReleaseButtons { get; set; }

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
      this.apu = new AudioSpace.APU();

      this.disassembler = new Disassembler(cpu, memory);

      this.buttons = Keypad.None;
      this.manualResetEvent = new ManualResetEventSlim(false);
      this.clockThread = new Thread(new ThreadStart(this.ThreadedRun));

      // Events
      this.cpu.BreakpointFound += BreakpointHandler;
      this.cpu.InterruptHappened += InterruptHandler;

      this.inBreakpoint = false;
      this.ReleaseButtons = true;
    }

    private void InterruptHandler(Interrupts interrupt)
    {
      inBreakpoint = true;
      Pause();
    }

    private void BreakpointHandler()
    {
      inBreakpoint = true;
      Pause();
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
      byte ticks = this.cpu.Step(ignoreNextStep || ignoreBreakpoints);

      // NOTE(Cristian): If the CPU is halted, the hardware carry on
      if(cpu.halted) { ticks = 4; }
      this.cpu.UpdateClockAndTimers(ticks);
      this.memory.Step(ticks);
      this.display.Step(ticks);

      this.tickCounter += ticks;
      this.stepCounter++;

      NotifyStepFinished();
    }

    /// <summary>
    /// Attempts to run until the heat death of the universe.
    /// </summary>
    public void Run()
    {
      this.manualResetEvent.Set();
      // NOTE(cdonoso): If we're running, we shouldn't restart. Or should we?
      if(this.run) { return; }
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
        //NotifyStepFinished();

        // TODO(Cristian, Wooo): Check what happens with the tickCounter upon STOP
        // Check timing issues
        if (this.stepCounter % stepCheck == 0)
        {
          long ellapsedms = this.stopwatch.ElapsedMilliseconds;
          long expectedms = (long)(1000 * targetSecondsPerTick * this.tickCounter);

          // Should we sleep?
          if (expectedms - ellapsedms >= minimumSleep)
          {
            this.manualResetEvent.Reset();
            this.manualResetEvent.Wait((int)(expectedms - ellapsedms));
            this.manualResetEvent.Set();

            this.stopwatch.Restart();
            this.tickCounter = 0;
            this.stepCounter = 0;
          }
        }
      }
    }

    /// <summary>
    /// Call this method when a button is pressed in the user interface.
    /// </summary>
    /// <param name="button">The button that was pressed. It can be a combination of buttons too.</param>
    public void PressButton(Keypad button)
    {
      this.buttons |= button;
      this.interruptController.UpdateKeypadState(this.buttons);
      if(cpu.stopped) { cpu.stopped = false; }
    }

    /// <summary>
    /// Call this method when a button is released in the user interface.
    /// </summary>
    /// <param name="button">The button that was released. It can be a combination of buttons too.</param>
    public void ReleaseButton(Keypad button)
    {
      if(ReleaseButtons)
      {
        this.buttons &= ~button;
        this.interruptController.UpdateKeypadState(this.buttons);
      }
    }

    /// <summary>
    /// Notifies subscribers that a step is completed.
    /// </summary>
    private void NotifyStepFinished()
    {
      if (StepFinished != null)
      {
        StepFinished();
      }
    }

    public Dictionary<MemoryMappedRegisters, ushort> GetRegisterDic()
    {
      Dictionary<MemoryMappedRegisters, ushort> registerDic = new Dictionary<MemoryMappedRegisters,ushort>();
      foreach(MemoryMappedRegisters registerEnum in Enum.GetValues(typeof(MemoryMappedRegisters)))
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
  }
}
