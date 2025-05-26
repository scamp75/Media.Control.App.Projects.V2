using System.Windows.Data;
using System.Globalization;

namespace Media.Control.App.RP.Model
{
    public class StringToBoolConverter: IValueConverter
    {
        // 문자열을 bool로 변환 (예: "true" => true, 그 외 => false)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.Equals(stringValue, "Player", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // bool을 문자열로 변환 (예: true => "true", false => "false")
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "true" : "false";
            }
            return "false";
        }
    }
}

