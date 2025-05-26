using System;
using Media.Control.App.ManagerBa.View;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Media.Control.App.ManagerBa.Model.Config;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Windows.Interop;
using System.IO.Pipes;
using System.Text;
using Media.Control.Logger;
using System.Reflection;
using System.Windows.Controls.Primitives;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using NewTek.NDI;
using static MaterialDesignThemes.Wpf.Theme;

namespace Media.Control.App.ManagerBa.ViewModel
{
    public class ManagerBaViewModel : INotifyPropertyChanged
    {
        
        private readonly MainWindow _mainWindow;
        private SystemConfigData SystemConfig = null;
        private string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private List<string> ProgramLists = new List<string>();
        private int Top = 48;
        private List<Position> Positions = new List<Position>();
        private double mainBarWidth { get; set; }

        public double MainBarWidth
        {
            get => mainBarWidth;
            set
            {   
                mainBarWidth = value; OnPropertyChanged();
            }
        }

        private bool _isMinimized;

        public bool IsMinimized
        {
            get { return _isMinimized; }
            set
            {
                if (_isMinimized != value)
                {
                    _isMinimized = value;
                    OnPropertyChanged(nameof(IsMinimized));
                }
            }
        }


        private string minImage { get; set; } = "Pin";

        public string MinImage
        {
            get => minImage;
            set { minImage = value; OnPropertyChanged(nameof(MinImage)); }
        }

        private bool isMin { get; set; }= true;

        public bool IsMin 
        {   get => isMin; 
            set { isMin = value; OnPropertyChanged(nameof(IsMin)); }
        }

        private Logger.Logger logger = null;

        public System.Windows.Controls.Primitives.ToggleButton SelectChanel { get; set; }

        public ICommand Command_Close { get; }
        public ICommand Command_Minus { get; }

        public ICommand Command_ShowLogView {  get; }   

        public ICommand Comand_ShowConfiogView { get; }

        public ICommand Command_ShowMediaView { get; }

        public ICommand Command_AllRun { get; }

        public ICommand Command_AllKill { get; }

        public ICommand Command_Position { get; }

        public ICommand Command_ChannelRun { get; }

        private string systemPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Config";

        public ManagerBaViewModel(MainWindow mainWindow) 
        {
            _mainWindow = mainWindow;
            Command_Close = new RelayCommand(CommandClose);
            Command_Minus = new RelayCommand(CommandMinus);
            Command_ShowLogView = new RelayCommand(CommandShowLogView);
            Comand_ShowConfiogView = new RelayCommand(CommandConfigView);
            Command_ShowMediaView = new RelayCommand(CommandShowMediaView);
            Command_AllRun = new RelayCommand(CommandAllRun);
            Command_AllKill = new RelayCommand(CommandAllKill);
            Command_Position = new RelayCommand(CommandPosition);
            Command_ChannelRun = new RelayCommand(CommandChannelRun);
            GetSystemConfig();

            Positions = new List<Position>();

            logger = new Logger.Logger();
            logger.ConnectHub();

            StartPipeServer();
        }
        private void CommandChannelRun(object? obj)
        {
            if (SelectChanel == null) return;
            
            var channel = SelectChanel.Tag.ToString();


#if DEBUG
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.RP\bin\Debug\net8.0-windows\Media.Control.App.RP.exe";
#else
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.RP\bin\Release\net8.0-windows\Media.Control.App.RP.exe";
#endif


            GetSystemConfig();

            int channelCount = 0;

            string inPutList = string.Empty;
            foreach (var item in SystemConfig.ChannelConfigData.InPutList)
            {
                inPutList += $",{item.InputName}";
            }

            if (inPutList != string.Empty)
                inPutList = inPutList.Substring(1);

            var jsonFromFile = File.ReadAllText($@"{systemPath}\SystemConfig.json");

            string  gangPort = string.Empty;
            foreach (ChannelConfig item in SystemConfig.ChannelConfigData.ChannelList)
            {
                if (item.Channel.ToString() == channel)
                {
                    if (item.ChannelType == Model.EnuChannelType.Recoder)
                    {
                        gangPort = SystemConfig.ControlConfigData.RecorderSetting.GangPort.ToString();
                    }
                    else
                        gangPort = SystemConfig.ControlConfigData.PlayerSetting.GangPort.ToString();

                    switch (channel)
                    {
                        case "Channel1": channelCount = 0; break;
                        case "Channel2": channelCount = 1; break;
                        case "Channel3": channelCount = 2; break;
                        case "Channel4": channelCount = 3; break;
                    }

                    double left = (450 + 2) * channelCount;

                    string argList = $"Create " +
                        $"{item.Channel} " +
                        $"{left} " +
                        $"{Top} " +
                        $"{$@"{systemPath}\SystemConfig.json"}";


                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = programName,
                        Arguments = argList,
                        Verb = "runas",  // 관리자 권한 실행
                        UseShellExecute = false
                    };

                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {item.Channel} Send Create", "Arguments = {argList}");

                    Positions.Where(c => c.Name == item.Channel.ToString()).ToList().ForEach(c => Positions.Remove(c));

                    Positions.Add(new Position
                    {
                        Name = item.Channel.ToString(),
                        Left = left,
                        Top = Top,
                        Height = 1000,
                    });

                    try
                    {
                        Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Failed to start process: {ex.Message} ");
                    }


                    break;
                }
            }

