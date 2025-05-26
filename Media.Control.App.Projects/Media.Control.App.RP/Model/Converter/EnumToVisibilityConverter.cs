using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Media.Control.App.RP.Model
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    if (value == null || parameter == null)
        //        return Visibility.Collapsed;

        //    string enumValue = value.ToString();
        //    string targetValue = parameter.ToString();

        //    return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase)
        //        ? Visibility.Visible
        //        : Visibility.Hidden;
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    // Conversion back logic here
        //    //return value.Equals(true) ? parameter : Binding.DoNothing;
        //    return System.Windows.Data.Binding.DoNothing;
        //}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            // 열거형 값끼리 비교
            return value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
