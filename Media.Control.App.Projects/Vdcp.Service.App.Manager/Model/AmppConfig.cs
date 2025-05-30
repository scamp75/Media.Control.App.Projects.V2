using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class AmppConfig
    {
        public string PlatformUrl { get; set; } = "https://ampp.vdcp.co.kr";
        
        public string PlatformKey { get; set; }

        public string WorkNode { get; set; }

        public string Fabric { get; set; }
    }
}
