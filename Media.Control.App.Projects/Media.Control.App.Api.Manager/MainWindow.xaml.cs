using System;
using System.Drawing;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Media.Control.App.Api.Manager.ViewModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Interop;

namespace Media.Control.App.Api.Manager
{
    public partial class MainWindow : Window
    {
        // Windows API 구조체 및 함수
        [StructLayout(LayoutKind.Sequential)]
        private struct NOTIFYICONIDENTIFIER
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uID;
            public Guid guidItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

       // private string jpgFilePath = @$"{}"; // 아이콘 파일 경로

        private MainWindowsViewModel mainWindowViewModel = null;
        private IHost _webHost; // ASP.NET Core 호스트 인스턴스
        private NotifyIcon _notifyIcon; // 트레이 아이콘

        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new MainWindowsViewModel(this);
            this.DataContext = mainWindowViewModel;

            StopServiceButton.IsEnabled = false; // 초기화 시 종료 버튼 비활성화

            this.WindowState = WindowState.Minimized;
            
            InitializeTrayIcon();

            StartService();
        }

        private Icon ConvertJpgToIcon()
        {
            using (Bitmap bitmap = new Bitmap(""))
            {
                IntPtr hIcon = bitmap.GetHicon(); // Bitmap을 Icon 핸들로 변환
                return System.Drawing.Icon.FromHandle(hIcon);
            }

            return null;
        }

        private Icon ConvertByteArrayToIcon(byte[] iconData)
        {
            using (var ms = new MemoryStream(iconData))
            {
                return new Icon(ms);
            }
        }

        private void InitializeTrayIcon()
        {
            // 트레이 아이콘 설정
            _notifyIcon = new NotifyIcon
            {
                Icon = ConvertByteArrayToIcon(Properties.Resources.AdonisJs), // byte[]를 Icon으로 변환
                Visible = true,
                Text = "Media Control App"
            };

            // 트레이 아이콘 메뉴 설정
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add("Show", null, (s, e) => ShowFormAtTrayIcon());
            contextMenu.Items.Add("Hide", null, (s, e) => WindowsMinimized());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            // NotifyIcon에 ContextMenuStrip 설정
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 트레이 아이콘 더블 클릭 이벤트
            _notifyIcon.DoubleClick += (s, e) => ShowFormAtTrayIcon();

            HideWindow();
        }

        private void WindowsMinimized()
        {
            this.WindowState = WindowState.Minimized;
        }

        private void HideWindow()
        {
            this.Visibility = Visibility.Hidden;
        }

        private void ExitApplication()
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to exit...?",
               "Information",
               MessageBoxButton.YesNo,
               MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                StopService();

                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void ShowFormAtTrayIcon()
        {
            // 화면의 전체 해상도 가져오기
            var screenWidth = SystemParameters.PrimaryScreenWidth;  // 화면 너비
            var screenHeight = SystemParameters.PrimaryScreenHeight; // 화면 높이

            // 창의 너비와 높이를 가져와서 우측 하단 좌표를 계산
            double windowWidth = this.Width;
            double windowHeight = this.Height;

            // 창의 좌표 설정: 우측 하단
            this.Left = screenWidth - 350 - 3;
            this.Top = screenHeight - 180 - 3;

            // 창을 표시
            this.Show();
            this.WindowState = WindowState.Normal;

            // 창을 최상단에 표시 (옵션)
            this.Topmost = true;

            // 창 활성화
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetForegroundWindow(hwnd);
        }


        public async void StartService()
        {
            try
            {
                if (_webHost == null)
                {
                    // ASP.NET Core 호스트 빌드
                    _webHost = CreateHostBuilder().Build();
                }

                if (_webHost != null)
                {
                    // ASP.NET Core 서비스 시작
                    await _webHost.StartAsync();
                    StartServiceButton.IsEnabled = false;  // 시작 버튼 비활성화
                    StopServiceButton.IsEnabled = true;   // 종료 버튼 활성화
                    mainWindowViewModel.IsIndeterminate = true;
                    mainWindowViewModel.ApiMessage = "Media Service Api Connected ...";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"서비스 시작 중 오류가 발생했습니다: {ex.Message}", "오류");
            }
        }

        private async void StartServiceButton_Click(object sender, RoutedEventArgs e)
        {

           StartService();

            #region
            //try
            //{
            //    if (_webHost == null)
            //    {
            //        // ASP.NET Core 호스트 빌드
            //        _webHost = CreateHostBuilder().Build();
            //    }

            //    if (_webHost != null)
            //    {
            //        // ASP.NET Core 서비스 시작
            //        await _webHost.StartAsync();
            //        StartServiceButton.IsEnabled = false;  // 시작 버튼 비활성화
            //        StopServiceButton.IsEnabled = true;   // 종료 버튼 활성화
            //        mainWindowViewModel.IsIndeterminate = true;
            //        mainWindowViewModel.ApiMessage = "Media Service Api Connected ...";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show($"서비스 시작 중 오류가 발생했습니다: {ex.Message}", "오류");
            //}
            #endregion
        }

        private async void StopServiceButton_Click(object sender, RoutedEventArgs e)
        {
            StopService();
        }

        private async void StopService()
        {
            try
            {
                if (_webHost != null)
                {
                    // ASP.NET Core 서비스 종료
                    await _webHost.StopAsync();
                    _webHost.Dispose();
                    _webHost = null;

                    StartServiceButton.IsEnabled = true;  // 시작 버튼 활성화
                    StopServiceButton.IsEnabled = false; // 종료 버튼 비활성화
                    mainWindowViewModel.IsIndeterminate = false;
                    mainWindowViewModel.ApiMessage = "Media Service Api DisConnected ...";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"서비스 종료 중 오류가 발생했습니다: {ex.Message}", "오류");
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(); // 콘솔에 로그 출력
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("https://localhost:5050"); // 서비스 URL
                    webBuilder.UseStartup<Startup>(); // Startup 클래스 지정
                });
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                HideWindow();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true; // 창 닫기를 취소하고 숨김
            HideWindow();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
