using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.Model;
using System.Text.Json;
using System.Collections.ObjectModel;
using VdcpService.lib;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.DirectoryServices.ActiveDirectory;
using System.Collections;



namespace Vdcp.Service.App.Manager.ViewModel
{
    public class MainWindowsViewModel : INotifyPropertyChanged
    {
        private readonly MainWindow mainWindow;
        private MediaApiConnecter MediaConnecter { get; set; } = null;

        public LoggerApiConnecter LoggerConnecter { get; set; } = null;

        public string MedaiUrl { get; set; } = "http://localhost:5050/api/MediaInfo";
        public string LoggerUrl { get; set; } = "http://localhost:5050/api/Logger";

        private List<VdcpServerViewModel> vdcpServerViewModels = null;
        private VdcpServerViewModel vdcpServerViewModel3 = null;
        private VdcpServerViewModel vdcpServerViewModel4 = null;
        private VdcpServerViewModel vdcpServerViewModel5 = null;
        private VdcpServerViewModel vdcpServerViewModel6 = null;
        private VdcpServerViewModel vdcpServerViewModel7 = null;
        private VdcpServerViewModel vdcpServerViewModel8 = null;
        private VdcpServerViewModel vdcpServerViewModel9 = null;

        private List<string> ClipList { get; set; } = new List<string>();

        private List<MediaDataInfo> MediaDataInfos { get; set; }

        private string bacePath { get; set; }
            = AppDomain.CurrentDomain.BaseDirectory;
        //System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VdcpService");
        private ObservableCollection<PortDataInfo> PortDataInfoList;
        

        public bool isServiceRunning { get; set; } = false;

        private bool _IsEnabledCom { get; set; } = true;
        public bool IsEnabledCom
        {
            get { return _IsEnabledCom; }
            set { _IsEnabledCom = value; OnPropertyChanged(nameof(IsEnabledCom)); }
        }

        private bool _IsCom3 { get; set; } = false;
        public bool IsCom3
        {
            get { return _IsCom3; }
            set { _IsCom3 = value; OnPropertyChanged(nameof(IsCom3)); }
        }

        private bool _IsCom4 { get; set; } = false;
        public bool IsCom4
        {
            get { return _IsCom4; }
            set { _IsCom4 = value; OnPropertyChanged(nameof(IsCom4)); }
        }

        private bool _IsCom5 { get; set; } = false;
        public bool IsCom5
        {
            get { return _IsCom5; }
            set { _IsCom5 = value; OnPropertyChanged(nameof(IsCom5)); }
        }

        private bool _IsCom6 { get; set; } = false;
        public bool IsCom6
        {
            get { return _IsCom6; }
            set { _IsCom6 = value; OnPropertyChanged(nameof(IsCom6)); }
        }
        private bool _IsCom7 { get; set; } = false;
        public bool IsCom7
        {
            get { return _IsCom7; }
            set { _IsCom7 = value; OnPropertyChanged(nameof(IsCom7)); }
        }
        private bool _IsCom8 { get; set; } = false;
        public bool IsCom8
        {
            get { return _IsCom8; }
            set { _IsCom8 = value; OnPropertyChanged(nameof(IsCom8)); }
        }
        private bool _IsCom9 { get; set; } = false;
        public bool IsCom9
        {
            get { return _IsCom9; }
            set { _IsCom9 = value; OnPropertyChanged(nameof(IsCom9)); }
        }

        private string apiMessage { get; set; } = "Vdcp Service Stop...";

        public string ApiMessage
        {
            get { return apiMessage; }
            set { apiMessage = value; OnPropertyChanged(nameof(ApiMessage)); }
        }

        private bool isIndeterminate { get; set; } = false;

        public bool IsIndeterminate
        {
            get => isIndeterminate;
            set { isIndeterminate = value; OnPropertyChanged(nameof(IsIndeterminate)); }
        }

        private string _selectedComPort { get; set; } = "Udp";
        public string SelectedComPort
        {
            get { return _selectedComPort; }
            set
            {
                _selectedComPort = value;
                OnPropertyChanged(nameof(SelectedComPort));
            }
        }


        private string _IpAddress { get; set; } = "127.0.0.1";

