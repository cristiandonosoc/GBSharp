using System;

namespace GBSharp.ViewModel
{
  public class RegisterViewModel : ViewModelBase, IDisposable
  {
    private readonly IRegister _register;

    private string _name;
    private int _value;
    private int _size;

    public string Name
    {
      get { return _name; }
    }

    public int Value
    {
      get { return _value; }
      set
      {
        if (_value != value)
        {
          _value = value;
          OnPropertyChanged(() => Value);
        }
      }
    }

    public int Size
    {
      get { return _size; }
    }

    public RegisterViewModel(IRegister register) 
    {
      if (register == null) throw new ArgumentNullException("register");
      _register = register;
      CopyFromDomain();
      RegisterEvents();
    }

    private void RegisterEvents()
    {
      _register.ValueChanged += RegisterValueChanged;
    }

    private void UnregisterEvents()
    {
      if (_register != null)
        _register.ValueChanged -= RegisterValueChanged;
    }

    private void CopyFromDomain()
    {
      _name = _register.Name;
      _value = _register.Value;
      _size = _register.Size;
    }
    
    private void RegisterValueChanged()
    {
      Value = _register.Value;
    }

    public void Dispose()
    {
      UnregisterEvents();
    }
  }
}