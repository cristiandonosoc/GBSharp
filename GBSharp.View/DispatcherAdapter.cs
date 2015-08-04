using System;
using System.Windows.Controls;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  public class DispatcherAdapter : IDispatcher
  {
    private readonly Control _dipatchingControl;

    public DispatcherAdapter(Control dipatchingControl)
    {
      _dipatchingControl = dipatchingControl;
    }

    public void Invoke(Action functionToInvoke)
    {
      _dipatchingControl.Dispatcher.Invoke(functionToInvoke);
    }
  }
}
