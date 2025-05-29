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

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class MainWindowsViewModel : INotifyPropertyChanged
    {
        private readonly MainWindow mainWindow;
        private MediaApiConnecter ApiConnecter { get; set; }
        public string MedaiUrl { get; set; } = "http://localhost:5050/api/MediaInfo";

        private List<VdcpServerViewModel> vdcpServerViewModels = null;
        private VdcpServerViewModel vdcpServerViewModel3 = null;
        private VdcpServerViewModel vdcpServerViewModel4 = null;
        private VdcpServerViewModel vdcpServerViewModel5 = null;
        private VdcpServerViewModel vdcpServerViewModel6 = null;
        private VdcpServerViewModel vdcpServerViewModel7 = null;
        private VdcpServerViewModel vdcpServerViewModel8 = null;
        private VdcpServerViewModel vdcpServerViewModel9 = null;

        private string bacePath { get; set; }
            = AppDomain.CurrentDomain.BaseDirectory;
            //System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VdcpService");

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

            if (args != null && args.Length > 1)
            {
                string jsonFromFile = string.Empty;
                string jsonpath = args[2];
                if (System.IO.File.Exists(jsonpath))
                    jsonFromFile = System.IO.File.ReadAllText(@jsonpath);

                if (jsonFromFile != string.Empty)
                {
                    var jObject = JObject.Parse(jsonFromFile);

                    var mediaUrl = jObject["ControlConfigData"]?["MediaViewSetting"]?["Url"];
                    MedaiUrl = mediaUrl?.ToString();
                }
            }

            SetMediaApi();

            ComPortTypeList = new ObservableCollection<string>()
            {
                "Udp",
                "Serial"
            };

            vdcpServerViewModels = new List<VdcpServerViewModel>();

            string json = System.IO.File.ReadAllText($"{bacePath}ComConfig.json");
            List<ComConfig> config = System.Text.Json.JsonSerializer.Deserialize<List<ComConfig>>(json);

            EnuPortType type = EnuPortType.Serial; // 기본값은 Serial로 설정
            
            if (SelectedComPort =="Udp")
                type = EnuPortType.Udp;
            

            foreach (var item in config)
            {
                switch (item.ComPort)
                {
                    case "COM3":
                        IsCom3 = item.IsEnabled;
                        if (IsCom3)
                        {
                            vdcpServerViewModel3 = new VdcpServerViewModel("ComPort3", "COM3");
                            vdcpServerViewModel3.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM3", 50000, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel3);
                        }
                        break;
                    case "COM4":
                        IsCom4 = item.IsEnabled;
                        if (IsCom4)
                        {
                            vdcpServerViewModel4 = new VdcpServerViewModel("ComPort4", "COM4");
                            vdcpServerViewModel4.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM4", 50001, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel4);
                        }
                        break;
                    case "COM5":
                        IsCom5 = item.IsEnabled;
                        if (IsCom5)
                        {
                            vdcpServerViewModel5 = new VdcpServerViewModel("ComPort5", "COM5");
                            vdcpServerViewModel5.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM5", 50002, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel5);
                        }
                        break;
                    case "COM6":
                        IsCom6 = item.IsEnabled;
                        if (IsCom6)
                        {
                            vdcpServerViewModel6 = new VdcpServerViewModel("ComPort6", "COM6");
                            vdcpServerViewModel6.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM6", 50003, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel6);
                        }
                        break;
                    case "COM7":
                        IsCom7 = item.IsEnabled;
                        if (IsCom7)
                        {
                            vdcpServerViewModel7 = new VdcpServerViewModel("ComPort7", "COM7");
                            vdcpServerViewModel7.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM7", 50004, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel7);
                        }
                        break;
                    case "COM8":
                        IsCom8 = item.IsEnabled;
                        if (IsCom8)
                        {
                            vdcpServerViewModel8 = new VdcpServerViewModel("ComPort8", "COM8");
                            vdcpServerViewModel8.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM8", 50005, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel8);
                        }
                        break;
                    case "COM9":
                        IsCom9 = item.IsEnabled;
                        if (IsCom9)
                        {
                            vdcpServerViewModel9 = new VdcpServerViewModel("ComPort9", "COM9");
                            vdcpServerViewModel9.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM9", 50006, true);
                            vdcpServerViewModels.Add(vdcpServerViewModel9);
                        }
                        break;
                }
            }

        }

        public void SetMediaApi()
        {
            ApiConnecter = new MediaApiConnecter("mediahub");
            ApiConnecter.IpAddress = MedaiUrl; // Set the IP address from the main window
            ApiConnecter.Connection();
            ApiConnecter.DoHubEventSend += ApiConnecter_DoHubEventSend1;
        }

        private void ApiConnecter_DoHubEventSend1(string type, string message)
        {
            
        }

        public async Task<bool> MediaSaveAsync(string item)
        {
            string query = $"MediaInfo/{item}";

            var response = await ApiConnecter.Client().GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var medias = JsonConvert.DeserializeObject<MediaDataInfo>(json);

                return medias != null ? true : false;
            }
            else return false ;
        }

        public async Task<List<MediaDataInfo>> GetMediaListAsync()
        {
            string query = $"MediaInfo";

            var response = await ApiConnecter.Client().GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var medias = JsonConvert.DeserializeObject<List<MediaDataInfo>>(json);

                return medias;
            }
            else return null;
               
        }

        public async void DeleteMedia(string item)
        {
            if (item == null) return;

            var response = await ApiConnecter.client.DeleteAsync($"MediaInfo/{item}");

            if (response.IsSuccessStatusCode)
            {
                //System.Windows.MessageBox.Show("Log created successfully!");
            }
            else
            {
                //System.Windows.MessageBox.Show("Failed to create log.");
            }
        }



        public void Start()
        {
            foreach(var item in vdcpServerViewModels)
            {
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
            // josn 파일로 저장하는 로직을 구현합니다.
            List<ComConfig> comConfig = new List<ComConfig>();

            comConfig.Add(new ComConfig { ComPort = "COM3", IsEnabled = IsCom3 });
            comConfig.Add(new ComConfig { ComPort = "COM4", IsEnabled = IsCom4 });
            comConfig.Add(new ComConfig { ComPort = "COM5", IsEnabled = IsCom5 });
            comConfig.Add(new ComConfig { ComPort = "COM6", IsEnabled = IsCom6 });
            comConfig.Add(new ComConfig { ComPort = "COM7", IsEnabled = IsCom7 });
            comConfig.Add(new ComConfig { ComPort = "COM8", IsEnabled = IsCom8 });
            comConfig.Add(new ComConfig { ComPort = "COM9", IsEnabled = IsCom9 });

            // JSON 직렬화
            string jsonString = System.Text.Json.JsonSerializer.Serialize(comConfig, new JsonSerializerOptions { WriteIndented = true });

            // 파일로 저장
            System.IO.File.WriteAllText($"{bacePath}ComConfig.json", jsonString);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
