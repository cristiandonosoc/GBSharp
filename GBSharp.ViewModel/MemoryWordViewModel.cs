using System;
using System.Windows.Media;

namespace GBSharp.ViewModel
{
  public class MemoryWordViewModel : ViewModelBase
  {
    private readonly uint _address;
    private readonly uint _value;

    private string _addressFormatString;
    private string _valueFormatString;

    private Color _color2 = Color.FromRgb(15, 56, 15);
    private Color _color1 = Color.FromRgb(155, 188, 15);

    public string Address
    {
      get { return _addressFormatString; }
    }

    public string Value
    {
      get { return _valueFormatString; }
    }

    public SolidColorBrush WordColor
    {
      get
      {
        var color = GetWordColor();
        return new SolidColorBrush(color);
      }
    }

    private Color GetWordColor()
    {
      var floatValue = (float) (_value/255.0f);
      var color = _color1 * floatValue + _color2 * (1 - floatValue);
      return color;
    }

    public SolidColorBrush InverseWordColor
    {
      get
      {
        var color = GetInverseWordColor();
        return new SolidColorBrush(color);
      }
    }

    private Color GetInverseWordColor()
    {
      var floatValue = 1.0f - (float)(_value / 255.0f);
      var color = _color1 * floatValue + _color2 * (1 - floatValue);
      return color;
    }

    public MemoryWordViewModel(uint address, uint value)
    {
      _address = address;
      _addressFormatString = address.ToString();
      _value = value;
      _valueFormatString = value.ToString();
    }

    public void UpdateAddressFormat(MemoryWordFormat addressFormat)
    {
      _addressFormatString = GetFormat(addressFormat, _address);
      OnPropertyChanged(() => Address);
    }

    public void UpdateValueFormat(MemoryWordFormat valueFormat)
    {
      _valueFormatString = GetFormat(valueFormat, _value, false);
      OnPropertyChanged(() => Value);
    }

    private string GetFormat(MemoryWordFormat format, uint value, bool usePrefix=true)
    {
      switch (format)
      {
        case MemoryWordFormat.Decimal:
          return "" + value;
        case MemoryWordFormat.Hexa:
          var s = "";
          if (usePrefix)
            s = "0x";
          return s + value.ToString("x2");
        case MemoryWordFormat.Binary:
          return Convert.ToString(value, 2);

      }
      return "";
    }

  }
}