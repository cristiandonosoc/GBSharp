using System;

namespace GBSharp.ViewModel
{
  public interface IWindow
  {
    event Action OnClosing;
  }
}