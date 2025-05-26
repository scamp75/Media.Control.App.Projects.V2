using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mpv.Player.App.Command;
using Mpv.Player.App;
using System.Windows.Input;
using System.Windows.Threading;
using Mpv.Player.App.Model;
using MahApps.Metro.IconPacks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Controls;
using System.Drawing;
using Media.Control.App.Model;

namespace Mpv.Player.App.ViewModel
{
    public enum EnmState
    {
        Play,
        Pause,
        Stop,
        FF,
        RW
    }
    public class PlayerViewModel : INotifyPropertyChanged
    {       

        private MainWindow mainWindow = null;
        private TimecodeCalculator Calculator = null;

        private bool threadStop = true;    
        public double Fps { get; set; } = 30;//29.97;
        private double Duration = 0;
        public MpvControl Player = null;
        public string MediaId = string.Empty; // 미디어 아이디
        private MediaApi mediaApi = null;

        private Task TimeTask = null;
        private long CurrentFrame { get; set; } = 0;
        private long TotalFrames { get; set; }

        public ICommand CommandIn { get; }
        public ICommand CommandInGo { get; }

        public ICommand CommandOut { get; }
        public ICommand CommandOutGo { get; }

        public ICommand CommandFrsit { get; }
        public ICommand CommandB10Frame { get; }

        public ICommand CommandB1Frame { get; }

        public ICommand CommandPlayStop { get; }

        public ICommand CommandF1Frame { get; }

        public ICommand CommandF10Frame { get; }

        public ICommand CommandEnd { get; }
        public ICommand CommandFF { get; }

        public ICommand CommandRW { get; }

        public ICommand CommandSave { get; }


        public ICommand CloseCommand { get; }

        private long maxDuration { get; set; } = 10;
        public long MaxDuration {  get => maxDuration; set { maxDuration = value; OnPropertyChanged(); } }

        private long outPoint { get; set; }
        public long OutPoint { get => outPoint; set { outPoint = value; OnPropertyChanged(); } }

        private long inPoint { get; set; } 
        public long InPoint { get => inPoint; set { inPoint = value; OnPropertyChanged(); } }

        private long sliderValue { get; set; }
        public long SliderValue { get => sliderValue; set { sliderValue = value; OnPropertyChanged(); } }

        private EnmState State { get; set; } = EnmState.Stop;



        private string _TimeCode { get; set; } = "00:00:00:00";

        public string TimeCode
        {
            get { return _TimeCode; }
            set
            {
                _TimeCode = value;
                OnPropertyChanged();
            }
        }

        private string _DetectTaskFileName = string.Empty;

        public string DetectTaskFileName
        {
            get { return _DetectTaskFileName; }
            set
            {
                _DetectTaskFileName = value;
                OnPropertyChanged();
            }
        }

        private Mpv.Player.App.Timecode ConvertTimecode = new Mpv.Player.App.Timecode();
        private string iconPlayStop { get; set; } = "Stop";  

        public string IconPlayStop { get => iconPlayStop; set { iconPlayStop = value; OnPropertyChanged(nameof(IconPlayStop)); } }        

        public PlayerViewModel(MainWindow window) 
        {
            mainWindow = window;
            Player = new MpvControl(window.PlayerHostPanel.Handle);

            mediaApi = new MediaApi();
            mediaApi.ConnectHub();
            mediaApi.MediaDataEvent += MediaApi_MediaDataEvent;
           // mediaApi.UpdateMediaDataEvent += MediaApi_UpdateMediaDataEvent;


            CommandPlayStop = new RelayCommand(Play);

            CommandF1Frame = new RelayCommand(Foraword1Frame);
            CommandB1Frame = new RelayCommand(Back1Frame);

            CommandF10Frame = new RelayCommand(Foraword10Frame);
            CommandB10Frame = new RelayCommand(Back10Frame);

            CommandFrsit = new RelayCommand(First);
            CommandEnd = new RelayCommand(End);

            CommandFF = new RelayCommand(FF);
            CommandRW = new RelayCommand(RW);

            CommandSave = new RelayCommand(async () =>
            {
                if (Player != null)
                {

                    if(!string.IsNullOrEmpty( MediaId) )
                    {
                        if (mainWindow.InPointTextBox.TimeCode != "00:00:00:00")
                        {
                            long inpoint = (long)Calculator.TimecodeToFrameNumber(Mpv.Player.App.Timecode.Parse(mainWindow.InPointTextBox.TimeCode));
                            long frame = (long)(Player.TotalDuration - inpoint);

                            await mediaApi.UpDateInPoint(MediaId
                                            , (int)inpoint
                                            , mainWindow.InPointTextBox.TimeCode
                                            , (int)frame
                                            , Calculator.FrameNumberToTimecode((ulong)frame));
                        }

                        if (mainWindow.OutPointTextBox.TimeCode != Calculator.FrameNumberToTimecode((ulong)Player.TotalDuration))
                        {
                            long outpoint = (long)Calculator.TimecodeToFrameNumber(Mpv.Player.App.Timecode.Parse(mainWindow.OutPointTextBox.TimeCode));
                            long frame = (long)(Player.TotalDuration - outpoint);

                            await mediaApi.UpDateOutPoint(MediaId
                                            , (int)outpoint
                                            , mainWindow.OutPointTextBox.TimeCode
                                            , (int)frame
                                            , Calculator.FrameNumberToTimecode((ulong)frame));
                        }

                    }
                }
            });

            CommandIn = new RelayCommand(() => 
            {
                mainWindow.InPointTextBox.TimeCode = TimeCode;
                InPoint = CurrentFrame;


            });
            
            CommandInGo = new RelayCommand(() => 
            {
                Player.Pause();

                //var inpoint = (long)Calculator.TimecodeToFrameNumber(mainWindow.InPointTextBox.TimeCode);
                SeekByFrames(InPoint);
            });
            
            CommandOut = new RelayCommand(() => 
            {
                mainWindow.OutPointTextBox.TimeCode = TimeCode;
                OutPoint = CurrentFrame;
            });

            CommandOutGo = new RelayCommand(() =>
            {
                Player.Pause();

               // var inpoint = (long)Calculator.TimecodeToFrameNumber(mainWindow.OutPointTextBox.TimeCode);
                SeekByFrames(OutPoint);

            });

            CloseCommand = new RelayCommand(Close);

            // fileInfo = new FileInfo();

            TimeTask = Task.Run(() =>
            {
                while (true)
                {
                    if(threadStop)
                    {
                        if (State == EnmState.FF)
                        {
                            var ffValue = (long)(CurrentFrame + 30);

                            if(Player.TotalDuration < ffValue)
                            {
                                Player.End();
                            }
                            else
                            {
                                SeekByFrames(ffValue);
                            }

                            if (CurrentFrame >= Player.TotalDuration) State = EnmState.Pause;
                        }
                        else if (State == EnmState.RW)
                        {
                            var rwvalue = (long)(CurrentFrame - 30);
                            if(0 > rwvalue)
                            {
                                Player.First();
                            }
                            else
                            {
                                SeekByFrames(rwvalue);
                            }

                            if (CurrentFrame <= 0) State = EnmState.Pause;
                        }

                        CurrentTime();
                    }

                    
                    Thread.Sleep(30);
                }
            });

        }