            _mainWindow.butAllRun.IsEnabled = false;
        }

        private void SendLog(LogType type, string title, string message = "")
        {
            logger?.Log(type.ToString(), "ManagerBa", title, message);
        }

        private void CommandAllKill(object? obj)
        {
           if( System.Windows.MessageBox.Show($"Forced to shut down all channels."
                    , "Info", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK)
           {
                List<Position> processs = new List<Position>();

                foreach (var item in Positions)
                {

                    try
                    {
                        using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", item.Name, PipeDirection.Out))
                        {
                            pipeClient.Connect(100);

                            using (StreamWriter writer = new StreamWriter(pipeClient, Encoding.UTF8))
                            {
                                writer.AutoFlush = true;
                                writer.Write("Close");
                            }
                        }

                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} Send Close");

                        processs.Add(item);
                    }
                    catch { }
                }

                foreach (var item in processs)
                {
                    Positions.Remove(item);
                }


                foreach (var button in _mainWindow._toggleButtons)
                {
                    button.IsEnabled = true;
                    button.IsChecked = false;
                }

                _mainWindow.butAllRun.IsEnabled = true;
                
            }
        }

        private void GetSystemConfig()
        {
            string jsonFromFile = string.Empty;
            if (File.Exists($@"{systemPath}\SystemConfig.json"))
                jsonFromFile = File.ReadAllText($@"{systemPath}\SystemConfig.json");

            if (jsonFromFile != string.Empty)
            {
                SystemConfig = null;
                SystemConfig = new SystemConfigData();
                SystemConfig = JsonConvert.DeserializeObject<SystemConfigData>(jsonFromFile);
            }

        }



