using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace GBSharp.ViewModel
{
  public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
  {

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private readonly Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();

    protected string GetPropertyNameFromExpression<T>(Expression<Func<T>> property)
    {
      var lambda = (LambdaExpression)property;
      MemberExpression memberExpression;

      if (lambda.Body is UnaryExpression)
      {
        var unaryExpression = (UnaryExpression)lambda.Body;
        memberExpression = (MemberExpression)unaryExpression.Operand;
      }
      else
      {
        memberExpression = (MemberExpression)lambda.Body;
      }

      return memberExpression.Member.Name;
    }

    public void OnPropertyChanged(string propertyName)
    {
      if (!string.IsNullOrEmpty(propertyName) && PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public void OnPropertyChanged<T>(Expression<Func<T>> property)
    {
      OnPropertyChanged(GetPropertyNameFromExpression<T>(property));
    }

    public void AddAndNotifyError<T>(Expression<Func<T>> property, string error)
    {
      string propertyName = GetPropertyNameFromExpression(property);
      AddAndNotifyError(propertyName, error);
    }

    public virtual void AddAndNotifyError(string error)
    {
      AddAndNotifyError(string.Empty, error);
      OnPropertyChanged(() => GlobalErrors);
      OnPropertyChanged(() => HasGlobalErrors);
    }

    public virtual void AddAndNotifyError(string propertyName, string error)
    {
        AddError(propertyName, error);
        NotifyError(propertyName);
    }

    public void AddError<T>(Expression<Func<T>> property, string error)
    {
      string propertyName = GetPropertyNameFromExpression(property);
      AddError(propertyName, error);
    }

    public void AddError(string propertyName, string error)
    {
      if (_validationErrors.ContainsKey(propertyName))
        _validationErrors[propertyName].Add(error);
      else
        _validationErrors.Add(propertyName, new List<string> { error });
    }

    public void NotifyError<T>(Expression<Func<T>> property)
    {
      string propertyName = GetPropertyNameFromExpression(property);
      NotifyError(propertyName);
    }

    protected void NotifyError(string propertyName)
    {
      if (ErrorsChanged != null)
        ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
      OnPropertyChanged(() => HasErrors);
    }

    public virtual void ClearValidation()
    {
      ClearValidation(string.Empty);
      OnPropertyChanged(() => GlobalErrors);
      OnPropertyChanged(() => HasGlobalErrors);
    }

    public virtual void ClearValidation(string propertyName)
    {
      if (_validationErrors.ContainsKey(propertyName))
      {
        _validationErrors.Remove(propertyName);
      }
      NotifyError(propertyName);
      //if(propertyName != string.Empty)
      //  ClearValidation();
    }

    protected void ClearValidation<T>(Expression<Func<T>> property)
    {
      ClearValidation(GetPropertyNameFromExpression(property));
    }

    public virtual System.Collections.IEnumerable GetErrors()
    {
      return GetErrors(string.Empty);
    }

    public System.Collections.IEnumerable GetErrors(string propertyName)
    {
      if (propertyName == null)
        return null;
      if (_validationErrors.ContainsKey(propertyName))
        return _validationErrors[propertyName];
      return null;
    }

    public IEnumerable<string> GlobalErrors
    {
      get
      {
        var errors = GetErrors(string.Empty);
        if (errors != null)
          return errors.Cast<string>().ToList();
        return new List<string>();
      }
    }


    public virtual bool HasErrors
    {
      get { return _validationErrors.Count > 0; }
    }

    public bool HasGlobalErrors
    {
      get { return GlobalErrors.Any(); }
    }

    public bool PropertyHasError<T>(Expression<Func<T>> property)
    {
      return PropertyHasError(GetPropertyNameFromExpression(property));
    }

    public bool PropertyHasError(string propertyName)
    {
      return _validationErrors.ContainsKey(propertyName);
    }

    public virtual void Validate()
    {
      
    }

    protected void ValidateAndNotifyDouble<T>(Expression<Func<T>> property, string value, string errorMessage)
    {
      double doubleValue;
      if (string.IsNullOrEmpty(value) || double.TryParse(value, out doubleValue))
      {
        OnPropertyChanged(property);
      }
      else
      {
        AddAndNotifyError(property, errorMessage);
      }
    }

    protected void ValidateAndNotifyDouble<T>(Expression<Func<T>> property, string value)
    {
      ValidateAndNotifyDouble(property, value, "NonNumeric");
    }

    protected void ClearValidationErrors()
    {
      _validationErrors.Clear();
    }
  }
}
