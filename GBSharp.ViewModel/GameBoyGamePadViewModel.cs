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

        private WriteableBitmap _screen;
        private string _textScreen;

        private int _frameCount;
        private double _fps;
        private DateTime _previousTime = DateTime.Now;
        private bool _releaseButtons;

        private bool _screenOnly;
        private bool _asciiMode;

        public WriteableBitmap Screen
        {
            get { return _screen; }
            set
            {
                _screen = value;
                OnPropertyChanged(() => Screen);
            }
        }

        public string TextScreen
        {
            get { return _textScreen; }
            set
            {
                _textScreen = value;
                OnPropertyChanged(() => TextScreen);
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
        private void ButtonStartDown() { _gameBoy.PressButton(Keypad.Start); }
        private void ButtonStartUp() { _gameBoy.ReleaseButton(Keypad.Start); }

        public bool ScreenOnly
        {
            get { return _screenOnly; }
            set
            {
                _screenOnly = value;
                OnPropertyChanged(() => ScreenOnly);
            }
        }

        public bool AsciiMode
        {
            get { return _asciiMode; }
            set
            {
                _asciiMode = value;
                OnPropertyChanged(() => AsciiMode);
            }
        }


        public GameBoyGamePadViewModel(IGameBoy gameBoy, IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _gameBoy = gameBoy;
            _display = _gameBoy.Display;
            _gameBoy.FrameCompleted += OnFrameCompleted;

            VideoSpace.DisplayDefinition disDef = _display.GetDisplayDefinition();
            _screen = new WriteableBitmap(disDef.ScreenPixelCountX, disDef.ScreenPixelCountY,
                                          96, 96,
                                          System.Windows.Media.PixelFormats.Bgra32, null);
            _frame = new uint[disDef.ScreenPixelCountX * disDef.ScreenPixelCountY];
        }

        uint[] _frame;

        private void OnFrameCompleted()
        {
#if TIMING
      GameBoy._swBeginInvoke.Start();
      _dispatcher.BeginInvoke(new Action(UpdateFromGameboy), null);
      GameBoy._swBeginInvoke.Stop();
#else
            _dispatcher.BeginInvoke(new Action(UpdateFromGameboy), null);
#endif
        }

        private void UpdateFromGameboy()
        {
            ReleaseButtons = _gameBoy.ReleaseButtons;

            // We copy the ready screen Frame
            var target = _gameBoy.Display.Screen;
            Array.Copy(target, _frame, target.Length);
            Utils.TransferBytesToWriteableBitmap(_screen, _frame);
            if (AsciiMode)
                TextScreen = Utils.DisplayBytesToString(_frame, new[] { ' ', ':', '*', '@' }, _screen.PixelWidth);
            OnPropertyChanged(() => Screen);

            UpdateFPS();
        }

        private void UpdateFPS()
        {
            _frameCount++;
            if (_frameCount > 30)
            {
                _fps = _gameBoy.FPS;
                OnPropertyChanged(() => FPS);
                _frameCount = 0;

            }
        }

        private void OnUpdateDisplay()
        {
            OnFrameCompleted();
        }

        public void Dispose()
        {
            _gameBoy.FrameCompleted -= OnFrameCompleted;
        }

        public void KeyUp(ButtonMapping mapping, KeyEventArgs args)
        {
            foreach(Key key in mapping.Keys)
            {
                if (args.Key == key)
                {
                    _gameBoy.ReleaseButton(mapping[key]);
                }
            }
        }

        public void KeyDown(ButtonMapping mapping, KeyEventArgs args)
        {
            foreach(Key key in mapping.Keys)
            {
                if (args.Key == key)
                {
                    _gameBoy.PressButton(mapping[key]);
                }
            }
        }
    }
}