using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MpvNet;
using static System.Windows.Forms.AxHost;

namespace Mpv.Player.App
{
    public enum EnuStat { Play, Pause, Eject, Seek, Error };
   
    public class MpvControl
    {
        private MainPlayer Player = null;
        private MediaInfo MediaInfo = null;
        private TimecodeCalculator Calculator = null;

        
        public double TotalDuration = 0;

        public EnuStat Statue { get; set; } = EnuStat.Pause;

        public string TotalTimecode { get; set; } = "00:00:00;00";
        public long TotalFrame { get; set; }    

        public double FPS { get; set; }

        public bool isLoad = false;

        public ulong Frame { get; set; }

        private double fSpeed = 1;

        public MpvControl()
        {
            Player = new MainPlayer();
            
        }

        public void Init(IntPtr Handel)
        {
            Player?.Init(Handel, true);
        }


        public void Close()
        {
            Player?.Destroy();
        }

        public void Load(string path)
        {
            Player.LoadFiles(new[] { path }, false, false);
//            Pause();

            Thread.Sleep(300);
            FPS = GetFps(path);
            isLoad = true;

            Calculator = new TimecodeCalculator(Convert.ToDecimal(FPS));

            double duration = Duration();

            TotalDuration = (ulong)Math.Round(Calculator.SecondToFrameNumber((decimal)duration));
            

            //if (FPS != 59.94)
            //    TotalDuration -= 0.033367;
            //else
            //    TotalDuration -= 0.016683;

          //  TotalFrame = (long)Math.Round(Calculator.SecondToFrameNumber((decimal)TotalDuration));

        }

        private double GetFps(string path)
        {
            MediaInfo = new MediaInfo(path);
            return Convert.ToDouble(MediaInfo.GetInfo(MediaInfoStreamKind.Video, "FrameRate")); 
        }

        public void Play()
        {

            var timecode = Player.GetPropertyDouble("time-pos", false);

            var Duration = (ulong)Math.Round(Calculator.SecondToFrameNumber((decimal)timecode));

            if (Duration + 1 < TotalDuration)
            {
                if (Player.GetPropertyDouble("speed") != 1)
                    Player.SetPropertyDouble("speed", 1);

                Player.SetPropertyString("pause", "no");

                Statue = EnuStat.Play;
            }

        }

        public void Pause()
        {
            Player.SetPropertyString("pause", "yes");
            Statue = EnuStat.Pause;

        }

        public void Stop()
        {
            Player.Command("stop");
            Statue = EnuStat.Eject;
        }

        private double Duration()
        {
            return Player.GetPropertyDouble("duration");
        }

        public string TimeCode()
        {
            string result = "00:00:00;00";

            try
            {
                if (isLoad)
                {
                    var timecode = Player.GetPropertyDouble("time-pos", false);
                    Frame = (ulong)Math.Round(Calculator.SecondToFrameNumber((decimal)timecode));
                    result = Calculator?.FrameNumberToTimecode(Frame); 


                    Console.WriteLine($"timecode :{timecode} frame : {Frame}");
                }
                
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        
        public void FF()
        {
            while (true) 
            {
                var timecode = Player.GetPropertyDouble("time-pos", false);

                var postion = timecode + 30;
                Player.SetPropertyDouble("time-pos", postion);

                var calSecond = Calculator.SecondToFrameNumber((decimal)timecode);
                if ((ulong)calSecond >= TotalDuration) Statue = EnuStat.Pause;

                if (Statue == EnuStat.Pause) break;
                Thread.Sleep(30);
            }
            
        }

        public void SetPostion(double postion)
        {
            Player.SetPropertyDouble("time-pos", postion);
        }

        public void First()
        {
            Pause();
            SetPostion(0.000001);
            Statue = EnuStat.Pause;
        }

        public void End()
        {
            Pause();
            SetPostion(TotalDuration);
            Statue = EnuStat.Pause;
        }


        public void Back1Frame()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false);

            if (0.000001 < timecode)
            {
                double count = 0.033367;
                if (FPS == 59.94) count = 0.016683;

                var second = timecode - count;
                if (second == 0) second = 0.000001;

                Player.Command("frame-back-step");

                Statue = EnuStat.Pause;
            }
        }

        public void Next1Frame()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false);
            var frame = (double)Math.Round(Calculator.SecondToFrameNumber((decimal)timecode));

            if(TotalDuration - (frame + 1) == 1)
            {
                End();
            }
            else
            {
                if (frame + 1 < TotalDuration)
                    Player.Command("frame-step");

            }

            Statue = EnuStat.Pause;
        }


        public void Next5Second()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false) + 5;
            
            var calSecond = Calculator.SecondToFrameNumber((decimal)timecode);
            if ((ulong)calSecond < TotalDuration)
                SetPostion(timecode);

            Statue = EnuStat.Pause;
        }

        public void Back5Second()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false);
            var second = timecode - 5;
            if (second <= 0) second = 0.000001;

            SetPostion(second);
            Statue = EnuStat.Pause;
        }


        public void Next10Second()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false) + 60;

            var calSecond = Calculator.SecondToFrameNumber((decimal)timecode);
            if ((ulong)calSecond < TotalDuration)
                SetPostion(timecode);

            Statue = EnuStat.Pause;
        }

        public void Back10Second()
        {
            var timecode = Player.GetPropertyDouble("time-pos", false);
            var second = timecode - 60;
            if (second <= 0) second = 0.000001;

            SetPostion(second);
            Statue = EnuStat.Pause;
        }

        public void Back10Frame()
        {
            //var timecode = Player.GetPropertyDouble("time-pos", false);


            
            if (Frame != 0)
            {
                if (Frame <= 10)
                    SetPostion(0.000001);
                else
                {
                    var rewtime = Calculator.FrameNumberToSecond( (decimal)Frame - 10);
                    SetPostion((double)rewtime);
                }

                Statue = EnuStat.Pause;
            }
        }
        public void Next10Frame()
        {
            //var timecode = Player.GetPropertyDouble("time-pos", false);

            if ((ulong)(Frame + 10) < TotalDuration)
            {
                var fftime = Calculator.FrameNumberToSecond((decimal)Frame + 10);
                SetPostion((double)fftime);
            }
            else End();


            Statue = EnuStat.Pause;
        }

        public void NextSpeed()
        {
            fSpeed = fSpeed + (fSpeed * 0.2);
            if (fSpeed >= 2.5) fSpeed = 2.5;

            Math.Round(fSpeed, 1);

            Player.CommandV("multiply", "speed", "2");

            //RemoteData.SetMultiply(false);
            //Commands.ShowText($"Speed : {RemoteData.GetSpeed() * 2}", 5000, 18);
        }

        public void Config(string path)
        {
            Player.SetPropertyString("config-dir", path);
        }


    }
}
