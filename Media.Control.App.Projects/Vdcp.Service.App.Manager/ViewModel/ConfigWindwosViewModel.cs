using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.Model;
using Vdcp.Service.App.Manager.View;

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class ConfigWindwosViewModel : INotifyPropertyChanged
    {

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
                    WorkNode1 = "",
                    WorkNode2 = "",
                    Macros1 = "",
                    Macros2 = ""
                });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
