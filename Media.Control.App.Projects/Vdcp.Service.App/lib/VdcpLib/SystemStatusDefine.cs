using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class SystemStatusDefine
    {
        #region
        private BooleanProperty diskFull = new BooleanProperty(0,7);
        private BooleanProperty systemDown = new BooleanProperty(1,7);
        private BooleanProperty diskDown = new BooleanProperty(2,7);
        private BooleanProperty remoteControlDisabled = new BooleanProperty(3,7);

        private BooleanProperty archiveAvailable = new BooleanProperty(0, 8);
        private BooleanProperty archiveFull = new BooleanProperty(1, 8);

        private BooleanProperty localOffline = new BooleanProperty(0, 9);
        private BooleanProperty systemOffline = new BooleanProperty(1, 9);
        private BooleanProperty localOfflineFull = new BooleanProperty(2, 9);
        private BooleanProperty systemOfflineFull = new BooleanProperty(3, 9);

        #endregion

        public string StoredId { get; set; }

        public string TotalRemainTime { get; set; } = "00:00:00:00";

        public string LargestBlock { get; set; }

        public string StandarTime { get; set; } = "00:00:00:00";

        public int SignalFullLevel { get; set; } = 80;


        public BooleanProperty DiskFull { get { return diskFull; } set { diskFull = value; } }

        public BooleanProperty SystemDown { get { return systemDown; } set { systemDown = value; } }

        public BooleanProperty DiskDown { get { return diskDown; } set { diskDown = value; } }

        public BooleanProperty RemoteControlDisabled { get { return remoteControlDisabled; } set { remoteControlDisabled = value; } }

        public BooleanProperty ArchiveAvailable { get { return archiveAvailable; } set { archiveAvailable = value; } }

        public BooleanProperty ArchiveFull { get { return archiveFull; } set { archiveFull = value; } }

        public BooleanProperty LocalOffline { get { return localOffline; } set { localOffline = value; } }

        public BooleanProperty SystemOffline { get { return systemOffline; } set { systemOffline = value; } }

        public BooleanProperty LocalOfflineFull { get { return localOfflineFull; } set { localOfflineFull = value; } }

        public BooleanProperty SystemOfflineFull { get { return systemOfflineFull; } set { systemOfflineFull = value; } }

    }       


    
}
