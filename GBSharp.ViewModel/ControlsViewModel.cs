using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
    public class ControlsViewModel : ViewModelBase
    {

        private string _statusText;
        public string StatusText
        {
            get
            {
                string message;
                if (!SetMode)
                {
                    message = "Press any button to insert the binding";
                }
                else
                {
                    message = "Input the binding";
                }

                return message;
            }
        }

        public string UpControl { get { return _mapping[Keypad.Up].ToString(); } }
        public string DownControl { get { return _mapping[Keypad.Down].ToString(); } }
        public string LeftControl { get { return _mapping[Keypad.Left].ToString(); } }
        public string RightControl { get { return _mapping[Keypad.Right].ToString(); } }

        public string AControl { get { return _mapping[Keypad.A].ToString(); } }
        public string BControl { get { return _mapping[Keypad.B].ToString(); } }
        public string StartControl { get { return _mapping[Keypad.Start].ToString(); } }
        public string SelectControl { get { return _mapping[Keypad.Select].ToString(); } }

        public string SpeedControl { get { return _mapping[Keypad.Speed].ToString(); } }

        ButtonMapping _mapping;
        GameBoyViewModel _gameboyViewModel;

        /// <summary>
        /// Initializes a new instance of the ControlsViewModel class.
        /// </summary>
        public ControlsViewModel(GameBoyViewModel gameboyViewModel, ButtonMapping mapping)
        {
            _gameboyViewModel = gameboyViewModel;
            _mapping = mapping;
            SetMode = false;
        }

        public ICommand ButtonDownUpCommand { get { return new DelegateCommand(ButtonDownUp); } }
        public void ButtonDownUp() { SetPressMode(Keypad.Up); }
        public ICommand ButtonDownDownCommand { get { return new DelegateCommand(ButtonDownDown); } }
        public void ButtonDownDown() { SetPressMode(Keypad.Down); }
        public ICommand ButtonDownLeftCommand { get { return new DelegateCommand(ButtonDownLeft); } }
        public void ButtonDownLeft() { SetPressMode(Keypad.Left); }
        public ICommand ButtonDownRightCommand { get { return new DelegateCommand(ButtonDownRight); } }
        public void ButtonDownRight() { SetPressMode(Keypad.Right); }

        public ICommand ButtonDownACommand { get { return new DelegateCommand(ButtonDownA); } }
        public void ButtonDownA() { SetPressMode(Keypad.A); }
        public ICommand ButtonDownBCommand { get { return new DelegateCommand(ButtonDownB); } }
        public void ButtonDownB() { SetPressMode(Keypad.B); }
        public ICommand ButtonDownStartCommand { get { return new DelegateCommand(ButtonDownStart); } }
        public void ButtonDownStart() { SetPressMode(Keypad.Start); }
        public ICommand ButtonDownSelectCommand { get { return new DelegateCommand(ButtonDownSelect); } }
        public void ButtonDownSelect() { SetPressMode(Keypad.Select); }

        public ICommand ButtonDownSpeedCommand { get { return new DelegateCommand(ButtonDownSpeed); } }
        public void ButtonDownSpeed() { SetPressMode(Keypad.Speed); }

        public bool SetMode { get; set; }
        private Keypad _keypadToBeSet;


        private void SetPressMode(Keypad keypad)
        {
            SetMode = true;
            _keypadToBeSet = keypad;
            Refresh();
        }

        internal void SetMapping(Key key)
        {
            _mapping[_keypadToBeSet] = key;
            SetMode = false;
            Refresh();
        }

        internal void Refresh()
        {
            OnPropertyChanged(() => UpControl);
            OnPropertyChanged(() => DownControl);
            OnPropertyChanged(() => LeftControl);
            OnPropertyChanged(() => RightControl);

            OnPropertyChanged(() => AControl);
            OnPropertyChanged(() => BControl);
            OnPropertyChanged(() => StartControl);
            OnPropertyChanged(() => SelectControl);

            OnPropertyChanged(() => SpeedControl);

            OnPropertyChanged(() => StatusText);
            OnPropertyChanged(() => SetMode);
        }
    }
}