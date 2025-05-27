using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class SystemStatusData
    {
        public string StoredId { get; set; } = "0";

        public string TotalRemainTime { get; set; } = "00:00:00:00";

        public string LargestBlock { get; set; } = "00:00:00:00";

        public string StandarTime { get; set; } = "00:00:00:00";

        public int SignalFullLevel { get; set; } = 80;

        public bool DiskFull { get; set; }
               
        public bool SystemDown { get; set; }
               
        public bool DiskDown { get; set; }
               
        public bool RemoteControlDisabled { get; set; }
               
        public bool ArchiveAvailable { get; set; }
               
        public bool ArchiveFull { get; set; }
               
        public bool LocalOffline { get; set; }
               
        public bool SystemOffline { get; set; }
               
        public bool LocalOfflineFull { get; set; }
               
        public bool SystemOfflineFull { get; set; }


    }
}
