using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  public interface IKeyboardHandler
  {
    event Action<KeyEventArgs> KeyDown;
    event Action<KeyEventArgs> KeyUp;
  }
}