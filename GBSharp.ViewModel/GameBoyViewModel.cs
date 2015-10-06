namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private IDispatcher _dispatcher;
    private readonly IWindow _window;
    private readonly IGameBoy _gameBoy;

    private readonly MemoryViewModel _memory;
    private readonly CPUViewModel _cpu;
    private readonly InterruptViewModel _interrupt;
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

    public InterruptViewModel Interrupt
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


    public GameBoyViewModel(IGameBoy gameBoy, IDispatcher dispatcher, IWindow window, IOpenFileDialogFactory fileDialogFactory)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _window = window;
      _window.OnClosing += HandleClosing;
      _gameBoyController = new GameBoyContollerViewModel(_gameBoy, fileDialogFactory);
      _gameBoyController.OnFileLoaded += FileLoadedHandler;
      _gameBoyController.OnStep += StepHandler;
      _memory = new MemoryViewModel(_gameBoy.Memory, "Memory View");
      _cpu = new CPUViewModel(_gameBoy, _dispatcher);
      _interrupt = new InterruptViewModel(_gameBoy, _dispatcher);
      _display = new DisplayViewModel(_gameBoy.Display, _gameBoy.Memory, _dispatcher);
      _gameBoyGamePad = new GameBoyGamePadViewModel(_gameBoy, _dispatcher, _display);
      _dissasemble = new DissasembleViewModel(_gameBoy);
    }

    private void StepHandler()
    {
      _cpu.CopyFromDomain();
      _display.CopyFromDomain();
    }

    private void FileLoadedHandler()
    {
      _memory.CopyFromDomain();
      _display.CopyFromDomain();
      _cpu.CopyFromDomain();
      _dissasemble.Dissasemble();
    }

    private void HandleClosing()
    {
      _display.Dispose();
      _interrupt.Dispose();
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