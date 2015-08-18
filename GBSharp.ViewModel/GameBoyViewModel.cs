namespace GBSharp.ViewModel
{
  public class GameBoyViewModel : ViewModelBase
  {
    private IDispatcher _dispatcher;
    private readonly IGameBoy _gameBoy;

    private readonly MemoryViewModel _memory;
    private readonly CPUViewModel _cpu;
    private readonly InterruptViewModel _interrupt;
    private readonly DisplayViewModel _display;
    private readonly GameBoyContollerViewModel _gameBoyController;
    private readonly GameBoyGamePadViewModel _gameBoyGamePad;
    
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


    public GameBoyViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
    {
      _gameBoy = gameBoy;
      _dispatcher = dispatcher;
      _gameBoyController = new GameBoyContollerViewModel(_gameBoy);
      _memory = new MemoryViewModel(_gameBoy.Cartridge, "Memory View");//, 0xC000, 0xE000);
      _cpu = new CPUViewModel(_gameBoy.CPU);
      _interrupt = new InterruptViewModel(_gameBoy);
      _display = new DisplayViewModel(_gameBoy.Display);
      _gameBoyGamePad = new GameBoyGamePadViewModel(_gameBoy);
    }

   
    
  }
}