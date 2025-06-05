using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Vdcp.Service.App.Manager.Model
{
    public static class StaticSystemCofnig
    {
      
        public static AmppConfig AmppConfig { get; set; } = new AmppConfig()
        {
            Fabric = "default",
            PlatformUrl = "https://ampp.vdcp.co.kr",
            PlatformKey = "",
            WorkNode = "default"
        };

    }
}
