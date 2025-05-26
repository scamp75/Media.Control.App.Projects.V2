using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.ManagerBa.Model.Config
{
    public class SystemConfigData
    {
        public ChannelConfigData ChannelConfigData { get; set; } = new ChannelConfigData();

        public ControlConfigData ControlConfigData { get; set; } = new ControlConfigData();

        public HotKeyConfigData HotKeyConfigData { get; set; } = new HotKeyConfigData();

    }
}
