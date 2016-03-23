using System.Threading;
using GBSharp.Cartridge;
using GBSharp.CPUSpace;
using GBSharp.MemorySpace;
using System;
using GBSharp.VideoSpace;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace GBSharp
{
  public class GameBoy : IGameBoy, IDisposable
  {
    internal static double targetFramerate = 60.0; // This is not the real gameboy framerate, but it's a nice number.
    internal static double stopwatchTicksPerFrame = Stopwatch.Frequency / targetFramerate;
    internal static double stopwatchTicksPerMs = Stopwatch.Frequency / 1000.0;

    internal static double targetMillisecondsPerTick = 0.0002384185791015625; // It is know that this is 2^-22.
    internal static int ticksPerMillisecond = 4194; // Actually it's 4194,304

    class State
    {
      internal bool Pause;
      internal bool Run;
      internal bool FrameReady;
      internal bool InBreakpoint;
    }

    State _state = new State();

    public event Action StepCompleted;
    public event Action FrameCompleted;
    public event Action PauseRequested;
    public event Action StopRequested;

    private CPUSpace.CPU _cpu;
    private CPUSpace.InterruptController _interruptController;
    private MemorySpace.Memory _memory;

    private Cartridge.Cartridge _cartridge;
    internal string CartridgeDirectory { get; private set; }
    internal string CartridgeFilename { get; private set; }

    private VideoSpace.Display _display;
    private AudioSpace.APU _apu;
    private SerialSpace.SerialController _serial;
    private Thread _gameLoopThread;

    private ManualResetEvent _pauseEvent;
    private ManualResetEventSlim _manualResetEvent;

    private Keypad _buttons;
    private Disassembler _disassembler;

    private Stopwatch _stopwatch;
    private long _stepCounter;

    public bool ReleaseButtons { get; set; }

    private int _frameCounter = 0;
    private long _totalFrameTicks = 0;
    public double FPS { get; private set; }

    // This is a debug counter
    public ulong DebugTickCounter { get; set; }

    #region INTERFACE GETTERS

    public ICPU CPU { get { return _cpu; } }
    public IMemory Memory { get { return _memory; } }
    public ICartridge Cartridge { get { return _cartridge; } }
    public IDisplay Display { get { return _display; } }
    public IAPU APU { get { return _apu; } }
    public IDisassembler Disassembler { get { return _disassembler; } }

    #endregion

#if TIMING
    private long[] _timingSamples;

    private int _timingFrameCounter = 0;
    private int _sampleCounter = 0;
    private int _maxSamples = 60 * 100;
    private int _sampleAmount = 5;

    private Stopwatch _swCPU;
    private Stopwatch _swDisplay;
    private Stopwatch _swBlit;
    private Stopwatch _swClockMem;
    public static Stopwatch _swBeginInvoke = new Stopwatch();
#endif

    /// <summary>
    /// Class constructor.
    /// Initializes cpu and memory.
    /// </summary>
    public GameBoy()
    {
      _state.Run = false;
      _stopwatch = new Stopwatch();

      _memory = new MemorySpace.Memory();
      _cpu = new CPUSpace.CPU(this._memory);
      _interruptController = this._cpu._interruptController;
      _display = new Display(this._interruptController, this._memory);
      _apu = new AudioSpace.APU(this._memory, 44000, 2, 2);
      _serial = new SerialSpace.SerialController(this._interruptController, this._memory);
      _disassembler = new Disassembler(_cpu, _memory);

      // Events
      _cpu.BreakpointFound += BreakpointHandler;
      _cpu.InterruptHappened += InterruptHandler;
      _display.FrameReady += FrameReadyHandler;

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
      if(_state.Run) { this.Stop(); }

      if(resetComponents)
      {
        _memory.Reset();
        _cpu.Reset();
        _interruptController.Reset();
        _display.Reset();
        _apu.Reset();
        _disassembler.Reset();
        //this.serial = new SerialSpace.SerialController(this.interruptController, this.memory);
      }

      // We re-hook information
      if(this._cartridge != null)
      {
        _apu.CartridgeFilename = this.CartridgeFilename; 
        _memory.SetMemoryHandler(GBSharp.MemorySpace.MemoryHandlers.
                                     MemoryHandlerFactory.CreateMemoryHandler(this));
      }

      _buttons = Keypad.None;
      _pauseEvent = new ManualResetEvent(true);
      _manualResetEvent = new ManualResetEventSlim(false);

      _state.InBreakpoint = false;
      ReleaseButtons = true;

      var disDef = _display.GetDisplayDefinition();

#if TIMING
      _timingSamples = new long[_sampleAmount * _maxSamples];
      _swCPU = new Stopwatch();
      _swDisplay = new Stopwatch();
      _swBlit = new Stopwatch();
      _swClockMem = new Stopwatch();
#endif
    }

    /// <summary>
    /// Provides access to the interrupt controller to every component connected to the gameboy, so interrupts
    /// can be requested from memory handlers, the display controller, serial ports, etc.
    /// If an interrupt can be notified from the memory handler (on write), then direct access from another component
    /// to the interrupt controller is not recommended and the memory.Write() call should be used instead.
    /// </summary>
    internal CPUSpace.InterruptController InterruptController
    {
      get { return _interruptController; }
    }

    /// <summary>
    /// Connects to the gameboy a new cartridge with the given contents.
    /// </summary>
    /// <param name="cartridgeData"></param>
    public void LoadCartridge(string cartridgeFullFilename, byte[] cartridgeData)
    {
      if (_state.Run) { Reset(); }
      _cartridge = new Cartridge.Cartridge();
      _cartridge.Load(cartridgeData);

      _cpu.ResetBreakpoints();
      CartridgeFilename = Path.GetFileNameWithoutExtension(cartridgeFullFilename);
      CartridgeDirectory = Path.GetDirectoryName(cartridgeFullFilename);
      _apu.CartridgeFilename = this.CartridgeFilename; 
      // We create the MemoryHandler according to the data
      // from the cartridge and set it to the memory.
      // From this point onwards, all the access to memory
      // are done throught the MemoryHandler
      _memory.SetMemoryHandler(GBSharp.MemorySpace.MemoryHandlers.
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
      if (_state.InBreakpoint)
      {
        _state.InBreakpoint = false;
        ignoreNextStep = true;
      }

#if TIMING 
      _swCPU.Start();
      // TODO(Cristian): Update rest of flow
      byte ticks = _cpu.DetermineStep(ignoreNextStep || ignoreBreakpoints);
      _swCPU.Stop();

      _swClockMem.Start();
      // NOTE(Cristian): If the CPU is halted, the hardware carry on
      if (_cpu.halted) { ticks = 4; }
      _cpu.UpdateClockAndTimers(ticks);
      _memory.Step(ticks);
      _swClockMem.Stop();

      _swDisplay.Start();
      _display.Step(ticks);
      _swDisplay.Stop();
#else
      /**
       * The CPU Step works as following
       * 1. Setup instruction and determine pre-execution ticks
       * 2. Advance gameboy the pre-execution ticks
       * 3. Execute the opcode
       * 4. Advance gameboy the post-execution ticks (if any)
       * 5. Execute the opcode post-execution code (if any)
       *
       * See the CPUSpace/Dictionaries to see how many clocks each pre/post opcode
       * has and what code is run at pre/post stage (in the case of pre stage, the 
       * dictionary is simply CPU*Instructions, no "Pre" appended)
       */

      // Sets the current instruction and obtain how many ticks the peripherals have
      // to be simulated before opcode execution
      byte prevTicks = this._cpu.DetermineStep(ignoreNextStep || ignoreBreakpoints);

      // NOTE(Cristian): If the CPU is halted, the hardware carry on at a simulated clock
      byte postTicks = 0;
      if (_cpu.Halted)
      {
        prevTicks = 4;
        StepPeripherals(prevTicks);
      }
      else
      {
        // We only need to run the rest of the machine if there where actually ticks
        // stepped. A 0 ticks means the cpu didnt clock, probably due to a breakpoint
        if (prevTicks > 0)
        {
          StepPeripherals(prevTicks);

          // We execute the opcode. Returns the amount of ticks that have to be
          // simulated after the opcode execution
          postTicks = this._cpu.ExecuteInstruction();
          // postTicks 0 means that the instruction run at its last clock
          // so there is no postprocessing to be done
          if (postTicks > 0)
          {
            StepPeripherals(postTicks);
            // Some opcodes run some code after execution (this is for two-stage opcodes
            // that read and write at different clocks)
            this._cpu.PostExecuteInstruction();
          }
        }
      }

      // We track the ticks
      DebugTickCounter += prevTicks;
      DebugTickCounter += postTicks;

#endif
    }

    private void StepPeripherals(byte ticks)
    {
      this._cpu.UpdateClockAndTimers(ticks);
      this._memory.Step(ticks);
      this._display.Step(ticks);
      this._apu.Step(ticks);
      this._stepCounter++;
      this._serial.Step(ticks);
    }

    /// <summary>
    /// Steps the Gameboy until a frame is ready
    /// </summary>
    private void CalculateFrame()
    {
      _display.StartFrame();
      while ((_state.Run) && (!_state.FrameReady))
      {
        // We check if the thread has been paused
        if (_state.Pause)
        {
          PauseRequested();
          _pauseEvent.WaitOne(Timeout.Infinite);
        }

        MachineStep(false);
      }
      _display.EndFrame();
    }

    /// <summary>
    /// Attempts to run until the heat death of the universe.
    /// </summary>
    public void Run()
    {
      // We first try to unpause
      if (_state.Pause)
      {
        this._pauseEvent.Set();
        _state.Pause = false;
        this._stopwatch.Start();
      }

      // NOTE(cdonoso): If we're running, we shouldn't restart. Or should we?
      if (_state.Run) { return; }
      if (this._cartridge == null) { return; }

      _state.Run = true;
      this._stepCounter = 0;
      this._stopwatch.Restart();
      this._manualResetEvent.Set();

      if(_gameLoopThread == null)
      {
        // This is the case if the gameboy is started of has been stoped
        this._gameLoopThread = new Thread(new ThreadStart(this.ThreadedRun));
        this._gameLoopThread.Start();
      }
    }

    /// <summary>
    /// Pauses the simulation until Run is called again.
    /// </summary>
    public void Pause()
    {
      if (!_state.Run) { return; }
      if (_state.Pause) { return; }
      _state.Pause = true;
      this._stopwatch.Stop();
      this._pauseEvent.Reset();
    }

    /// <summary>
    /// Cancels the current simulation. The internal state of the cpu and memory is discarded.
    /// </summary>
    public void Stop()
    {
      if (!_state.Run) { return; }
      _state.Run = false;
      this._stopwatch.Stop();

      // We unpause if needed
      this._manualResetEvent.Set();
      this._pauseEvent.Set();

      this._gameLoopThread.Join();
      // We get rid of the current thread (a new will be created on restart)
      this._gameLoopThread = null;
      StopRequested();
    }

    /// <summary>
    /// Method that is going to be running in a separate thread, calling Step() forever.
    /// </summary>Infinite
    private void ThreadedRun()
    {
      long drama = 0;
      while (_state.Run)
      {
        CalculateFrame();

        // Check timing issues
        long ellapsedStopwatchTicks = this._stopwatch.ElapsedTicks;
        ellapsedStopwatchTicks += drama;

        // Should we sleep?
        if (ellapsedStopwatchTicks < stopwatchTicksPerFrame)
        {
          this._manualResetEvent.Reset();
          int timeToWait = (int)(/* 0.5 + */1000.0 * (stopwatchTicksPerFrame - ellapsedStopwatchTicks) / Stopwatch.Frequency);
          this._manualResetEvent.Wait(timeToWait);
          this._manualResetEvent.Set();
        }

        long finalTicks = this._stopwatch.ElapsedTicks;
        while (finalTicks < stopwatchTicksPerFrame)
        {
          // NOTE(Cristian): Sometimes the thread would be trapped here because when
          //                 the process close signal gets, the timers stop working and
          //                 the finalTicks variable would never update.
          if ((!_state.Run) || (_state.Pause)) { break; }
          finalTicks = this._stopwatch.ElapsedTicks;
        }

        drama = finalTicks - (long)stopwatchTicksPerFrame;

        // We calculate how many FPS we're giving
        _totalFrameTicks += finalTicks;
        ++_frameCounter;
        if (_frameCounter >= 30)
        {
          FPS = Math.Round(60 * (double)(30 * stopwatchTicksPerFrame) / (double)_totalFrameTicks);
          _frameCounter = 0;
          _totalFrameTicks = 0;
        }

#if TIMING
        if (_sampleCounter < _maxSamples)
        {
          int index = _sampleCounter * _sampleAmount;
          _timingSamples[index + 0] = _swCPU.ElapsedTicks;
          _timingSamples[index + 1] = _swClockMem.ElapsedTicks;
          _timingSamples[index + 2] = _swDisplay.ElapsedTicks;
          _timingSamples[index + 3] = _swBlit.ElapsedTicks;
          _timingSamples[index + 4] = _swBeginInvoke.ElapsedTicks;
          ++_sampleCounter;
        }
        ++_timingFrameCounter;

        _swCPU.Reset();
        _swClockMem.Reset();
        _swDisplay.Reset();
        _swBlit.Reset();

        _swBeginInvoke.Reset();
#endif

        _stopwatch.Restart();
        _stepCounter = 0;
        _state.FrameReady = false;

        // We see if the APU needs to close things this frame
        _apu.EndFrame();

        // Finally here we trigger the notification
        NotifyFrameCompleted();

      }
    }

    public void SaveState()
    {
      // We stop pause the simulation
      Pause();

      string filename = "save_state.stt";

      SaveStateFileFormat saveState = new SaveStateFileFormat();
      // We ge the states
      saveState.MemoryState = _memory.GetState();
      saveState.CartridgeState = _cartridge.GetState();
      saveState.DisplayState = _display.GetState();
      saveState.InterruptState = _interruptController.GetState();

      FileStream saveStateStream;
      if (!File.Exists("save_state.stt"))
      {
        saveStateStream = File.Create(filename);
      }
      else
      {
        saveStateStream = File.Open(filename, FileMode.Truncate);
      }
      IFormatter formatter = new BinaryFormatter();
      formatter.Serialize(saveStateStream, saveState);

      Run();
    }

    public void LoadState()
    {
      Pause();

      string filename = "save_state.stt";

      if (File.Exists(filename))
      {
        FileStream loadStateStream = File.Open(filename, FileMode.Open);

        BinaryFormatter formatter = new BinaryFormatter();
        SaveStateFileFormat state = formatter.Deserialize(loadStateStream) as SaveStateFileFormat;
        _memory.SetState(state.MemoryState);
        _cartridge.SetState(state.CartridgeState);
        _display.SetState(state.DisplayState);
        _interruptController.SetState(state.InterruptState);
      }

      Run();
    }

    /// <summary>
    /// Debugger. Handles the cpu.BreakPointFound event.
    /// </summary>
    private void BreakpointHandler()
    {
      _state.InBreakpoint = true;
      Pause();
    }

    /// <summary>
    /// Debugger. Handles the cpu.InterruptHappened event.
    /// </summary>
    private void InterruptHandler(Interrupts interrupt)
    {
      _state.InBreakpoint = true;
      Pause();
    }

    /// <summary>
    /// Handles a new frame from the display.
    /// </summary>
    private void FrameReadyHandler()
    {
      _state.FrameReady = true;
    }

    /// <summary>
    /// Call this method when a button is pressed in the user interface.
    /// </summary>
    /// <param name="button">The button that was pressed. It can be a combination of buttons too.</param>
    public void PressButton(Keypad button)
    {
      _buttons |= button;
      _interruptController.UpdateKeypadState(_buttons);
      if (_cpu.Stopped) { _cpu.Stopped = false; }
    }

    /// <summary>
    /// Call this method when a button is released in the user interface.
    /// </summary>
    /// <param name="button">The button that was released. It can be a combination of buttons too.</param>
    public void ReleaseButton(Keypad button)
    {
      if (ReleaseButtons)
      {
        _buttons &= ~button;
        _interruptController.UpdateKeypadState(_buttons);
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
        _swBlit.Start();
        _display.CopyTargets();
        _swBlit.Stop();
#else
        _display.CopyTargets();
#endif

        FrameCompleted();
      }
    }

    public Dictionary<MMR, ushort> GetRegisterDic()
    {
      Dictionary<MMR, ushort> registerDic = new Dictionary<MMR, ushort>();
      foreach (MMR registerEnum in Enum.GetValues(typeof(MMR)))
      {
        registerDic.Add(registerEnum, _memory.LowLevelRead((ushort)registerEnum));
      }
      return registerDic;
    }

    public void Dispose()
    {
      _apu.Dispose();
      _memory.Dispose();
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
        for (int i = 0; i < _sampleCounter; ++i)
        {
          int index = i * _sampleAmount;
          file.WriteLine("{0},{1},{2},{3},{4}",
                         _timingSamples[index + 0],
                         _timingSamples[index + 1],
                         _timingSamples[index + 2],
                         _timingSamples[index + 3],
                         _timingSamples[index + 4]);
        }
      }
#endif
    }
  }
}
