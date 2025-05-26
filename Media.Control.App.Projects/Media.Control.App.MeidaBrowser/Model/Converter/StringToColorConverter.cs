using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Media.Control.App.MeidaBrowser.Model
{
    public class StringToColorConverter : IValueConverter
    {
        /// <summary>
        /// 입력 문자열 값에 따라 Color를 반환합니다.
        /// "Wait"  => Yellow
        /// "Play"  => Green
        /// "Cue"   => Blue
        /// "Error" => Red
        /// 그 외의 값은 Gray를 반환합니다.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            if (string.IsNullOrEmpty(status))
            {
                return System.Windows.Media.Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1F);
            }

            switch (status.Trim().ToLower())
            {
                case "done":
                case "wait":
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media
                               .ColorConverter.ConvertFromString("#FF1E1E1F"));
                case "play":
                case "recoding":
                    return new SolidColorBrush(Colors.Red);
                case "cue":
                case "prepared":
                    return new  SolidColorBrush((System.Windows.Media.Color)System.Windows.Media
                               .ColorConverter.ConvertFromString("#7CFC00"));
                case "error":
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media
                               .ColorConverter.ConvertFromString("#EE82EE"));
                default:
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media
                              .ColorConverter.ConvertFromString("#FF1E1E1F"));
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
