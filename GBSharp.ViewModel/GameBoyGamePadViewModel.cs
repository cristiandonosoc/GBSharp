using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GBSharp.ViewModel
{
  public class GameBoyGamePadViewModel : ViewModelBase, IDisposable
  {
    private readonly IGameBoy _gameBoy;
    private readonly IDisplay _display;
    private readonly IDispatcher _dispatcher;
    private readonly DisplayViewModel _displayVm;

    private WriteableBitmap _screen;

    private int _frameCount;
    private double _fps;
    private DateTime _previousTime = DateTime.Now;
    private bool _releaseButtons;

    private bool _screenOnly;

    public WriteableBitmap Screen
    {
      get { return _screen; }
      set
      {
        _screen = value;
        OnPropertyChanged(() => Screen);
      }
    }

    public bool ReleaseButtons
    {
      get { return _releaseButtons; }
      set
      {
        _releaseButtons = value;
        _gameBoy.ReleaseButtons = value;
        OnPropertyChanged(() => ReleaseButtons);
      }
    }

    public string FPS
    {
      get { return _fps.ToString("0.00"); }
    }

    public ICommand ButtonUpDownCommand { get { return new DelegateCommand(ButtonUpDown); } }
    public ICommand ButtonUpUpCommand { get { return new DelegateCommand(ButtonUpUp); } }
    private void ButtonUpDown() { _gameBoy.PressButton(Keypad.Up); }
    private void ButtonUpUp() { _gameBoy.ReleaseButton(Keypad.Up); }

    public ICommand ButtonDownDownCommand { get { return new DelegateCommand(ButtonDownDown); } }
    public ICommand ButtonDownUpCommand { get { return new DelegateCommand(ButtonDownUp); } }
    private void ButtonDownDown() { _gameBoy.PressButton(Keypad.Down); }
    private void ButtonDownUp() { _gameBoy.ReleaseButton(Keypad.Down); }

    public ICommand ButtonLeftDownCommand { get { return new DelegateCommand(ButtonLeftDown); } }
    public ICommand ButtonLeftUpCommand { get { return new DelegateCommand(ButtonLeftUp); } }
    private void ButtonLeftDown() { _gameBoy.PressButton(Keypad.Left); }
    private void ButtonLeftUp() { _gameBoy.ReleaseButton(Keypad.Left); }

    public ICommand ButtonRightDownCommand { get { return new DelegateCommand(ButtonRightDown); } }
    public ICommand ButtonRightUpCommand { get { return new DelegateCommand(ButtonRightUp); } }
    private void ButtonRightDown() { _gameBoy.PressButton(Keypad.Right); }
    private void ButtonRightUp() { _gameBoy.ReleaseButton(Keypad.Right); }

    public ICommand ButtonADownCommand { get { return new DelegateCommand(ButtonADown); } }
    public ICommand ButtonAUpCommand { get { return new DelegateCommand(ButtonAUp); } }
    private void ButtonADown() { _gameBoy.PressButton(Keypad.A); }
    private void ButtonAUp() { _gameBoy.ReleaseButton(Keypad.A); }

    public ICommand ButtonBDownCommand { get { return new DelegateCommand(ButtonBDown); } }
    public ICommand ButtonBUpCommand { get { return new DelegateCommand(ButtonBUp); } }
    private void ButtonBDown() { _gameBoy.PressButton(Keypad.B); }
    private void ButtonBUp() { _gameBoy.ReleaseButton(Keypad.B); }

    public ICommand ButtonSelectDownCommand { get { return new DelegateCommand(ButtonSelectDown); } }
    public ICommand ButtonSelectUpCommand { get { return new DelegateCommand(ButtonSelectUp); } }
    private void ButtonSelectDown() { _gameBoy.PressButton(Keypad.Select); }
    private void ButtonSelectUp() { _gameBoy.ReleaseButton(Keypad.Select); }

    public ICommand ButtonStartDownCommand { get { return new DelegateCommand(ButtonStartDown); } }
    public ICommand ButtonStartUpCommand { get { return new DelegateCommand(ButtonStartUp); } }

    public bool ScreenOnly
    {
      get { return _screenOnly; }
      set
      {
        _screenOnly = value;
        OnPropertyChanged(() => ScreenOnly);
      }
    }

    private void ButtonStartDown() { _gameBoy.PressButton(Keypad.Start); }
    private void ButtonStartUp() { _gameBoy.ReleaseButton(Keypad.Start); }

    public GameBoyGamePadViewModel(IGameBoy gameBoy, IDispatcher dispatcher, DisplayViewModel displayVM)
    {
      _dispatcher = dispatcher;
      _displayVm = displayVM;
      _gameBoy = gameBoy;
      _display = _gameBoy.Display;
      //_displayVm.UpdateDisplay += OnUpdateDisplay;
      _gameBoy.FrameCompleted += OnFrameCompleted;

      VideoSpace.DisplayDefinition disDef = _display.GetDisplayDefinition();
      _screen = new WriteableBitmap(disDef.screenPixelCountX, disDef.screenPixelCountY,
                                    96, 96,
                                    System.Windows.Media.PixelFormats.Bgra32, null);
      _frame = new uint[disDef.screenPixelCountX * disDef.screenPixelCountY];
    }

    uint[] _frame;

    private void OnFrameCompleted()
    {
      _dispatcher.BeginInvoke(new Action(TransferImageToBitmap), null);
      //_dispatcher.Invoke(CopyFromDomain);
      //_dispatcher.Invoke(UpdateFPS);
    }

    private void CopyFromDomain()
    {
      ReleaseButtons = _gameBoy.ReleaseButtons;
      // BeginInvoke returns immediatelly
      _dispatcher.BeginInvoke(new Action(TransferImageToBitmap), null);
    }

    private void TransferImageToBitmap()
    {

      // We copy the ready screen Frame
      var target = _gameBoy.ScreenFrame;
      Array.Copy(target, _frame, target.Length);
      Utils.TransferBytesToWriteableBitmap(_screen, _frame);
      OnPropertyChanged(() => Screen);

      UpdateFPS();

    }

    private void UpdateFPS()
    {
      _frameCount++;
      if (_frameCount % 10 == 0)
      {
        var currentTime = DateTime.Now;
        var deltaTime = currentTime - _previousTime;
        _previousTime = currentTime;
        _fps = (float)(_frameCount) / (deltaTime.TotalSeconds);
        _frameCount = 0;
        OnPropertyChanged(() => FPS);
      }
    }

    private void OnUpdateDisplay()
    {
      OnFrameCompleted();
    }

    public void Dispose()
    {
      _gameBoy.FrameCompleted -= OnFrameCompleted;
      //_displayVm.UpdateDisplay -= OnUpdateDisplay;
    }

    public void KeyUp(KeyEventArgs args)
    {
      if (args.Key == Key.Enter)
        _gameBoy.ReleaseButton(Keypad.Start);
      else if (args.Key == Key.Space)
        _gameBoy.ReleaseButton(Keypad.Select);
      else if (args.Key == Key.O)
        _gameBoy.ReleaseButton(Keypad.A);
      else if (args.Key == Key.P)
        _gameBoy.ReleaseButton(Keypad.B);
      else if (args.Key == Key.A)
        _gameBoy.ReleaseButton(Keypad.Left);
      else if (args.Key == Key.D)
        _gameBoy.ReleaseButton(Keypad.Right);
      else if (args.Key == Key.W)
        _gameBoy.ReleaseButton(Keypad.Up);
      else if (args.Key == Key.S)
        _gameBoy.ReleaseButton(Keypad.Down); 
    }

    public void KeyDown(KeyEventArgs args)
    {
      if (args.Key == Key.Enter)
        _gameBoy.PressButton(Keypad.Start);
      else if (args.Key == Key.Space)
        _gameBoy.PressButton(Keypad.Select);
      else if (args.Key == Key.O)
        _gameBoy.PressButton(Keypad.A);
      else if (args.Key == Key.P)
        _gameBoy.PressButton(Keypad.B);
      else if (args.Key == Key.A)
        _gameBoy.PressButton(Keypad.Left);
      else if (args.Key == Key.D)
        _gameBoy.PressButton(Keypad.Right);
      else if (args.Key == Key.W)
        _gameBoy.PressButton(Keypad.Up);
      else if (args.Key == Key.S)
        _gameBoy.PressButton(Keypad.Down); 
      
    }
  }
}