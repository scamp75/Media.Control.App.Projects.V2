using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Drawing;

namespace Media.Control.App.MeidaBrowser.Model
{
    public class ImagePathToDrawingImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    // FileStream을 사용하여 파일 잠금 문제 없이 이미지를 로드합니다.
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        // Image.FromStream으로 이미지를 생성한 후, 새 Bitmap으로 복사하여 스트림을 안전하게 닫습니다.
                        using (Image tempImage = Image.FromStream(stream))
                        {
                            return new Bitmap(tempImage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 예외 발생 시 로그를 남기거나 적절한 처리를 할 수 있습니다.
                    System.Diagnostics.Debug.WriteLine($"이미지 로드 오류: {ex.Message}");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 양방향 바인딩 시 사용되지만, 여기서는 OneWay 바인딩이므로 구현하지 않습니다.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