        public string IpAddress
        {
            get { return _IpAddress; }
            set
            {
                _IpAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }

        public ObservableCollection<string> ComPortTypeList { get; set; }

        public MainWindowsViewModel(MainWindow window)
        {
            this.mainWindow = window;

            string[] args = Environment.GetCommandLineArgs();
            string ApiIpaddress = string.Empty;

            if (args != null && args.Length > 1)
            {
                string jsonFromFile = string.Empty;
                string jsonpath = args[2];
                if (System.IO.File.Exists(jsonpath))
                    jsonFromFile = System.IO.File.ReadAllText(@jsonpath);

                if (jsonFromFile != string.Empty)
                {
                    var jObject = JObject.Parse(jsonFromFile);

                    ApiIpaddress = (string)jObject["ControlConfigData"]?["MediaViewSetting"]?["Url"];
                }
            }

            MediaConnecter = new MediaApiConnecter("MediaInfo");
            MediaConnecter.IpAddress = ApiIpaddress;
            MediaConnecter.DoHubEventSend += MediaApiConnecter_DoHubEventSend;

            MediaConnecter.Connection();
            MediaConnecter.StartHub();

            GetMediaData();

            LoggerConnecter = new  LoggerApiConnecter(ApiIpaddress);
            LoggerConnecter.Connection();
            LoggerConnecter.ConnectHub();

            ComPortTypeList = new ObservableCollection<string>()
            {
                "Udp",
                "Serial"
            };

            vdcpServerViewModels = new List<VdcpServerViewModel>();

            PortDataInfoList = new ObservableCollection<PortDataInfo>();

            if(!File.Exists($"{bacePath}ComConfig.json"))
                return;


            string json = System.IO.File.ReadAllText($"{bacePath}AmppConfig.json");
            StaticSystemCofnig.AmppConfig = System.Text.Json.JsonSerializer.Deserialize<AmppConfig>(json);


            json = System.IO.File.ReadAllText($"{bacePath}ComConfig.json");
            PortDataInfoList = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PortDataInfo>>(json);

            EnuPortType type = EnuPortType.Serial; // 기본값은 Serial로 설정
            
            if (SelectedComPort =="Udp")
                type = EnuPortType.Udp;


            foreach (var item in PortDataInfoList)
            {
                item.AmppConfig = StaticSystemCofnig.AmppConfig;

                switch (item.PortName)
                {
                    case "COM3":
                        IsCom3 = item.IsEnabled;
                        if (IsCom3)
                        {
                            vdcpServerViewModel3 = new VdcpServerViewModel(item);
                            vdcpServerViewModel3.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM3", 50000, true);
                            vdcpServerViewModel3.ClipList = ClipList;
                            vdcpServerViewModel3.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel3);

                            LoggerConnecter.Log("Info", "VdcpService",  $"COM3 Port Opened with {SelectedComPort} at {IpAddress} on port 50000", "");
                        }
                        break;
                    case "COM4":
                        IsCom4 = item.IsEnabled;
                        if (IsCom4)
                        {
                            vdcpServerViewModel4 = new VdcpServerViewModel(item);
                            

                            vdcpServerViewModel4.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM4", 50001, true);
                            vdcpServerViewModel4.ClipList = ClipList;
                            vdcpServerViewModel4.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel4);
                            LoggerConnecter.Log("Info", "VdcpService", $"COM4 Port Opened with {SelectedComPort} at {IpAddress} on port 50001", "");
                        }
                        break;
                    case "COM5":
                        IsCom5 = item.IsEnabled;
                        if (IsCom5)
                        {
                            vdcpServerViewModel5 = new VdcpServerViewModel(item);
                            vdcpServerViewModel5.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM5", 50002, true);
                            vdcpServerViewModel5.ClipList = ClipList;
                            vdcpServerViewModel5.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel5);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM5 Port Opened with {SelectedComPort} at {IpAddress} on port 50002", "");
                        }
                        break;
                    case "COM6":
                        IsCom6 = item.IsEnabled;
                        if (IsCom6)
                        {
                            vdcpServerViewModel6 = new VdcpServerViewModel(item);
                            vdcpServerViewModel6.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM6", 50003, true);
                            vdcpServerViewModel6.ClipList = ClipList;
                            vdcpServerViewModel6.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel6);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM6 Port Opened with {SelectedComPort} at {IpAddress} on port 50003", "");
                        }
                        break;
                    case "COM7":
                        IsCom7 = item.IsEnabled;
                        if (IsCom7)
                        {
                            vdcpServerViewModel7 = new VdcpServerViewModel(item);
                            vdcpServerViewModel7.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM7", 50004, true);
                            vdcpServerViewModel7.ClipList = ClipList;
                            vdcpServerViewModel7.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel7);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM7 Port Opened with {SelectedComPort} at {IpAddress} on port 50004", "");
                        }
                        break;
                    case "COM8":
                        IsCom8 = item.IsEnabled;
                        if (IsCom8)
                        {
                            vdcpServerViewModel8 = new VdcpServerViewModel(item);
                            vdcpServerViewModel8.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM8", 50005, true);
                            vdcpServerViewModel8.ClipList = ClipList;
                            vdcpServerViewModel8.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel8);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM8 Port Opened with {SelectedComPort} at {IpAddress} on port 50005", "");
                        }
                        break;
                    case "COM9":
                        IsCom9 = item.IsEnabled;
                        if (IsCom9)
                        {
                            vdcpServerViewModel9 = new VdcpServerViewModel(item);
                            vdcpServerViewModel9.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM9", 50006, true);
                            vdcpServerViewModel9.ClipList = ClipList;
                            vdcpServerViewModel9.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel9);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM9 Port Opened with {SelectedComPort} at {IpAddress} on port 50006", "");
                        }
                        break;
                }
            }

        }

        public string GetClipPath(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);

            return mediaData == null ? string.Empty : mediaData.Path;
        }


        public int GetFps(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
           
            return mediaData == null ? 0 : mediaData.Fps;
        }

        public string GetDuration(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);

            return mediaData == null ? "00:00:00:00" : mediaData.Duration;
        }


        public long GetFrame(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? 0 : mediaData.Frame;
        }

        public string GetSom(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? "00:00:00:00" : mediaData.InTimeCode;
        }

        public string GetEom(string clipName)
        {
            var mediaData = MediaDataInfos.FirstOrDefault(m => m.Name == clipName);
            return mediaData == null ? "00:00:00:00" : mediaData.OutTimeCode;
        }

        private async void GetMediaData()
        {
            string query = $"MediaInfo";

            var response = await MediaConnecter.Client().GetAsync(query);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var medias = JsonConvert.DeserializeObject<ObservableCollection<MediaDataInfo>>(json);

                ClipList.Clear();

                foreach (var item in medias.ToList())
                {
                    ClipList.Add(item.Name);
                }
                
                MediaDataInfos = medias.ToList();
            }
        }

        private void MediaApiConnecter_DoHubEventSend(string type, string message)
        {
            GetMediaData();

            foreach (var item in vdcpServerViewModels)
            {
                item.ClipList = ClipList;
            }
        }

        public void Start()
        {
            foreach(var item in vdcpServerViewModels)
            {
                if(item.WorkLoad1 != string.Empty)
                    item.Start();
            }
        }

        public void Close()
        {
            foreach(var item in vdcpServerViewModels)
            {
                item.Stop();
                item.Close();  
            }
        }

        public void SaveConfig()
        {

            if(File.Exists( $"{bacePath}ComConfig.json"))
            {
                string jsonString = System.IO.File.ReadAllText($"{bacePath}ComConfig.json");
                PortDataInfoList = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PortDataInfo>>(jsonString);

            }

            //// josn 파일로 저장하는 로직을 구현합니다.
            //List<ComConfig> comConfig = new List<ComConfig>();

            //comConfig.Add(new ComConfig { ComPort = "COM3", IsEnabled = IsCom3 });
            //comConfig.Add(new ComConfig { ComPort = "COM4", IsEnabled = IsCom4 });
            //comConfig.Add(new ComConfig { ComPort = "COM5", IsEnabled = IsCom5 });
            //comConfig.Add(new ComConfig { ComPort = "COM6", IsEnabled = IsCom6 });
            //comConfig.Add(new ComConfig { ComPort = "COM7", IsEnabled = IsCom7 });
            //comConfig.Add(new ComConfig { ComPort = "COM8", IsEnabled = IsCom8 });
            //comConfig.Add(new ComConfig { ComPort = "COM9", IsEnabled = IsCom9 });

            //// JSON 직렬화
            //string jsonString = System.Text.Json.JsonSerializer.Serialize(comConfig, new JsonSerializerOptions { WriteIndented = true });

            //// 파일로 저장
            //System.IO.File.WriteAllText($"{bacePath}ComConfig.json", jsonString);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
