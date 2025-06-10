using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class TimecodeCalculator
    {
        //public static Dictionary<string, TimecodeCalculator> Preset;
        /// <summary>
        /// true: 59.94
        /// false: 29.97
        /// </summary>
        public static Dictionary<bool, TimecodeCalculator> Preset;

        public readonly decimal FPS;
        public readonly bool IsDropFrame;
        public readonly ulong DropFrames;

        private readonly ulong ref_fps;
        private readonly ulong frameNumberPerHourForFPS;
        private readonly ulong frameNumberPerHourForRefFPS;
        private readonly ulong frameNumberPerMinuteForRefFPS;
        private readonly ulong frameNumberPerSecondForRefFPS;

        private readonly ulong frameNumberPerTenMinutesForFPS;
        private readonly ulong frameNumberPerOneMinutesForFPS;

        private readonly ulong diffFrameNumberPerTenMinutesForFPS;

        static TimecodeCalculator()
        {
            //var _24 = new TimecodeCalculator(24M);
            //var _24M = new TimecodeCalculator(24 / 1.001M);

            //var _25 = new TimecodeCalculator(25M);

            //var _30 = new TimecodeCalculator(30M);
            var _30M = new TimecodeCalculator(30 / 1.001M);

            //var _50 = new TimecodeCalculator(50M);

            //var _60 = new TimecodeCalculator(60M);
            var _60M = new TimecodeCalculator(60 / 1.001M);


            //Preset = new Dictionary<string, TimecodeCalculator>
            //{
            //    ["24"] = _24,
            //    ["24/1001"] = _24M,
            //    ["24M"] = _24M,
            //    ["23.98"] = _24M,

            //    ["25"] = _25,

            //    ["30"] = _30,
            //    ["30/1001"] = _30M,
            //    ["30M"] = _30M,
            //    ["29.97"] = _30M,

            //    ["50"] = _50,

            //    ["60"] = _60,
            //    ["60/1001"] = _60M,
            //    ["60M"] = _60M,
            //    ["59.94"] = _60M,
            //};

            Preset = new Dictionary<bool, TimecodeCalculator>
            {
                [false] = _30M,
                [true] = _60M,
            };
        }

        public TimecodeCalculator(decimal fps)
        {
            if (fps <= 0M) return;


            FPS = fps;
            ref_fps = (ulong)Math.Ceiling(FPS);


            frameNumberPerHourForFPS = (ulong)(FPS * 3600M);
            frameNumberPerHourForRefFPS = ref_fps * 3600;

            // 1분마다 (60번) 드랍하지만 10으로 나눠지는 0분,10분,20분,30분,40분,50분 (6번)은 드랍하지 않음
            // 즉 드랍 횟수는 60 - 6 = (54번)
            DropFrames = (frameNumberPerHourForRefFPS - frameNumberPerHourForFPS) / 54;

            if (DropFrames == 0)
                IsDropFrame = false;
            else
                IsDropFrame = true;

            frameNumberPerMinuteForRefFPS = ref_fps * 60;
            frameNumberPerSecondForRefFPS = ref_fps;

            frameNumberPerTenMinutesForFPS = (ulong)(FPS * 600);
            frameNumberPerOneMinutesForFPS = (ulong)(FPS * 60);

            diffFrameNumberPerTenMinutesForFPS = ref_fps * 600 - frameNumberPerTenMinutesForFPS;
        }


        public ulong TimecodeToFrameNumber(Timecode timecode)
        {
            return TimecodToFrameNumber(timecode.Hour, timecode.Minute, timecode.Second, timecode.Frame);
        }

        public ulong TimecodToFrameNumber(ulong hours, ulong minutes, ulong seconds, ulong framese)
        {
            if (IsDropFrame)
            {
                ulong totalMinutes = 60ul * hours + minutes;

                return frameNumberPerHourForRefFPS * hours + frameNumberPerMinuteForRefFPS * minutes + frameNumberPerSecondForRefFPS * seconds + framese - DropFrames * (totalMinutes - totalMinutes / 10ul);
            }
            else
            {
                return frameNumberPerHourForRefFPS * hours + frameNumberPerMinuteForRefFPS * minutes + frameNumberPerSecondForRefFPS * seconds + framese;
            }
        }

        public Timecode FrameNumberToTimecode(ulong frameNumber)
        {
            if (frameNumber == 0)
                return Timecode.Zero;

            if (IsDropFrame)
            {
                // 10 분의 개수
                ulong quotient = frameNumber / frameNumberPerTenMinutesForFPS;
                // 10 분 이하의 프레임 수
                ulong remainder = frameNumber % frameNumberPerTenMinutesForFPS;

                // Non Drop frames 계산
                if (remainder < DropFrames)
                    frameNumber += diffFrameNumberPerTenMinutesForFPS * quotient;
                else
                    frameNumber += diffFrameNumberPerTenMinutesForFPS * quotient + DropFrames * ((remainder - DropFrames) / frameNumberPerOneMinutesForFPS);
            }

            return new Timecode()
            {
                Frame = frameNumber % ref_fps,
                Second = frameNumber / ref_fps % 60,
                Minute = frameNumber / ref_fps / 60 % 60,
                Hour = frameNumber / ref_fps / 60 / 60
            };
        }

        public decimal FrameNumberToSecond(decimal frameNumber)
        {
            return frameNumber / FPS;
        }

        public decimal SecondToFrameNumber(decimal seconds)
        {
            return seconds * FPS;
        }
    }
}
