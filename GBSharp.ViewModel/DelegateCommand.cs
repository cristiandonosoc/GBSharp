using System;
using System.Windows.Input;

namespace GBSharp.ViewModel
{
  /// <summary>
  /// Provides a bindable object for view model methods
  /// </summary>
  /// <history>
  /// 11/8/2012 12:30:26 PM - Eduardo Vila : Class creation
  /// </history>
  public class DelegateCommand : ICommand
  {

    private readonly Func<object, bool> _canExecute;
    private readonly Action<object> _execute;

    bool _canExecuteCache;

    public bool CanExecute(object parameter)
    {

      bool result = _canExecute(parameter);

      if (result != _canExecuteCache)
      {
        _canExecuteCache = result;

        if (CanExecuteChanged != null)
          CanExecuteChanged(this, new EventArgs());

      }

      return _canExecuteCache;
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      _execute(parameter);
    }

    public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute", "execute cannot be null");
      if (canExecute == null)
        throw new ArgumentNullException("canExecute", "canExecute cannot be null");

      _execute = execute;
      _canExecute = canExecute;
    }

    public DelegateCommand(Action<object> execute)
      : this(execute, (o) => true)
    {

    }

    public DelegateCommand(Action execute)
      : this((o) => execute())
    {

    }

  }
}
