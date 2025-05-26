using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Media.Control.App.RP.Model
{
    public static class ImageConversion
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// System.Drawing.Bitmap을 WPF ImageSource로 변환합니다.
        /// </summary>
        /// <param name="bitmap">변환할 Bitmap 객체</param>
        /// <returns>변환된 ImageSource 객체</returns>
        public static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            
            try
            {
                ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height)); // .FromEmptyOptions());
                return wpfBitmap;
            }
            finally
            {
                // 메모리 누수를 방지하기 위해 핸들을 해제합니다.
                DeleteObject(hBitmap);
            }
        }
    }
}
