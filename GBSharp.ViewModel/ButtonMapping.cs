using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        internal IEnumerable<Key> Keys
        {
            get
            {
                foreach(Keymap keymap in _mapping)
                {
                    yield return keymap.Key;
                }
                yield break;
            }
        }

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


}
