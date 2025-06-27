using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Media.Control.App.ManagerBa.Model;
using Media.Control.App.ManagerBa.View;
using Media.Control.App.ManagerBa.Model.HotKey;
using Media.Control.App.ManagerBa.Model.Config;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Documents;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;



namespace Media.Control.App.ManagerBa.ViewModel
{
    public partial class ConfigWindowViewModel : INotifyPropertyChanged
    {
        private ConfigWindowView configWindow = null;

        public ICommand Command_Save { get; }
      
        public ICommand Command_AddChannel { get; }
        public ICommand Command_DeleteChannel { get; }  

        public ICommand Command_AddItem { get; }

        public ICommand Command_DeleteItem { get; }

        public ICommand Commnd_KeyClean { get; }    

        public ICommand Command_AddInput { get; }

        public ICommand Command_DeleteInput { get; }

        public ICommand Command_AddCom { get; }

        public ICommand Command_DeleteCom { get; }


        public ObservableCollection<VdcpPortConfig> VdcpPortConfigList { get; set; } 

        public ObservableCollection<EnuVdcpType> PortTypeList { get; set; } 

        public ObservableCollection<string> ComPortList { get; set; } 

        public ObservableCollection<int> SelectPortList { get; set; } 

        public ObservableCollection<OverlayFilter> OverlayFiltersList { get; set; }
       
        public ObservableCollection<ChannelConfig> ChannelSettingList { get; set; }

        public ObservableCollection<string> VideoFilterList { get; set; }
        public ObservableCollection<string> AudioFilterList { get; set; }

        public ObservableCollection<string> NDIFilterList { get; set; }

        public ObservableCollection<EnuOverlayMode> OverlayModeList { get; set; }

        public ObservableCollection<PlayerCleancutConfig> PlayerCleancutList { get; set; }


        public ObservableCollection<InputConfigData> InputSettingList { get; set; }  

        public ObservableCollection<HotKeyDefin> HotKeyDefinList { get; set; }

        public ObservableCollection<EnuEnaginType> EngineTypeList { get; set; } 


        public List<EnuChannelType> ChannelTypeList { get; set; }

        private VdcpPortConfig _SelectedVdcpconfig { get; set; }
        public VdcpPortConfig SelectedVdcpconfig
        {
            get => _SelectedVdcpconfig;
            set { _SelectedVdcpconfig = value; OnPropertyChanged(); }
        }

        private ChannelConfig selectedChannelItem { get; set; } 

        public ChannelConfig SelectedChannelItem { 
            get => selectedChannelItem ; 
            set { 
                if(value != null)
                {
                    selectedChannelItem = value;
                    OnPropertyChanged(nameof(SelectedChannelItem));
                }
            }
        }

        private OverlayFilter _selectedDeckLinkFilter { get; set; }
        public OverlayFilter SelectedDeckLinkFilter 
        { 
            get=> _selectedDeckLinkFilter;
            set { _selectedDeckLinkFilter = value;  
                OnPropertyChanged();
            } 
        }
        private EnuVdcpType _SelectPortType { get; set; }

        public EnuVdcpType SelectPortType
        {
            set
            {
                _SelectPortType = value;
                OnPropertyChanged(nameof(_SelectPortType));
            }
            get { return _SelectPortType; }
        }

        private PlayerCleancutConfig selectedPlayerItem { get; set; }  
        public PlayerCleancutConfig SelectedPlayerItem
        {
            get => selectedPlayerItem;
            set { selectedPlayerItem = value; OnPropertyChanged(); }
        }

        private double gridRow2Heigth { get; set; }
        public double GridRow2Heigth { get => gridRow2Heigth; set { gridRow2Heigth = value; OnPropertyChanged(); } }


        public string _InputName { get; set; }

        public string InputName
        {
            get => _InputName;
            set
            {
                _InputName = value;
                OnPropertyChanged();
            }
        }

        public InputConfigData _InputSelectItem { get; set; }

        public InputConfigData InputSelectItem
        {
            get => _InputSelectItem;
            set 
            { 
                _InputSelectItem = value;
                
                InputName = value?.InputName;
                 OnPropertyChanged();
            }            
        }

        public string _AudioOverlay { get; set; }

