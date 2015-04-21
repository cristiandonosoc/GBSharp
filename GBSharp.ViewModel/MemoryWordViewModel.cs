namespace GBSharp.ViewModel
{
  public class MemoryWordViewModel : ViewModelBase
  {
    private readonly int _address;
    private readonly int _value;

    private string _addressFormatString = string.Empty;
    private string _valueFormatString = string.Empty;

    public string Address
    {
      get { return _address.ToString(_addressFormatString); }
    }

    public string Value
    {
      get { return _value.ToString(_valueFormatString); }
    }

    public MemoryWordViewModel(int address, int value)
    {
      _address = address;
      _value = value;
    }

    public void UpdateAddressFormat(MemoryWordFormat addressFormat)
    {
      _addressFormatString = GetFormat(addressFormat);
      OnPropertyChanged(() => Address);
    }

    public void UpdateValueFormat(MemoryWordFormat valueFormat)
    {
      _valueFormatString = GetFormat(valueFormat);
      OnPropertyChanged(() => Value);
    }

    private string GetFormat(MemoryWordFormat format)
    {
      switch (format)
      {
        case MemoryWordFormat.Decimal:
          return "";
        case MemoryWordFormat.Hexa:
          return "x2";
        case MemoryWordFormat.Binary:
          return "";

      }
      return "";
    }

  }
}