using System;
using System.Threading.Tasks;
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
      try
      {
        _dipatchingControl.Dispatcher.Invoke(functionToInvoke);
      }
      catch (TaskCanceledException)
      {
        
        Console.WriteLine("Task canceled");
      }
      
    }

    public void BeginInvoke(Delegate del, params object[] args)
    {
      try
      {
        _dipatchingControl.Dispatcher.BeginInvoke(del, args);
      }
      catch(TaskCanceledException)
      {
        Console.WriteLine("Task canceled");
      }
    }
  }
}