        public string AudioOverlay
        { 
            get=> _AudioOverlay;
            set 
            {
                _AudioOverlay = value;

                //DeckLinkFiltersList.Where(c => c.Channel == SelectedDeckLinkFilter.Channel).FirstOrDefault().Name = value;
                //DeckLinkFiltersList.Where(c => c.Channel == SelectedDeckLinkFilter.Channel).FirstOrDefault().Clsid
                //    = AudioDeckLinkFilters.Where(c => c.Name == value).FirstOrDefault().Clsid;

                OnPropertyChanged(nameof(AudioOverlay));
            } 
        }

        public string _VideoOverlay { get; set; }

        public string VideoOverlay
        {
            get => _VideoOverlay;
            set
            {
                _VideoOverlay = value;

                //DeckLinkFiltersList.Where(c => c.Channel == SelectedDeckLinkFilter.Channel).FirstOrDefault().Name = value;
                //DeckLinkFiltersList.Where(c => c.Channel == SelectedDeckLinkFilter.Channel).FirstOrDefault().Clsid 
                //    = VedioDeckLinkFilters.Where(c => c.Name == value).FirstOrDefault().Clsid;

                OnPropertyChanged(nameof(VideoOverlay));
            }
        }

        public EnuOverlayMode _SelectOverlaySetting { get; set; }
        public EnuOverlayMode SelectOverlaySetting
        {
            get => _SelectOverlaySetting;
            set
            {
                _SelectOverlaySetting = value;
                OnPropertyChanged(nameof(SelectOverlaySetting));
            }
        }



        private List<OverlayFilter> VedioDeckLinkFilters { get; set; }
        private List<OverlayFilter> AudioDeckLinkFilters { get; set; }

        private string systemPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Config";

        public ConfigWindowViewModel(ConfigWindowView window)
        {
            configWindow = window;
            Command_Save = new RelayCommand(CommandSvae);

            Command_AddChannel = new RelayCommand(CommandAddChannel);
            Command_DeleteChannel = new RelayCommand(CommandDeleteChannel);

            Command_AddItem = new RelayCommand(CommandAddItem);
            Command_DeleteItem = new RelayCommand(CommandDeleteItem);

            Commnd_KeyClean = new RelayCommand(CommandKeyClean);

            Command_AddInput = new RelayCommand(CommandAddInput);
            Command_DeleteInput = new RelayCommand(CommandDeleteInput);

            Command_AddCom = new RelayCommand(CommandAddCom);
            Command_DeleteCom = new RelayCommand(CommandDeleteCom);

            ChannelTypeList = new List<EnuChannelType>() {EnuChannelType.None,
                EnuChannelType.Player, EnuChannelType.Recoder };

            ChannelSettingList = new ObservableCollection<ChannelConfig>();

            PlayerCleancutList = new ObservableCollection<PlayerCleancutConfig>();
            //  InputSettingList = new ObservableCollection<InputConfigData>();

            OverlayFiltersList = new ObservableCollection<OverlayFilter>();

            FindDeckLinkFilter findDeckLinkFilter = new FindDeckLinkFilter();
            VideoFilterList = new ObservableCollection<string>();
            
            VedioDeckLinkFilters = findDeckLinkFilter.GetVideoDeckLinkFilter();
            VideoFilterList.Add("None");
            foreach (var item in VedioDeckLinkFilters)
            {
                VideoFilterList.Add(item.VideoFilter);
            }

            AudioFilterList = new ObservableCollection<string>();
            AudioDeckLinkFilters = findDeckLinkFilter.GetAudioDeckLinkFilter();
            AudioFilterList.Add("None");
            foreach (var item in AudioDeckLinkFilters)
            {
                AudioFilterList.Add(item.AudioFilter);
            }

            DNIReceivelib dNIReceivelib = new DNIReceivelib();
            dNIReceivelib.Initialize();
            NDIFilterList = new ObservableCollection<string>();

            NDIFilterList.Add("None");
            foreach (var item in dNIReceivelib.GetSourecName())
            {
                NDIFilterList.Add(item);
            }

            dNIReceivelib.Stop();

            OverlayModeList = new ObservableCollection<EnuOverlayMode>();
            OverlayModeList.Add(EnuOverlayMode.None);
            OverlayModeList.Add(EnuOverlayMode.NDI);
            OverlayModeList.Add(EnuOverlayMode.Decklink);

            PortTypeList = new ObservableCollection<EnuVdcpType>();
            PortTypeList.Add(EnuVdcpType.Serial);
            PortTypeList.Add(EnuVdcpType.Udp);

            VdcpPortConfigList = new ObservableCollection<VdcpPortConfig>();

            ComPortList = new ObservableCollection<string>();

            for (int i = 3; i < 10; i++)
            {
                string port = $"COM{i}";
                ComPortList.Add(port);
            }

            SelectPortList = new ObservableCollection<int>
            {
                 -1, -2 , -3, -4, -5, -6, -7, -8, -9,
                 1, 2 , 3, 4, 5, 6, 7, 8, 9
            };

            InputSettingList = new ObservableCollection<InputConfigData>
            {
                new InputConfigData { InputName = "Item 1" },
                new InputConfigData { InputName = "Item 2"},
                new InputConfigData { InputName = "Item 3"}
            };

            EngineTypeList = new ObservableCollection<EnuEnaginType>();
            EngineTypeList.Add(EnuEnaginType.Ampp);
            EngineTypeList.Add(EnuEnaginType.Vdcp);

            GridRow2Heigth = window.GridRowHeigth;
            SelectOverlaySetting = EnuOverlayMode.None;
            


            SetHotKeyInit();
            DisplaySystemConfig();
        }

