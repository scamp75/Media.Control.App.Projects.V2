using System;
using System.IO;
using System.Windows.Media.Imaging;

public static class ImageConveter
{
    public static BitmapImage ByteArrayToBitmapImage(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
            return null;

        BitmapImage bitmap = new BitmapImage();
        using (MemoryStream stream = new MemoryStream(imageData))
        {
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 이미지를 메모리에 캐시
            bitmap.StreamSource = stream;
            bitmap.EndInit();
        }
        bitmap.Freeze(); // 멀티스레딩 환경에서 안전하게 사용하려면 필요
        return bitmap;
    }
}