        private void CommandAllRun(object? obj)
        {
            string argList = string.Empty;
            //  string programName = System.IO.Path.Combine(basePath, "RPControl", "Media.Control.App.RP.exe");



#if DEBUG
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.RP\bin\Debug\net8.0-windows\Media.Control.App.RP.exe";
#else
            string programName = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.RP\bin\Release\net8.0-windows\Media.Control.App.RP.exe";
#endif

            GetSystemConfig();

            int channelCount = 0;
           
            string inPutList = string.Empty;
            foreach (var item in SystemConfig.ChannelConfigData.InPutList)
            {
                inPutList += $",{item.InputName}";
            }

            if (inPutList != string.Empty)
                inPutList = inPutList.Substring(1);

            var jsonFromFile = File.ReadAllText($@"{systemPath}\SystemConfig.json");

            foreach (ChannelConfig channel in  SystemConfig.ChannelConfigData.ChannelList)
            {
                string gangPort = string.Empty;
             

                if (channel.ChannelType == Model.EnuChannelType.Recoder)
                {
                    gangPort = SystemConfig.ControlConfigData.RecorderSetting.GangPort.ToString();
                }
                else
                    gangPort = SystemConfig.ControlConfigData.PlayerSetting.GangPort.ToString();

                double left = (450 + 2) * channelCount;
                
                argList = $"Create " +
                    $"{channel.Channel} " +
                    $"{left} " +
                    $"{Top} " +
                    $"{$@"{systemPath}\SystemConfig.json"}";

                ++channelCount;

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = argList,
                    Verb = "runas",  // 관리자 권한 실행
                    UseShellExecute = false
                };

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {channel.Channel} Send Run", $"Arguments = {argList}");

                Positions.Where(c => c.Name == channel.Channel.ToString()).ToList().ForEach(c => Positions.Remove(c));

                Positions.Add(new Position
                {
                    Name = channel.Channel.ToString(),
                    Left = left,
                    Top = Top,
                    Height = 1000,
                });

                try
                {
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Failed to start process: { ex.Message} ");
                }
                
                foreach (var button in _mainWindow._toggleButtons)
                {
                    button.IsEnabled = false;
                    button.IsChecked = true;
                }

                _mainWindow.butAllRun.IsEnabled = false;

                Thread.Sleep(100);
                //Debug.WriteLine(argList);
            }
        }

        private void CommandPosition(object? obj)
        {
            string json = JsonConvert.SerializeObject(Positions, Formatting.Indented);

            foreach(var item in Positions)
            {
                try
                {
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", item.Name, PipeDirection.Out))
                    {
                        pipeClient.Connect(100);

                        using (StreamWriter writer = new StreamWriter(pipeClient, Encoding.UTF8))
                        {
                            writer.AutoFlush = true;
                            writer.WriteLine(json);
                        }

                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} Send Position " , "Json Data = {json}");
                    }
                }
                catch (TimeoutException)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 연결 실패");
                }
                catch (IOException ioEx)
                {
                    
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 오류: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 예외: {ex.Message}");
                    
                }
            }
        }

        private void CommandShowLogView(object? obj)
        {
            // 현재 창이 위치한 모니터 정보 가져오기
            var handle = new WindowInteropHelper(_mainWindow).Handle;
            // 창이 위치한 모니터 얻기
            var screen = Screen.FromHandle(handle);
            // 작업 표시줄을 제외한 사용 가능한 우측 상단에 위치
            var workingArea = screen.WorkingArea;

            int top = workingArea.Bottom - 400;
            double left = workingArea.Right - 750;

            string argList = $"Create " +
               $"{left} " +
               $"{top} " +
               $"{SystemConfig.ChannelConfigData.ChannelList.Count} " +
               $"{$@"{systemPath}\SystemConfig.json"}";



#if DEBUG
            string path = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.LogView\bin\Debug\net8.0-windows\Media.Control.App.LogView.exe";
#else
            string path = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.LogView\bin\Release\net8.0-windows\Media.Control.App.LogView.exe";
#endif

            //string path = System.IO.Path.Combine(basePath, "LogView", "Media.Control.App.LogView.exe");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = argList,
                Verb = "runas",  // 관리자 권한 실행
                UseShellExecute = false
            };

            Positions.Where(c => c.Name == "LogView").ToList().ForEach(c => Positions.Remove(c));

            Positions.Add(new Position
            {
                Name = "LogView",
                Left = left,
                Top = top,
                Height = 300,
            });

            bool result = IsProgramRunningExact(path); // 실행하려는 프로그램 이름

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] LogView Send Create" ,"Arguments = ({argList})");

            if (!result)
            {
                try
                {
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to start process: " + ex.Message);
                }

            }
            else
            {
                SendLog(LogType.Warning, $"[{MethodBase.GetCurrentMethod().Name}] Media.Control.App.LogView 이미 실행 중입니다.");
                System.Windows.MessageBox.Show($"Media.Control.App.LogView 이미 실행 중입니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }


        private void CommandShowMediaView(object? obj)
        {
            // 현재 창이 위치한 모니터 정보 가져오기
            var handle = new WindowInteropHelper(_mainWindow).Handle;
            // 창이 위치한 모니터 얻기
            var screen = Screen.FromHandle(handle);
            // 작업 표시줄을 제외한 사용 가능한 우측 상단에 위치
            var workingArea = screen.WorkingArea;
            
            
            double left = workingArea.Right - 750;

            string argList = $"Create " +
                  $"{left} " +
                  $"{Top} " +                
                  $"{$@"{systemPath}\SystemConfig.json"}";

            // string path = System.IO.Path.Combine(basePath, "MediaBrowser", "Media.Control.App.MeidaBrowser.exe");

//#if DEBUG
//           string path = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.MeidaBrowser\bin\Debug\net8.0-windows\Media.Control.App.MeidaBrowser.exe";
//#else
//            string path = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.MeidaBrowser\bin\Release\net8.0-windows\Media.Control.App.MeidaBrowser.exe";
//#endif


            string path = @"C:\Users\Leejunghee\source\repos\Media.Control.App.Projects\Media.Control.App.Projects\Media.Control.App.MeidaBrowser\bin\Debug\net8.0-windows\Media.Control.App.MeidaBrowser.exe";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = argList,
                Verb = "runas",  // 관리자 권한 실행
                UseShellExecute = false
            };

            Positions.Where(c => c.Name == "MediaBrowser").ToList().ForEach(c => Positions.Remove(c));

            Positions.Add(new Position
            {
                Name = "MediaBrowser",
                Left = left,
                Top = Top,
                Height = 300,
            });

            bool result = IsProgramRunningExact(path); // 실행하려는 프로그램 이름

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] MediaBrowser Send Create", "Arguments = {argList}");
            if (!result)
            {
                try
                {
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name} Failed to start process: {ex.Message} ");
                }

            }
            else
            {
                SendLog(LogType.Warning, $"[{MethodBase.GetCurrentMethod().Name}] Media.Control.App.MeidaBrowser 이미 실행 중입니다.");
                System.Windows.MessageBox.Show($"Media.Control.App.MeidaBrowser 이미 실행 중입니다.", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CommandConfigView(object? obj)
        {
            ConfigWindowView configWindow = new ConfigWindowView();
            configWindow.ShowDialog();
        }
     
    
        private void CommandMinus(object? obj)
        {
            
        }

        private void CommandClose(object? obj)
        {
            foreach (var item in Positions)
            {
                try
                {
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", item.Name, PipeDirection.Out))
                    {
                        // ⏱️ 최대 100ms까지 기다림
                        pipeClient.Connect(100);

                        using (StreamWriter writer = new StreamWriter(pipeClient, Encoding.UTF8))
                        {
                            writer.AutoFlush = true;
                            writer.Write("AllClose");
                        }

                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} Send AllClose");
                    }
                }
                catch (TimeoutException)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 연결 실패");
                }
                catch (IOException ioEx)
                {

                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 오류: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {item.Name} pipe 예외: {ex.Message}");

                }
            }

            Thread.Sleep(300);

            _mainWindow.Close();   
        }

        public static bool IsProgramRunningExact(string fullProgramPath)
        {
            string targetExe = Path.GetFileNameWithoutExtension(fullProgramPath);
            string normalizedPath = Path.GetFullPath(fullProgramPath).ToLowerInvariant();

            try
            {
                return Process.GetProcessesByName(targetExe).Any(p =>
                {
                    try
                    {
                        string processPath = p.MainModule.FileName;
                        return string.Equals(
                            Path.GetFullPath(processPath).ToLowerInvariant(),
                            normalizedPath,
                            StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        // 접근 불가한 프로세스는 무시
                        return false;
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        private async Task HandleClientAsync(NamedPipeServerStream pipe)
        {
            using (pipe)
            using (var reader = new StreamReader(pipe))
            {
                try
                {
                    string message = await reader.ReadToEndAsync(); // 전체 메시지 수신

                    // Dispatcher를 _mainWindow.Dispatcher로 수정하여 인스턴스 참조를 사용
                    await _mainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        string[] parts = message.Split('/');

                        _mainWindow._toggleButtons.Where(c => c.Tag.ToString() == parts[1]).ToList().ForEach(c => c.IsChecked = false);
                        _mainWindow._toggleButtons.Where(c => c.Tag.ToString() == parts[1]).ToList().ForEach(c => c.IsEnabled = true);


                    });
                }
                catch (IOException ex)
                {
                    // 읽기 실패 예외 처리
                }
            }
        }

        private void StartPipeServer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var server = new NamedPipeServerStream(
                        "ManagerBa",
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
