using System;
using System.ComponentModel;
using System.Windows;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  public class WindowAdapter : IWindow
  {
    public event Action OnClosing;
    
    private Window _window;

    public WindowAdapter(Window window)
    {
      _window = window;
      _window.Closing += OnClosingHandle;
    }

    private void OnClosingHandle(object sender, CancelEventArgs e)
    {
      if (OnClosing != null)
        OnClosing();
    }
  }
}