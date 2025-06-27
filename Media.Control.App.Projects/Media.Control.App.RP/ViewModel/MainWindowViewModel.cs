
using System.IO;
using System.Windows.Media;
using System.Text.Json;
using System.Data;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Newtonsoft.Json.Linq;
using Ampp.Control.lib;
using Ampp.Control.lib.Model;
using Newtonsoft.Json;
using Mpv.Player.App;
using DeckLinkDirectShowLib;
using Media.Control.App.RP.Model;
using Media.Control.App.RP.Command;
using Media.Control.App.RP.Controls;
using Media.Control.App.RP.Model.Config;
using Media.Control.App.RP.Model.Engine;
using Media.Control.App.RP.Model.Logger;
using System.Windows;
using System.Windows.Shapes;
using System.Net.ServerSentEvents;
using MpvNet.ExtensionMethod;
using MpvNet;
using System;
using static System.Windows.Forms.AxHost;
using System.Globalization;
using System.Windows.Media.Animation;
using NewTek.NDI;
using ControlzEx.Standard;
using System.Threading.Tasks;
using System.Drawing;
using System.IO.Pipes;
using System.Text;
using Vdcp.Control.Client;
using static Vdcp.Control.Client.VdcpUdpAdapter;

namespace Media.Control.App.RP.ViewModel
{
    public enum AppMode {Player, Recoder }
    public enum EnmMediaState { Play, FF, Rew, Pause, Eject, Frsit, End, Cue, Prepared ,Done ,Error  }
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private MainWindow _mainWindow = null;
        private EngineControl engineControl1 = null;
        private EngineControl engineControl2 = null;
        private EngineControl engineCleanCut = null;
        private Logger logger = null;
        private MediaApi mediaApi = null;
        private Model.Engine.Ping ping = null;
        private bool isThread = true;
        private bool isIncreasing = true;
        private AppMode  appMode = AppMode.Player;
        private MediaListControlViewModel mediaListControlViewModel = null;
        private OnairMediaList onariMediaInfo = null;
        private ProxyFileTransfer proxyFileTransfer = null;

        //= EnmMediaState.Eject;
        public EnmChannel ChannelName { get; set; }

        private long PlayItemDuration = 0;
        private bool isClipLoad { get; set; } = false;
        private string RecoderCreateName { get;set; }
        private string RecoderId { get; set; }

        private string DefaultPath { get; set; }

        private System.Windows.Controls.Image imageHandle;
        private TimecodeCalculator Calculator = null;
        private DeckLinkDirectShowLib.DeckLinkDirectShow deckLinkCapture;

        private DNIReceivelib ndiReceivelib = null;
        private double FPS { get; set; } = 30;


        private System.Windows.Controls.Image GetImageHandle()
        {
            return imageHandle;
        }

        private void SetImageHandle(System.Windows.Controls.Image value)
        {
            imageHandle = value;
        }

        private string reconKey = "AmppControlSdk";
        private int WindowsCount = 4;


        private string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


        public ObservableCollection<string> InputItemsLIst { get; set; }

        public string SelectInput { get; set; }

        public string Stateus { get; set; } = string.Empty;

        public ICommand Command_Min { get; }

        public ICommand Command_Max { get; }

        public ICommand Command_Close { get; }

        #region Property

        private EnmControlType _currentStatus;
        private EnmMediaState _mediaState;

