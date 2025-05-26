using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.ManagerBa.Model.Config
{
    public class ControlConfigData
    {
     
        public RecorderSetting RecorderSetting { get; set; }    

        public PlayerSetting PlayerSetting { get; set; }    

        public MediaViewSetting MediaViewSetting { get; set; }

        public ControlConfigData() 
        {
            RecorderSetting = new RecorderSetting();
            PlayerSetting = new PlayerSetting();
            MediaViewSetting = new MediaViewSetting();
        }
    }

    public class RecorderSetting
    {
        public int GangPort { get; set; }

        public string DefaultName { get; set; }

        public string DefaultFolder { get; set; }

        public int RecordOffset { get; set; }
    }

    public class PlayerSetting
    {
        public int GangPort { get; set; }

        public List<PlayerCleancutConfig> PlayerCleancutConfigs { get; set; }

        public PlayerSetting()
        {
            PlayerCleancutConfigs = new List<PlayerCleancutConfig>();
        }
    }

    public class MediaViewSetting
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }

        public string WorkLoad { get; set; }
    }
}
