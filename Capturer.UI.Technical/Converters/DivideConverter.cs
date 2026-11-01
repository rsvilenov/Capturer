using System;
using System.Globalization;
using System.Windows.Data;

namespace Capturer.UI.Technical.Converters
{
    public class DivideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, CultureInfo culture)
        {
            if ((double)value == 0)
                return 0;
            return System.Convert.ToDouble(value) / System.Convert.ToDouble(parameter) ;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