        private void CommandDeleteCom(object? obj)
        {
            VdcpPortConfigList.Remove(SelectedVdcpconfig);
        }

        private void CommandAddCom(object? obj)
        {
            int index = VdcpPortConfigList.Count + 3;

            string comName = $"COM{index}";

            EnuChannel channel = (EnuChannel)Enum.Parse(typeof(EnuChannel), $"Channel{index}");

            VdcpPortConfigList.Add(new
                VdcpPortConfig
            { Channel = channel, ComPort = comName, SelectComPort = 0, Port = 0 });
        }

        private void CommandDeleteItem(object? obj)
        {
            PlayerCleancutList.Remove(selectedPlayerItem);

        }

        private void CommandAddItem(object? obj)
        {
            int index = PlayerCleancutList.Count + 1;

            EnuChannel channel = (EnuChannel)Enum.Parse(typeof(EnuChannel), $"Channel{index}");

            PlayerCleancutList.Add(new
                PlayerCleancutConfig
            { Channel = channel, Cleancut= ""  });
        }

        private void CommandDeleteInput(object? obj)
        {
            var item = InputSettingList.Where(c => c.InputName == InputName).ToList();

            foreach(InputConfigData i in item)
            {
                InputSettingList.Remove(i);
            }
        }

        private void CommandAddInput(object? obj)
        {
            if(InputSettingList.Where(c => c.InputName == InputName).Count() == 0)
            {
                InputSettingList.Insert(0, new InputConfigData() { InputName = InputName });
            }
        }

