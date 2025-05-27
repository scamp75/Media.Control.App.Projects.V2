using System.Collections.ObjectModel;
using System.IO.Pipes;
using System.IO;
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
using Media.Control.App.LogView.ViewModel;
using Media.Control.App.LogView.Model;
using Newtonsoft.Json;
using System.Diagnostics;
using System;
using Newtonsoft.Json.Linq;

namespace Media.Control.App.LogView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private LogViewViewModel mainWindowViewModel = null;

        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new LogViewViewModel(this);

            this.DataContext = mainWindowViewModel;

            //argList = $"Create " + 1 
            //$"{left} " + 2
            //$"{Top} " + 3
            //$"{jsonFromFile}"; 4

            string[] args = Environment.GetCommandLineArgs();
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

                if (jsonFromFile != string.Empty)
                {
                    var jObject = JObject.Parse(jsonFromFile);

                    var mediaUrl = jObject["ControlConfigData"]?["MediaViewSetting"]?["Url"];
                    mainWindowViewModel.MedaiUrl = mediaUrl?.ToString() ?? string.Empty;
                }

                mainWindowViewModel.SetLoggApi();
            }
            else
            {
                Left = Convert.ToDouble(args[2]);
                Top = Convert.ToDouble(args[3]);
            }

            ShowWindows();
            StartPipeServer();
        }

        private void StartPipeServer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var server = new NamedPipeServerStream(
                        "LogView",
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
                                if (item.Name == "LogView")
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
                            MessageBox.Show("JSON 처리 오류: " + ex.Message);
                        }
                    });
                }
                catch (IOException ex)
                {
                    // 읽기 실패 시 처리
                }
            }
        }

        //private async void StartPipeServer()
        //{
        //    await Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            var server = new NamedPipeServerStream("LogView",
        //                PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances,
        //                PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        //            server.BeginWaitForConnection(ar =>
        //            {
        //                var pipe = (NamedPipeServerStream)ar.AsyncState;
        //                try
        //                {
        //                    pipe.EndWaitForConnection(ar);

        //                    using (var reader = new StreamReader(pipe))
        //                    {
        //                        string message = reader.ReadToEnd();

        //                        Dispatcher.Invoke(() =>
        //                        {
        //                            //Debugger.Launch();
        //                            List<Position> positions = JsonConvert.DeserializeObject<List<Position>>(message);

        //                            foreach (var item in positions)
        //                            {
        //                                if (item.Name == "LogView")
        //                                {
        //                                    Left = item.Left;
        //                                    Top = item.Top;
        //                                    ShowWindows();
        //                                    break;
        //                                }
        //                            }

        //                        });
        //                    }
        //                }
        //                catch (IOException ex)
        //                {
        //                    // 예외 처리
        //                }

        //            }, server);

        //            Thread.Sleep(1); // 잠시 대기
        //        }
        //    });
        //}

        private void ShowWindows()
        {
            this.Left = Left; // workingArea.Right - this.Width;
            this.Top = Top; //workingArea.Top;
            this.Height = 410;//SystemParameters.PrimaryScreenHeight - 900;
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void butSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindowViewModel.Close();
            Thread.Sleep(1000);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.HeightChanged)
            {
                mainWindowViewModel.GridHeight = (int)e.NewSize.Height - 80;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.ConnectHub();
        }
    }
}