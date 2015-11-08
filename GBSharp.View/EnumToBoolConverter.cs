using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GBSharp.ViewModel;

namespace GBSharp.View
{
  [ValueConversion(typeof(TileMapOptions), typeof(bool))]
  public class EnumToBoolConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var ps = parameter as string;
      if (string.IsNullOrEmpty(ps))
        return DependencyProperty.UnsetValue;
      if (!Enum.IsDefined(typeof(TileMapOptions), value))
        return DependencyProperty.UnsetValue;

      var param = Enum.Parse(value.GetType(), ps);

      var ret = param.Equals(value);

      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var ps = parameter as string;
      if (string.IsNullOrEmpty(ps))
      {
        return DependencyProperty.UnsetValue;
      }
      return Enum.Parse(targetType, ps);
    }
  }
}