        private void DisplaySystemConfig()
        {
            if (!Directory.Exists(systemPath))
                Directory.CreateDirectory(systemPath);

            if (!File.Exists($@"{systemPath}\SystemConfig.json")) 
                File.Create($@"{systemPath}\SystemConfig.json");

            if (!File.Exists($@"{systemPath}\Hotkeys.json"))
                File.Create($@"{systemPath}\Hotkeys.json");

            string jsonFromFile = string.Empty;
            if (File.Exists($@"{systemPath}\SystemConfig.json"))
                jsonFromFile = File.ReadAllText($@"{systemPath}\SystemConfig.json");


            if(jsonFromFile != string.Empty)
            {
                SystemConfigData systemConfigData = new SystemConfigData();
                systemConfigData = JsonConvert.DeserializeObject<SystemConfigData>(jsonFromFile);

                if (systemConfigData != null)
                {
                    SelectEnaginType = systemConfigData.ChannelConfigData.EnginType;
                    PlatformUrl = systemConfigData.ChannelConfigData.AmppConfig.Platformurl;
                    ApiKey = systemConfigData.ChannelConfigData.AmppConfig.ApiKey;
                    WorkNode = systemConfigData.ChannelConfigData.AmppConfig.WorkNode;
                    Fabric = systemConfigData.ChannelConfigData.AmppConfig.Fabric;
                    //////////////////////////////////////////////////////////////////////////////////

                    SelectPortType = systemConfigData.ChannelConfigData.VdcpConfigs.VdcpType;
                    PortIpAddress = systemConfigData.ChannelConfigData.VdcpConfigs.IpAddress;

                    ChannelSettingList = new ObservableCollection<ChannelConfig>(systemConfigData.ChannelConfigData.ChannelList);
                    OverlayFiltersList = new ObservableCollection<OverlayFilter>(systemConfigData.ChannelConfigData.OverlayFilters);
                    VdcpPortConfigList = new ObservableCollection<VdcpPortConfig>(systemConfigData.ChannelConfigData.VdcpConfigs.VdcpPortConfigs);


                    if (OverlayFiltersList.Count == 0)
                    {
                        foreach(var item in ChannelSettingList)
                        {
                            OverlayFiltersList.Add(new OverlayFilter
                            {
                                Channel = item.Channel,
                                OverlayMode = EnuOverlayMode.None,
                                VideoFilter = "",
                                VideoClsid = "",
                                AudioFilter = "",
                                AudioClsid = "",
                            });
                        }
                    }

                    InputSettingList = new ObservableCollection<InputConfigData>(systemConfigData.ChannelConfigData.InPutList);
                    PlayerCleancutList = new ObservableCollection<PlayerCleancutConfig>(systemConfigData.ControlConfigData.PlayerSetting.PlayerCleancutConfigs );

                    //////////////////////////////////////////////////////////////////////////////////
                    RecorderGangPort = systemConfigData.ControlConfigData.RecorderSetting.GangPort;
                    DefaultRcName = systemConfigData.ControlConfigData.RecorderSetting.DefaultName;
                    DefaultReFolder = systemConfigData.ControlConfigData.RecorderSetting.DefaultFolder;
                    RecordOffset = systemConfigData.ControlConfigData.RecorderSetting.RecordOffset;

                    PlayerGangPort = systemConfigData.ControlConfigData.PlayerSetting.GangPort;

                    MediaUrl = systemConfigData.ControlConfigData.MediaViewSetting.Url;
                    MediaApiKey = systemConfigData.ControlConfigData.MediaViewSetting.ApiKey;
                    MediaWorkNode = systemConfigData.ControlConfigData.MediaViewSetting.WorkLoad;

                }
            }

            jsonFromFile = string.Empty;
            jsonFromFile = File.ReadAllText($@"{systemPath}\Hotkeys.json");

            if (jsonFromFile != string.Empty)
            {
                HotKeyConfigData hotKeyConfigData = new HotKeyConfigData();
                var hotKeyDefins = JsonConvert.DeserializeObject<HotKeyConfigData>(jsonFromFile);

                if (hotKeyDefins != null)
                    HotKeyDefinList = new ObservableCollection<HotKeyDefin>(hotKeyDefins.HotKeys);
            }
        }

        public void CommandKeyClean(object? obj)
        {
            var selectedItem = configWindow.HotkeyGridList.SelectedItem as HotKeyDefin;
            if (selectedItem != null)
            {
                // 특정 열 값 지우기 (예: Address 열)
                selectedItem.Value = string.Empty;

                // UI 새로고침
                configWindow.HotkeyGridList.Items.Refresh();
            }
        }

