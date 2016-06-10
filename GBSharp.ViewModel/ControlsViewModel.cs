using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
    public class ButtonMapping
    {
        private Dictionary<Key, Keypad> _mappingDictionary;

        public Keypad UpButton;
        public Keypad DownButton;
        public Keypad LeftButton;
        public Keypad RightButton;

        public Keypad AButton;
        public Keypad BButton;
        public Keypad StartButton;
        public Keypad SelectButton;

        public Keypad FastButton;

        public ButtonMapping()
        {
            _mappingDictionary = new Dictionary<Key, Keypad>();
            _mappingDictionary.Add(Key.Up, Keypad.Up);
            _mappingDictionary.Add(Key.Down, Keypad.Down);
            _mappingDictionary.Add(Key.Left, Keypad.Left);
            _mappingDictionary.Add(Key.Right, Keypad.Right);

            _mappingDictionary.Add(Key.A, Keypad.A);
            _mappingDictionary.Add(Key.B, Keypad.B);
            _mappingDictionary.Add(Key.Enter, Keypad.Start);
            _mappingDictionary.Add(Key.RightShift, Keypad.Select);

            _mappingDictionary.Add(Key.Oem3, Keypad.Speed);
        }
    }

    public class ControlsViewModel : ViewModelBase, IDisposable
    {

        private string _upControl;
        public string UpControl
        {
            get { return _upControl; }
            set
            {
                if (_upControl == value) { return; }
                _upControl = value;
                OnPropertyChanged(() => UpControl);
            }
        }

        private string _downControl;
        public string DownControl
        {
            get { return _downControl; }
            set
            {
                if (_downControl == value) { return; }
                _downControl = value;
                OnPropertyChanged(() => DownControl);
            }
        }

        private string _leftControl;
        public string LeftControl
        {
            get { return _leftControl; }
            set
            {
                if (_leftControl == value) { return; }
                _leftControl = value;
                OnPropertyChanged(() => LeftControl);
            }
        }

        private string _rightControl;
        public string RightControl
        {
            get { return _rightControl; }
            set
            {
                if (_rightControl == value) { return; }
                _rightControl = value;
                OnPropertyChanged(() => RightControl);
            }
        }

        private string _aControl;
        public string AControl
        {
            get { return _aControl; }
            set
            {
                if (_aControl == value) { return; }
                _aControl = value;
                OnPropertyChanged(() => AControl);
            }
        }

        private string _bControl;
        public string BControl
        {
            get { return _bControl; }
            set
            {
                if (_bControl == value) { return; }
                _bControl = value;
                OnPropertyChanged(() => BControl);
            }
        }

        private string _startControl;
        public string StartControl
        {
            get { return _startControl; }
            set
            {
                if (_startControl == value) { return; }
                _startControl = value;
                OnPropertyChanged(() => StartControl);
            }
        }

        private string _selectControl;
        public string SelectControl
        {
            get { return _selectControl; }
            set
            {
                if (_selectControl == value) { return; }
                _selectControl = value;
                OnPropertyChanged(() => SelectControl);
            }
        }

        private string _fastControl;
        public string FastControl
        {
            get { return _fastControl; }
            set
            {
                if (_fastControl == value) { return; }
                _fastControl = value;
                OnPropertyChanged(() => FastControl);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ControlsViewModel class.
        /// </summary>
        public ControlsViewModel()
        {
        }

        public void Dispose()
        {
        }
    }
}