        public EnmMediaState MediaState
        {
            get => _mediaState;
            set
            {
                _mediaState = value;

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    if (_mainWindow.playerStateControl != null)
                        _mainWindow.playerStateControl.PlayerState = value.ToString();
                });
            }
        }

        public EnmControlType CurrentControlType
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                OnPropertyChanged(nameof(CurrentControlType));
            }
        }

        private string _ControlName { get; set; }
        public string ControlName 
        {   
            get => _ControlName; 
            set { _ControlName = value; OnPropertyChanged(nameof(ControlName)); } 
        }

        private double _TitleMaxWidth { get; set; }

        public double TitleMaxWidth
        {
            get => _TitleMaxWidth;
            set { _TitleMaxWidth = value; OnPropertyChanged(nameof(TitleMaxWidth)); }
        }


        private bool _AppState { get; set; } = true;

        public bool AppState
        {
            get => _AppState;
            set { _AppState = value; OnPropertyChanged(nameof(AppState)); }
        }

        private string _ApplicationName { get; set; } = "Ampp Control App";

        public string ApplicationName
        {
            get => _ApplicationName;
            set { _ApplicationName = value; OnPropertyChanged(nameof(ApplicationName)); }
        }

        private double _InPoint { get; set; }    
        public double InPoint
        {
            get => _InPoint;
            set { _InPoint = value; OnPropertyChanged(nameof(InPoint)); }
        }

        private double _OutPoint { get; set; }

        public double OutPoint
        {
            get => _OutPoint;
            set { _OutPoint = value; OnPropertyChanged(nameof(OutPoint)); }
        }


        private string _InTimeCode { get; set; }
        public string InTimeCode
        {
            get => _InTimeCode;
            set { _InTimeCode = value; OnPropertyChanged(nameof(InTimeCode)); }
        }

        private string _OutTimeCode { get; set; }

        public string OutTimeCode
        {
            get => _OutTimeCode;
            set { _OutTimeCode = value; OnPropertyChanged(nameof(OutTimeCode)); }
        }



        private int _Duration { get; set; }

        public int Duration
        {
            get => _Duration;
            set { _Duration = value; OnPropertyChanged(nameof(Duration));}
        }


        private int _TotalDuration { get; set; }

        public int TotalDuration
        {
            get => _TotalDuration;
            set { _TotalDuration = value; OnPropertyChanged(nameof(TotalDuration)); }
        }


        private double _AudioLeftValue { get; set; }   

        public double AudioLeftValue
        {
            get => _AudioLeftValue;
            set
            {
                _AudioLeftValue = value;
                OnPropertyChanged(nameof(AudioLeftValue));
            }
        }

        private double _AudioReightValue { get;set; }
        public double AudioReightValue
        {
            get => _AudioReightValue;
            set 
            {   
                _AudioReightValue = value;
                OnPropertyChanged(nameof(AudioReightValue));
            }
        }


        private string _SelectPlayList { get; set; }

        public string SelectPlayList
        {
            get => _SelectPlayList;
            set 
            {
                _SelectPlayList = value; 
                OnPropertyChanged(nameof(SelectPlayList));

                DisPlayPlayLists();

            }
        }

        #endregion

        private string iconPlayStop { get; set; } = "Stop";

        public string IconPlayStop
        { 
            get => iconPlayStop; 
            set 
            {
                iconPlayStop = value;
                if (_mainWindow != null)
                    _mainWindow.playerButton.IconPlayStop = value;
                OnPropertyChanged(nameof(IconPlayStop)); 
            } 
        }


        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            ping = new Model.Engine.Ping();

            Command_Close = new RelayCommand(CommandClose);
            Command_Min = new RelayCommand(CommandMinus);
            Command_Max = new RelayCommand(CommandMax);
            TitleMaxWidth = _mainWindow.Width - 190;

            InputItemsLIst = new ObservableCollection<string> { };

            string listPath = $@"{appPath}\PlayList";
            if (!Directory.Exists(listPath)) Directory.CreateDirectory(listPath);

       

            // decklink
            deckLinkCapture = new DeckLinkDirectShowLib.DeckLinkDirectShow();
            deckLinkCapture.VideoFrameReceived += DeckLinkCapture_VideoFrameReceived;
            deckLinkCapture.AudioSampleReceived += DeckLinkCapture_AudioSampleReceived;


            ndiReceivelib = new DNIReceivelib();
            ndiReceivelib.Initialize();
            ndiReceivelib.VideoFrameReceived += NDIReceivelib_VideoFrameReceived;
            ndiReceivelib.AudioLevelsReceived += NDIReceivelib_AudioLevelsReceived;
            
            FrameSmoother.FrameUpdated += FrameSmoother_FrameUpdated;
            mediaListControlViewModel = _mainWindow.MediaListControl.mediaListViewModel;


            onariMediaInfo = new OnairMediaList();
        }

        private void NDIReceivelib_AudioLevelsReceived((double leftLevel, double rightLevel) tuple)
        {
            UpdateMultiColorBar(tuple.leftLevel, tuple.rightLevel);
        }

        private void NDIReceivelib_VideoFrameReceived(WriteableBitmap bitmap)
        {
            
        }

        private void MediaApi_MediaDataEvent(object? sender, MediaDataInfo2 e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentControlType == EnmControlType.Recoder)
                {
                    if(e.Name == RecoderCreateName)
                    {
                        int index = mediaListControlViewModel.MeidaListItems.Count;
                        var mediaDataInfo = new MediaDataInfo(index + 1,
                                                            e.MediaId,
                                                            e.Image,
                                                            e.Name,
                                                            e.Duration,
                                                            e.Frame,
                                                            e.CreatDate,
                                                            e.InPoint,
                                                            e.InTimeCode,
                                                            e.OutPoint,
                                                            e.OutTimeCode,
                                                            e.Creator,
                                                            e.Type,
                                                            e.Proxy,
                                                            e.Path,
                                                            e.Fps,
                                                            e.Des,
                                                            e.State);

                        mediaListControlViewModel.MeidaListItems.Insert(0, mediaDataInfo);
                        mediaListControlViewModel.SetMedisState(e.MediaId, e.State);
                    }
                }
            });
        }

        private void MediaApi_UpdateMediaDataEvent(object? sender, UpdateMediaData e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {

                if(CurrentControlType == EnmControlType.Recoder)
                {
                    var item = mediaListControlViewModel.MeidaListItems.Where(c => c.MediaId == e.Id).FirstOrDefault();
                    if (item != null)
                    {
                        item.State = e.State;
                        mediaListControlViewModel.SetMedisState(item.MediaId, e.State);

                        if(e.State == "Recoding")
                        {
                            string ProxyPath = @$"{SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder}"
                                                + @$"\{RecoderCreateName}.mp4";


                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {ProxyPath} Transfer Proxy File Start...");

                            proxyFileTransfer = new ProxyFileTransfer();
                            proxyFileTransfer.TargetPath = item.Proxy;
                            proxyFileTransfer.SourcePath = ProxyPath;
                            
                            Task.Run(async () =>
                            {
                                Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] 비동기 작업 시작");
                                await Task.Delay(3000); // 3초 비동기 대기

                                proxyFileTransfer.StartWatchingAsync();

                                Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] 비동기 작업 종료");
                            });

                            
                            
                        }
                        else if(e.State =="Done")
                        {
                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {item.MediaId} Recoding  State = {e.State}");
                        }
                    }
                }

            });
        }

        private void DeckLinkCapture_AudioSampleReceived(object? sender, AudioSampleEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                var levels = StereoAudioLevel.CalculateStereoAudioLevel(e.AudioData);
                AudioReightValue =  levels.right;
                AudioLeftValue = levels.left;

            });
        }

        public void SaveImageSourceToPng(ImageSource imageSource, string filePath)
        {
            // ImageSource를 BitmapSource로 캐스팅합니다.
            BitmapSource bitmapSource = imageSource as BitmapSource;
            if (bitmapSource == null)
            {
                throw new InvalidOperationException("ImageSource가 BitmapSource로 변환되지 않습니다.");
            }

            // PNG 인코더를 생성하고 BitmapFrame을 추가합니다.
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            // 파일 스트림을 열어 PNG 데이터를 저장합니다.
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        public static void SaveBitmapToPng(Bitmap bitmap, string filePath)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            // Bitmap 객체를 PNG 형식으로 저장합니다.
            bitmap.Save(filePath, ImageFormat.Png);
        }

        private void DeckLinkCapture_VideoFrameReceived(object? sender, VideoFrameEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                //SaveBitmapToPng(e.VideoFrame , @"d:\bitmap.png" );
                System.Windows.Media.ImageSource imgSrc = ImageConversion.BitmapToImageSource(e.VideoFrame);
                _mainWindow.PreviewImage.Source = imgSrc;

                //SaveImageSourceToPng(imgSrc, @"D:\Temp.png");
            });
        }

        private void CommandMinus()
        {

        }

        private void CommandMax()
        {

        }

        public void CommandClose()
        {
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Close Button Clicked");


            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "ManagerBa", PipeDirection.Out))
            {
                try
                {
                    pipeClient.Connect(100);

                    using (StreamWriter writer = new StreamWriter(pipeClient, Encoding.UTF8))
                    {
                        writer.AutoFlush = true;
                        writer.Write($"Close/{ChannelName}");
                    }
                }
                catch
                {

                }
            }

            ndiReceivelib?.Stop();
            deckLinkCapture?.Stop();

            engineControl1?.Stop(SystemConfigDataStatic.ChannelConfigData.EnginType);

            if(engineControl1 != null)
            {
                engineControl1.OnAmppControlNotifyEvent -= EngineControl_OnAmppControlNotifyEvent;
                engineControl1.OnAmppControlErrorEvent -= EngineControl_OnAmppControlErrorEvent;
                engineControl1.OnStateEvent -= EngineControl_OnStateEvent;
            }

            engineControl2?.Stop(SystemConfigDataStatic.ChannelConfigData.EnginType);
            if (engineControl2 != null)
            {
                engineControl2.OnAmppControlNotifyEvent -= EngineControl_OnAmppControlNotifyEvent;
                engineControl2.OnAmppControlErrorEvent -= EngineControl_OnAmppControlErrorEvent;
                engineControl2.OnStateEvent -= EngineControl_OnStateEvent;
            }

            _mainWindow?.Close();

            if(proxyFileTransfer != null)
                proxyFileTransfer?.DisposeAsync();
        }

        private void EngineControl_OnAmppControlErrorEvent1(object? sender, AmppControlErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        // hot key 처리시 이함수 이용
        public void RecorderButtonClicked(string name)
        {
            isThread = false;
            switch (name)
            {
                case "ButRecord":
                    Recorder();
                    FrameSmoother.ResetFrame(0);
                    break;
                case "ButStop":
                    FrameSmoother.Stop();
                    RecStop();
                
                    //FrameSmoother.ResetFrame(0);
                    break;
                case "ButPrepared":
                    Prepared();
                    //RecEject();
                    break;
                case "butOptin":
                   // _mainWindow.RecorderButton.Height = 95;
                    break;
            }

            isThread = true;
        }

        public void PlayerButtonClicked(string name)
        {
            isThread = false;

            try
            {
                isIncreasing = true;

                switch (name)
                {
                    case "ButFrsit":
                        Stop("$now");
                        First();
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButReWind":
                        Rewind();
                        isIncreasing = false;
                        MediaState = EnmMediaState.Rew;
                        break;
                    case "ButBack10Frame":
                        Seek("Back10Frame", 0);
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButBack1Frame":
                        StepBack();
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButStop":
                        Stop("$now");
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButPlay":

                        if (MediaState == EnmMediaState.FF
                            || MediaState == EnmMediaState.Rew
                            || MediaState == EnmMediaState.Play)
                        {
                            Stop("$now");
                            MediaState = EnmMediaState.Pause;
                            FrameSmoother.Stop();
                        }
                        else
                        {
                            Start(engineControl1, "$now");
                            MediaState = EnmMediaState.Play;
                            FrameSmoother.Start();
                        }
                        
                        break;
                    case "ButForward1Frame":
                        StepForward();
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButForward10Frame":
                        Seek("Forward10Frame", 0);
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButFoward":
                        Fastforward();
                        MediaState = EnmMediaState.FF;
                        break;
                    case "ButEnd":
                        Stop("$now");
                        End();
                        MediaState = EnmMediaState.Pause;
                        break;
                    case "ButEject":
                        Eject(engineControl1);
                        _mainWindow.playerStateControl.InitSetting();
                        TotalDuration = 0;

                        MediaState = EnmMediaState.Eject;
                        FrameSmoother.ResetFrame(0);
                        StartPlayList = false;

                        break;
                    case "butGotoOut":
                        GotoOut();
                        break;
                    case "butGotoIn":
                        GotoIn();
                        break;
                    case "butInPoint":
                        InPoint = _mainWindow.playerStateControl.SliderValue;
                        MarkIn();
                        _mainWindow.playerStateControl.AddMaker(MarkerShape.RightTriangle, InPoint, duration:0, TriangleDirection.Left);
                        break;
                    case "butOutPoint":
                        OutPoint = _mainWindow.playerStateControl.SliderValue;
                         MarkOut();
                        _mainWindow.playerStateControl.AddMaker(MarkerShape.RightTriangle, OutPoint, duration: 0, TriangleDirection.Right);
                        break;
                    case "butInDelete":
                        _mainWindow.playerStateControl.SliderValue = 0;
                        DeleteIn();
                        _mainWindow.playerStateControl.DeleteMarker(InPoint);
                        break;
                    case "butOutDelete":
                        _mainWindow.playerStateControl.SliderValue = TotalDuration;
                        DeleteOut();
                        _mainWindow.playerStateControl.DeleteMarker(OutPoint-1);
                        break;
                    case "butReCue":

                        mediaListControlViewModel.SetMediInitState();
                        var media = mediaListControlViewModel.GetFirstItem();
                        MediaItemCue(media);

                        _mainWindow.playerGroupControl.SetButtonEnabled("Cue");
                        MediaState = EnmMediaState.Cue;

                        _mainWindow.playerButton.IsAtion(false);
                        _mainWindow.playerStateControl.IsAtion(false);
                        break;
                    case "butNext":
                        //if(!StartPlayList)
                        
                        var nextItem = mediaListControlViewModel.MediaSelectItem;

                        if (nextItem == null)
                            nextItem = mediaListControlViewModel.GetFirstItem();

                        if (nextItem.State != "Wait" && nextItem.State != "Done")
                        {
                            System.Windows.MessageBox.Show($"상태가 {nextItem.State}일 경우 Cue 할수 없는다.");
                            return;
                        }
                        else
                        {
                            mediaListControlViewModel.SetMediInitState();
                            for (int i = nextItem.Index - 1; i >= 1; i--)
                            {
                                var item = mediaListControlViewModel.MeidaListItems[i - 1];
                                item.State = "Done";
                            }

                            MediaItemCue(nextItem);
                        }

                        _mainWindow.playerGroupControl.SetButtonEnabled("Cue");

                        _mainWindow.playerButton.IsAtion(false);
                        _mainWindow.playerStateControl.IsAtion(false);

                        MediaState = EnmMediaState.Cue;

                        break;
                    case "butListPlay":
                        if (StartPlayList)
                        {
                            if(MediaState == EnmMediaState.Cue)
                            {
                                var playitem = mediaListControlViewModel.GetCueItem();
                             
                                if(playitem != null)
                                {
                                    // 플레이이를 위한 Cue -> Play 정보와 교체
                                    onariMediaInfo.ChangePlayMediaInfo();

                                    Start(onariMediaInfo.PlayMedia.Control, "$now");
                                    MediaState = EnmMediaState.Play;


                                    if (playitem != null) playitem.State = "Play";
                                    _mainWindow.playerGroupControl.ListPlayContent = "Play";
                                    _mainWindow.playerGroupControl.SetButtonEnabled("Play");
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("미디어를 먼저 Load 해주세요.");
                                    return;
                                }
                            }

                            isLastMediaPlay = false;
                        }

                        break;
                    case "butClean":

                        string playlist = _mainWindow.playerGroupControl.SelectPlayList;

                        if (playlist != "")
                        {
                            string filePath = $@"{appPath}\PlayList\{playlist}.json";
                            if (File.Exists(filePath))
                            {
                                File.WriteAllText(filePath, string.Empty);
                            }
                        }

                        StartPlayList = false;

                        _mainWindow.playerGroupControl.TotalTimeCode = "00:00:00;00";
                        mediaListControlViewModel.CleanMedia();
                        _mainWindow.playerGroupControl.SetButtonEnabled(false);

                        break;
                    case "butDelete":

                        if(StartPlayList && System.Windows.MessageBox.Show("송출중 입니다.\r정말로 중지 하시겠습니까?", "Stop"
                            , MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return;
                        
                        MediaItemDone();
                        mediaListControlViewModel.SetMediInitState();
                        _mainWindow.playerGroupControl.SetButtonEnabled("Stop");
                        MediaState = EnmMediaState.Pause;
                        StartPlayList = false;
                        isLastMediaPlay = false;
                        #region 
                        //string playlist1 = _mainWindow.playerGroupControl.SelectPlayList;

                        //if (playlist1 != "")
                        //{
                        //    string filePath = $@"{appPath}\PlayList\{playlist1}.json";
                        //    if (File.Exists(filePath))
                        //    {
                        //        var mediaLists = LoadMediaList(filePath);
                        //        var item = mediaListControlViewModel.MediaSelectItem;
                        //        var deleteItem = mediaLists.Where(c => c.Index == item.Index).FirstOrDefault();
                        //        if (deleteItem != null) mediaLists.Remove(deleteItem);
                        //    }
                        //}

                        //mediaListControlViewModel.RemoveMediaItem();

                        //if (playlist1 != "")
                        //{
                        //    int count = 1;
                        //    foreach (var dataInfo in mediaListControlViewModel.MeidaListItems.ToList())
                        //    {
                        //        dataInfo.Index = count;
                        //        ++count;
                        //    }

                        //    SavePlayLists();
                        //}
                        #endregion
                        break;
                    case "PlayListSave":

                        //  SavePlayLists();

                        break;
                }
                
                _mainWindow.playerStateControl.InitJogShuttle();

               // GetState();
            }
            catch (Exception ex)
            {
                isThread = true;
            }

          
       
            if (MediaState == EnmMediaState.Eject)
                isThread = false;
            else isThread = true;

        }
        public async Task MediaItemDone()
        {
            FrameSmoother.Stop();
            onariMediaInfo.Init();

            Eject(engineControl1);
            Eject(engineControl2);

            ChangeCleancut(1);
        }

        public void MediaItemCue(MediaDataInfo media)
        {
            onariMediaInfo.Init();
         
            Eject(engineControl1);
            Eject(engineControl2);

            Cue(media, engineControl1);   // 자동으로 CuemediaData 정보 세팅 됨

            FrameSmoother.Initialize(0, (int)onariMediaInfo.CueMedia.Fps);
            FrameSmoother.Stop();
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] FrameSmoother Initialize");

            _mainWindow.playerStateControl.InTimeCode.TimeCode = onariMediaInfo.CueMedia.MediaData.InTimeCode;
            _mainWindow.playerStateControl.OutTimeCode.TimeCode = onariMediaInfo.CueMedia.MediaData.OutTimeCode; 

            _mainWindow.playerStateControl.RemainTimecode
                    = Calculator.FrameNumberToTimecode((ulong)(onariMediaInfo.CueMedia.MediaData.OutPoint - 1));

            if (TotalDuration == 0)
            {
                _mainWindow.playerStateControl.MaxDuration = onariMediaInfo.CueMedia.MediaData.OutPoint - 1;
                TotalDuration = (int)onariMediaInfo.CueMedia.MediaData.OutPoint;

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] TotalDuration : {TotalDuration}");
            }

        }

        public bool CheckMedia(MediaDataInfo media)
        {
            bool result = true;
            if(!File.Exists(media.Path))
            {
                //mediaListControlViewModel.SetMediaState(media.Index, "Error");
                media.State = "Error";

                result = false;
            }

            return result; 
        }
             

        public void SavePlayLists()
        {
            var medialist = mediaListControlViewModel.MeidaListItems;

            foreach(var item in medialist)
            {
                if(item.State != "Error" && item.State != "Skip")
                    item.State = "Wait";
            }

            if (!string.IsNullOrEmpty(SelectPlayList))
            {
                string path = $@"{appPath}\PlayList\{SelectPlayList}.json";
                System.IO.File.Delete(path);

                string jsonString = JsonConvert.SerializeObject(medialist, Formatting.Indented);

                // JSON 문자열을 파일에 저장
                File.WriteAllText(path, jsonString);
            }

          
            _mainWindow.playerGroupControl.TotalFrame = 0;
            
            foreach (var item in medialist)
                SetTotalTimecode(item);
        }

        private void DisPlayPlayLists()
        {
            string path = $@"{appPath}\PlayList\{SelectPlayList}.json";
            mediaListControlViewModel.MeidaListItems =  LoadMediaList(path);
        }

        public ObservableCollection<MediaDataInfo> LoadMediaList(string filePath)
        {
            if (!File.Exists(filePath))
                return new ObservableCollection<MediaDataInfo>();

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new ObservableCollection<MediaDataInfo>();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // 대소문자 무시
                AllowTrailingCommas = true,          // 후행 콤마 허용
            };

            try
            {
                List<MediaDataInfo> list = System.Text.Json.JsonSerializer.Deserialize<List<MediaDataInfo>>(json, options);

                _mainWindow.playerGroupControl.TotalFrame = 0;
                foreach (var item in list)
                    SetTotalTimecode(item);

                if(list.Count != 0)
                {
                    _mainWindow.playerGroupControl.SetDoneButtonEnabled();
                }


                return list != null ? new ObservableCollection<MediaDataInfo>(list) : new ObservableCollection<MediaDataInfo>();
            }
            catch (Exception ex)
            {
                // 예외 발생 시 로그 출력 후 빈 컬렉션 반환
                Console.WriteLine("System.Text.Json 역직렬화 오류: " + ex.Message);
                return new ObservableCollection<MediaDataInfo>();
            }
        }

        public void SetTotalTimecode(MediaDataInfo media)
        {
            if(media == null) return;

            if(media.State != "Error")
            {
                _mainWindow.playerGroupControl.TotalFrame += media.Frame;

                Calculator = new TimecodeCalculator(media.Fps);

                _mainWindow.playerGroupControl.TotalTimeCode
                = Calculator.FrameNumberToTimecode((ulong)_mainWindow.playerGroupControl.TotalFrame);
            }
        }
        private double GetFps(string path)
        {

            if (File.Exists(path) == false)
            {
                MpvNet.MediaInfo mediaInfo = new MpvNet.MediaInfo(path);
                var framerate = (dynamic)Convert.ToDouble(mediaInfo.GetInfo(MediaInfoStreamKind.Video, "FrameRate"));
                mediaInfo.Dispose();
                return (double)Math.Round((decimal)framerate);
            }
            else
            {
                return 30;
            }
        }

        #region 0. Vdcp Timecode / State control =============>

        public string GetTimecode(byte Postiontype)
        {
            var output = new JObject();

            var obj = new 
            {
                Postiontype = Postiontype
            };

            engineControl1.Vdcp(EumCommandKey.POSTIONREQUEST, out output, JObject.FromObject(obj));

            if (output != null && output.ContainsKey("Postion"))
            {
                return output["Postion"].Value<string>();
            }
            else return "00:00:00:00";
        }


        public List<string> GetState()
        {
            var output = new JObject();

            engineControl1.Vdcp(EumCommandKey.PORTSTATUS, out output);

            if (output != null && output.ContainsKey("State"))
            {
                return output["State"].Value<List<string>>();
            }
            else return null;
        }

        #endregion

        #region 1. Recoder control =============>

        private void RecEject()
        {
            _mainWindow.RecorderStateControl.Timecode = "00:00:00;00";
        }

        private async void RecStop()
        {
            bool StopResult = false;
            string StopTime = "2022-05-13T20:20:39";

            if (_mainWindow.RecorderButton.isStopTime)
            {
                DateTime parsedTime = DateTime.Parse(_mainWindow.RecorderButton.StopTimeAt
                                .SelectedDateTime?.ToString());

                StopTime = parsedTime.ToString("o");

                var result = engineControl1.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                    EnmRecoderControl.Stopat, JObject.FromObject(new
                                    {
                                        stop = StopTime
                                    }));

                StopResult = result.Result;
            }
            else
            {
                var result = engineControl1.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                            EnmRecoderControl.Stop, new JObject());
                StopResult = result.Result;
            }

            _mainWindow.RecorderButton.SetButtonEnable(Stateus);
            _mainWindow.RecorderStateControl.FileName = "No File";

        }


        private async void Prepared()
        {
            RecoderCreateName = $"{_mainWindow.RecorderSettingControl.txtTitle.Text}" +
                                $"_{SystemConfigDataStatic.ChannelConfigData.ChannelList.Name}" +
                                $"_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            var RecorderPrepare = new
            {
                Source = SelectInput,
                filename = RecoderCreateName
            };

            var result = engineControl1.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                    EnmRecoderControl.Prepare,
                    JObject.FromObject(RecorderPrepare));

            if (result.Result)
            {
                SaveMediaData();
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} Prepare Success");
            }

        }

        public async void ChangeInput(string input)
        {
            var result = engineControl1.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                               EnmRecoderControl.Recordingstate, JObject.FromObject(new
                               {
                                   Source = input,
                                   RecordingStatus = "Ready"
                               }));

            if (result.Result)
            {
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] ChangeInput : {input} Success ");
            }
            else
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] ChangeInput : {input} Fail ");
            }
                
        }

        private async void Recorder()
        {
            if (Stateus == "Prepared")
            {
                string startTime = "2022-01-10T20:00:00.000Z";
                string Duration = "00:00:00:00";

                if (_mainWindow.RecorderButton.isDuration)
                    Duration = _mainWindow.RecorderButton.DurationTimeCode.TimeCode;

                if (_mainWindow.RecorderButton.isStartTime)
                { 
                    DateTime parsedTime = DateTime.Parse(_mainWindow.RecorderButton.StartTimeAt
                                      .SelectedDateTime?.ToString());

                    startTime = parsedTime.ToString("o");

                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Setting StartTime  : {parsedTime} ");
                }


                string ImagePath = @$"{SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder}\Thumbnail\{RecoderCreateName}.png";

                if (_mainWindow.PreviewImage.Source is BitmapSource bitmapSource)
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    try
                    {
                        // 파일 스트림 생성
                        using (FileStream stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            // PNG로 저장
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                            encoder.Save(stream);
                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Save Thumbnail  Path : {ImagePath} ");
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    //System.Windows.MessageBox.Show($"InPut : {RecorderPrepare} No Signal");
                    //SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] No Signal", $"InPut : {RecorderPrepare} No Signal");
                    System.Windows.MessageBox.Show($"{RecoderCreateName} Error Record ");
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Error Record");
                    return;
                }

                //Thread.Sleep(300);

                var rescoder = engineControl1.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                        EnmRecoderControl.Startat,
                        JObject.FromObject(new
                        {
                            start = startTime,
                            duration = Duration
                        }));
            }
            else
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Prepared Fail");
            }
        }

        private async void UpDateMediaDate(string id, string state)
        {
            var api = await mediaApi.UpDateMedia(id, state);

            if (api)
            {
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} {id} Update State = {state} Success");
            }
            else
            {
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} {id} Update State = {state} Fail");
            }

        }


        private void SetRecoderTimcode(int Duration)
        {
            string timecode = Calculator.FrameNumberToTimecode((ulong)Duration -1);
            mediaListControlViewModel.SetDuration(RecoderId, timecode, Duration - 1);

            string outtimecode = Calculator.FrameNumberToTimecode((ulong)Duration);
            mediaListControlViewModel.SetOutTimeCode(RecoderId, outtimecode, Duration);

        }    
        

        private async void SaveMediaData()
        {
            Guid newGuid = Guid.NewGuid();

            string ffpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\ffmpeg\\ffmpeg.exe";
            string ImagePath = @$"{SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder}\Thumbnail\{RecoderCreateName}.png";
            string ProxyPath = @$"{SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder}\Proxy\{RecoderCreateName}.mp4";
            string MediaPath = @$"{SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder}\{RecoderCreateName}.mxf";

            MediaDataInfo2 mediaData = new MediaDataInfo2();

            mediaData.MediaId = newGuid.ToString();
            mediaData.CreatDate = DateTime.Now;
            
            mediaData.Creator = SystemConfigDataStatic.ChannelConfigData.ChannelList.Name;
            mediaData.Frame = (int)Duration;
            mediaData.Duration = _mainWindow.RecorderStateControl.Timecode;
            mediaData.Image = ImagePath;
            mediaData.InPoint = 0;
            mediaData.InTimeCode = "00:00:00;00";
            mediaData.OutPoint = (int)(Duration - 1);
            mediaData.OutTimeCode = "00:00:00;00";
            mediaData.Name = RecoderCreateName;
            mediaData.Path = MediaPath;
            mediaData.Proxy = ProxyPath;
            mediaData.Type = "HD";
            mediaData.Des = "";
            mediaData.Fps = (int)FPS;
            mediaData.State = EnmMediaState.Prepared.ToString();

            RecoderId = mediaData.MediaId;
            var api = await mediaApi.MediaSave(mediaData);

            if (api)
            {
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} MediaDataApi Save Success");
            }
            else
            {
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} MediaDataApi Save Fail");
            }
        }

        #endregion


        #region 2. Player control ============>

        public void SetMediaInfo(MediaDataInfo media)
        {
            
            // out = duratino +1 
            _mainWindow.playerStateControl.InTimeCode.TimeCode = media.InTimeCode;
            _mainWindow.playerStateControl.OutTimeCode.TimeCode = media.OutTimeCode;
            _mainWindow.playerStateControl.RemainTimecode = media.Duration;

            if (TotalDuration == 0)
            {
                _mainWindow.playerStateControl.MaxDuration = media.OutPoint - 1;
                TotalDuration = (int)media.OutPoint;
            }
        }

     

        private void PlayPause(EngineControl control)
        {

            if(control != null)
            {
                var result = control.Player(EnmEnaginType.Ampp, EnmPlayerControl.Playpause);

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Playpause Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Playpause Fail");
            }

        }

        private void Start(EngineControl control, string startTime)
        {
            var start = new { Start = $"{startTime}" };
            var result = control.Player(EnmEnaginType.Ampp, EnmPlayerControl.Startat, JObject.FromObject(start));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Play Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Play Fail");

        }

        private void Stop(string stopTime)
        {
            var start = new { Stop = $"{stopTime}" };
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Stopat, JObject.FromObject(start));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Play Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Play Fail");

        }

        private void Eject(EngineControl control)
        {

            var eject = new {
                isCleared = true
            };

            if(control  != null)
            {
                var result = control.Player(EnmEnaginType.Ampp, EnmPlayerControl.Clearassets, JObject.FromObject(eject));

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Eject Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Eject Fail");

            }
        }

        private void First()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Gotostart);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player First Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player First Fail");

        }

        private void End()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Gotoend);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player End Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player End Fail");
        }


        private void Fastforward()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Fastforward);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Fastforward Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Fastforward Fail");
        }

        private void Rewind()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Rewind);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Rewind Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Rewind Fail");
        }


        private void StepBack()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Stepback);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Back 1Frame Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Back 1Frame Fail");
        }

        private void StepForward()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Stepforward);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Forwar 1Frame Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Forwar 1Frame Fail");
        }

        public void Rate(double value)
        {
            var obj = new { Rate = value };

            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Rate, JObject.FromObject(obj));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Shuttle {value} Rate Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Shuttle {value} Rate Fail");

            isThread = true;
        }

        public void Shuttle(double value)
        {
            var obj = new { Rate = value };

            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Shuttle, JObject.FromObject(obj));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Shuttle {value} Rate Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Shuttle {value} Rate Fail");

            isThread = true;
        }

        public void Seek(string type, int seek =0)
        {
            int value = 0;

            switch (type)
            {
                case "Seek":
                    value = seek;
                    break;
                case "Back10Frame":
                    value = (int)Duration - 10;
                    break;
                case "Forward10Frame":
                    value = (int)Duration + 10;
                    break;
            }


            var obj = new { frame = value };

            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Seek, JObject.FromObject(obj));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Seek {value} Frame Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Seek {value} Frame Fail");

        }


        private void MarkIn()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Markin);
            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player MarkIn Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player MarkIn Fail");
        }

        private void MarkOut()
        {
            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Markout);

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player MarkOut Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player MarkOut Fail");
        }


        private void Mark(int Index = 1, int inPoint = 0, int outPoint = 0)
        {
            var transport = new
            {
                start = "$now",
                position = 0,
                inPosition = inPoint,
                outPosition = outPoint,
                rate = 1.0,
                endBehaviour = "recue"
            };

            

            //var result = control.Player(EnuEnaginType.Ampp, EnmPlayerControl.Transportcommand, JObject.FromObject(transport));

            //if (result.Result)
            //    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Mark In {InPoint} Out {OutPoint} Success");
            //else
            //    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Mark In {InPoint} Out {OutPoint} Fail");
        }

        private void GotoIn()
        {
            if (MediaState != EnmMediaState.Eject)
            {
                var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Gotostart);

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Goto Markin Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Goto Markin Fail");
            }
        }

        private void GotoOut()
        {
            if (MediaState != EnmMediaState.Eject)
            {
                var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Gotoend);

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Goto Markout Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Goto Markout Fail");
            }
        }

        private void DeleteIn()
        {
            if (MediaState != EnmMediaState.Eject)
            {
                var transport = new
                {
                    start = "$now",
                    position = 0,
                    inPosition = 0,
                    outPosition = OutPoint,
                    rate = 0,
                    endBehaviour = "repeat"
                };

                var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Transportcommand, JObject.FromObject(transport));

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Delete MarkIn Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Delete MarkIn Fail");
            }
        }

        private void DeleteOut()
        {
            if (MediaState != EnmMediaState.Eject)
            {
                var transport = new
                {
                    start = "$now",
                    position = TotalDuration,
                    inPosition = InPoint,
                    outPosition = TotalDuration,
                    rate = 0,
                    endBehaviour = "repeat"
                };

                var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Transportcommand, JObject.FromObject(transport));

                if (result.Result)
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Delete MarkOut Success");
                else
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Delete MarkOut Fail");
            }
        }

        public void Cue(string path)
        {
            TotalDuration = 0;
            _mainWindow.playerStateControl.InitSetting();

            //var fps = GetFps(path);
            //FPS = (long)Math.Round((decimal)fps);

            onariMediaInfo.SetCueMediaInfo(engineControl1, path, null, 1, FPS);


            var PlayPrepare = new
            {
                file = path
            };

            var result = engineControl1.Player(EnmEnaginType.Ampp, EnmPlayerControl.Clip, JObject.FromObject(PlayPrepare));

            if (result.Result)
            {
                isThread = true;
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Cue Success");

            }
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Cue Fail");


            if (!StartPlayList)
                _mainWindow.playerStateControl.InitSetting();
        }

        public void Cue(MediaDataInfo media, EngineControl control = null)
        {
            if(media == null) return;

            TotalDuration = 0;

            //var fps = GetFps(media.Path);
            //FPS = (long)Math.Round((decimal)fps);

            if (control == null)
            {
                _mainWindow.playerStateControl.InitSetting();
                control = engineControl1;
            }
            else
            {
               // onariMediaInfo.SetCueMediaInfo(control, media.Path, media, 1, FPS);
                StartPlayList = true;
            }

            // Cue 정보 설정
            int cleancut = control == engineControl1 ? 1 : 2;
            onariMediaInfo.SetCueMediaInfo(control, media.Path, media, cleancut, FPS);

            var PlayPrepare = new
            {
                file = media.Path
            };

            var result = control.Player(EnmEnaginType.Ampp, EnmPlayerControl.Clip, JObject.FromObject(PlayPrepare));
          

            if (result.Result && (media.InPoint != 0 || media.InPoint != 0))
            {

                var transport = new
                {
                    start = "$now",
                    position = media.InPoint,
                    inPosition = media.InPoint,
                    outPosition = media.OutPoint,
                    rate = 0.0,
                    endBehaviour = "repeat"
                };

                control.Player(EnmEnaginType.Ampp, EnmPlayerControl.Transportcommand, JObject.FromObject(transport));

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Transportcommand Info InPoint {media.InPoint} OutPoint {media.OutPoint}");
            }

            if (result.Result)
            {
                isThread = true;

                // SetMediaInfo(media);
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player Cue Success");

            }
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Cue Fail");
        }

        //private async void SetMacre(int index , string path , int inPoint, int outPoint)
        //{
        //    //"V:\\MEDIA\\CM004965___00909.mxf"
        //    var payloads = new Dictionary<string, object>
        //        { { "file", path } };

        //    if(PlayListDefines.Count != 0)
        //    {
        //        var define = PlayListDefines.Where(c => c.Index == index).FirstOrDefault();

        //        if (define != null)
        //        {
        //            AmppControlMacro macro = ControlMacros.Where(c => c.Uuid == define.MediaName).FirstOrDefault();

        //            macro.Commands[2].Payload = payloads;

        //            var result = define.Control.PutMacro(macro.Uuid, macro);

        //            if (result.Result)
        //                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player  Macro : {macro.Uuid} File :{path} Success");
        //            else
        //                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player Macro : {macro.Uuid} File :{path} Fail");

        //            if(inPoint != 0 || outPoint != 0 )
        //            {
        //                Mark(index, inPoint, outPoint);
        //            }

        //        }
        //    }
        //}

        #endregion


        #region 3. PlayList Control ===========>

        public bool StartPlayList { get; set; } = false;

        private bool isPlayStart { get; set; } = false;

        public void ChangeCleancut(int index)
        {
            var Prepare = new
            {
                index = index,
                Program = true
            };

            var result = engineCleanCut.CleanCut(EnmEnaginType.Ampp, EnmCleancut.Inputstate, JObject.FromObject(Prepare));

            if (result.Result)
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Player CleanCut {index} Success");
            else
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Player CleanCut {index} Fail");
        }

      

        // 플레이된 영상의 길이
        private long PlayListFrame = 0;

        private bool isNextCue { get; set; } = false;

        public void NextMediaCuePlay()
        {   
            if (onariMediaInfo.CueMedia.MediaData != null)
            {
                Start(onariMediaInfo.CueMedia.Control, "$now");
               // ChangeCleancut(onariMediaInfo.CueMedia.Cleancut);
            }

            //onariMediaInfo.ChangePlayMediaInfo();

            var cueitem = mediaListControlViewModel.GetNextItem();
            if (cueitem != null) Cue(cueitem, onariMediaInfo.CueMedia.Control);

        }


        #endregion

        #region Config 설정
        public bool SetConfigData(string channel, string path)
        {

            bool result = false;
           // path = @"D:\source\project\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.ManagerBa\bin\Debug\net8.0-windows\Config\SystemConfig.json";

            string jsonFromFile = string.Empty;
            if (System.IO.File.Exists(@path))
                jsonFromFile = System.IO.File.ReadAllText(@path);


            ChannelName = Enum.Parse<EnmChannel>(channel);
            SystemConfigData ConfigData = JsonConvert.DeserializeObject<SystemConfigData>(jsonFromFile);
            SystemConfigDataStatic.Load(ChannelName, ConfigData);


            if(SystemConfigDataStatic.ChannelConfigData.ChannelList != null)
            {

                CurrentControlType = SystemConfigDataStatic.ChannelConfigData.ChannelList.ChannelType;
                ControlName = SystemConfigDataStatic.ChannelConfigData.ChannelList.Name;
                if (CurrentControlType == EnmControlType.Player)
                {
                    appMode = AppMode.Player;
                   // ControlMacros = new List<AmppControlMacro>();
                }
                else
                {
                    appMode = AppMode.Recoder;
                
                    InputItemsLIst.Clear();
                    foreach (var item in SystemConfigDataStatic.ChannelConfigData.InPutList)
                    {
                        InputItemsLIst.Add(item.InputName);
                    }

                    _mainWindow.RecorderButton.InputList.SelectedItem = InputItemsLIst[0];

                    SelectInput = InputItemsLIst[0];
                }

                result = true;
            }
            else
            {
                System.Windows.MessageBox.Show($"{Enum.Parse<EnmChannel>(channel)}의 설정 정보가 존재 하지 않습니다. 확인 후 다시 실행 해주세요.");
            }
            return result;
        }


        private void SendLog(LogType type, string title, string message = "" )
        {
            logger?.Log(type.ToString(), ControlName, title, message);
        }


        private void UpdateMultiColorBar( double Llevel, double Rlevel)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                double totalHeight = 250; // 예: 전체 바 너비 (단위: px)


                double greenRatio = Math.Min(Llevel, 0.5);
                double yellowRatio = Math.Min(Math.Max(Llevel - 0.5, 0), 0.3);
                double redRatio = Math.Max(Llevel - 0.8, 0);

                //double greenHeight = greenRatio / 0.5 * (totalHeight * 0.5);   // 비율 환산
                //double yellowHeight = yellowRatio / 0.2 * (totalHeight * 0.2);
                //double redHeight = redRatio / 0.3 * (totalHeight * 0.3);

                double greenHeight = (greenRatio / 0.5) * (totalHeight * 0.5);
                double yellowHeight = (yellowRatio / 0.3) * (totalHeight * 0.3);
                double redHeight = (redRatio / 0.2) * (totalHeight * 0.2);

                _mainWindow.LBarGreen.Height = greenHeight;
                _mainWindow.LBarYellow.Height = yellowHeight;
                _mainWindow.LBarYellow.Margin = new Thickness(0, 0, 0, greenHeight);
                _mainWindow.LBarRed.Height = redHeight;
                _mainWindow.LBarRed.Margin = new Thickness(0, 0, 0, greenHeight + yellowHeight);

                greenRatio = Math.Min(Rlevel, 0.5);
                yellowRatio = Math.Min(Math.Max(Rlevel - 0.5, 0), 0.3);
                redRatio = Math.Max(Rlevel - 0.8, 0);

                //greenHeight = greenRatio / 0.5 * (totalHeight * 0.5);   // 비율 환산
                //yellowHeight = yellowRatio / 0.2 * (totalHeight * 0.2);
                //redHeight = redRatio / 0.3 * (totalHeight * 0.3);

                greenHeight = (greenRatio / 0.5) * (totalHeight * 0.5);
                yellowHeight = (yellowRatio / 0.3) * (totalHeight * 0.3);
                redHeight = (redRatio / 0.2) * (totalHeight * 0.2);

                _mainWindow.RBarGreen.Height = greenHeight;
                _mainWindow.RBarYellow.Height = yellowHeight;
                _mainWindow.RBarYellow.Margin = new Thickness(0, 0, 0, greenHeight);
                _mainWindow.RBarRed.Height = redHeight;
                _mainWindow.RBarRed.Margin = new Thickness(0, 0, 0, greenHeight + yellowHeight);
                
            });
        }

        private int interval = 600;

        public async void SetConfig(System.Windows.Controls.Image Handle)
        {
            logger = new Logger();
            logger.ConnectHub();

            mediaApi = new MediaApi();
            mediaApi.ConnectHub();
            mediaApi.MediaDataEvent += MediaApi_MediaDataEvent;
            mediaApi.UpdateMediaDataEvent += MediaApi_UpdateMediaDataEvent;


            if (SystemConfigDataStatic.ChannelConfigData.OverlayFilters.OverlayMode == EnmOverlayMode.Decklink)
            {
                string aclsid = SystemConfigDataStatic.ChannelConfigData.OverlayFilters.AudioClsid;
                string vclsid = SystemConfigDataStatic.ChannelConfigData.OverlayFilters.VideoClsid;

                /// Decklink Capture Start
                deckLinkCapture.Start(vclsid, aclsid);
            }
            else if (SystemConfigDataStatic.ChannelConfigData.OverlayFilters.OverlayMode == EnmOverlayMode.NDI)
            {
                // NDI Capture Start
                ndiReceivelib.VideoImage = Handle;
                //ndiReceivelib.AudioLeveLeft = _mainWindow.LiftProgressBar;
                //ndiReceivelib.AudioLeveRight = _mainWindow.ReigthProgressBar;
                ndiReceivelib.Start(SystemConfigDataStatic.ChannelConfigData.OverlayFilters.NDIFilter);
            }
            else if (SystemConfigDataStatic.ChannelConfigData.OverlayFilters.OverlayMode == EnmOverlayMode.None)
            {
                //MessageBox.Show("Overlay Mode None");
            }


            engineControl1 = new EngineControl(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                                 SystemConfigDataStatic.ChannelConfigData.ChannelList.WorkLoad1,
                                                  ChannelName + "_EngineControl1");
            engineControl1.OnAmppControlNotifyEvent += EngineControl_OnAmppControlNotifyEvent;
            engineControl1.OnAmppControlErrorEvent += EngineControl_OnAmppControlErrorEvent;
            engineControl1.OnStateEvent += EngineControl_OnStateEvent;


            
            var result = await engineControl1.Connect(SystemConfigDataStatic.ChannelConfigData.EnginType); 

            if (result)
            {

                if (SystemConfigDataStatic.ChannelConfigData.EnginType == EnmEnaginType.Ampp)
                {
                    #region Ampp key frame 받는 부분 삭제
                    // Ampp key frame 받는 부분 삭제
                    SetImageHandle(Handle);
                    //var startResult = await engineControl.Start(SystemConfigDataStatic.ChannelConfigData.EnginType,
                    //    SystemConfigDataStatic.ChannelConfigData.ChannelList.ProducerName, Handle);
                    //if (startResult)
                    //    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] EngineControl Start Success");
                    //else
                    //    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] EngineControl Start Fail");

                    #endregion

                    if (CurrentControlType == EnmControlType.Recoder)
                    {
                        _mainWindow.RecorderSettingControl.txtPath.Text = SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultFolder;
                        _mainWindow.RecorderSettingControl.txtTitle.Text = SystemConfigDataStatic.ControlConfigData.RecorderSetting.DefaultName;
                    }
                    else
                    {
                        string cleancut = SystemConfigDataStatic.ControlConfigData.PlayerSetting.PlayerCleancutConfigs
                                             .Where(c => c.Channel == ChannelName)?.FirstOrDefault().Cleancut;

                        engineCleanCut = new EngineControl(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                                           cleancut, ChannelName + "_CleanCut");

                        engineCleanCut.OnAmppControlNotifyEvent += EngineControl_OnAmppControlNotifyEvent;
                        engineCleanCut.OnAmppControlErrorEvent += EngineControl_OnAmppControlErrorEvent;
                        engineCleanCut.OnStateEvent += EngineControl_OnStateEvent;

                        result = await engineCleanCut.Connect(SystemConfigDataStatic.ChannelConfigData.EnginType);

                        engineControl2 = new EngineControl(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                                           SystemConfigDataStatic.ChannelConfigData.ChannelList.WorkLoad2,
                                                           ChannelName + "_EngineControl2");

                        engineControl2.OnAmppControlNotifyEvent += EngineControl_OnAmppControlNotifyEvent;
                        engineControl2.OnAmppControlErrorEvent += EngineControl_OnAmppControlErrorEvent;
                        engineControl2.OnStateEvent += EngineControl_OnStateEvent;

                        result = await engineControl2.Connect(SystemConfigDataStatic.ChannelConfigData.EnginType);
                    }

                    if (appMode == AppMode.Recoder)
                    {
                        engineControl1?.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                            EnmRecoderControl.Getstate, new JObject());
                    }
                    else
                    {
                        engineControl1?.Player(SystemConfigDataStatic.ChannelConfigData.EnginType,
                            EnmPlayerControl.Getstate, new JObject());

                        MediaItemDone();
                    }

                    if (appMode == AppMode.Recoder)
                    {
                        interval = 10000;
                    }
                    else
                    {
                        ChangeCleancut(1);
                        onariMediaInfo.PlayMedia.Control = engineControl1;
                        onariMediaInfo.PlayMedia.Cleancut = 1;
                    }
                }
                else if(SystemConfigDataStatic.ChannelConfigData.EnginType == EnmEnaginType.Vdcp)
                {

                }
            }
            else
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] EngineControl Connect Fail");
                System.Windows.MessageBox.Show("EngineControl Connect Fail");
            }
                


            Task task = new Task(async () =>
            {
                while (true)
                {
                    if (isThread)
                    {
                        _mainWindow.Dispatcher.Invoke(() =>
                        {
                            if (SystemConfigDataStatic.ChannelConfigData.EnginType == EnmEnaginType.Vdcp)
                            {
                               //  obj.ContainsKey("Postiontype")

                               //_mainWindow.playerStateControl.CurrentTimecode = engineControl1
                               // .Vdcp(EumCommandKey.POSTIONREQUEST, new JObject()).Result;


                            }
                            else if (SystemConfigDataStatic.ChannelConfigData.EnginType == EnmEnaginType.Ampp)
                            {
                                if (appMode == AppMode.Recoder)
                                {
                                    var size = MediaDiskTime.GetDriveSize(@"V:\");
                                    if (FPS != 0)
                                    {
                                        var recTime = MediaDiskTime.GetRecordTimeString(size, FPS);

                                        _mainWindow.RecorderStateControl.DiskSize = $"RecTime: {recTime}";
                                    }

                                    //engineControl1?.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                    //    EnmRecoderControl.Ping, new JObject());

                                    // var reult1 = engineControl1.ExSubscribeToWorkload(WorkloadId, "recorderinfo.notify");

                                    //engineControl1?.Recoder(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                    //        EnmRecoderControl.Getstate, new JObject());

                                }
                                else
                                {

                                    if (StartPlayList)
                                    {
                                        if (onariMediaInfo.PlayMedia.Control != null)
                                        {
                                            await onariMediaInfo.PlayMedia.Control.Player(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                                            EnmPlayerControl.Getstate, new JObject());

                                            Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] {onariMediaInfo.PlayMedia.Control.EnagineName} Thread Start ......");
                                        }
                                    }
                                    else
                                    {
                                        await engineControl1.Player(SystemConfigDataStatic.ChannelConfigData.EnginType,
                                                        EnmPlayerControl.Getstate, new JObject());
                                    }

                                }
                            }
                        });
                    }

                    System.Threading.Thread.Sleep(interval);
                }
            });

            task.Start();

        }


        #endregion

        #region Ampp Notify Event 

        private void EngineControl_OnStateEvent(object? sender, string e)
        {
            SendLog( LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] State : {e}");
            Debug.WriteLine($"State:\t{e}");
        }

        private void EngineControl_OnAmppControlErrorEvent(object? sender, AmppControlErrorEventArgs e)
        {          

            SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Workload : {e.Key} [{e.Workload}] Command : {e.Command}", e.Details);

            Debug.WriteLine("************Error Notification**************");
            Debug.WriteLine($"Workload:\t{e.Workload}");
            Debug.WriteLine($"Command:\t{e.Command}");
            Debug.WriteLine($"Status:\t{e.Status}");
            Debug.WriteLine($"Error:\t{e.Error}");
            Debug.WriteLine($"Details:\t{e.Details}");
            Debug.WriteLine("**************************************");
        }

        private static int loopCounter = 1; //
        private bool isStartMedia = false;
        private bool isLastMediaPlay = false;
        private void EngineControl_OnAmppControlNotifyEvent(object? sender, AmppControlNotificationEventArgs e)
        {
            _mainWindow.Dispatcher.Invoke(async () =>
            {
                var payload = JsonConvert.SerializeObject(e.Notification.Payload);
                
                if (SystemConfigDataStatic.ChannelConfigData.ChannelList.ChannelType == EnmControlType.Recoder)
                {
                    if(e.Workload == SystemConfigDataStatic.ChannelConfigData.ChannelList.WorkLoad1)
                    {
                        switch (e.Command)
                        {
                            case "recordconfig":

                                var config = JsonConvert.DeserializeObject<Recordconfig>(payload);

                                if (config != null)
                                {

                                    if(config.ProfileParameters.Count != 0)
                                    {
                                        _mainWindow.RecorderStateControl.MediaFormate
                                        = config.ProfileParameters[0].Value;
                                    }

                                    switch (config.VideoStandard.FrameRate)
                                    {
                                        case "30000x1001":
                                            FPS = 30;
                                            break;
                                        case "60000x1001":
                                            FPS = 60;
                                            break;
                                    }

                                    Calculator = new TimecodeCalculator((decimal)FPS);
                                    FrameSmoother.Initialize(0, (int)FPS);
                                }

                                break;
                            case "recordingstate":
                                var state = JsonConvert.DeserializeObject<Recordingstate>(payload);

                                if(state.Source != null)
                                {
                                    _mainWindow.RecorderButton.SelectInput = SelectInput;
                                }   
                                break;
                            case "recorderinfo":

                                var info = JsonConvert.DeserializeObject<RecordInfo>(payload);

                                if (info != null)
                                {
                                    if (info.RecordedFrames != null)
                                    {
                                        Duration = Convert.ToInt32(info.RecordedFrames);
                                        FrameSmoother.UpdateExternalFrame((int)Duration, 1, true);

                                    }

                                    if (info.State == "started")
                                    {
                                        if (Stateus == "Prepared")
                                        {
                                            Stateus = "Record";
                                            mediaApi.UpDateMedia(RecoderId, "Recoding");
                                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} Recording  Success");
                                            _mainWindow.RecorderButton.SetButtonEnable(Stateus);
                                            FrameSmoother.Start();
                                        }
                                    }
                                    else if (info.State == "prepared")
                                    {
                                        Stateus = "Prepared";
                                        _mainWindow.RecorderButton.SetButtonEnable(Stateus);
                                    }
                                    else if (info.State == "ready")
                                    {
                                        if (Stateus == "Record")
                                        {
                                            int outPoint = (int)Duration - 1;
                                            string outTimeCode = Calculator?.FrameNumberToTimecode((ulong)outPoint);
                                            string totalTimcode = Calculator?.FrameNumberToTimecode((ulong)Duration);

                                            _mainWindow.RecorderStateControl.Timecode = totalTimcode;

                                            mediaApi.UpDateOutPoint(RecoderId, outPoint, outTimeCode, (int)Duration, totalTimcode);

                                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} Duration = {Duration} TotalTimcdoe = {totalTimcode}  MediaData Save");
                                            Thread.Sleep(100);

                                            mediaApi.UpDateMedia(RecoderId, "Done");
                                            SetRecoderTimcode(Duration);

                                            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {RecoderCreateName} Recording  Success");

                                            FrameSmoother.Stop();
                                        }

                                        Stateus = "Pause";
                                        _mainWindow.RecorderButton.SetButtonEnable(Stateus);
                                    }
                                    else if (info.State == "Error")
                                    {
                                        Stateus = "Error";

                                        mediaApi.UpDateMedia(RecoderId, "Error");
                                        SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}]  {RecoderCreateName} Recording Fail");
                                    }
                                    _mainWindow.RecorderStateControl.State = Stateus;

                                    Debug.WriteLine($"RecordedFrames : {Duration}");
                                    //{ "ServiceHealth":{ "Health":"OK","HealthText":"Service is healthy"},"State":"ready","RecordedFrames":676}
                                }

                                break;
                        }
                    }
                    

                }
                else // player
                {
                    if (e.Notification.Key == ChannelName + "_EngineControl1"
                        || e.Notification.Key == ChannelName + "_EngineControl2"
                        || e.Notification.Key == ChannelName + "_CleanCut")
                    {
                        if (e.Notification.Key.Contains("CleanCut"))
                        {
                            switch (e.Command)
                            {
                                case "getstate":
                                    break;
                                case "inputstate":
                                    List<Inputstate> Inputstates = JsonConvert.DeserializeObject<List<Model.Engine.Inputstate>>(payload);

                                    var cleancut = Inputstates.Where(c => c.Program == true).ToList();

                                    if (cleancut.Count != 0)
                                    {
                                        //if (cleancut[0].Index == 1)
                                        //{
                                        //    ActionEngine = engineControl1;
                                        //    CueEngine = engineControl2;
                                        //}
                                        //else if (cleancut[0].Index == 2)
                                        //{
                                        //    ActionEngine = engineControl2;
                                        //    CueEngine = engineControl1;
                                        //}

                                        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] ({e.Notification.Key})Cleancut State [ Index : {cleancut[0].Index} Program : {cleancut[0].Program} ]");

                                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Cleancut State [ Index : {cleancut[0].Index} Program : {cleancut[0].Program} ]");
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            switch (e.Command)
                            {
                                case "ping":
                                    ping = JsonConvert.DeserializeObject<Model.Engine.Ping>(payload);

                                    if (ping != null)
                                    {
                                        ApplicationName = ping.Application;
                                        AppState = ping.Success == "OK" ? true : false;
                                    }

                                    break;
                                case "clearassets":
                                    var clear = JsonConvert.DeserializeObject<Model.Engine.ClaearAssets>(payload);

                                    if (StartPlayList)
                                    {

                                    }
                                    else
                                    {
                                        if (clear.isCleared)
                                        {
                                            FrameSmoother.Stop();
                                            FrameSmoother.ResetFrame(0);
                                        }

                                        if (clear.isCleared)
                                        {
                                            MediaState = EnmMediaState.Eject;
                                            isClipLoad = false;
                                            IconPlayStop = "Stop";
                                            _mainWindow.playerButton.IsAtion(false);
                                            _mainWindow.playerStateControl.IsAtion(false);
                                        }
                                    }

                                    break;
                                case "transportcommand": // 진행 정보   1..
                                    var command = JsonConvert.DeserializeObject<Model.Engine.TransportCommand>(payload);

                                    if (onariMediaInfo.PlayMedia.Control != null && onariMediaInfo.PlayMedia.Control.EnagineName == e.Notification.Key)
                                    {
                                        InPoint = command.InPosition;
                                        OutPoint = command.OutPosition;
                                        _mainWindow.playerStateControl.InPoint = InPoint;
                                        _mainWindow.playerStateControl.OutPoint = OutPoint;
                                        _mainWindow.playerStateControl.JogShuttleValue
                                                = $"x{Math.Floor(Convert.ToDouble(command.Rate) * 10) / 10}";


                                        double point = OutPoint - 1;
                                        // 타임코드 업데이트 
                                        FrameSmoother.UpdateExternalFrame(Convert.ToInt32(command.Position), 1, isIncreasing);
                                        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] ({e.Notification.Key})============> Rate : {command.Rate} isStart : {FrameSmoother.IsStart} -/- {command.Position}");

                                        ////////////////////////////////////////////////////////////////////////////////////////////
                                        // 이슈... FPS 가 60인경우 command.Rate 가 진행중 0.0 온다. 대한이 필요 .......
                                        ////////////////////////////////////////////////////////////////////////////////////////////

                                        if (double.TryParse(command.Rate, out double temp))
                                        {
                                            if (temp == 0)  // stop 상태
                                            {
                                                if (FrameSmoother.IsStart == true) FrameSmoother.Stop();
                                            }
                                            else if (temp > 0 && temp < 1) // jog 상태
                                            {
                                                FrameSmoother.isFrameSmoother = false;
                                                DisPlayTimcode((int)command.Position);
                                                Debug.WriteLine($"타임코드 직정입력 {command.Position}");

                                            }
                                            else if (temp == 1 || temp > 1)  // play 상태
                                            {
                                                // 영상이 송출중일때 맨 처음 감지 구간 listPlay
                                                // 인경우 1초 후 다음 영상 Cue 작업 실행 (오직 한번만 실행됨) 
                                                if (StartPlayList && FrameSmoother.IsStart == false)
                                                {
                                                    if (!mediaListControlViewModel.isNextItem())
                                                         SetNextCueMedia();// 2 째 
                                                }

                                                FrameSmoother.isFrameSmoother = true;
                                                if (FrameSmoother.IsStart == false) 
                                                    FrameSmoother.Start();
                                            }

                                            if (!StartPlayList) // 단일 송출 
                                            {
                                                if (temp == 0) // 정지
                                                {
                                                    isThread = false;
                                                    IconPlayStop = "Play";
                                                    DisPlayTimcode((int)command.Position);
                                                }
                                                else  // 재생
                                                {
                                                    isThread = true;
                                                    IconPlayStop = "Stop";
                                                }
                                            }
                                        }
                                    
                                        if (StartPlayList) // 리스트 송출 
                                        {
                                            if (point == command.Position)                     // 영상 송출이 끝나면
                                            {
                                            
                                                if (_mainWindow.playerGroupControl.isLopp)
                                                {
                                                    if (mediaListControlViewModel.isPlayFinish())
                                                    {
                                                        onariMediaInfo.PlayMedia.MediaData.State = "Done";
                                                        isLastMediaPlay = true;
                                                    }
                                                    else
                                                    {

                                                        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] 영상 플레이 완료 = {OutPoint - Duration} 진입 ");
                                                        SetStartData(0, 0);

                                                        FrameSmoother.Initialize(0, (int)onariMediaInfo.PlayMedia.Fps);
                                                        FrameSmoother.Start();

                                                        isStartMedia = true;
                                                        ChangeOnairData();

                                                        if (isLastMediaPlay)
                                                        {
                                                            isLastMediaPlay = false;
                                                            var item = mediaListControlViewModel.LastMedia();
                                                            item.State = "Wait";
                                                        }

                                                        if (!mediaListControlViewModel.isNextItem())
                                                            await SetNextCueMedia();
                                                    }
                                                }
                                                else 
                                                {
                                                    if (mediaListControlViewModel.isPlayFinish())
                                                    {
                                                        onariMediaInfo.PlayMedia.MediaData.State = "Done";
                                                        FrameSmoother.Stop();
                                                        _mainWindow.playerGroupControl.SetDoneButtonEnabled();

                                                        isThread = false;
                                                        StartPlayList = false;
                                                    }
                                                    else
                                                    {

                                                        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] 영상 플레이 완료 = {OutPoint - Duration} 진입 ");
                                                        SetStartData(0, 0);

                                                        FrameSmoother.Initialize(0, (int)onariMediaInfo.PlayMedia.Fps);
                                                        FrameSmoother.Start();

                                                        isStartMedia = true;
                                                        ChangeOnairData();

                                                        var media = mediaListControlViewModel.LastMedia();
                                                        
                                                        
                                                        if (!mediaListControlViewModel.isNextItem() && media.State != "Play")
                                                            await SetNextCueMedia();

                                                    }
                                                }

                                                #region

                                                //if (mediaListControlViewModel.isPlayFinish())  // 마지막 영상이 play 인경우
                                                //{
                                                //    if (_mainWindow.playerGroupControl.isLopp) // 마지막 영상이 Loop 송출 인경우
                                                //    {
                                                //        onariMediaInfo.PlayMedia.MediaData.State = "Done";
                                                //        isLastMediaPlay = true;
                                                //    }
                                                //    else
                                                //    {
                                                //        onariMediaInfo.PlayMedia.MediaData.State = "Done";
                                                //        FrameSmoother.Stop();
                                                //        _mainWindow.playerGroupControl.SetDoneButtonEnabled();

                                                //        isThread = false;
                                                //        StartPlayList = false;
                                                //    }
                                                //}
                                                //else                                        // 남을 영상이 있는 경우
                                                //{
                                                //    isStartMedia = true;
                                                //    ChangeOnairData();

                                                //    // 마지막 영상이 송출이 끝나면 State = "Done" 으로 변경됨
                                                //    // lopp 진행중 마지막 영상 상태 wait 유지를 위한 다시 Wait 로 벼경 시킴
                                                //    if(_mainWindow.playerGroupControl.isLopp)
                                                //    {
                                                //        if (isLastMediaPlay)
                                                //        {
                                                //            isLastMediaPlay = false;
                                                //            var item = mediaListControlViewModel.LastMedia();
                                                //            item.State = "Wait";
                                                //        }

                                                //        if (!mediaListControlViewModel.isNextItem())
                                                //            await SetNextCueMedia();
                                                //    }
                                                //    else
                                                //    {
                                                //        if (!mediaListControlViewModel.isNextItem())
                                                //            await SetNextCueMedia();
                                                //    }

                                                //}
                                                #endregion
                                            }
                                            else if (10 >= point - command.Position) // 다음 영상 플레이 시작
                                            {
                                                //if (mediaListControlViewModel.isNextItem())
                                                //{
                                                //    Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] FPS : {FPS} Remaining {point - command.Position}");
                                                //    SetStartData(command.Position, point);
                                                //}
                                            }
                                            else
                                            {
                                                // 마지막 영상이 처음 시작할때 Loop 송출 처리 
                                                if (isStartMedia && mediaListControlViewModel.isPlayFinish())
                                                {
                                                    isStartMedia = false;
                                                    if (_mainWindow.playerGroupControl.isLopp)
                                                    {
                                                        Debug.WriteLine($"Loop 송출 시작");

                                                        mediaListControlViewModel.SetLoopState();

                                                        Task task = new Task(() =>
                                                        {
                                                            var cueitem = mediaListControlViewModel.GetNextItem();
                                                            Eject(onariMediaInfo.CueMedia.Control);
                                                            Thread.Sleep(1000);
                                                            Cue(cueitem, onariMediaInfo.CueMedia.Control);
                                                            //Thread.Sleep(300);
                                                            //string startTime = AddFramesToTime(DateTime.UtcNow, cueitem.Frame , (int)FPS);
                                                            //Start(onariMediaInfo.CueMedia.Control, startTime);
                                                        });
                                                        task.Wait(0);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    break;
                                case "playerconfig": // Player 설정 정보  2...
                                    #region 주석 처리
                                    //if (isClipLoad)
                                    //{
                                    //    isNextCue = false;

                                    //    var config = JsonConvert.DeserializeObject<Model.Engine.PlayerConfig>(payload);

                                    //    switch (config.FrameRate)
                                    //    {
                                    //        case "30000x1001":
                                    //            FPS = 30;
                                    //            break;
                                    //        case "60000x1001":
                                    //            FPS = 60;
                                    //            break;
                                    //    }

                                    //    Calculator = new TimecodeCalculator((decimal)FPS);
                                    //    FrameSmoother.Initialize(0, (int)FPS);


                                    //    InTimeCode = Calculator.FrameNumberToTimecode((ulong)InPoint);
                                    //    _mainWindow.playerStateControl.InTimeCode.TimeCode = InTimeCode;

                                    //    OutTimeCode = Calculator.FrameNumberToTimecode((ulong)OutPoint);
                                    //    _mainWindow.playerStateControl.OutTimeCode.TimeCode = OutTimeCode;

                                    //    _mainWindow.playerStateControl.RemainTimecode
                                    //            = Calculator.FrameNumberToTimecode((ulong)(OutPoint - 1));

                                    //    if (TotalDuration == 0)
                                    //    {
                                    //        _mainWindow.playerStateControl.MaxDuration = OutPoint - 1;
                                    //        TotalDuration = (int)OutPoint;
                                    //    }
                                    //}
                                    #endregion
                                    break;
                                case "clip":  //4...
                                    var clip = JsonConvert.DeserializeObject<Model.Engine.Clip>(payload);

                                    if (clip.Loaded)
                                    {
                                        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] key : {e.Notification.Key} cuemedia : {onariMediaInfo.CueMedia.Control.EnagineName}");
                                        if (onariMediaInfo.CueMedia.Control != null && onariMediaInfo.CueMedia.Control.EnagineName == e.Notification.Key)
                                        {
                                            if (StartPlayList)
                                            {
                                                isNextCue = false;
                                                int index = onariMediaInfo.CueMedia.MediaData.Index;
                                                onariMediaInfo.CueMedia.MediaData.State = "Cue";
                                                loopCounter = 1;

                                                if(!clip.Loaded)
                                                {
                                                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Clip Load Name : {clip.File} Loaded = {clip.Loaded}");
                                                }
                                                else
                                                {
                                                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Clip Load Name : {clip.File} Loaded = {clip.Loaded}");
                                                }
                                                    

                                                //Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] {onariMediaInfo.CueMedia.Control.EnagineName} index {index} Cue");

                                            }
                                            else
                                            {
                                                Calculator = new TimecodeCalculator((decimal)onariMediaInfo.CueMedia.Fps);
                                                FrameSmoother.Initialize(0, (int)onariMediaInfo.CueMedia.Fps);

                                                InTimeCode = Calculator.FrameNumberToTimecode((ulong)InPoint);
                                                _mainWindow.playerStateControl.InTimeCode.TimeCode = InTimeCode;

                                                OutTimeCode = Calculator.FrameNumberToTimecode((ulong)OutPoint);
                                                _mainWindow.playerStateControl.OutTimeCode.TimeCode = OutTimeCode;

                                                _mainWindow.playerStateControl.RemainTimecode
                                                        = Calculator.FrameNumberToTimecode((ulong)(OutPoint - 1));

                                                if (TotalDuration == 0)
                                                {
                                                    _mainWindow.playerStateControl.MaxDuration = OutPoint - 1;
                                                    TotalDuration = (int)OutPoint;
                                                }

                                                IconPlayStop = "Play";
                                                isClipLoad = true;
                                                MediaState = EnmMediaState.Cue;
                                                _mainWindow.playerButton.IsAtion(true);
                                                _mainWindow.playerStateControl.IsAtion(true);
                                            }
                                        }
                                    }

                                    #region 주석  

                                    //if (StartPlayList)
                                    //{
                                    //    if (clip.Loaded)
                                    //    {
                                    //        Debug.WriteLine($"Notification {e.Notification.Key} ||| ActionEngine {listEngineControl.ActionControlName}");
                                    //        if (listEngineControl.ActionControlName == e.Notification.Key)
                                    //            isClipLoad = true;
                                    //        else
                                    //            isClipLoad = false;
                                    //    }
                                    //    else
                                    //    {
                                    //        isClipLoad = false;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    if (clip.Loaded)
                                    //    {
                                    //        MediaState = EnmMediaState.Cue;
                                    //        IconPlayStop = "Play";
                                    //        isClipLoad = true;
                                    //        _mainWindow.playerButton.IsAtion(true);
                                    //        _mainWindow.playerStateControl.IsAtion(true);
                                    //    }
                                    //    else
                                    //    {
                                    //        isClipLoad = false;
                                    //    }
                                    //}
                                    #endregion

                                    break;
                                case "transportstate":  // 상태 정보
                                case "videostandard":  //3...
                                case "shuttle":
                                case "rate":
                                case "recue":
                                    break;
                            }
                        }
                    }
                }
            });
            

            //Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] -------------------------------------------");
            //Debug.WriteLine($"Notification {e.Notification.Key}");
            //Debug.WriteLine($"Workload:\t{e.Workload}");
            //Debug.WriteLine($"Command:\t{e.Command}");
            //Debug.WriteLine($"Details:\t{e.Notification.Status}");
            //Debug.WriteLine($"Payload:\t{JsonConvert.SerializeObject(e.Notification.Payload)}");
        }

        public async Task SetNextCueMedia()
        {
            if (onariMediaInfo.CueMedia.Control == null)
            {
                onariMediaInfo.CueMedia.Control = engineControl2;
                onariMediaInfo.CueMedia.Cleancut = 2;
            }
            else if(onariMediaInfo.CueMedia.Control.EnagineName == engineControl1.EnagineName)
            {
                onariMediaInfo.CueMedia.Control = engineControl2;
                onariMediaInfo.CueMedia.Cleancut = 2;
            }
            else
            {
                onariMediaInfo.CueMedia.Control = engineControl1;
                onariMediaInfo.CueMedia.Cleancut = 1;
            }

            var cueitem = mediaListControlViewModel.GetNextItem();

            
            if(cueitem == null)
            {
                SendLog(LogType.Warning, $"[{MethodBase.GetCurrentMethod().Name}] NextCue ====> {onariMediaInfo.CueMedia.Control.EnagineName} Cue Clip Name = No Clip");
                return;
            }
            else
            {
                //  Eject(onariMediaInfo.CueMedia.Control);
                await Task.Delay(1000);
                Cue(cueitem, onariMediaInfo.CueMedia.Control);

                // cue 작업이 끝나면 1초 대기후 다음 영상 송출 명령 내려 frame 만틈 지난후 자동 송출 방법 고민 해자
                //await Task.Delay(300);
                //string startTime = AddFramesToTime(DateTime.UtcNow, cueitem.Frame , (int)FPS);
                //Start(onariMediaInfo.CueMedia.Control, startTime);

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] NextCue ====> {onariMediaInfo.CueMedia.Control.EnagineName} Cue Clip Name = {cueitem.Name}");
            }

            

            //Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] NextCue End  ===========> {onariMediaInfo.CueMedia.Control.EnagineName} Cleancut : {onariMediaInfo.CueMedia.Cleancut}");

        }

        public void SetStartData(double position, double point)
        {
           // if (!isPlayStart)
            {
                isPlayStart = true;
                //string startTime = AddFramesToTime(DateTime.UtcNow, (int)(position - point), (int)FPS);
                string startTime = "$now";
                Start(onariMediaInfo.CueMedia.Control, startTime);
                //PlayPause(onariMediaInfo.CueMedia.Control);

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Remaining Time {position - point} Play Start ===> {startTime}");
                Debug.WriteLine($"FPS : {FPS} Remaining {position - point} Play Start ==============================================================> {startTime}");
            }
        }


        public void ChangeOnairData()
        {
            isPlayStart = false;

            ChangeCleancut(onariMediaInfo.CueMedia.Cleancut);
            onariMediaInfo.PlayMedia.MediaData.State = "Done";
            onariMediaInfo.ChangePlayMediaInfo(); // 송출 정보 갱신

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Change Play MediaInfo");

            _mainWindow.playerStateControl.InTimeCode.TimeCode = onariMediaInfo.PlayMedia.MediaData.InTimeCode;
            _mainWindow.playerStateControl.OutTimeCode.TimeCode = onariMediaInfo.PlayMedia.MediaData.OutTimeCode;

            _mainWindow.playerStateControl.RemainTimecode
                    = Calculator.FrameNumberToTimecode((ulong)(onariMediaInfo.PlayMedia.MediaData.OutPoint - 1));

            onariMediaInfo.PlayMedia.MediaData.State = "Play";

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {onariMediaInfo.PlayMedia.MediaData.Name}" +
                             $" State = {onariMediaInfo.PlayMedia.MediaData.State} 변경");

            if (TotalDuration == 0)
            {
                _mainWindow.playerStateControl.MaxDuration = onariMediaInfo.PlayMedia.MediaData.OutPoint - 1;
                TotalDuration = (int)onariMediaInfo.PlayMedia.MediaData.OutPoint;
            }

            Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] {onariMediaInfo.CueMedia.Control.EnagineName} - {onariMediaInfo.CueMedia.Cleancut}NextCue Play Start  ===========>");
        }
             


        // frameRate는 초당 프레임 수 (예: 30fps)
        public string AddFramesToTime(DateTime startTime, int framesToAdd, int frameRate)
        {
            if (frameRate <= 0) throw new ArgumentException("Frame rate must be positive.");

            // 1. UTC로 변환 (만약 Local이면)
            DateTime utcTime = startTime.Kind == DateTimeKind.Utc
                ? startTime
                : startTime.ToUniversalTime();

            // 2. 프레임 수를 초 단위로 변환
            double secondsToAdd = framesToAdd / (double)frameRate;

            // 3. 시간 추가
            DateTime newTime = utcTime.AddSeconds(secondsToAdd);

            // 4. ISO 8601 형식으로 출력 (UTC 기준)
            return newTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }



        private void FrameSmoother_FrameUpdated(object? sender, int e)
        {
            _mainWindow.Dispatcher.Invoke(() =>
            {
                DisPlayTimcode(e);
            });
        }

        private void DisPlayTimcode(int dur)
        {

            if (SystemConfigDataStatic.ChannelConfigData.ChannelList.ChannelType == EnmControlType.Recoder)
            {
                var timecode = Calculator?.FrameNumberToTimecode((ulong)dur);
                if (timecode != null) _mainWindow.RecorderStateControl.Timecode = timecode;

               // Debug.WriteLine($"Timecode : {timecode} Frame :{dur}");
            }
            else
            {
                //  if(StartPlayList) PlayListAction(e);

                Duration = dur;
                _mainWindow.playerStateControl.SliderValue = Duration;

                if (Calculator != null)
                {
                    if(Duration <= OutPoint)
                    {
                        _mainWindow.playerStateControl.CurrentTimecode
                            = Calculator.FrameNumberToTimecode((ulong)Duration);

                        if((OutPoint - InPoint) - Duration > 0)
                        {
                            _mainWindow.playerStateControl.RemainTimecode
                            = Calculator.FrameNumberToTimecode((ulong)(OutPoint- Duration - 1));

                            //if (StartPlayList) // 리스트 송출 
                            //{
                            //    if (OutPoint - Duration <= 10  && mediaListControlViewModel.isNextItem())
                            //    {
                            //        Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] ====> Frame = {OutPoint - Duration} 진입 ");
                            //        SetStartData(0, 0);
                            //    }

                            //}
                                

                            Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] Duration = {Duration}");

                            //if ((OutPoint - Duration <= 5) && !isNextCue)
                            //{
                            //    Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] ====> Frame = {OutPoint - Duration} 진입 ");
                            //    //if (StartPlayList)
                            //    //{
                            //    //    isNextCue = true;
                            //        //NextMediaCuePlay();
                            //    //}
                            //}
                        }
                    }
                }

                //Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] Timecode " +
                //    $": {_mainWindow.playerStateControl.CurrentTimecode} {_mainWindow.playerStateControl.RemainTimecode}" +
                //    $"  Frame :{dur}");
            }
        }

        #endregion

        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
