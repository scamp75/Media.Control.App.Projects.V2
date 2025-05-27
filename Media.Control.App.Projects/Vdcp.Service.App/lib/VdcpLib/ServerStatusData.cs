using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public static class ServerStatusData
    {
        private static object obj = new object();
        private static VdcpCommandAdapter vdcpCommandAdapter = null;
        
        public static PortMediaStatus portMediaStatus = null;
        public static PortHwMediaStatus portHwMediaStatus = null;
        public static PortStatus1 portStatus1 = null;
        public static PortStatus2 portStatus2 = null;
        public static PortStatus3 portStatus3 = null;
        public static PortSetting portSetting = null;

        public static SystemStatusData systemStatusData = null;
        public static string CurrentTimeCode { get; set; } = "00:00:00:00";
        public static string RemainingTimeCode { get; set; } = "00:00:00:00";

        static ServerStatusData()
        {   
            vdcpCommandAdapter = new VdcpCommandAdapter();
            portMediaStatus = new PortMediaStatus();
            portHwMediaStatus = new PortHwMediaStatus();
            portStatus1 = new PortStatus1();
            portStatus2 = new PortStatus2();
            portStatus3 = new PortStatus3();
            portSetting = new PortSetting();

            systemStatusData = new SystemStatusData();
        }

        public static byte[] GetSystemStatusData()
        {
            
            byte[] systemStatusResult = new byte[15];

            lock (obj)
            {
                SystemStatusDefine systemStatus = new SystemStatusDefine();

                systemStatus.StoredId = systemStatusData.StoredId;
                systemStatus.TotalRemainTime = systemStatusData.TotalRemainTime;
                systemStatus.LargestBlock = systemStatusData.LargestBlock;
                systemStatus.StandarTime = systemStatusData.StandarTime;
                systemStatus.SignalFullLevel = systemStatusData.SignalFullLevel;
                systemStatus.ArchiveAvailable.PValue = systemStatusData.ArchiveAvailable;
                systemStatus.ArchiveFull.PValue = systemStatusData.ArchiveFull;
                systemStatus.DiskDown.PValue = systemStatusData.DiskDown;
                systemStatus.DiskFull.PValue = systemStatusData.DiskFull;
                systemStatus.LocalOffline.PValue = systemStatusData.LocalOffline;
                systemStatus.LocalOfflineFull.PValue = systemStatusData.LocalOfflineFull;
                systemStatus.RemoteControlDisabled.PValue = systemStatusData.RemoteControlDisabled;
                systemStatus.SystemDown.PValue = systemStatusData.SystemDown;
                systemStatus.SystemOffline.PValue = systemStatusData.SystemOffline;
                systemStatus.SystemOfflineFull.PValue = systemStatusData.SystemOfflineFull;

                byte[] statusResult = new byte[15];
                statusResult = vdcpCommandAdapter.SetSystemStatusDataProperty(systemStatus);
                systemStatusResult = vdcpCommandAdapter.MakeCommand(EumCommandKey.SYSTEMSTATUS,
                                                                   statusResult, statusResult.Length);
            }

            return systemStatusResult;
        }

        public static byte[] GetPortStatusData()
        {
            byte[] portStatusResult = new byte[8];

            lock (obj)
            {   
                PortStatusDefine portStatus = new PortStatusDefine();

                portStatus.Idle.PValue = portMediaStatus.Idle;
                portStatus.CueInit.PValue = portMediaStatus.CueInit;
                
                portStatus.PlayRecord.PValue = portMediaStatus.PlayRecord;
                
                portStatus.Still.PValue = portMediaStatus.Still;
                portStatus.Jog.PValue = portMediaStatus.Jog;
                portStatus.VarPlay.PValue = portMediaStatus.VarPlay;
                portStatus.PortBusy.PValue = portMediaStatus.PortBusy;
                portStatus.CueInitDone.PValue = portMediaStatus.CueInitDone;
                

                portStatus.PortDown.PValue = portHwMediaStatus.PortDown;
                portStatus.IDAdded.PValue = portHwMediaStatus.IDAdded;
                portStatus.IDDelete.PValue = portHwMediaStatus.IDDelete;
                portStatus.IDAddtoArch.PValue = portHwMediaStatus.IDAddtoArch;
                portStatus.NoRefInput.PValue = portHwMediaStatus.NoRefInput;
                portStatus.NoVideoInput.PValue = portHwMediaStatus.NoVideoInput;
                portStatus.NoAuioInput.PValue = portHwMediaStatus.NoAuioInput;
                portStatus.AudioOverLoad.PValue = portHwMediaStatus.AudioOverLoad;

                portStatus.SystemError.PValue = portStatus1.SystemError;
                portStatus.IllegalValue.PValue = portStatus1.IllegalValue;
                portStatus.InvalidPort.PValue = portStatus1.InvalidPort;
                portStatus.WrongPortType.PValue = portStatus1.WrongPortType;
                portStatus.CommandQueueFull.PValue = portStatus1.CommandQueueFull;
                portStatus.DiskFull.PValue = portStatus1.DiskFull;
                portStatus.CmdwhileBusy.PValue = portStatus1.CmdwhileBusy;
                portStatus.NotSupport.PValue = portStatus1.NotSupport;

                portStatus.InvalidId.PValue = portStatus2.InvalidId;
                portStatus.IDNotFound.PValue = portStatus2.IDNotFound;
                portStatus.IDAlreadyExists.PValue = portStatus2.IDAlreadyExists;
                portStatus.IDStilRecording.PValue = portStatus2.IDStilRecording;
                portStatus.IDStillPlaying.PValue = portStatus2.IDStillPlaying;
                portStatus.IDNotTransferredFromaArchive.PValue = portStatus2.IDNotTransferredFromaArchive;
                portStatus.IDNotTransferredToArchive.PValue = portStatus2.IDNotTransferredToArchive;
                portStatus.IDDeleteProtected.PValue = portStatus2.IDDeleteProtected;

                portStatus.NotInCue.PValue = portStatus3.NotInCue;
                portStatus.InitState.PValue = portStatus3.InitState;
                portStatus.CueNotDone.PValue = portStatus3.CueNotDone;
                portStatus.PortNotIdle.PValue = portStatus3.PortNotIdle;
                portStatus.PortPlaying.PValue = portStatus3.PortPlaying;
                portStatus.Active.PValue = portStatus3.Active;
                portStatus.PortNotAchive.PValue = portStatus3.PortNotAchive;
                portStatus.CueOrOperAtionfalied.PValue = portStatus3.CueOrOperAtionfalied;
                portStatus.NetWorKError.PValue = portStatus3.NetWorKError;
                portStatus.SystemReBooted.PValue = portStatus3.SystemReBooted;

                portStatus.Off.PValue = portSetting.Off;
                portStatus.Composite.PValue = portSetting.Composite;
                portStatus.SVidoe.PValue = portSetting.SVidoe;
                portStatus.Yuv.PValue = portSetting.Yuv;
                portStatus.D1.PValue = portSetting.D1;


                byte[] statusResult = new byte[8];
                statusResult = vdcpCommandAdapter.SetPortStatusDataProperty(portStatus);
                portStatusResult = vdcpCommandAdapter.MakeCommand(EumCommandKey.PORTSTATUS,
                                                            statusResult, statusResult.Length);

            }

            return portStatusResult;
        }

        public static byte[] GetTimeCode(EumTimeCodeType type)
        {
            byte[] bresult = null;

            lock (obj)
            {
                byte[] timecode = vdcpCommandAdapter
                    .ConvertStringToUpSideDownTCByteArray(
                    type == EumTimeCodeType.CurrentTime 
                    ? ServerStatusData.CurrentTimeCode
                    : ServerStatusData.RemainingTimeCode);

                bresult = vdcpCommandAdapter.MakeCommand(EumCommandKey.POSTIONREQUEST, timecode, timecode.Length);
            }

            return bresult;
        }

    }
}
