using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace DeckLinkDirectShowLib
{
    public class VideoFrameConverter
    {
        /// <summary>
        /// DirectShow에서 전달받은 영상 샘플 버퍼를 Bitmap으로 변환합니다.
        /// 이 예제는 영상 데이터가 RGB24 포맷이고, 해상도가 640x480임을 가정합니다.
        /// </summary>
        /// <param name="sampleBuffer">영상 샘플 데이터가 있는 포인터</param>
        /// <param name="bufferSize">버퍼의 전체 크기 (바이트)</param>
        /// <param name="width">영상의 너비 (기본값 640)</param>
        /// <param name="height">영상의 높이 (기본값 480)</param>
        /// <returns>변환된 Bitmap 객체</returns>
        public Bitmap ConvertBufferToBitmap(IntPtr sampleBuffer, int bufferSize, int width, int height, bool isTopDown)
        {
            // 픽셀당 3바이트(RGB24)라고 가정
            int bytesPerPixel = 3;
            int stride = width * bytesPerPixel;
            int expectedSize = stride * height;

            if (bufferSize < expectedSize)
                throw new ArgumentException("버퍼 크기가 해상도와 일치하지 않습니다.");

            // Bitmap 생성 (24bpp)
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // BitmapData 잠금
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bitmap.LockBits(rect,
                                          System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                          System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // 라인 단위 복사
            // Top-Down이면 메모리상에서 위->아래 순으로 라인이 저장되어 있으므로
            // Bitmap에 복사할 때는 y=0부터 순서대로 복사
            // Bottom-Up이면 메모리상에서 아래->위 순으로 저장되어 있으므로
            // Bitmap에 복사할 때는 뒤집어서 복사
            byte[] rowData = new byte[stride];

            if (!isTopDown)
            {
                // Bottom-Up
                for (int y = 0; y < height; y++)
                {
                    IntPtr src = IntPtr.Add(sampleBuffer, y * stride);
                    Marshal.Copy(src, rowData, 0, stride);

                    // Bitmap 내부는 Top-Down이므로 반대 인덱스로 써야 함
                    IntPtr dest = IntPtr.Add(bmpData.Scan0, (height - 1 - y) * bmpData.Stride);
                    Marshal.Copy(rowData, 0, dest, stride);
                }
            }
            else
            {
                // Top-Down
                for (int y = 0; y < height; y++)
                {
                    IntPtr src = IntPtr.Add(sampleBuffer, y * stride);
                    Marshal.Copy(src, rowData, 0, stride);

                    IntPtr dest = IntPtr.Add(bmpData.Scan0, y * bmpData.Stride);
                    Marshal.Copy(rowData, 0, dest, stride);
                }
            }

            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

    }
}
