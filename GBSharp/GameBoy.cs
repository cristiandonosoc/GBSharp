using System.Threading;
using GBSharp.Cartridge;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using GBSharp.VideoSpace;
using System.Diagnostics;

namespace GBSharp
{
  public class GameBoy : IGameBoy
  {
    public event Action StepFinished;

    private CPUSpace.CPU cpu;
    private CPUSpace.InterruptController interruptController;
    private MemorySpace.Memory memory;
    private Cartridge.Cartridge cartridge;
    private Display display;
    private bool run;
    private Thread clockThread;
    private ManualResetEventSlim manualResetEvent;
    private Keypad buttons;
    
    private Stopwatch stopwatch;
    private long tickCounter;
    private long stepCounter;
    private const int stepCheck = 5000; // ~ 5ms. Steps ellapsed between timer checks.
    private const double targetSecondsPerTick = 0.0000002384185791015625; // It is know that this is 2^-22.
    private const int minimumSleep = 5; // Used to avoid sleeping intervals that are too short.


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

      this.buttons = Keypad.None;
      this.manualResetEvent = new ManualResetEventSlim(false);
      this.clockThread = new Thread(new ThreadStart(this.ThreadedRun));
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

// DEBUG ONLY, COLOR BARS:
#if DEBUG

      byte[] internalMemory = (memory as IMemory).Data;

      // We write the sample tiles
      for (uint i = 0x8000;
          i < 0x9800;
          i++)
      {
        if (i < 0x8800)
        {
          internalMemory[i] = (byte)((i % 2 == 0) ? 0xFF : 0x00);
        }
        else if (i < 0x9000)
        {
          internalMemory[i] = (byte)((i % 2 == 0) ? 0x00 : 0xFF);
        }
        else
        {
          internalMemory[i] = (byte)0xCC;
        }
      }

      // We write the sample display memory
      for (uint i = 0;
           i < 1024;
           i++)
      {
        internalMemory[i + 0x9800] = (byte)(1023 - i);
      }

#endif
    }

    /// <summary>
    /// Runs the simulation for the smallest amount of time possible.
    /// This should be 1 whole instruction, arbitrary machine and clock cycles.
    /// </summary>
    public void Step()
    {
      byte ticks = this.cpu.Step();
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
      // NOTE(cdonoso): If we're running, we shouldn't restart. Or should we?
      if(this.run) { return; }
      if (this.cartridge == null) { return; }
      this.run = true;
      this.stepCounter = 0;
      this.tickCounter = 0;
      this.stopwatch.Restart();
      this.manualResetEvent.Set();
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
        this.Step();
        NotifyStepFinished();

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
    }

    /// <summary>
    /// Call this method when a button is released in the user interface.
    /// </summary>
    /// <param name="button">The button that was released. It can be a combination of buttons too.</param>
    public void ReleaseButton(Keypad button)
    {
      this.buttons &= ~button;
      this.interruptController.UpdateKeypadState(this.buttons);
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
  }
}
