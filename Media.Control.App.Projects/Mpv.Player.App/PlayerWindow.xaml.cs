using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Mpv.Player.App.ViewModel;
using Media.Control.App.Model;
using Newtonsoft.Json;

namespace Mpv.Player.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private PlayerViewModel mainWindowViewModel = null; 
        private bool _isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new PlayerViewModel(this);

            this.DataContext = mainWindowViewModel;

             //Debugger.Launch();

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                mainWindowViewModel.MediaId = args[1];

                if (!string.IsNullOrEmpty(args[2]) &&  File.Exists(args[2]))
                {
                    mainWindowViewModel.Load(args[2]);
                }

                mainWindowViewModel.DetectTaskFileName = args[3];
            }
        }

        public MainWindow(string inputData = null)
        {
            InitializeComponent();

            //System.Diagnostics.Debugger.Launch();

            mainWindowViewModel = new PlayerViewModel(this);
            this.DataContext = mainWindowViewModel;

            
            if (inputData != null)
            {
                mainWindowViewModel.Load(inputData);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            //{
            //   // mainWindowViewModel.SetWindowsHandle(this.PlayerHostPanel.Handle);
            //}));
        }


        private void PlayerHostPanel_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                mainWindowViewModel.Load(files[0]);
            }
            else if (e.Data.GetDataPresent(typeof(MediaDataInfo)))
            {
                MediaDataInfo mediaData = (MediaDataInfo)e.Data.GetData(typeof(MediaDataInfo));
                mainWindowViewModel.Load(mediaData.Path);
            }
            else if (e.Data.GetDataPresent(typeof(string)))
            {
                MediaDataInfo droppedData = null;

                string dataStr = e.Data.GetData(typeof(string)) as string;
                var jsonData = JsonConvert.DeserializeObject<MediaDataInfo2>(dataStr);
                
                mainWindowViewModel.Load(jsonData.Path);
            }

        }

        private void PlayerHostPanel_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MediaDataInfo)) 
                || e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) 
                ||(e.Data.GetDataPresent(typeof(string)))) 
            {
                e.Effect = System.Windows.Forms.DragDropEffects.Copy;
            }
            else
            {
                e.Effect = System.Windows.Forms.DragDropEffects.None;
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        private void PlayerHostPanel_Drop(object sender, System.Windows.DragEventArgs e)
        {
         

        }

      

        private void ValidateTimecode(System.Windows.Controls.TextBox textBox)
        {
            string[] parts = textBox.Text.Split(':');

            for (int i = 0; i < parts.Length; i++)
            {
                if (!Regex.IsMatch(parts[i], @"^\d{2}$"))
                {
                    parts[i] = "00"; // 잘못된 부분 초기화
                }
            }

            textBox.Text = string.Join(":", parts);
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

        }

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Slider slider = sender as Slider;

            if (slider != null && _isDragging)
            {
                _isDragging = false;
                slider.ReleaseMouseCapture();
                e.Handled = true;
                mainWindowViewModel.MouseUp(sliderValue);
            }

        }

        private void Slider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null && _isDragging)
            {
                UpdateSliderValue(slider, e);
                e.Handled = true;

                mainWindowViewModel.MouseUp(sliderValue);
            }

            //
        }

        private double sliderValue = 0;

        private void UpdateSliderValue(Slider slider, MouseEventArgs e)
        {
            // Slider 내에서의 마우스 위치 (Horizontal Slider는 X 좌표 사용)
            Point pos = e.GetPosition(slider);

            // Slider의 전체 넓이에 대한 클릭 위치의 비율 계산
            double ratio = pos.X / slider.ActualWidth;

            // 최소값과 최대값 사이에 비율에 해당하는 값 계산
            double newValue = slider.Minimum + (slider.Maximum - slider.Minimum) * ratio;

            // 계산된 값 할당 (값 범위를 벗어나지 않도록 보정할 수도 있음)
            slider.Value = newValue;

            sliderValue = newValue;
        }
    }

}