        private void MediaApi_MediaDataEvent(object? sender, MediaDataInfo2 e)
        {
            //throw new NotImplementedException();
        }

        public void MouseDown(double value)
        {
            Player.Pause();
            

            threadStop = false;
        }

        public void MouseUp(double value)
        {
            SliderValue = (long)value;

            if (SliderValue == TotalFrames)
                SeekByFrames(SliderValue - 1);
            else
                SeekByFrames(SliderValue);

            threadStop = true;
        }

        private void RW()
        {
            State = EnmState.RW;
        }

        private void FF()
        {
            State = EnmState.FF;
        }
      

        private void Close()
        {
            mainWindow.Close();
        }

        // 타임코드의 각 부분을 검증하고 수정하는 메서드
        private string CorrectTimecode(string timecode)
        {
            string[] parts = timecode.Split(':');

            while (parts.Length < 4)
            {
                Array.Resize(ref parts, parts.Length + 1);
                parts[parts.Length - 1] = "00";
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (!Regex.IsMatch(parts[i], @"^\d{2}$"))
                {
                    parts[i] = "00"; // 각 섹션이 두 자리 숫자가 아니면 초기화
                }
            }

            return string.Join(":", parts);
        }

        private void CurrentTime()
        {
            if (Player != null)
            {
                try
                {
                    
                    if(Player.isLoad)
                    {
                        TimeCode = Player.TimeCode(); //ConvertSecondsToTimeCode(CurrentFrame);
                        CurrentFrame = (long)Player.Frame;
                      
                        SliderValue = CurrentFrame;
                    }

                }
                catch (Exception ex) { }
            }
        }

        private string ConvertSecondsToTimeCode(double frameNumber)
        {
            // 프레임 번호를 초로 변환
            double seconds = frameNumber / Fps;

            // 초를 시, 분, 초로 변환
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            // 해당 프레임 번호를 계산 (초당 프레임 값으로 나머지를 계산)
            int currentFrame = (int)(frameNumber % Fps);

            // 시간, 분, 초 및 프레임을 포함한 타임코드 반환
            return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", time.Hours, time.Minutes, time.Seconds, currentFrame);
        }

        private void First()
        {
            Player.First();
            State = EnmState.Pause;
        }

        private void End()
        {
            Player.End();
            State = EnmState.Pause;
        }

        public void Load(string path)
        {

            Player.Load(path);
            IconPlayStop = "Play";
            MaxDuration = (long)Player.TotalDuration;

            State = EnmState.Pause;

            Calculator = new TimecodeCalculator((decimal)Player.FPS);
            Thread.Sleep(300);

            mainWindow.OutPointTextBox.TimeCode = Calculator.FrameNumberToTimecode((ulong)Player.TotalDuration);
        }

        private void Play()
        {
            if(Player.Statue == EnuStat.Pause )
            {
                Player.Play();
                IconPlayStop = "Stop";
                State = EnmState.Play;
            }
            else
            {
                Player.Pause();
                IconPlayStop = "Play";
                State = EnmState.Pause;
            }
        }

        private void Stop()
        {
            Player.Pause();
            State = EnmState.Pause;
        }

        private void Foraword10Frame()
        {
            Player.Pause();
            Player.Next10Frame();

            State = EnmState.Pause;
        }

        private void Back10Frame()
        {
            Player.Pause();
            Player.Back10Frame();
            State = EnmState.Pause;
        }

        private void Foraword1Frame()
        {
            //Player.Pause();
            Player.Next1Frame();
            State = EnmState.Pause;
        }

        private void Back1Frame()
        {
            //Player.Pause();

            Player.Back1Frame();

            State = EnmState.Pause;
        }


        private double FrameToTime(long frameNumber)
        {
            double timeInSeconds = frameNumber / Fps;

            return timeInSeconds;
        }


        private void SeekByFrames(long frameCount)
        {
            try
            {
                var sendSeek = (double)Calculator.FrameNumberToSecond(frameCount);
                Player.SetPostion(sendSeek);
            }
            catch
            {

            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }


}
