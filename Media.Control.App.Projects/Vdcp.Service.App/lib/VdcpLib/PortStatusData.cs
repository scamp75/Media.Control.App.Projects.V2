using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class PortStatusData
    {   

    }

    public class PortMediaStatus
    {
        public bool Normal { get; set; }
        public bool Idle { get; set; }
        public bool CueInit { get; set; }
        public bool PlayRecord { get; set; }
        public bool Still { get; set; }
        public bool Jog { get; set; }
        public bool VarPlay { get; set; }
        public bool PortBusy { get; set; }
        public bool CueInitDone { get; set; }
    }

    public class PortHwMediaStatus
    {
        public bool Normal { get; set; }
        public bool PortDown { get; set; }
        public bool IDAdded { get; set; }
        public bool IDDelete { get; set; }
        public bool IDDeleteArch { get; set; }
        public bool IDAddtoArch { get; set; }
        public bool NoRefInput { get; set; }
        public bool NoVideoInput { get; set; }
        public bool NoAuioInput { get; set; }
        public bool AudioOverLoad { get; set; }
    }


    public class PortStatus1
    {
        public bool Normal { get; set; }
        public bool SystemError { get; set; }
        public bool IllegalValue { get; set; }
        public bool InvalidPort { get; set; }
        public bool WrongPortType { get; set; }
        public bool CommandQueueFull { get; set; }
        public bool DiskFull { get; set; }
        public bool CmdwhileBusy { get; set; }
        public bool NotSupport { get; set; }
    }

    public class PortStatus2
    {
        public bool Normal { get; set; }
        public bool InvalidId { get; set; }
        public bool IDNotFound { get; set; }
        public bool IDAlreadyExists { get; set; }
        public bool IDStilRecording { get; set; }
        public bool IDStillPlaying { get; set; }
        public bool IDNotTransferredFromaArchive { get; set; }
        public bool IDNotTransferredToArchive { get; set; }
        public bool IDDeleteProtected { get; set; }
    }


    public class PortStatus3
    {
        public bool Normal { get; set; }
        public bool NotInCue { get; set; }
        public bool InitState { get; set; }
        public bool CueNotDone { get; set; }
        public bool PortNotIdle { get; set; }
        public bool PortPlaying { get; set; }
        public bool Active { get; set; }
        public bool PortNotAchive { get; set; }
        public bool CueOrOperAtionfalied { get; set; }
        public bool NetWorKError { get; set; }
        public bool SystemReBooted { get; set; }
    }

    public class PortSetting
    {
        public bool Off { get; set; }
        public bool Composite { get; set; }
        public bool SVidoe { get; set; }
        public bool Yuv { get; set; }
        public bool D1 { get; set; }
    }
}
