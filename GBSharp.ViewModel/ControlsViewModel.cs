using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
    public class ButtonMapping
    {
        class Keymap
        {
            internal Key Key { get; set; }
            internal Keypad Keypad { get; set; }

            internal Keymap(Key key, Keypad keypad)
            {
                Key = key;
                Keypad = keypad;
            }
        }

        private List<Keymap> _mapping;

        public Key this[Keypad keypad]
        {
            get
            {
                foreach (Keymap map in _mapping)
                {
                    if (map.Keypad == keypad) { return map.Key; }
                }
                return 0;
            }
            set
            {
                foreach (Keymap map in _mapping)
                {
                    if (map.Keypad == keypad)
                    {
                        map.Key = value;
                    }
                }
            }
        }

        public Keypad this[Key key]
        {
            get
            {
                foreach (Keymap map in _mapping)
                {
                    if (map.Key == key) { return map.Keypad; }
                }
                return 0;
            }
            set
            {
                foreach (Keymap map in _mapping)
                {
                    if (map.Key == key)
                    {
                        map.Keypad = value;
                    }
                }
            }
        }


        public ButtonMapping()
        {
            _mapping = new List<Keymap>();

            _mapping.Add(new Keymap(Key.W, Keypad.Up));
            _mapping.Add(new Keymap(Key.S, Keypad.Down));
            _mapping.Add(new Keymap(Key.A, Keypad.Left));
            _mapping.Add(new Keymap(Key.D, Keypad.Right));

            _mapping.Add(new Keymap(Key.O, Keypad.A));
            _mapping.Add(new Keymap(Key.P, Keypad.B));
            _mapping.Add(new Keymap(Key.Enter, Keypad.Start));
            _mapping.Add(new Keymap(Key.RightShift, Keypad.Select));

            _mapping.Add(new Keymap(Key.Oem3, Keypad.Speed));
        }
    }

    public class ControlsViewModel : ViewModelBase, IDisposable
    {

        private string _upControl;
        public string UpControl
        {
            get
            {
                return _mapping[Keypad.Up].ToString();
            }
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
            get
            {
                return _mapping[Keypad.Down].ToString();
            }
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
            get
            {
                return _mapping[Keypad.Left].ToString();
            }
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
            get
            {
                return _mapping[Keypad.Right].ToString();
            }
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
            get
            {
                return _mapping[Keypad.A].ToString();
            }
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
            get
            {
                return _mapping[Keypad.B].ToString();
            }
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
            get
            {
                return _mapping[Keypad.Start].ToString();
            }
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
            get
            {
                return _mapping[Keypad.Select].ToString();
            }
            set
            {
                if (_selectControl == value) { return; }
                _selectControl = value;
                OnPropertyChanged(() => SelectControl);
            }
        }

        private string _speedControl;
        public string SpeedControl
        {
            get
            {
                return _mapping[Keypad.Speed].ToString();
            }
            set
            {
                if (_speedControl == value) { return; }
                _speedControl = value;
                OnPropertyChanged(() => SpeedControl);
            }
        }

        ButtonMapping _mapping;

        /// <summary>
        /// Initializes a new instance of the ControlsViewModel class.
        /// </summary>
        public ControlsViewModel()
        {
            _mapping = new ButtonMapping();
        }

        public void Dispose()
        {
        }
    }
}