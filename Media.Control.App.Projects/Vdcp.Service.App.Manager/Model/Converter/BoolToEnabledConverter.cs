using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Vdcp.Service.App.Manager.Model
{
    public class BoolToEnabledConverter : IValueConverter
    {
        // bool → IsEnabled (true or false)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue;

            return DependencyProperty.UnsetValue;
        }

        // IsEnabled → bool (보통 필요 없음)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue;

            return DependencyProperty.UnsetValue;
        }
    }
}
