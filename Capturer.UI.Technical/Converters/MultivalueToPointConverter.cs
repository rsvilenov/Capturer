using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Capturer.UI.Technical.Converters
{
    public class MultivalueToPointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var returnValue = new Point();
            if (values != null && values.Length == 2)
            {
                returnValue = new Point((double)(values[0] ?? 0d), (double)(values[1] ?? 0d));
            }

            return returnValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
