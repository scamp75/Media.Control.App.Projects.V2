using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.Model;
using Vdcp.Service.App.Manager.View;

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class ConfigWindwosViewModel : INotifyPropertyChanged
    {
        private AmppConfig amppConfig = null;
        private ObservableCollection<PortDataInfo> _PortDataInfoList;
        public ObservableCollection<PortDataInfo> PortDataInfoList
        {
            get => _PortDataInfoList;
            set
            {
                if (_PortDataInfoList != value)
                {
                    _PortDataInfoList = value;
                    OnPropertyChanged(nameof(PortDataInfoList));
                }
            }
        }

        private ObservableCollection<string> _PortNameList;
        public ObservableCollection<string> PortNameList
        {
            get => _PortNameList;
            set
            {
                if (_PortNameList != value)
                {
                    _PortNameList = value;
                    OnPropertyChanged(nameof(PortNameList));
                }
            }
        }

        private ObservableCollection<string> _PortTypeList;
        public ObservableCollection<string> PortTypeList
        {
            get => _PortTypeList;
            set
            {
                if (_PortTypeList != value)
                {
                    _PortTypeList = value;
                    OnPropertyChanged(nameof(PortTypeList));
                }
            }
        }

        private ObservableCollection<int> _SeletePortList;
        public ObservableCollection<int> SeletePortList
        {
            get => _SeletePortList;
            set
            {
                if (_SeletePortList != value)
                {
                    _SeletePortList = value;
                    OnPropertyChanged(nameof(SeletePortList));
                }
            }
        }

        private string _PlatformUrl;
        public string PlatformUrl
        {
            get => _PlatformUrl;
            set
            {
                if (_PlatformUrl != value)
                {
                    _PlatformUrl = value;
                    OnPropertyChanged(nameof(PlatformUrl));
                }
            }
        }
        private string _PlatformKey;
        public string PlatformKey
        {
            get => _PlatformKey;
            set
            {
                if (_PlatformKey != value)
                {
                    _PlatformKey = value;
                    OnPropertyChanged(nameof(PlatformKey));
                }
            }
        }
        private string _WorkNode;
        public string WorkNode
        {
            get => _WorkNode;
            set
            {
                if (_WorkNode != value)
                {
                    _WorkNode = value;
                    OnPropertyChanged(nameof(WorkNode));
                }
            }
        }

        private string _Fabric;
        public string Fabric
        {
            get => _Fabric;
            set
            {
                if (_Fabric != value)
                {
                    _Fabric = value;
                    OnPropertyChanged(nameof(Fabric));
                }
            }
        }

        private ConfigWindwos _configWindwos;

        public ConfigWindwosViewModel(ConfigWindwos windwos)
        {

            _configWindwos = windwos;

            PortNameList = new ObservableCollection<string>()
            {
               "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9"
            };

            PortTypeList = new ObservableCollection<string>()
            { string.Empty, "Recoder", "Player" };

            SeletePortList = new ObservableCollection<int>()
            { 0, 1, 2 , 3 , 4 , 5 , 6 ,7 };

            PortDataInfoList = new ObservableCollection<PortDataInfo>();

            for(int i =3; i < 10; ++i)
            {

                PortDataInfoList.Add(new PortDataInfo()
                {
                    PortName = $"COM{i}",
                    Type = string.Empty,
                    SelectPort = 0,
                    WorkLoad1 = "",
                    WorkLoad2 = "",
                    Macros1 = "",
                    Macros2 = ""
                });
            }
        }

        public void ConfigSave()
        {
             string bacePath  = AppDomain.CurrentDomain.BaseDirectory;

            AmppConfig ampp = new AmppConfig
            {
                PlatformUrl = PlatformUrl,
                PlatformKey = PlatformKey,
                WorkNode = WorkNode,
                Fabric = Fabric
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(ampp, new JsonSerializerOptions { WriteIndented = true });
            // 파일로 저장
            System.IO.File.WriteAllText($"{bacePath}AmppConfig.json", jsonString);

            foreach(var portData in PortDataInfoList)
            {
                if (portData.SelectPort != 0 && portData.Type != string.Empty)
                {
                    portData.IsEnabled = true;
                }
            }

            jsonString = System.Text.Json.JsonSerializer.Serialize(PortDataInfoList, new JsonSerializerOptions { WriteIndented = true });
            // 파일로 저장
            System.IO.File.WriteAllText($"{bacePath}ComConfig.json", jsonString);

            _configWindwos.Close();


        }

        public void ConfigLoad()
        {
            string bacePath = AppDomain.CurrentDomain.BaseDirectory;

            if (System.IO.File.Exists($"{bacePath}AmppConfig.json"))
            {
                string jsonString = System.IO.File.ReadAllText($"{bacePath}AmppConfig.json");
                amppConfig = System.Text.Json.JsonSerializer.Deserialize<AmppConfig>(jsonString);


                if (amppConfig != null)
                {
                    PlatformUrl = amppConfig.PlatformUrl;
                    PlatformKey = amppConfig.PlatformKey;
                    WorkNode = amppConfig.WorkNode;
                    Fabric = amppConfig.Fabric;
                }
            }

            if (System.IO.File.Exists($"{bacePath}ComConfig.json"))
            {
                string jsonString = System.IO.File.ReadAllText($"{bacePath}ComConfig.json");
                PortDataInfoList = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PortDataInfo>>(jsonString);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
