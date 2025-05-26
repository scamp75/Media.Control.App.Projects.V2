using System.Text;
using System.Windows;
using Media.Control.App.ManagerBa.ViewModel;
using System.Net.Http;
using Newtonsoft.Json;
using Media.Control.App.ManagerBa.Model.DB;
using System.Net.Http.Json;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;

//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Media.Control.App.ManagerBa
{
    public partial class MainWindow : Window
    {
    
        private ManagerBaViewModel mainWindowViewModel = null;
        private HttpClient _httpClient;
        private const double OffscreenTop = -500; // 창이 숨겨질 위치
        private const double OnscreenTop = 0;    // 창이 정상적으로 보이는 위치
        private DispatcherTimer mousePositionTimer;
        private bool isHidden = false;
        private bool isMouseInside = false; // 마우스가 폼 내부에 있는지 여부
                                            //  private bool isMin = false; 

        public List<System.Windows.Controls.Primitives.ToggleButton> _toggleButtons;
        public MainWindow()
        {
             InitializeComponent();

            mainWindowViewModel = new ManagerBaViewModel(this);
            
            this.DataContext = mainWindowViewModel;

            this.Width = SystemParameters.PrimaryScreenWidth;
            mainWindowViewModel.MainBarWidth = SystemParameters.PrimaryScreenWidth;
            
            _httpClient = new HttpClient();

            this.StateChanged += MainWindow_StateChanged;
            // 마우스 이벤트 등록
            this.MouseEnter += MainWindow_MouseEnter;
            this.MouseLeave += MainWindow_MouseLeave;

            mousePositionTimer = new DispatcherTimer();
            mousePositionTimer.Interval = TimeSpan.FromMilliseconds(100);
            mousePositionTimer.Tick += MousePositionTimer_Tick;
            mousePositionTimer.Start();


            _toggleButtons = new List<System.Windows.Controls.Primitives.ToggleButton>
            {
               butChanel1,
               butChanel2,
               butChanel3,
               butChanel4
            };

            try
            {

                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mca_log.png");
                BitmapImage image = new BitmapImage(new Uri(path, UriKind.Absolute));
                logImage.Source = image;


                //var uri = new Uri("pack://application:,,,/resources/mca_log.png", UriKind.Absolute);
                //var bmp = new BitmapImage(uri);
                //this.logImage.Source = bmp;  // your Image name
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("이미지 로딩 실패: " + ex.Message);
            }
        }

        private void MainWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mainWindowViewModel.IsMin)
            {
                isMouseInside = false;

                // 마우스가 밖으로 나가면 창을 숨김
                if (!isHidden)
                {
                    SlideUpAndHide();
                }
            }
        }

        private void MainWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isMouseInside = true;
        }

        private void MousePositionTimer_Tick(object? sender, EventArgs e)
        {
            if(!mainWindowViewModel.IsMin)
            {
                var mousePosition = System.Windows.Input.Mouse.GetPosition(this); // 마우스 위치 가져오기

                //var mousePosition = System.Windows.Forms.Control.MousePosition; // Get mouse position

                // Check if mouse is at the top of the screen
                if (mousePosition.Y <= 10 && isHidden)
                {
                    SlideDownAndShow();
                }

            }
        }
        private void SlideDownAndShow()
        {
            this.Show(); // 창 표시
            isHidden = false;

            // 윈도우를 화면 아래로 이동하는 애니메이션
            var slideDownAnimation = new DoubleAnimation
            {
                To = OnscreenTop, // 윈도우를 화면 아래로 이동
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            this.BeginAnimation(Window.TopProperty, slideDownAnimation);
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            // 최소화 상태 처리
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal; // 기본 최소화 동작 방지
                SlideUpAndHide();
            }
        }
        private void SlideUpAndHide()
        {
            // 윈도우를 화면 위로 이동하는 애니메이션
            var slideUpAnimation = new DoubleAnimation
            {
                To = OffscreenTop, // 윈도우를 화면 위로 이동
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            slideUpAnimation.Completed += (s, e) =>
            {
                this.Hide(); // 애니메이션 완료 후 창 숨기기
                isHidden = true;
            };

            this.BeginAnimation(Window.TopProperty, slideUpAnimation);
        }



        //private async void GetData(string channel, string createdate)
        //{
        //    var response = await _httpClient.GetStringAsync($"http://localhost:5000/?channel={channel}&createdate={createdate}");
        //    // ... 데이터 처리 코드 ...
        //}

        private async void PutData(string logData)
        {
            var json = JsonConvert.SerializeObject(logData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync("http://localhost:5100/", content);
            // ... 데이터 처리 코드 ...
        }

        private void DeleteData(string channel, string createdate)
        {
            //var logDataToRemove = _logDataList
            //    .FirstOrDefault(log => log.Channel == channel && log.CreateDate == createdate);

            //if (logDataToRemove != null)
            //{
            //    _logDataList.Remove(logDataToRemove);
            //}
        }


        private void butAllRun_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void butPosition_Click(object sender, RoutedEventArgs e)
        {

        }

        private void butMeidaview_Click(object sender, RoutedEventArgs e)
        {

        }

        
        private void butMin_Click(object sender, RoutedEventArgs e)
        {
            
            //if(!isMin)
            //{
            //    isMin = true;
            //    mainWindowViewModel.MinImage = "PinOff";
            //}
            //else
            //{

            //    isMin = false;
            //    mainWindowViewModel.MinImage = "Pin";
            //}
        }

       

        private void butChanel_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as System.Windows.Controls.Primitives.ToggleButton;
            
            if (clickedButton == null) return;

            mainWindowViewModel.SelectChanel = clickedButton;
            clickedButton.IsEnabled = false;

            // 선택된 버튼은 체크 유지, 다른 버튼 해제 방지
            //foreach (var button in _toggleButtons)
            //{
            //    if (button != clickedButton && button.IsChecked == true)
            //    {
            //        //button.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 55, 52));
            //    }
            //}
        }
    }
}