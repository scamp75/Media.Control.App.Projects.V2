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
using Point = System.Windows.Point;

namespace Media.Control.App.MeidaBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MeidaBrowserViewModel mainWindowViewModel = null;
    
        private bool _isDragging = false;
        public MainWindow()
        {
            InitializeComponent();
            
            mainWindowViewModel = new MeidaBrowserViewModel(this);
            this.DataContext = mainWindowViewModel;


            this.MediaListControl.ButtonClicked += MediaListControl_ButtonClicked;
            this.MediaListControl.ButtonMouseDoubleClicked += MediaListControl_ButtonMouseDoubleClicked; ;
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
           
            mainWindowViewModel.PreviewHight = 0;

            System.Windows.Forms.Panel newPanel = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Red
            };

            WindowsFormsHostControl.Child = newPanel;
            WindowsFormsHostControl.Child.Visible = false; // 패널 숨기기
            WindowsFormsHostControl.Visibility = Visibility.Hidden; // 패널 숨기기


            mainWindowViewModel.MpvControlInitealize(newPanel.Handle);

            ShowWindows();
            StartPipeServer();
        }

        private void MediaListControl_ButtonMouseDoubleClicked(object? sender, object e)
        {
            mainWindowViewModel.PreviewHight = 240;
            WindowsFormsHostControl.Child.Visible = true; // 패널 숨기기
            WindowsFormsHostControl.Visibility = Visibility.Visible; // 패널 숨기기
            mainWindowViewModel.Load();

        }

        private void MediaListControl_ButtonClicked(object? sender, object e)
        {
            if (mainWindowViewModel.PreviewHight == 0)
            {
                WindowsFormsHostControl.Child.Visible = true; // 패널 숨기기
                WindowsFormsHostControl.Visibility = Visibility.Visible; // 패널 숨기기
                mainWindowViewModel.PreviewHight = 240;
            }
            else
            {
                WindowsFormsHostControl.Child.Visible = false; // 패널 숨기기
                WindowsFormsHostControl.Visibility = Visibility.Hidden; // 패널 숨기기
                mainWindowViewModel.PreviewHight = 0;

            }
                
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

        private double sliderValue = 0;


        private void UpdateSliderValue(Slider slider, System.Windows.Input.MouseEventArgs e)
        {
            // Slider 내에서의 마우스 위치 (Horizontal Slider는 X 좌표 사용)
            System.Windows.Point pos = e.GetPosition(slider);
          
            // Slider의 전체 넓이에 대한 클릭 위치의 비율 계산
            double ratio = pos.X / slider.ActualWidth;

            // 최소값과 최대값 사이에 비율에 해당하는 값 계산
            double newValue = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;

            // 계산된 값 할당 (값 범위를 벗어나지 않도록 보정할 수도 있음)
            slider.Value = newValue;

            sliderValue = newValue;
        }

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Slider slider = sender as Slider;

            if (slider != null && _isDragging)
            {
                _isDragging = false;
                slider.ReleaseMouseCapture();
                e.Handled = true;
                mainWindowViewModel.MpvControlModel.MouseUp(sliderValue);
            }

        }

        private void Slider_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null && _isDragging)
            {
                UpdateSliderValue(slider, e);
                e.Handled = true;

                mainWindowViewModel.MpvControlModel.MouseUp(sliderValue);
            }
        }

        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider == null)
                return;

            _isDragging = true;
            // 마우스 캡처: 슬라이더 영역 외부로 마우스가 이동해도 이벤트를 받을 수 있음
            slider.CaptureMouse();
            UpdateSliderValue(slider, e);
            e.Handled = true;

            mainWindowViewModel.MpvControlModel.MouseDown(sliderValue);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ButtonBase;


            //return;

            switch (clickedButton.Name)
            {
                case "butFirst":
                    mainWindowViewModel.MpvControlModel.First();
                    break;
                case "butRew":
                    mainWindowViewModel.MpvControlModel.RW();
                    break;
                case "butEnd":
                    mainWindowViewModel.MpvControlModel.End();
                    break;
                case "butPlayStop":
                    mainWindowViewModel.MpvControlModel.Play();
                    break;
                case "butF1Frame":
                    mainWindowViewModel.MpvControlModel.Foraword1Frame();
                    break;

                case "butF10Frame":
                    mainWindowViewModel.MpvControlModel.Foraword10Frame();
                    break;
                case "butFF":
                    mainWindowViewModel.MpvControlModel.FF();
                    break;
                case "butB10Frame":
                    mainWindowViewModel.MpvControlModel.Back10Frame();
                    break;
                case "butB1Frame":
                    mainWindowViewModel.MpvControlModel.Back1Frame();
                    break;
                default:
                    break;
            }   
        }
    }
}