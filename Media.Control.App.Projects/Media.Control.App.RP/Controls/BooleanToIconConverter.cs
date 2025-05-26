using System;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;  // iconPacks 네임스페이스가 포함된 어셈블리 참조

namespace Media.Control.App.RP.Controls
{
    public class BooleanToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            return isChecked ? PackIconRemixIconKind.PlayFill : PackIconRemixIconKind.PauseMiniFill;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}