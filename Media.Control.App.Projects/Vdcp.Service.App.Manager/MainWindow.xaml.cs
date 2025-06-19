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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Interop;
using Vdcp.Service.App.Manager.ViewModel;
using Vdcp.Service.App.Manager.View;
using Media.Control.App.Api;


namespace Vdcp.Service.App.Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
   
        private MainWindowsViewModel mainWindowViewModel = null;
        private NotifyIcon _notifyIcon; // 트레이 아이콘

        private IHost _webHost; // ASP.NET Core 호스트 인스턴스

        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new MainWindowsViewModel(this);
            this.DataContext = mainWindowViewModel;

            StopServiceButton.IsEnabled = false; // 초기화 시 종료 버튼 비활성화
            StartService();
          //  this.WindowState = WindowState.Maximized;

            InitializeTrayIcon();


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
                Text = "Vdpc Service App..."
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
                mainWindowViewModel.Close();

                StopService();

                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private async void StopService()
        {
            try
            {
                if (_webHost != null )
                {
                    _webHost.StopAsync();
                    _webHost = null;
                }
                    
                StartServiceButton.IsEnabled = true;  // 시작 버튼 활성화
                StopServiceButton.IsEnabled = false; // 종료 버튼 비활성화
                mainWindowViewModel.IsIndeterminate = false;
                mainWindowViewModel.ApiMessage = "Vdcp Service Stop...";
                mainWindowViewModel.isServiceRunning = false;
                mainWindowViewModel.IsEnabledCom = true;
                
                 mainWindowViewModel.Close(); // ViewModel 종료
                //}
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"서비스 종료 중 오류가 발생했습니다: {ex.Message}", "오류");
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
            this.Left = screenWidth - 450 - 3;
            this.Top = screenHeight - 250 - 3;

            // 창을 표시
            this.Show();
            this.WindowState = WindowState.Normal;

            // 창을 최상단에 표시 (옵션)
            this.Topmost = true;

            // 창 활성화
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetForegroundWindow(hwnd);
        }

        private async void StartServiceButton_Click(object sender, RoutedEventArgs e)
        {
           
            StartService();
        }

        private async void StopServiceButton_Click(object sender, RoutedEventArgs e)
        {
            StopService();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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
                    await _webHost.StartAsync();

                await mainWindowViewModel.ConnectApi(); // API 연결 시작

                await mainWindowViewModel.CreatComPort(); // COM 포트 생성 시작

                StartServiceButton.IsEnabled = false;  // 시작 버튼 비활성화
                StopServiceButton.IsEnabled = true;   // 종료 버튼 활성화
                mainWindowViewModel.IsIndeterminate = true;
                mainWindowViewModel.ApiMessage = "Vdcp Service Start...";
                mainWindowViewModel.isServiceRunning = true;
                mainWindowViewModel.IsEnabledCom = false;
                mainWindowViewModel.Start();
                //mainWindowViewModel.SaveConfig();
                //}
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"서비스 시작 중 오류가 발생했습니다: {ex.Message}", "오류");
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
                    webBuilder.UseUrls("http://0.0.0.0:5050"); // 서비스 URL
                    webBuilder.UseStartup<Startup>(); // Startup 클래스 지정
                });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void butConfig_Click(object sender, RoutedEventArgs e)
        {
            var form = new ConfigWindwos();

            var screenWidth = SystemParameters.PrimaryScreenWidth;  // 화면 너비
            var screenHeight = SystemParameters.PrimaryScreenHeight; // 화면 높이

            // 창의 너비와 높이를 가져와서 우측 하단 좌표를 계산
            double windowWidth = this.Width;
            double windowHeight = this.Height;

            // 창의 좌표 설정: 우측 하단
            form.Left = screenWidth - 3 - form.Width;
            form.Top = screenHeight - 250 - 3 - form.Height;

            form.Show(); // 또는 ShowDialog() - 모달로 열기
        }
    }
}