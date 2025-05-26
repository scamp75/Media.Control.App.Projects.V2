using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace Media.Control.App.RP.Model
{
    public class LevelToBrushConverter : IValueConverter
    {
        public System.Windows.Media.Brush LowBrush { get; set; }
        public System.Windows.Media.Brush HighBrush { get; set; }
        public double Threshold { get; set; } = 0.7;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double level)
            {
                return level >= Threshold ? HighBrush : LowBrush;
            }
            return LowBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
