using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private IDispatcher _dispatcher;
    private readonly IWindow _window;
    private readonly IKeyboardHandler _keyboardHandler;
    private readonly IGameBoy _gameBoy;

    private readonly MemoryViewModel _memory;
    private readonly CPUViewModel _cpu;
    private readonly InterruptManagerViewModel _interrupt;
    private readonly IORegistersManagerViewModel _ioRegisters;
    private readonly DisplayViewModel _display;
    private readonly GameBoyContollerViewModel _gameBoyController;
    private readonly GameBoyGamePadViewModel _gameBoyGamePad;
    private readonly DissasembleViewModel _dissasemble;

    public MemoryViewModel Memory
    {
      get { return _memory; }
    }

    public CPUViewModel CPU
    {
      get { return _cpu; }
    }

    public InterruptManagerViewModel Interrupt
    {
      get { return _interrupt; }
    }

    public DisplayViewModel Display
    {
      get { return _display; }
    }

    public GameBoyContollerViewModel GameBoyController
    {
      get { return _gameBoyController; }
    }

    public GameBoyGamePadViewModel GameBoyGamePad
    {
      get { return _gameBoyGamePad; }
    }

    public DissasembleViewModel Dissasemble
    {
      get { return _dissasemble; }
    }

    public IORegistersManagerViewModel IORegisters
    {
      get { return _ioRegisters; }
    }


    public GameBoyViewModel(IGameBoy gameBoy, IDispatcher dispatcher, IWindow window, IOpenFileDialogFactory fileDialogFactory, IKeyboardHandler keyboardHandler)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _window = window;
      _keyboardHandler = keyboardHandler;
      _keyboardHandler.KeyDown += OnKeyDown;
      _keyboardHandler.KeyUp += OnKeyUp;
      _window.OnClosing += HandleClosing;
      _gameBoyController = new GameBoyContollerViewModel(_gameBoy, fileDialogFactory);
      _gameBoyController.OnFileLoaded += FileLoadedHandler;
      _gameBoyController.OnStep += StepHandler;
      _memory = new MemoryViewModel(_gameBoy.Memory, "Memory View");
      _gameBoy.Memory.MemoryWritten += _memory.MemoryWrittenHandler;
      _cpu = new CPUViewModel(_gameBoy, _dispatcher);

      // TODO(aaecheve): Should this be another function handling this?
      _gameBoy.CPU.BreakpointFound += BreakpointHandler;
      _gameBoy.CPU.InterruptHappened += InterruptHandler;

      _interrupt = new InterruptManagerViewModel(_gameBoy, _dispatcher);
      _ioRegisters = new IORegistersManagerViewModel(_gameBoy, _dispatcher);
      _display = new DisplayViewModel(_gameBoy, _gameBoy.Display, _gameBoy.Memory, _dispatcher);
      _gameBoyGamePad = new GameBoyGamePadViewModel(_gameBoy, _dispatcher, _display);
      _dissasemble = new DissasembleViewModel(_gameBoy);
    }

    public void OnClosed()
    {
      _gameBoyController.OnClosed();
    }

    private void OnKeyUp(KeyEventArgs args)
    {
      _gameBoyGamePad.KeyUp(args);
    }

    private void OnKeyDown(KeyEventArgs args)
    {
      _gameBoyGamePad.KeyDown(args);
    }

    private void InterruptHandler(GBSharp.CPUSpace.Interrupts interrupt)
    {
      // TODO(Cristian, aaecheve): Do something special with each interrupt?
      _dispatcher.Invoke(StepHandler);
    }

    private void BreakpointHandler()
    {
      // NOTE(Cristian): This handler comes from an event from the _gameboy, thus
      //                 it happens in the gameboy's thread. If we want to modify UI
      //                 constructs, we need to run them in the UI thread
      _dispatcher.Invoke(StepHandler);
    }

    private void StepHandler()
    {
      _cpu.CopyFromDomain();
      _display.CopyFromDomain();
      _dissasemble.SetCurrentSelectedInstruction();
      _interrupt.CopyFromDomain();
      _ioRegisters.CopyFromDomain();

      if(_memory.HighlightUpdated == false)
      {
        _memory.CopyFromDomain();
        _memory.HighlightUpdated = true;
      }
    }

    private void FileLoadedHandler()
    {
      _memory.CopyFromDomain();
      _display.CopyFromDomain();
      _cpu.CopyFromDomain();
      _dissasemble.DissasembleCommandWrapper();
    }

    private void HandleClosing()
    {
      _display.Dispose();
      _interrupt.Dispose();
      _ioRegisters.Dispose();
      _gameBoyGamePad.Dispose();
      _cpu.Dispose();
      _gameBoyController.OnFileLoaded -= FileLoadedHandler;
      _gameBoyController.OnStep -= StepHandler;
      _gameBoyController.Dispose();
      _gameBoy.Stop();

      _window.OnClosing -= HandleClosing;
    }
  }
}