using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.ManagerBa.Model.Config
{
    public class ChannelConfigData
    {
        public EnuEnaginType EnginType { get; set; } = EnuEnaginType.Ampp;

        public AmppConfig AmppConfig { get; set; }

        public VdcpConfig VdcpConfigs { get; set; }

        public List<ChannelConfig> ChannelList { get; set; }
        
        public List<OverlayFilter> OverlayFilters { get; set; }   

        public List<InputConfigData> InPutList { get; set; }

        public ChannelConfigData() 
        { 
            AmppConfig = new AmppConfig();
            VdcpConfigs = new VdcpConfig();
            ChannelList = new List<ChannelConfig>();
            OverlayFilters = new List<OverlayFilter>();
            InPutList = new List<InputConfigData>(); 
        }
    }
}
