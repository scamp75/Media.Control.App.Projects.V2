using Media.Control.App.RP.Model.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class CueMediaInfo
    {
        
        public string Path { get; set; } = string.Empty;
        public MediaDataInfo MediaData { get; set; } = null;
        public EngineControl Control { get; set; }

        public int Cleancut { get; set; } = 0;

        public double Fps { get; set; } = 0;

        public void Init()
        {

            Control = null;
            Path = string.Empty;
            MediaData = null;
            Cleancut = 0;
            Fps = 0;
        }
    }
}
