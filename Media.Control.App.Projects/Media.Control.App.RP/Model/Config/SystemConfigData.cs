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

        public static VdcpConfig VdcpConfigData { get; set; }
           


        public static void Load(EnmChannel channel, SystemConfigData config)
        {

            ChannelConfigData = new ChannelConfigData2();
            VdcpConfigData = new VdcpConfig();
            ControlConfigData = new ControlConfigData();
            HotKeyConfigData = new HotKeyConfigData();  

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ChannelConfig channelConfig = config.ChannelConfigData.ChannelList.Where(x => x.Channel == channel).FirstOrDefault();
            OverlayFilter overlayFilter = config.ChannelConfigData.OverlayFilters.Where(x => x.Channel == channel).FirstOrDefault();
            VdcpPortConfig vdcpPortConfig = config.ChannelConfigData.VdcpConfigs.VdcpPortConfigs.Where(x => x.Channel == channel).FirstOrDefault();

            ChannelConfigData2 channelData2 = new ChannelConfigData2();
            channelData2.EnginType = config.ChannelConfigData.EnginType;
            channelData2.AmppConfig = config.ChannelConfigData.AmppConfig;
            channelData2.ChannelList = channelConfig;
            channelData2.OverlayFilters = overlayFilter;
            channelData2.InPutList = config.ChannelConfigData.InPutList;
            channelData2.VdcpType = config.ChannelConfigData.VdcpConfigs.VdcpType; 
            channelData2.IpAddress = config.ChannelConfigData.VdcpConfigs.IpAddress;
            channelData2.VdcpPortConfig = vdcpPortConfig;

            ChannelConfigData = channelData2;
            ControlConfigData = config.ControlConfigData;
            HotKeyConfigData = config.HotKeyConfigData;
        }
    }

}
