using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    //public class VdcpEumStateData
    //{
    //}

    public enum EumTimeCodeType
    {
        RemainingTime = 0,
        CurrentTime = 1
    }
    public enum EumPosrtSettings
    {
        Normal,
        Off,
        Composite,
        SVidoe,
        Yuv,
        D1
    }

    public enum EumPortStatus1
    {
        Normal,
        SystemError,
        IllegalValue,
        InvalidPort,
        WrongPortType,
        CommandQueueFull,
        DiskFull,
        CmdWhileBusy,
        NotSupport
    }

    public enum EumPortStatus2
    {
        Normal,
        InvalidId,
        IDNotFound,
        IDAlreadyExists,
        IDStilRecording,
        IDStillPlaying,
        IDNotTransferredFromaArchive,
        IDNotTransferredToArchive,
        IDDeleteProtected
    }

    public enum EumPortMediaStatus
    {
        Normal,
        Idle,
        CueInit,
        PlayRecord,
        Still,
        Jog,
        VarPlay,
        PortBusy,
        CueInitDone
    }

    public enum EumPortStatus3
    {
        Normal,
        NotInCue,
        InitState,
        CueNotDone,
        PortNotIdle,
        PortPlaying,
        Active,
        PortNotAchive,
        CueOrOperAtionfalied,
        NetWorKError,
        SystemReBooted
    }

    public enum EumPortHwMediaStatus
    {
        Normal,
        PortDown,
        IDAdded,
        IDDelete,
        IDDeleteArch,
        NoRefInput,
        NoVideoInput,
        IDAddtoArch,
        NoAuioInput,
        AudioOverLoad
    }


    public enum EumDiskstatus
    {
        Normal = 0x00,
        DiskFull = 0x01,
        SystemDown = 0x02,
        DiskDown = 0x04,
        RemoteControlDisabled = 0x08
    }

    public enum EumSubSystemSatus1
    {
        Normal,
        ArchvieAvalable,
        ArchiveFuil
    }

    public enum EumSubSystemStatus2
    {
        Normal,
        LocalOfflineStorageAvailable,
        SystemOfflineStorageAvailable,
        LocalOfflineStorageFull,
        SystemOfflineStorageFull
    }

    public enum EumEEMode
    {
        Off,
        On,
        Auto
    }
}
