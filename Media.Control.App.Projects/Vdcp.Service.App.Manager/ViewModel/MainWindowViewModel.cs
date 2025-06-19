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
using System.Net.Http;
using Vdcp.Service.App.Manager.Model;



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

        private EnuPortType PortType = EnuPortType.Serial; // 기본값은 Serial로 설정

        private MediaApiService mediaApiService = null;

        public List <string> ClipList { get; set; } = new List<string>();

        public List<MediaDataInfo> MediaDataInfos { get; set; }

        private MediaApiService MediaApiService { get; set; } = null;

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

                    IpAddress = (string)jObject["ControlConfigData"]?["MediaViewSetting"]?["Url"];
                }
            }

            mediaApiService = new MediaApiService();

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

            string jsonRecorde = System.IO.File.ReadAllText($"{bacePath}RecordeConfig.json");
            StaticSystemCofnig.RecordeConfig = System.Text.Json.JsonSerializer.Deserialize<RecordeConfig>(jsonRecorde);


            json = System.IO.File.ReadAllText($"{bacePath}ComConfig.json");
            PortDataInfoList = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PortDataInfo>>(json);

            if (SelectedComPort =="Udp")
                PortType = EnuPortType.Udp;

            #region
            //foreach (var item in PortDataInfoList)
            //{
            //    item.AmppConfig = StaticSystemCofnig.AmppConfig;

            //    switch (item.PortName)
            //    {
            //        case "COM3":
            //            IsCom3 = item.IsEnabled;
            //            if (IsCom3)
            //            {
            //                vdcpServerViewModel3 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel3.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM3", 50000, true);
            //                vdcpServerViewModel3.ClipList = ClipList;
            //                vdcpServerViewModel3.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel3);

            //                LoggerConnecter.Log("Info", "VdcpService",  $"COM3 Port Opened with {SelectedComPort} at {IpAddress} on port 50000", "");
            //            }
            //            break;
            //        case "COM4":
            //            IsCom4 = item.IsEnabled;
            //            if (IsCom4)
            //            {
            //                vdcpServerViewModel4 = new VdcpServerViewModel(item);


            //                vdcpServerViewModel4.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM4", 50001, true);
            //                vdcpServerViewModel4.ClipList = ClipList;
            //                vdcpServerViewModel4.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel4);
            //                LoggerConnecter.Log("Info", "VdcpService", $"COM4 Port Opened with {SelectedComPort} at {IpAddress} on port 50001", "");
            //            }
            //            break;
            //        case "COM5":
            //            IsCom5 = item.IsEnabled;
            //            if (IsCom5)
            //            {
            //                vdcpServerViewModel5 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel5.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM5", 50002, true);
            //                vdcpServerViewModel5.ClipList = ClipList;
            //                vdcpServerViewModel5.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel5);

            //                LoggerConnecter.Log("Info", "VdcpService", $"COM5 Port Opened with {SelectedComPort} at {IpAddress} on port 50002", "");
            //            }
            //            break;
            //        case "COM6":
            //            IsCom6 = item.IsEnabled;
            //            if (IsCom6)
            //            {
            //                vdcpServerViewModel6 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel6.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM6", 50003, true);
            //                vdcpServerViewModel6.ClipList = ClipList;
            //                vdcpServerViewModel6.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel6);

            //                LoggerConnecter.Log("Info", "VdcpService", $"COM6 Port Opened with {SelectedComPort} at {IpAddress} on port 50003", "");
            //            }
            //            break;
            //        case "COM7":
            //            IsCom7 = item.IsEnabled;
            //            if (IsCom7)
            //            {
            //                vdcpServerViewModel7 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel7.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM7", 50004, true);
            //                vdcpServerViewModel7.ClipList = ClipList;
            //                vdcpServerViewModel7.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel7);

            //                LoggerConnecter.Log("Info", "VdcpService", $"COM7 Port Opened with {SelectedComPort} at {IpAddress} on port 50004", "");
            //            }
            //            break;
            //        case "COM8":
            //            IsCom8 = item.IsEnabled;
            //            if (IsCom8)
            //            {
            //                vdcpServerViewModel8 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel8.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM8", 50005, true);
            //                vdcpServerViewModel8.ClipList = ClipList;
            //                vdcpServerViewModel8.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel8);

            //                LoggerConnecter.Log("Info", "VdcpService", $"COM8 Port Opened with {SelectedComPort} at {IpAddress} on port 50005", "");
            //            }
            //            break;
            //        case "COM9":
            //            IsCom9 = item.IsEnabled;
            //            if (IsCom9)
            //            {
            //                vdcpServerViewModel9 = new VdcpServerViewModel(item);
            //                vdcpServerViewModel9.Open(type, SelectedComPort == "Udp" ? IpAddress : "COM9", 50006, true);
            //                vdcpServerViewModel9.ClipList = ClipList;
            //                vdcpServerViewModel9.MainWindow = this;
            //                vdcpServerViewModels.Add(vdcpServerViewModel9);

            //                LoggerConnecter.Log("Info", "VdcpService", $"COM9 Port Opened with {SelectedComPort} at {IpAddress} on port 50006", "");
            //            }
            //            break;
            //    }
            //}
            #endregion

        }

        public async Task CreatComPort()
        {
            var mediaData  = await mediaApiService.GetMediaData();


            foreach (var item in mediaData)
            {
                if (!ClipList.Contains(item.Name))
                {
                    ClipList.Add(item.Name);
                }
            }

            MediaDataInfos = mediaData;

            foreach (var item in PortDataInfoList)
            {
                switch (item.PortName)
                {
                    case "COM3":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel3 = new VdcpServerViewModel(item);
                            vdcpServerViewModel3.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM3", 50000, true);
                            vdcpServerViewModel3.ClipList = ClipList;
                            vdcpServerViewModel3.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel3.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel3);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM3 Port Opened with {SelectedComPort} at {IpAddress} on port 50000", "");
                        }

                        break;
                    case "COM4":

                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel4 = new VdcpServerViewModel(item);
                            vdcpServerViewModel4.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM4", 50001, true);
                            vdcpServerViewModel4.ClipList = ClipList;
                            vdcpServerViewModel4.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel4.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel4);
                            LoggerConnecter.Log("Info", "VdcpService", $"COM4 Port Opened with {SelectedComPort} at {IpAddress} on port 50001", "");

                        }

                        break;
                    case "COM5":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel5 = new VdcpServerViewModel(item);
                            vdcpServerViewModel5.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM5", 50002, true);
                            vdcpServerViewModel5.ClipList = ClipList;
                            vdcpServerViewModel5.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel5.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel5);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM5 Port Opened with {SelectedComPort} at {IpAddress} on port 50002", "");
                        }
                        break;
                    case "COM6":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel6 = new VdcpServerViewModel(item);
                            vdcpServerViewModel6.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM6", 50003, true);
                            vdcpServerViewModel6.ClipList = ClipList;
                            vdcpServerViewModel6.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel6.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel6);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM6 Port Opened with {SelectedComPort} at {IpAddress} on port 50003", "");
                        }
                        
                        break;
                    case "COM7":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel7 = new VdcpServerViewModel(item);
                            vdcpServerViewModel7.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM7", 50004, true);
                            vdcpServerViewModel7.ClipList = ClipList;
                            vdcpServerViewModel7.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel7.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel7);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM7 Port Opened with {SelectedComPort} at {IpAddress} on port 50004", "");
                        }
                        
                        break;
                    case "COM8":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel8 = new VdcpServerViewModel(item);
                            vdcpServerViewModel8.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM8", 50005, true);
                            vdcpServerViewModel8.ClipList = ClipList;
                            vdcpServerViewModel8.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel8.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel8);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM8 Port Opened with {SelectedComPort} at {IpAddress} on port 50005", "");
                        }
                        break;
                    case "COM9":
                        if (item.SelectPort != 0)
                        {
                            vdcpServerViewModel9 = new VdcpServerViewModel(item);
                            vdcpServerViewModel9.Open(PortType, SelectedComPort == "Udp" ? IpAddress : "COM9", 50006, true);
                            vdcpServerViewModel9.ClipList = ClipList;
                            vdcpServerViewModel9.mediaDataInfos = MediaDataInfos;
                            vdcpServerViewModel9.MainWindow = this;
                            vdcpServerViewModels.Add(vdcpServerViewModel9);

                            LoggerConnecter.Log("Info", "VdcpService", $"COM9 Port Opened with {SelectedComPort} at {IpAddress} on port 50006", "");
                        }   
                        break;
                }
            }

        }

        public async Task ConnectApi()
        { 
            mediaApiService.ConnectApi(IpAddress);
          
            mediaApiService.DoHubEventSend += (type, message) =>
            {
                ApiMessage = message;
                IsIndeterminate = type == "Info" ? false : true;
                var medis = mediaApiService.GetMediaData();

                foreach (var item in vdcpServerViewModels)
                {
                    foreach (var clip in medis.Result)
                    {
                        if (!item.ClipList.Contains(clip.Name))
                        {
                            item.ClipList.Add(clip.Name);
                        }
                    }

                    item.mediaDataInfos = medis.Result;
                }
            };

            LoggerConnecter = new LoggerApiConnecter(IpAddress);
            LoggerConnecter.Connection();
            LoggerConnecter.ConnectHub();
        }

        public void Pause()
        {
            foreach (var item in vdcpServerViewModels)
            {
                item.isThreadStart = false;
            }
        }

        public async Task Start()
        {
            foreach(var item in vdcpServerViewModels)
            {
                if(item.WorkLoad1 != string.Empty)
                   await item.Start();
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
