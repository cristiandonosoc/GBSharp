using System;
using System.Windows.Input;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  public class KeyboardHandler : IKeyboardHandler
  {
    public event Action<KeyEventArgs> KeyDown;
    public event Action<KeyEventArgs> KeyUp;

    public void NotifyKeyDown(KeyEventArgs args)
    {
      if (KeyDown != null)
        KeyDown(args);
    }

    public void NotifyKeyUp(KeyEventArgs args)
    {
      if (KeyUp != null)
        KeyUp(args);
    }
  }
}