using System;

namespace GBSharp.ViewModel
{
  public interface IDispatcher
  {
    void Invoke(Action functionToInvoke);
  }
}