using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    public enum EnuVdcpType { Serial, Udp }

    public class VdcpConfig
    {
        public EnuVdcpType VdcpType { get; set; }
        public string IpAddress { get; set; }

        public List<VdcpPortConfig> VdcpPortConfigs { get; set; } = new List<VdcpPortConfig>();
    }


    public class VdcpPortConfig
    {
        public EnmChannel Channel { get; set; }
        public string ComPort { get; set; } 
        public int SelectComPort { get; set; } 
        public int Port { get; set; } 

    }
}
