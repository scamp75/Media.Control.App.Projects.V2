using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Media.Control.App.MeidaBrowser.Model
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        // bool -> Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool invert = parameter != null && parameter.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase);
                return invert ? (boolValue ? Visibility.Collapsed : Visibility.Visible)
                              : (boolValue ? Visibility.Visible : Visibility.Collapsed);
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
