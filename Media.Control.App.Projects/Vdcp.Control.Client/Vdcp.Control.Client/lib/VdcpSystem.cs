using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Control.Client
{
    public class DiskStatusData
    {
        public bool DisRemoteControl { get; set; }
        public bool DiskDown { get; set; }
        public bool SystemDown { get; set; }
        public bool DiskFull { get; set; }

    }

    public class SystemStatus
    {
        public string TotalRemainTime { get; set; }

        public string StoredID { get; set; }

        public DiskStatusData DiskStatus { get; set; }

        public string StandardTime { get; set; }

        public int SignalFullLevel { get; set; }
    }
    
}
