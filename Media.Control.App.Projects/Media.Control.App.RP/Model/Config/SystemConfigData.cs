using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    //public static class SystemConfigData
    //{
    //    public static ChannelConfigData ChannelConfigData { get; set; }

    //    public static ControlConfigData ControlConfigData { get; set; }

    //    public static HotKeyConfigData HotKeyConfigData { get; set; }

    //}

    public class SystemConfigData
    {
        public  ChannelConfigData ChannelConfigData { get; set; }

        public  ControlConfigData ControlConfigData { get; set; }

        public  HotKeyConfigData HotKeyConfigData { get; set; }

        public SystemConfigData()
        {
            ChannelConfigData = new ChannelConfigData();
            ControlConfigData = new ControlConfigData();
            HotKeyConfigData = new HotKeyConfigData();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

    public static class SystemConfigDataStatic
    {
        public static ChannelConfigData2 ChannelConfigData { get; set; } 
        public static ControlConfigData ControlConfigData { get; set; } 
        public static HotKeyConfigData HotKeyConfigData { get; set; } 

        public static void Load(EnuChannel channel, SystemConfigData config)
        {

            ChannelConfigData = new ChannelConfigData2();
            ControlConfigData = new ControlConfigData();
            HotKeyConfigData = new HotKeyConfigData();  

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ChannelConfig channelConfig = config.ChannelConfigData.ChannelList.Where(x => x.Channel == channel).FirstOrDefault();
            OverlayFilter overlayFilter = config.ChannelConfigData.OverlayFilters.Where(x => x.Channel == channel).FirstOrDefault();

            ChannelConfigData2 channelData2 = new ChannelConfigData2();
            channelData2.EnginType = config.ChannelConfigData.EnginType;
            channelData2.AmppConfig = config.ChannelConfigData.AmppConfig;
            channelData2.ChannelList = channelConfig;
            channelData2.OverlayFilters = overlayFilter;
            channelData2.InPutList = config.ChannelConfigData.InPutList;

            ChannelConfigData = channelData2;
            ControlConfigData = config.ControlConfigData;
            HotKeyConfigData = config.HotKeyConfigData;
        }
    }

}
