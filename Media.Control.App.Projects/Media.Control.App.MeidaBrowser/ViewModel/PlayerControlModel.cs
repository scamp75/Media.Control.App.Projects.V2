using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Media.Control.App.MeidaBrowser.Model;
using Mpv.Player.App;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Media.Control.App.MeidaBrowser.ViewModel
{

    public enum EnmState
    {
        Play,
        Pause,
        Stop,
        FF,
        Cue,
        RW
    }

    public partial class PlayerControlModel : INotifyPropertyChanged
    {

        private MainWindow _mainWindow = null;
        private TimecodeCalculator Calculator = null;

        public event EventHandler<long> TimecodeChanged;

        private bool threadStop = true;
        public double Fps { get; set; } = 29.97;
        private double Duration = 0;
        public MpvControl Player = null;
        public string MediaId = string.Empty; // 미디어 아이디
        
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
        public long MaxDuration { get => maxDuration; set { maxDuration = value; OnPropertyChanged(); } }

        private long outPoint { get; set; }
        public long OutPoint { get => outPoint; set { outPoint = value; OnPropertyChanged(); } }

        private long inPoint { get; set; }
        public long InPoint { get => inPoint; set { inPoint = value; OnPropertyChanged(); } }

        private long sliderValue { get; set; }
        public long SliderValue 
        { 
            get => sliderValue; 
            set 
            { 
                sliderValue = value; 
                OnPropertyChanged();

                TimecodeChanged(null, value);
            } 
        }


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


        public PlayerControlModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Player = new MpvControl();
            //Player.Init( ((System.Windows.Forms.Panel)mainWindow
            //                .WindowsFormsHostControl.Child).Handle);

            CommandPlayStop = new RelayCommand(Play);
            CommandF1Frame = new RelayCommand(Foraword1Frame);
            CommandB1Frame = new RelayCommand(Back1Frame);
            CommandF10Frame = new RelayCommand(Foraword10Frame);
            CommandB10Frame = new RelayCommand(Back10Frame);
            CommandFrsit = new RelayCommand(First);
            CommandEnd = new RelayCommand(End);
            CommandFF = new RelayCommand(FF);
            CommandRW = new RelayCommand(RW);

            #region save
            //CommandSave = new RelayCommand(async () =>
            //{
            //    if (Player != null)
            //    {

            //        if (!string.IsNullOrEmpty(MediaId))
            //        {
            //            if (mainWindow.InPointTextBox.TimeCode != "00:00:00:00")
            //            {
            //                long inpoint = (long)Calculator.TimecodeToFrameNumber(Mpv.Player.App.Timecode.Parse(mainWindow.InPointTextBox.TimeCode));
            //                long frame = (long)(Player.TotalDuration - inpoint);

            //                await mediaApi.UpDateInPoint(MediaId
            //                                , (int)inpoint
            //                                , mainWindow.InPointTextBox.TimeCode
            //                                , (int)frame
            //                                , Calculator.FrameNumberToTimecode((ulong)frame));
            //            }

            //            if (mainWindow.OutPointTextBox.TimeCode != Calculator.FrameNumberToTimecode((ulong)Player.TotalDuration))
            //            {
            //                long outpoint = (long)Calculator.TimecodeToFrameNumber(Mpv.Player.App.Timecode.Parse(mainWindow.OutPointTextBox.TimeCode));
            //                long frame = (long)(Player.TotalDuration - outpoint);

            //                await mediaApi.UpDateOutPoint(MediaId
            //                                , (int)outpoint
            //                                , mainWindow.OutPointTextBox.TimeCode
            //                                , (int)frame
            //                                , Calculator.FrameNumberToTimecode((ulong)frame));
            //            }

            //        }
            //    }
            //});
            

            //CommandIn = new RelayCommand(() =>
            //{
            //    mainWindow.InPointTextBox.TimeCode = TimeCode;
            //    InPoint = CurrentFrame;


            //});

            //CommandInGo = new RelayCommand(() =>
            //{
            //    Player.Pause();

            //    //var inpoint = (long)Calculator.TimecodeToFrameNumber(mainWindow.InPointTextBox.TimeCode);
            //    SeekByFrames(InPoint);
            //});

            //CommandOut = new RelayCommand(() =>
            //{
            //    mainWindow.OutPointTextBox.TimeCode = TimeCode;
            //    OutPoint = CurrentFrame;
            //});

            //CommandOutGo = new RelayCommand(() =>
            //{
            //    Player.Pause();

            //    // var inpoint = (long)Calculator.TimecodeToFrameNumber(mainWindow.OutPointTextBox.TimeCode);
            //    SeekByFrames(OutPoint);

            //});

            //CloseCommand = new RelayCommand(Close);
            #endregion
            // fileInfo = new FileInfo();

            TimeTask = Task.Run(() =>
            {
                while (true)
                {
                    if (threadStop)
                    {
                        if (State == EnmState.FF)
                        {
                            var ffValue = (long)(CurrentFrame + 30);

                            if (Player.TotalDuration < ffValue)
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
                            if (0 > rwvalue)
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


        public void Init(IntPtr Handle)
        {
            
            Player?.Init(Handle);
        }

        public void RW()
        {
            State = EnmState.RW;
            
        }

        public void FF()
        {
            State = EnmState.FF;
        }

        public void First()
        {
            Player.First();
            State = EnmState.Pause;
        }

        public void End()
        {
            Player.End();
            State = EnmState.Pause;
        }


        public void Stop()
        {
            Player?.Stop();
            State = EnmState.Stop;
        }
        public void Load(string path)
        {
            Player.Load(path);
            IconPlayStop = "Play";
            MaxDuration = (long)Player.TotalDuration;

            State = EnmState.Cue;
         
            Calculator = new TimecodeCalculator((decimal)Fps);
            Thread.Sleep(600);
        }

        public ulong GetCurrentFrame()
        {
            return (ulong)Player.Frame;
        }

        public ulong GetTotalFrame()
        {
            return (ulong)Player.TotalDuration;
        }

        public void Play()
        {
            if (State == EnmState.Pause || State == EnmState.Cue)
            {
                Player?.Play();
                IconPlayStop = "Stop";
                State = EnmState.Play;
            }
            else
            {
                Player.Pause();
                IconPlayStop = "Play";
                State = EnmState.Pause;
            }

            threadStop = true;
        }

        public void Close()
        {
            Player?.Stop();
            Thread.Sleep(300);
            Player?.Close();
            IconPlayStop = "Play";
            State = EnmState.Stop;
        }

        public void CurrentTime()
        {
            if (Player != null)
            {
                try
                {

                    if (Player.isLoad)
                    {
                        TimeCode = Player.TimeCode(); //ConvertSecondsToTimeCode(CurrentFrame);
                        CurrentFrame = (long)Player.Frame;

                        SliderValue = CurrentFrame;
                    }

                }
                catch (Exception ex) { }
            }
        }


        public void SeekByFrames(long frameCount)
        {
            try
            {
                var sendSeek = (double)Calculator.FrameNumberToSecond(frameCount);
                Player.SetPostion(sendSeek);
                State = EnmState.Pause;
            }
            catch
            {

            }

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

        public void Foraword10Frame()
        {
            Player.Pause();
            Player.Next10Frame();

            State = EnmState.Pause;
        }

        public void Back10Frame()
        {
            Player.Pause();
            Player.Back10Frame();
            State = EnmState.Pause;
        }

        public void Foraword1Frame()
        {
            //Player.Pause();
            Player.Next1Frame();
            State = EnmState.Pause;
        }

        public void Back1Frame()
        {
            //Player.Pause();

            Player.Back1Frame();

            State = EnmState.Pause;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
