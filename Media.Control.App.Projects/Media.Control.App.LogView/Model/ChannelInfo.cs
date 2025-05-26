using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.LogView.Model
{
    public class ChannelInfo
    {
        public int Channel { get; set; }
        public string Name { get; set; }
    }


    public class ChannelConfigData
    {
        public List<ChannelInfo> ChannelList { get; set; }
    }

    public class RootObject
    {
        public ChannelConfigData ChannelConfigData { get; set; }
    }

    
}
