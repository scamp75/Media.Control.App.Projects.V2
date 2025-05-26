using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    public  class ChannelConfigData
    {
        public EnuEnaginType EnginType { get; set; } = EnuEnaginType.Ampp;

        public AmppConfig AmppConfig { get; set; }

        public List<ChannelConfig> ChannelList { get; set; }

        public List<OverlayFilter> OverlayFilters { get; set; }

        public List<InputConfigData> InPutList { get; set; }
        public ChannelConfigData()
        {
            AmppConfig = new AmppConfig();
            
            ChannelList = new List<ChannelConfig>();
            OverlayFilters = new List<OverlayFilter>();
            InPutList = new List<InputConfigData>();
        }

    }


    public class ChannelConfigData2
    {
        public EnuEnaginType EnginType { get; set; } = EnuEnaginType.Ampp;

        public AmppConfig AmppConfig { get; set; }

        public ChannelConfig ChannelList { get; set; }

        public OverlayFilter OverlayFilters { get; set; }

        public List<InputConfigData> InPutList { get; set; }
        public ChannelConfigData2()
        {
            AmppConfig = new AmppConfig();
            ChannelList = new ChannelConfig();
            OverlayFilters = new OverlayFilter();
            InPutList = new List<InputConfigData>();
        }

    }
}
