using Newtonsoft.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Media.Control.App.RP.Model.Config;
using static MaterialDesignThemes.Wpf.Theme;
using System.Collections.ObjectModel;
using ComboBox = MaterialDesignThemes.Wpf.Theme.ComboBox;
using System.Diagnostics;
using Media.Control.App.RP.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Threading.Channels;
using Media.Control.App.RP.Controls;
using Media.Control.App.RP.ViewModel;
using Newtonsoft.Json.Linq;
using Media.Control.App.RP.Model.Engine;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO.Pipes;
using System.IO;
using System.Windows.Forms;

namespace Media.Control.App.RP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        private MainWindowViewModel _viewModel = null;
        private double WindowsCount { get; set; } = 4;
        private string ProgrameType { get; set; } = "Player";
        private string ProgrameName { get; set; } = "Media.Control.App";
        private double? Left = null;
        private double? Top = null;
        

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel(this);
            this.DataContext = _viewModel;
            this.MediaListControl.MediaListEvent += MediaListControl_MediaListEvent;
            this.SourceInitialized += MainWindow_SourceInitialized;
           // Debugger.Launch();

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                if (args[1] == "Create")
                {
                    //argList = $"Create " + 1 
                    //$"{channel.Channel} " + 2
                    //$"{left} " + 3
                    //$"{Top} " + 4
                    //$"{jsonFromFile}"; 5
                    this.WindowStartupLocation = WindowStartupLocation.Manual;

                    Left = Convert.ToDouble(args[3]);
                    Top = Convert.ToDouble(args[4]);

                    if ( _viewModel.SetConfigData(args[2], args[5]))
                    {

                        if (_viewModel.CurrentControlType == EnmControlType.Recoder)
                        {
                            this.RecorderButton.SelectionChanged += RecorderButton_SelectionChanged;
                            this.RecorderButton.ButtonClicked += RecorderButton_ButtonClicked;

                            this.RecorderStateControl.Timecode = "00:00:00;00";
                            this.RecorderStateControl.State = "Pause";
                            this.RecorderStateControl.FileName = "No File";
                            this.RecorderStateControl.MediaFormate = "No Media";
                            this.RecorderStateControl.DiskSize = "RecTime:0.0";

                            this.RecorderButton.SetButtonEnable("Pause");
                        }
                        else
                        {

                            this.playerButton.ButtonClicked += PlayerButton_ButtonClicked;
                            this.playerStateControl.ButtonClicked += PlayerStateControl_ButtonClicked;
                            this.playerStateControl.TriangleButtonClicked += PlayerStateControl_TriangleButtonClicked; 
                            this.playerStateControl.ProgressMouseclicked += PlayerStateControl_ProgressMouseclicked;
                            this.playerStateControl.SilderValueChanged += PlayerStateControl_SliderMouseclicked;

                            this.playerStateControl.InTimeCode.TimeCode = "00;00:00;00";
                            this.playerStateControl.OutTimeCode.TimeCode = "00;00:00;00";

                        }

                        

                        _viewModel.SetConfig(PreviewImage);

                        this.MediaListControl.MediaListEvent += MediaListControl_MediaListEvent1;
                        this.MediaListControl.ChannelType = _viewModel.CurrentControlType.ToString();


                        //ShowWindows();

                    }
                    else
                    {
                        this.Close();   
                    }

                }
                else
                {
                    Left = Convert.ToDouble(args[2]);
                    Top = Convert.ToDouble(args[3]);
                }

                StartPipeServer();
                
               // ShowWindows();
            }
        }

        private void StartPipeServer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var server = new NamedPipeServerStream(
                        _viewModel.ChannelName.ToString(),
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    try
                    {
                        await server.WaitForConnectionAsync(); // 비동기 대기
                        _ = HandleClientAsync(server);         // 클라이언트 처리 분리
                    }
                    catch (IOException ex)
                    {
                        // 연결 실패 예외 처리 (필요 시 로깅)
                    }
                }
            });
        }

        private async Task HandleClientAsync(NamedPipeServerStream pipe)
        {
            using (pipe)
            using (var reader = new StreamReader(pipe))
            {
                try
                {
                    string message = await reader.ReadToEndAsync(); // 전체 메시지 수신

                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (message == "Close" || message == "AllClose")
                        {
                            _viewModel.CommandClose();
                            return;
                        }

                        try
                        {
                            List<Position> positions = JsonConvert.DeserializeObject<List<Position>>(message);

                            foreach (var item in positions)
                            {
                                if (item.Name == _viewModel.ChannelName.ToString())
                                {
                                    Left = item.Left;
                                    Top = item.Top;
                                    ShowWindows();
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("JSON 처리 오류: " + ex.Message);
                        }
                    });
                }
                catch (IOException ex)
                {
                    // 읽기 실패 예외 처리
                }
            }
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            ShowWindows();
        }


        private void PlayerStateControl_SliderMouseclicked(object? sender, object e)
        {
            var slider = (e as SliderChangeValue);

            if (slider != null)
            {

                if(slider.Type == "Jog")
                    _viewModel.Rate(Math.Floor(slider.Value * 10) / 10);
                else 
                    _viewModel.Shuttle(slider.Value);
            }
        }

        private void MediaListControl_MediaListEvent1(object? sender, object e)
        {

            if(!_viewModel.StartPlayList )
            {
                this.playerGroupControl.SetButtonEnabled("butReCue", true);
                this.playerGroupControl.SetButtonEnabled("butNext", true);
                this.playerGroupControl.SetButtonEnabled("butClean", true);
                this.playerGroupControl.SetButtonEnabled("butDelete", true);
            }
            else
            { 
            }


            #region kaba 이후 추가 코딩
            //var media = (e as MediaDataInfo);


            //this.playerStateControl.MaxDuration += Convert.ToDouble(media.Frame);

            //if (this.playerStateControl.GetMarkerCount() == 0)
            //{
            //    this.playerStateControl.AddMaker(MarkerShape.Equilateral, 1, media.OutPoint);
            //}
            //else
            //{

            //    var value = this.playerStateControl.GetDuration();
            //    this.playerStateControl.AddMaker(MarkerShape.Equilateral, value , media.OutPoint);

            //}

            //this.playerStateControl.SliderValue = 0;
            #endregion
        }

        private void PlayerStateControl_TriangleButtonClicked(object? sender, object e)
        {
            _viewModel.Seek("Seek", Convert.ToInt32(e));
        }

        private void PlayerStateControl_ProgressMouseclicked(object? sender, object e)
        {
            if(e == null)
            {
                _viewModel.PlayerButtonClicked("ButStop");
            }
            else
            {
                _viewModel.Seek("Seek",Convert.ToInt32(e));
            }
        }

        // MediaList 아이템  Drag 된 경우
        private void MediaListControl_MediaListEvent(object? sender, object e)
        {
            var media = (e as MediaDataInfo);

            _viewModel.CheckMedia(media);
            _viewModel.SavePlayLists();
        }

        private void PlayerStateControl_ButtonClicked(object? sender, object e)
        {
            _viewModel.PlayerButtonClicked((string)e);
        }

        private void RecorderButton_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var input = RecorderButton.SelectInput;

            _viewModel.ChangeInput(input);
        }

        private void ShowWindows()
        {
            this.Left = Left;
            this.Top = Top;
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, IntPtr.Zero, (int)this.Left, (int)this.Top, 0, 0,
                SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

            if(_viewModel.CurrentControlType == EnmControlType.Recoder)
            {
                this.MediaListControl.Listheight = this.Height - 540 - 15; 
            }
            else
            {
                this.MediaListControl.Listheight = this.Height - 540 - 45;
            }

        }
    

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(_viewModel.CurrentControlType == EnmControlType.Recoder)
            {
                this.MediaListControl.Listheight = this.Height - 540 - 15; 
            }
            else
            {
                this.MediaListControl.Listheight = this.Height - 540 - 45;
            }
        }

        // Player Button Event
        private void PlayerButton_ButtonClicked(object? sender, object e)
        {
            _viewModel.PlayerButtonClicked((string)e);
        }

        private void GroupButton_ButtonClicked(object sender, object e)
        {
            _viewModel.PlayerButtonClicked((string)e);
        }

        // Recorder Button Event
        private void RecorderButton_ButtonClicked(object? sender, object e)
        {
            _viewModel.RecorderButtonClicked((string)e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void MediaListControl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MediaListControl_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            // 드롭할 데이터가 MediaItem 타입이면 Copy 효과, 아니면 None 효과를 부여
            if (e.Data.GetDataPresent(typeof(string)))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        private void PreviewImage_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) ||
               e.Data.GetDataPresent(System.Windows.DataFormats.StringFormat))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }

        }

        private void PreviewImage_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] files = null;

            // FileDrop 형식이면 배열로 받음
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                files = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    _viewModel.Cue(files[0]);
                }
            }
            // 문자열 형식인 경우 (예: 파일 경로)
            else if (e.Data.GetDataPresent(System.Windows.DataFormats.StringFormat))
            {
                string droppedItem = e.Data.GetData(System.Windows.DataFormats.StringFormat) as string;

                var MediaItem = JsonConvert.DeserializeObject<MediaDataInfo>(droppedItem);

                int ipoint = MediaItem.InPoint;
                int opoint = 0;

                if(ipoint != 0)
                    opoint = MediaItem.OutPoint;
                

                if (MediaItem.Frame != MediaItem.OutPoint + 1)
                    opoint = MediaItem.OutPoint;

                _viewModel.Cue(MediaItem, null);
                _viewModel.MediaState = EnmMediaState.Cue;

            }
            else if (e.Data.GetDataPresent(typeof(MediaDataInfo)))
            {
                var droppedData = e.Data.GetData(typeof(MediaDataInfo)) as MediaDataInfo;

                int ipoint = droppedData.InPoint;
                int opoint = 0;

                if (ipoint != 0)
                    opoint = droppedData.OutPoint;

                if (droppedData.Frame != droppedData.OutPoint + 1)
                    opoint = droppedData.OutPoint;

                _viewModel.Cue(droppedData, null);
                _viewModel.MediaState = EnmMediaState.Cue;
            }
        }

        private void MediaListControl_Drop(object sender, System.Windows.DragEventArgs e)
        {

        }

       
    }
}
