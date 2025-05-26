using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public static class MediaDiskTime
    {

        public static double GetDriveSize (string path)
        {
            double size = 0;
            DriveInfo drive = new DriveInfo(path);

            if (drive.IsReady) // 사용 가능한 드라이브만
            {
                size = BytesToGB(drive.AvailableFreeSpace);
            }
            
            return size;
        }

        // 바이트 → GB 단위로 변환
        private static double BytesToGB(long bytes)
        {
            return bytes / 1024.0 / 1024.0 / 1024.0;
        }

        public static string GetRecordTimeString(double diskGB, double bitrateMbps)
        {
            // 1 GB = 1024 MB, 1 Byte = 8 bit → 1 Mbps = 0.125 MBps
            double diskMB = diskGB * 1024;
            double bitrateMBps = bitrateMbps / 8.0;

            if (bitrateMBps <= 0)
                throw new ArgumentException("비트레이트는 0보다 커야 합니다.");

            double totalSeconds = diskMB / bitrateMBps;

            // TimeSpan 으로 변환
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

            // hh:mm:ss 형식 문자열 반환 (24시간 이상도 지원)
            string timeString = string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)time.TotalHours,
                time.Minutes,
                time.Seconds);

            return timeString;
        }

        public static (double seconds, double minutes, double hours) CalculateRecordTime(double diskGB, double bitrateMbps)
        {
            // 1 GB = 1024 MB, 1 Byte = 8 bit → 1 Mbps = 0.125 MBps
            double diskMB = diskGB * 1024;
            double bitrateMBps = bitrateMbps / 8.0;

            if (bitrateMBps <= 0)
                throw new ArgumentException("비트레이트는 0보다 커야 합니다.");

            double seconds = diskMB / bitrateMBps;
            double minutes = seconds / 60.0;
            double hours = minutes / 60.0;

            return (seconds, minutes, hours);
        }

    }

}
