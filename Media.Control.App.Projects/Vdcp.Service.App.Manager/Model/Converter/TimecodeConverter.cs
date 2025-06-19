using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public static class TimecodeConverter
    {
        public static string FrameToTimecode(int frameCount, double fps)
        {
            if (fps <= 0)
                throw new ArgumentException("FPS must be positive.");

            int hours, minutes, seconds, frames;

            bool isDropFrame =
                Math.Abs(fps - 29.97) < 0.01 ||
                Math.Abs(fps - 59.94) < 0.01;

            if (!isDropFrame)
            {
                int totalSeconds = (int)(frameCount / fps);
                frames = (int)(frameCount % fps);
                seconds = totalSeconds % 60;
                minutes = (totalSeconds / 60) % 60;
                hours = totalSeconds / 3600;

                return $"{hours:D2}:{minutes:D2}:{seconds:D2}:{frames:D2}";
            }
            else
            {
                int fr = (int)Math.Round(fps);
                int dropFrames = (fps < 30.0) ? 2 : 4;
                int framesPer10Minutes = (fps < 30.0) ? 17982 : 35964;

                int d = frameCount / framesPer10Minutes;
                int m = frameCount % framesPer10Minutes;

                int totalMinutes = d * 10 + (m - dropFrames) / ((fr * 60) - dropFrames);
                int droppedFrames = dropFrames * (totalMinutes - (totalMinutes / 10));
                int adjustedFrames = frameCount + droppedFrames;

                frames = adjustedFrames % fr;
                seconds = (adjustedFrames / fr) % 60;
                minutes = (adjustedFrames / (fr * 60)) % 60;
                hours = adjustedFrames / (fr * 60 * 60);

                return $"{hours:D2}:{minutes:D2}:{seconds:D2};{frames:D2}";
            }
        }

        public static int TimecodeToFrame(string timecode, double fps)
        {
            if (fps <= 0)
                throw new ArgumentException("FPS must be positive.");

            bool isDropFrame =
                Math.Abs(fps - 29.97) < 0.01 ||
                Math.Abs(fps - 59.94) < 0.01;

            string[] parts = timecode.Split(new char[] { ':', ';' });

            if (parts.Length != 4)
                throw new FormatException("Timecode must be in HH:MM:SS:FF or HH:MM:SS;FF format.");

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            int frames = int.Parse(parts[3]);

            int fr = (int)Math.Round(fps);
            int totalFrames = ((hours * 3600) + (minutes * 60) + seconds) * fr + frames;

            if (!isDropFrame)
            {
                return totalFrames;
            }
            else
            {
                int dropFrames = (fps < 30.0) ? 2 : 4;
                int totalMinutes = hours * 60 + minutes;
                int droppedFrames = dropFrames * (totalMinutes - (totalMinutes / 10));
                return totalFrames - droppedFrames;
            }
        }
    }
}
