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

    private BitmapImage _screen;

    private int _frameCount;
    private double _fps;
    private DateTime _previousTime = DateTime.Now;

    public BitmapImage Screen
    {
      get { return _screen; }
      set
      {
        _screen = value;
        OnPropertyChanged(() => Screen);
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
    private void ButtonStartDown() { _gameBoy.PressButton(Keypad.Start); }
    private void ButtonStartUp() { _gameBoy.ReleaseButton(Keypad.Start); }

    public GameBoyGamePadViewModel(IGameBoy gameBoy, IDispatcher dispatcher, DisplayViewModel displayVM)
    {
      _dispatcher = dispatcher;
      _displayVm = displayVM;
      _gameBoy = gameBoy;
      _display = _gameBoy.Display;
      _displayVm.UpdateDisplay += OnUpdateDisplay;
      _display.RefreshScreen += OnRefreshScreen;
    }

    private void CopyFromDomain()
    {
      Screen = Utils.BitmapToImageSource(_display.Screen);
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

    private void OnRefreshScreen()
    {
      _dispatcher.Invoke(CopyFromDomain);
      _dispatcher.Invoke(UpdateFPS);
    }

    private void OnUpdateDisplay()
    {
      OnRefreshScreen();
    }

    public void Dispose()
    {
      _display.RefreshScreen -= OnRefreshScreen;
      _displayVm.UpdateDisplay -= OnUpdateDisplay;
    }

  }
}