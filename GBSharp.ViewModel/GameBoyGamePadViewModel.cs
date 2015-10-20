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

    public ICommand ButtonACommand
    {
      get { return new DelegateCommand(ButtonA); }
    }

    public ICommand ButtonBCommand
    {
      get { return new DelegateCommand(ButtonB); }
    }



    public ICommand ButtonLeftCommand
    {
      get { return new DelegateCommand(ButtonLeft); }
    }


    public ICommand ButtonUpCommand
    {
      get { return new DelegateCommand(ButtonUp); }
    }



    public ICommand ButtonRightCommand
    {
      get { return new DelegateCommand(ButtonRight); }
    }



    public ICommand ButtonDownCommand
    {
      get { return new DelegateCommand(ButtonDown); }
    }


    public ICommand ButtonStartCommand
    {
      get { return new DelegateCommand(ButtonStart); }
    }


    public ICommand ButtonSelectCommand
    {
      get { return new DelegateCommand(ButtonSelect); }
    }

    public GameBoyGamePadViewModel(IGameBoy gameBoy, IDispatcher dispatcher, DisplayViewModel displayVM)
    {
      _dispatcher = dispatcher;
      _displayVm = displayVM;
      _gameBoy = gameBoy;
      _display = _gameBoy.Display;
      _displayVm.UpdateDisplay += OnUpdateDisplay;
      _display.RefreshScreen += OnRefreshScreen;
    }

    private void ButtonA()
    {
      _gameBoy.PressButton(Keypad.A);
    }

    private void ButtonB()
    {
      _gameBoy.PressButton(Keypad.B);
    }

    private void ButtonLeft()
    {
      _gameBoy.PressButton(Keypad.Left);
    }

    private void ButtonUp()
    {
      _gameBoy.PressButton(Keypad.Up);
    }

    private void ButtonRight()
    {
      _gameBoy.PressButton(Keypad.Right);
    }

    private void ButtonDown()
    {
      _gameBoy.PressButton(Keypad.Down);
    }

    private void ButtonStart()
    {
      _gameBoy.PressButton(Keypad.Start);
    }


    private void ButtonSelect()
    {
      _gameBoy.PressButton(Keypad.Select);
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