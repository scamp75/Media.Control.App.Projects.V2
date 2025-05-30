using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.Model;
using VdcpService.lib;

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class VdcpServerViewModel : INotifyPropertyChanged
    {

        private VdcpServer vdcpServer = null;

        private string _type = "Player";
        public string Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; OnPropertyChanged(nameof(ServerName)); }
        }

        private string _PortName;
        public string PortName
        {
            get { return _PortName; }
            set { _PortName = value; OnPropertyChanged(nameof(PortName)); }
        }

        private string _Macros1 { get; set; } = string.Empty;
        public string Macros1
        {
            get { return _Macros1; }
            set { _Macros1 = value; OnPropertyChanged(nameof(Macros1)); }
        }

        private string _Macros2 { get; set; } = string.Empty;
        public string Macros2
        {
            get { return _Macros2; }
            set { _Macros2 = value; OnPropertyChanged(nameof(Macros2)); }
        }

        private int _selectPort = 0;
        public int SelectPort
        {
            get { return _selectPort; }
            set { _selectPort = value; OnPropertyChanged(nameof(SelectPort)); }
        }

        private string _workLoad1 = string.Empty;

        public string WorkLoad1
        {
            get { return _workLoad1; }
            set { _workLoad1 = value; OnPropertyChanged(nameof(WorkLoad1)); }
        }

        private string _workLoad2 = string.Empty;
        public string WorkLoad2
        {
            get { return _workLoad2; }
            set { _workLoad2 = value; OnPropertyChanged(nameof(WorkLoad2)); }
        }

        public VdcpServerViewModel(PortDataInfo portData)
        {
            Type = portData.Type;
            ServerName = $"Port_{portData.PortName}";
            PortName = portData.PortName;
            SelectPort = Type != "Player" ? -portData.SelectPort : portData.SelectPort;
            Macros1 = portData.Macros1;
            Macros2 = portData.Macros2;
            WorkLoad1 = portData.WorkLoad1;
            WorkLoad2 = portData.WorkLoad2;

            vdcpServer = new VdcpServer(PortName);
        }

        public bool Open( EnuPortType type, string address, int port, bool b)
        {
            bool result = false;

            if (vdcpServer != null)
                result = vdcpServer.Open(type, address, port, b);

            return result;
        }

        public void Close()
        {
            if (vdcpServer != null) vdcpServer.Close();
        }

        public void Start()
        {
            vdcpServer.thrStart = true;
        }

        public void Stop()
        {
            vdcpServer.thrStart = false;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
