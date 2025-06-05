using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class PortDataInfo
    {
        public string PortName { get; set; }
        public string Type { get; set; }
        public int SelectPort { get; set; }

        public AmppConfig AmppConfig { get; set; } = null;
        

        public string WorkLoad1 { get; set; }
        public string WorkLoad2 { get; set; }

        public string Macros1 { get; set; }
        public string Macros2 { get; set; }

        public bool IsEnabled { get; set; } = false;

    }
}
