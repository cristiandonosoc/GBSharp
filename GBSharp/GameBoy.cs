using System.Threading;
using GBSharp.Catridge;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;

namespace GBSharp
{
  public class GameBoy : IGameBoy
  {
    private CPUSpace.CPU cpu;
    private MemorySpace.Memory memory;
    private Cartridge cartridge;
    private bool run;
    private Thread clockThread;
    private ManualResetEventSlim manualResetEvent;

    /// <summary>
    /// Class constructor.
    /// Initializes cpu and memory.
    /// </summary>
    public GameBoy()
    {
      this.run = false;
      this.memory = new MemorySpace.Memory();
      this.cpu = new CPUSpace.CPU(this.memory);
      this.cartridge = new Cartridge();
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

    /// <summary>
    /// Loads 
    /// </summary>
    /// <param name="cartridgeData"></param>
    public void LoadCartridge(byte[] cartridgeData)
    {
      this.cartridge.Load(cartridgeData);
      // We create the MemoryHandler according to the data
      // from the cartridge and set it to the memory.
      // From this point onwards, all the access to memory
      // are done throught the MemoryHandler
      this.memory.SetMemoryHandler(
        GBSharp.MemorySpace.MemoryHandlers.
        MemoryHandlerFactory.CreateMemoryHandler(this.cartridge));

    }

    /// <summary>
    /// Runs the simulation for the smallest amount of time possible.
    /// This should be 1 whole instruction, arbitrary machine and clock cycles.
    /// </summary>
    public void Step()
    {
      this.cpu.Step();
    }

    /// <summary>
    /// Attempts to run until the heat death of the universe.
    /// </summary>
    public void Run()
    {
      this.run = true;
      this.manualResetEvent.Set();
      this.clockThread.Start();
    }
    
    /// <summary>
    /// Pauses the simulation until Run is called again.
    /// </summary>
    public void Pause()
    {
      // Do not change this.run to false or simulation will be stopped!
      this.manualResetEvent.Reset();
    }

    /// <summary>
    /// Cancels the current simulation. The internal state of the cpu and memory is discarded.
    /// </summary>
    public void Stop()
    {
      this.run = false; // Allow the thread to exit.
      this.manualResetEvent.Set(); // Unlock the thread to make that happen.
      
      // Dispose CPU and memory, create a new one and load rom again?
      throw new NotImplementedException();
    }

    /// <summary>
    /// Method that is going to be running in a separate thread, calling Step() forever.
    /// </summary>
    private void ThreadedRun()
    {
      while (this.run)
      {
        this.manualResetEvent.Wait();
        this.Step();

        // TODO: Check here timing issues
      }
    }
  }
}
