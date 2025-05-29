using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VdcpService.lib;

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class VdcpServerViewModel : INotifyPropertyChanged
    {

        private VdcpServer vdcpServer = null;

        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; OnPropertyChanged(nameof(ServerName)); }
        }

        private string _ComNumber;
        public string ComNumber
        {
            get { return _ComNumber; }
            set { _ComNumber = value; OnPropertyChanged(nameof(ComNumber)); }
        }


        public VdcpServerViewModel( string serverName, string comNumber)
        {
            ServerName = serverName;
            ComNumber = comNumber;
            vdcpServer = new VdcpServer(comNumber);
            
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