        private void SetHotKeyInit()
        {
            HotKeyDefinList = new ObservableCollection<HotKeyDefin>();
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.PLAY, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.RECORD, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.STOP, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.PUSE, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.FIST, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.END, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.FF, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.REW, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.M1F, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.M10F, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.P1F, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.P10F, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.INMARK, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.OUTMAR, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.INDEL, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.OUTDEL, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.OUTGO, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.INGO, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.JOG, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.SHUTTLE, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.LISTPLAY, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.LISTNEXT, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.LISTDEL, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.LISTCLEAN, Defin = "", Value = "None" });
            HotKeyDefinList.Add(new HotKeyDefin { Key = AtionEventDefin.LISTCUE, Defin = "", Value = "None" });
            
        }

        private void CommandDeleteChannel(object? obj)
        {
            ChannelSettingList.Remove(SelectedChannelItem);

            if (SelectedChannelItem != null)
            {
                OverlayFiltersList.Where(c => c.Channel == SelectedChannelItem.Channel).ToList().ForEach(c => OverlayFiltersList.Remove(c));
            }


        }

        private void CommandAddChannel(object? obj)
        {
            int index =  ChannelSettingList.Count + 1;

            EnuChannel channel = (EnuChannel)Enum.Parse(typeof(EnuChannel), $"Channel{index}");
            
            ChannelSettingList.Add(new 
                ChannelConfig { Channel = channel, ChannelType = EnuChannelType.None, Name = "", WorkLoad1 = "", WorkLoad2 = "" });

            OverlayFiltersList.Add(new OverlayFilter { Channel = channel, 
                                                        VideoFilter = ""
                                                        , VideoClsid = "" , AudioFilter ="", AudioClsid="", NDIFilter ="" });


        }

        private void CommandSvae(object? obj)
        {
            SaveConfigData();
            configWindow.SetClose();
        }

        private void SaveConfigData()
        {

            foreach(var filter in OverlayFiltersList.ToList())
            {

                if(!string.IsNullOrEmpty(filter.AudioFilter))
                {
                    if (filter.AudioFilter != "None")
                    {
                        var aclsi = AudioDeckLinkFilters.Where(c => c.AudioFilter == filter.AudioFilter).FirstOrDefault().AudioClsid;
                        OverlayFiltersList.Where(c => c.Channel == filter.Channel).FirstOrDefault().AudioClsid = aclsi;
                    }

                    
                }

                if(!string.IsNullOrEmpty(filter.VideoFilter))
                {
                    if(filter.VideoFilter != "None")
                    {
                        var vclsi = VedioDeckLinkFilters.Where(c => c.VideoFilter == filter.VideoFilter).FirstOrDefault().VideoClsid;
                        OverlayFiltersList.Where(c => c.Channel == filter.Channel).FirstOrDefault().VideoClsid = vclsi;
                    }
                }
            }


            SystemConfigData systemConfigData = new SystemConfigData();

            systemConfigData.ChannelConfigData.EnginType = SelectEnaginType;
            systemConfigData.ChannelConfigData.AmppConfig.Platformurl = PlatformUrl;
            systemConfigData.ChannelConfigData.AmppConfig.ApiKey = ApiKey;
            systemConfigData.ChannelConfigData.AmppConfig.WorkNode = WorkNode;
            systemConfigData.ChannelConfigData.AmppConfig.Fabric = Fabric;
            //////////////////////////////////////////////////////////////////////////////////
            ///
            systemConfigData.ChannelConfigData.VdcpConfigs.VdcpPortConfigs.Clear();

            systemConfigData.ChannelConfigData.VdcpConfigs.IpAddress = PortIpAddress;
            systemConfigData.ChannelConfigData.VdcpConfigs.VdcpType = SelectPortType;
            systemConfigData.ChannelConfigData.VdcpConfigs.VdcpPortConfigs = VdcpPortConfigList.ToList();

            systemConfigData.ChannelConfigData.ChannelList.Clear();
            systemConfigData.ChannelConfigData.ChannelList = ChannelSettingList.ToList();

            systemConfigData.ChannelConfigData.OverlayFilters.Clear();
            systemConfigData.ChannelConfigData.OverlayFilters = OverlayFiltersList.ToList();

            systemConfigData.ChannelConfigData.InPutList.Clear();
            systemConfigData.ChannelConfigData.InPutList = InputSettingList.ToList();   
            //////////////////////////////////////////////////////////////////////////////////
            systemConfigData.ControlConfigData.RecorderSetting.GangPort = RecorderGangPort;
            systemConfigData.ControlConfigData.RecorderSetting.DefaultName = DefaultRcName;
            systemConfigData.ControlConfigData.RecorderSetting.DefaultFolder = DefaultReFolder;
            systemConfigData.ControlConfigData.RecorderSetting.RecordOffset = RecordOffset;

            systemConfigData.ControlConfigData.PlayerSetting.GangPort = PlayerGangPort;
            systemConfigData.ControlConfigData.PlayerSetting.PlayerCleancutConfigs = PlayerCleancutList.ToList(); ;

            systemConfigData.ControlConfigData.MediaViewSetting.Url = MediaUrl;
            systemConfigData.ControlConfigData.MediaViewSetting.ApiKey = MediaApiKey;
            systemConfigData.ControlConfigData.MediaViewSetting.WorkLoad = MediaWorkNode;



            System.IO.File.Delete($@"{systemPath}\SystemConfig.json");

            string configjson = JsonConvert.SerializeObject(systemConfigData, Formatting.Indented);
            System.IO.File.WriteAllText($@"{systemPath}\SystemConfig.json", configjson);
            

            HotKeyConfigData hotKeyConfigData = new HotKeyConfigData();
            hotKeyConfigData.HotKeys = HotKeyDefinList.ToList();

            System.IO.File.Delete($@"{systemPath}\Hotkeys.json");
            string hotkeyjson = JsonConvert.SerializeObject(hotKeyConfigData, Formatting.Indented);
            System.IO.File.WriteAllText($@"{systemPath}\Hotkeys.json", hotkeyjson);

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
