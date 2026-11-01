using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Capturer.UI.Technical.Converters
{
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 4 || values.OfType<double>().Count() != 4) return new Rect(0, 0, 0, 0);
            for (var i = 0; i < values.Length; i++)
                if (double.IsNaN((double)values[i]))
                    values[i] = 0.0;
            return new Rect((double)values[0], (double)values[1]+3, (double)values[2], (double)values[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}
