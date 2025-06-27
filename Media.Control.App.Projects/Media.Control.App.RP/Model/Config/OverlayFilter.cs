using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    public class OverlayFilter
    {
        public EnmChannel Channel { get; set; }

        public EnmOverlayMode OverlayMode { get; set; }
        public string VideoFilter { get; set; }

        public string AudioFilter { get; set; }

        public string VideoClsid { get; set; }
        public string AudioClsid { get; set; }

        public string NDIFilter { get; set; }

    }
}
