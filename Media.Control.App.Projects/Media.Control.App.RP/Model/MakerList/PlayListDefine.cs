using Media.Control.App.RP.Model.Engine;
using Media.Control.App.RP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class PlayListDefine
    {
        public int Index { get; set; }
        
        public  EngineControl Control { get; set; }   

        public string Cleancut { get; set; }

        public string MediaName { get; set; }
                
        public string State { get; set; } = "Wait";

        public string Timecode { get; set; } = "00:00:00;00";
        public long Duration { get; set; }

        public bool IsPlaying { get; set; }

    }
}
