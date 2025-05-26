using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Media.Control.App.MeidaBrowser.Model
{
    public class DateTimeToStringConverter : IValueConverter
    {
        // DateTime → String 변환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // Format 파라미터가 있으면 해당 포맷으로 변환
                string format = parameter as string ?? "yyyy-MM-dd";
                return dateTime.ToString(format, culture);
            }
            return string.Empty;
        }

        // String → DateTime 변환
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString && DateTime.TryParseExact(
                dateString,
                parameter as string ?? "yyyy-MM-dd",
                culture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }
            return DateTime.Now; // 변환 실패 시 현재 시간을 반환
        }
    }
}
