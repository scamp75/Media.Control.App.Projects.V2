using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Media.Control.App.MeidaBrowser.ViewModel;
using Media.Control.App.MeidaBrowser.Model;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace Media.Control.App.MeidaBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MeidaBrowserViewModel mainWindowViewModel = null;
        public MainWindow()
        {
            InitializeComponent();
            
            mainWindowViewModel = new MeidaBrowserViewModel(this);
            this.DataContext = mainWindowViewModel;

            // 이미지 경로 설정
            var imagePath = "pack://application:,,,/Resources/browser.png";
            // BitmapImage 생성
            var bitmap = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            // Image 컨트롤에 이미지 적용
            imageTtle.Source = bitmap;
            ComChannel.SelectedIndex = 0;

            string[] args = Environment.GetCommandLineArgs();
            //argList = $"Create " + 1 
            //$"{left} " + 2
            //$"{Top} " + 3
            //$"{jsonFromFile}"; 4
            if (args.Count() == 1)
            {
                args = new string[4];
                args = new string[] { "", "Create", "0", "0" };
            }

            if (args != null && args.Length > 1)
            {
                if (args[1] == "Create")
                {
                    Left = Convert.ToDouble(args[2]);
                    Top = Convert.ToDouble(args[3]);
                }

                string jsonFromFile = string.Empty;
                string jsonpath = args[4];
                if (System.IO.File.Exists(jsonpath))
                    jsonFromFile = System.IO.File.ReadAllText(@jsonpath);

                if(jsonFromFile != string.Empty)
                {
                    var jObject = JObject.Parse(jsonFromFile);
                    var channelList = jObject["ChannelConfigData"]?["ChannelList"];

                    if (channelList != null)
                    {
                        foreach (var item in channelList)
                        {

                            var type = item["ChannelType"]?.ToString();
                            if(type == "2")
                            {
                                mainWindowViewModel.ChannelLists.Add(item["Name"]?.ToString());
                            }

                            Console.WriteLine(item["Name"]?.ToString());
                        }
                    }
                }


                this.MediaListControl.MenuItemClicked += MediaListControl_MenuItemClicked;

                // JSON 파일을 읽어와서 SystemConfigData 객체로 변환

                //SystemConfigData ConfigData = JsonConvert.DeserializeObject<SystemConfigData>(jsonFromFile);
                //SystemConfigDataStatic.Load(ChannelName, ConfigData);
            }
            else
            {
                Left = Convert.ToDouble(args[2]);
                Top = Convert.ToDouble(args[3]);

                //  SetMove();
            }
            
            ShowWindows();
            StartPipeServer();
        }

        private void MediaListControl_MenuItemClicked(object? sender, MediaDataInfo e)
        {
            mainWindowViewModel.DeleteMedia(e);
        }

        private void StartPipeServer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var server = new NamedPipeServerStream(
                        "MediaBrowser",
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    try
                    {
                        await server.WaitForConnectionAsync();  // 🔹 async/await로 대기
                        _ = HandlePipeConnectionAsync(server);  // 🔹 클라이언트 처리 병렬 분기
                    }
                    catch (IOException ex)
                    {
                        // 연결 실패 시 로그 처리 가능
                    }
                }
            });
        }

        private async Task HandlePipeConnectionAsync(NamedPipeServerStream pipe)
        {
            using (pipe)
            using (var reader = new StreamReader(pipe))
            {
                try
                {
                    string message = await reader.ReadToEndAsync();  // 🔹 전체 JSON 수신 (멀티라인도 OK)

                    await Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {

                            if (message == "AllClose")
                            {
                                mainWindowViewModel.CommandClose(null);
                                return;
                            }

                            List<Position> positions = JsonConvert.DeserializeObject<List<Position>>(message);

                            foreach (var item in positions)
                            {
                                if (item.Name == "MediaBrowser")
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
                            //MessageBox.Show("JSON 처리 오류: " + ex.Message);
                        }
                    });
                }
                catch (IOException ex)
                {
                    // 읽기 실패 시 처리
                }
            }
        }


        private void ShowWindows()
        {

            this.Left = Left; // workingArea.Right - this.Width;
            this.Top = Top; //workingArea.Top;
            this.Height = SystemParameters.PrimaryScreenHeight - 450;
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
            this.MediaListControl.Listheight = this.Height - 80;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.ConnectHub();
        }
    }
}