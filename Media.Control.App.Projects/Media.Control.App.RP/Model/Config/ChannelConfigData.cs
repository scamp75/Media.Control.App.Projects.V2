using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    public  class ChannelConfigData
    {
        public EnmEnaginType EnginType { get; set; } = EnmEnaginType.Ampp;

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


    public class ChannelConfigData2
    {
        public EnmEnaginType EnginType { get; set; } = EnmEnaginType.Ampp;

        public EnuVdcpType VdcpType { get; set; } = EnuVdcpType.Udp;

        public string IpAddress { get; set; } = string.Empty;

        public AmppConfig AmppConfig { get; set; }

        public ChannelConfig ChannelList { get; set; }

        public VdcpPortConfig VdcpPortConfig { get; set; }

        public OverlayFilter OverlayFilters { get; set; }

    

        public List<InputConfigData> InPutList { get; set; }
        public ChannelConfigData2()
        {
            VdcpPortConfig = new VdcpPortConfig();
            AmppConfig = new AmppConfig();
            ChannelList = new ChannelConfig();
            OverlayFilters = new OverlayFilter();
            InPutList = new List<InputConfigData>();
        }

    }
}
