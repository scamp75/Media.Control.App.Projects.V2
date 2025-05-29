using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VdcpService.lib;

namespace Vdcp.Service.App
{
 
    public delegate void VdcpConnecterEventHandlerArg(VdcpEventArgsDefine commandData);

    public class VdcpService : IDisposable
    {
        public string Name { get; private set; }
        public event VdcpConnecterEventHandlerArg VdcpActionEvent;

        public EnuLoggerLevel LoggerLevet
        {
            get { return Logger.LoggerLevel; }
            set { Logger.LoggerLevel = value; }
        }

        private SerialConnecterAdapter VdcpConnect = null;

        public string AtcivClip { get; set; }

        #region 생성 및 초기화
        public VdcpService(string name)
        {
            try
            {
                Name = name;
                VdcpConnect = new SerialConnecterAdapter();
                VdcpConnect.EventActionCallbacks += VdcpConnect_EventActionCallbacks;
            }
            catch(Exception ex)
            {

            }
        }

        private void VdcpConnect_EventActionCallbacks(VdcpEventArgsDefine commandData)
        {
            if (VdcpActionEvent != null)
                VdcpActionEvent(commandData);
        }
        #endregion

        #region VDCP Command


        /// <summary>
        /// VDPC 초기화 및 시리얼 포트명 설정
        /// </summary>
        /// <param name="portName">IP Address , Serial 명 ex) COM3 / 127.0.0.1 </param>
        /// <param name="portNum"> Port 번호 지정 
        /// <param name="StatusLog">Port Status Log 작성 유무</param>
        /// <returns></returns>
        public bool Open(EnuPortType portType, string Name, int portNum, bool StatusLog)
        {
            bool result = false;

            if (VdcpConnect.Open(portType, Name, portNum, StatusLog))
                result = true;


            return result;
        }

        /// <summary>
        /// VDPC Close
        /// </summary>
        public void Close()
        {
            VdcpConnect.Close();
        }

        /// <summary>
        /// 영상 리스트  (최초 1번만 불리고 Next 호출함)
        /// </summary>
        /// <param name="ClipList">전체 영상 리스트</param>
        /// <param name="sendType"> Normal    : Clip Length 8보다 같거나 작다.
        ///                         Expansion : Clip Length 8보다 크다 </param>
        /// <returns></returns>
        public bool List(List<string> ClipList, SendType sendType)
        {
            return VdcpConnect.List(ClipList, sendType);
        }

        /// <summary>
        /// List 이후 나머지 영상 정보 10개식...
        /// (10 이상 보내면 리턴값 false)
        /// </summary>
        /// <param name="ClipList">전체 영상 리스트</param>
        /// <param name="sendType"> Normal    : Clip Length 8보다 같거나 작다.
        ///                         Expansion : Clip Length 8보다 크다 </param>

        /// <returns></returns>
        public bool Next(List<string> ClipList, SendType sendType)
        {
            return VdcpConnect.Next(ClipList, sendType);
        }

        /// <summary>
        /// 현재 진행 중인 미디어에 남은 시간
        /// </summary>
        public string RemainingTimeCode
        {
            get { return ServerStatusData.RemainingTimeCode; }
            set { ServerStatusData.RemainingTimeCode = value; }
        }

        /// <summary>
        /// 현재 진행 중인 진행 시간
        /// </summary>
        public string CurrentTimeCode
        {
            get { return ServerStatusData.CurrentTimeCode; }
            set { ServerStatusData.CurrentTimeCode = value; }
        }


        /// <summary>
        /// 영상의 존재 유무 확인
        /// sendType : Clip Length 9보다 작은면 Normal, 크면 Expansion  
        /// </summary>
        /// <param name="status">존재 유무</param>
        /// <param name="sendType"> Normal    : Clip Length 8보다 같거나 작다.
        ///                         Expansion : Clip Length 8보다 크다 </param>
        /// <returns></returns>
        public bool IDRequest(bool status, SendType sendType)
        {
            return VdcpConnect.IDRequest(status, sendType);
        }

        /// <summary>
        /// 영상의 길이 정보 리턴 (ex: 00:00:00:00)
        /// </summary>
        /// <param name="timeCode"></param>
        /// <param name="sendType"> Normal    : Clip Length 8보다 같거나 작다.
        ///                         Expansion : Clip Length 8보다 크다 </param>
        /// <returns></returns>
        public bool IDSizeRequest(string timeCode, SendType sendType)
        {
            return VdcpConnect.IDSizeRequest(timeCode, sendType);
        }

        public bool ActiveIDRequest(string AtcivClip, SendType sendType)
        {
            return VdcpConnect.ActiveIDRequest(AtcivClip, sendType);
        }


        public bool SystemStatus()
        {
            return VdcpConnect.SystemStatus();
        }
        #endregion

        #region Set StateFlagStatus
        public void SetStateFlagStatus(params EumPortMediaStatus[] portMediaStatus)
        {
            ServerStatusData.portMediaStatus.Idle = false;
            ServerStatusData.portMediaStatus.CueInit = false;
            ServerStatusData.portMediaStatus.PlayRecord = false;
            ServerStatusData.portMediaStatus.Still = false;
            ServerStatusData.portMediaStatus.Jog = false;
            ServerStatusData.portMediaStatus.VarPlay = false;
            ServerStatusData.portMediaStatus.PortBusy = false;
            ServerStatusData.portMediaStatus.CueInitDone = false;

            foreach (EumPortMediaStatus status in portMediaStatus)
            {
                switch (status)
                {
                    case EumPortMediaStatus.Idle: ServerStatusData.portMediaStatus.Idle = true; break;
                    case EumPortMediaStatus.CueInit: ServerStatusData.portMediaStatus.CueInit = true; break;
                    case EumPortMediaStatus.PlayRecord: ServerStatusData.portMediaStatus.PlayRecord = true; break;
                    case EumPortMediaStatus.Still: ServerStatusData.portMediaStatus.Still = true; break;
                    case EumPortMediaStatus.Jog: ServerStatusData.portMediaStatus.Jog = true; break;
                    case EumPortMediaStatus.VarPlay: ServerStatusData.portMediaStatus.VarPlay = true; break;
                    case EumPortMediaStatus.PortBusy: ServerStatusData.portMediaStatus.PortBusy = true; break;
                    case EumPortMediaStatus.CueInitDone: ServerStatusData.portMediaStatus.CueInitDone = true; break;
                }
            }
        }

        #endregion

        #region Set PortHWMediaStatus
        public void SetPortHwMediaStatus(params EumPortHwMediaStatus[] portHwMediaStatus)
        {
            ServerStatusData.portHwMediaStatus.Normal = false;
            ServerStatusData.portHwMediaStatus.PortDown = false;
            ServerStatusData.portHwMediaStatus.IDAdded = false;
            ServerStatusData.portHwMediaStatus.IDDelete = false;
            ServerStatusData.portHwMediaStatus.IDDeleteArch = false;
            ServerStatusData.portHwMediaStatus.IDAddtoArch = false;
            ServerStatusData.portHwMediaStatus.NoRefInput = false;
            ServerStatusData.portHwMediaStatus.NoVideoInput = false;
            ServerStatusData.portHwMediaStatus.NoAuioInput = false;
            ServerStatusData.portHwMediaStatus.AudioOverLoad = false;

            foreach (EumPortHwMediaStatus status in portHwMediaStatus)
            {
                switch (status)
                {
                    case EumPortHwMediaStatus.PortDown: ServerStatusData.portHwMediaStatus.PortDown = true; break;
                    case EumPortHwMediaStatus.IDAdded: ServerStatusData.portHwMediaStatus.IDAdded = true; break;
                    case EumPortHwMediaStatus.IDDelete: ServerStatusData.portHwMediaStatus.IDDelete = true; break;
                    case EumPortHwMediaStatus.IDDeleteArch: ServerStatusData.portHwMediaStatus.IDDeleteArch = true; break;
                    case EumPortHwMediaStatus.IDAddtoArch: ServerStatusData.portHwMediaStatus.IDAddtoArch = true; break;
                    case EumPortHwMediaStatus.NoRefInput: ServerStatusData.portHwMediaStatus.NoRefInput = true; break;
                    case EumPortHwMediaStatus.NoVideoInput: ServerStatusData.portHwMediaStatus.NoVideoInput = true; break;
                    case EumPortHwMediaStatus.NoAuioInput: ServerStatusData.portHwMediaStatus.NoAuioInput = true; break;
                    case EumPortHwMediaStatus.AudioOverLoad: ServerStatusData.portHwMediaStatus.AudioOverLoad = true; break;
                }

            }
        }
        #endregion

        #region Set PortStatus1
        public void SetPortStatus1(params EumPortStatus1[] portStatus1)
        {
            ServerStatusData.portStatus1.SystemError = false;
            ServerStatusData.portStatus1.IllegalValue = false;
            ServerStatusData.portStatus1.InvalidPort = false;
            ServerStatusData.portStatus1.WrongPortType = false;
            ServerStatusData.portStatus1.CommandQueueFull = false;
            ServerStatusData.portStatus1.DiskFull = false;
            ServerStatusData.portStatus1.CmdwhileBusy = false;
            ServerStatusData.portStatus1.NotSupport = false;


            foreach (EumPortStatus1 status in portStatus1)
            {
                switch (status)
                {
                    case EumPortStatus1.SystemError: ServerStatusData.portStatus1.SystemError = true; break;
                    case EumPortStatus1.IllegalValue: ServerStatusData.portStatus1.IllegalValue = true; break;
                    case EumPortStatus1.InvalidPort: ServerStatusData.portStatus1.InvalidPort = true; break;
                    case EumPortStatus1.WrongPortType: ServerStatusData.portStatus1.WrongPortType = true; break;
                    case EumPortStatus1.CommandQueueFull: ServerStatusData.portStatus1.CommandQueueFull = true; break;
                    case EumPortStatus1.DiskFull: ServerStatusData.portStatus1.DiskFull = true; break;
                    case EumPortStatus1.CmdWhileBusy: ServerStatusData.portStatus1.CmdwhileBusy = true; break;
                    case EumPortStatus1.NotSupport: ServerStatusData.portStatus1.NotSupport = true; break;
                }

            }
        }
        #endregion

        #region Set PortStatus2
        public void SetPortStatus2(params EumPortStatus2[] portStatus2)
        {

            ServerStatusData.portStatus2.InvalidId = false;
            ServerStatusData.portStatus2.IDNotFound = false;
            ServerStatusData.portStatus2.IDAlreadyExists = false;
            ServerStatusData.portStatus2.IDStilRecording = false;
            ServerStatusData.portStatus2.IDStillPlaying = false;
            ServerStatusData.portStatus2.IDNotTransferredFromaArchive = false;
            ServerStatusData.portStatus2.IDNotTransferredToArchive = false;
            ServerStatusData.portStatus2.IDDeleteProtected = false;

            foreach (EumPortStatus2 status in portStatus2)
            {
                switch (status)
                {
                    case EumPortStatus2.InvalidId: ServerStatusData.portStatus2.InvalidId = false; break;
                    case EumPortStatus2.IDNotFound: ServerStatusData.portStatus2.IDNotFound = false; break;
                    case EumPortStatus2.IDAlreadyExists: ServerStatusData.portStatus2.IDAlreadyExists = false; break;
                    case EumPortStatus2.IDStilRecording: ServerStatusData.portStatus2.IDStilRecording = false; break;
                    case EumPortStatus2.IDStillPlaying: ServerStatusData.portStatus2.IDStillPlaying = false; break;
                    case EumPortStatus2.IDNotTransferredFromaArchive: ServerStatusData.portStatus2.IDNotTransferredFromaArchive = false; break;
                    case EumPortStatus2.IDNotTransferredToArchive: ServerStatusData.portStatus2.IDNotTransferredToArchive = false; break;
                    case EumPortStatus2.IDDeleteProtected: ServerStatusData.portStatus2.IDDeleteProtected = false; break;
                }
            }
        }
        #endregion

        #region Set PortStatus3
        public void SetPortStatus3(params EumPortStatus3[] portStatus3)
        {
            ServerStatusData.portStatus3.NotInCue = false;
            ServerStatusData.portStatus3.InitState = false;
            ServerStatusData.portStatus3.CueNotDone = false;
            ServerStatusData.portStatus3.PortNotIdle = false;
            ServerStatusData.portStatus3.PortPlaying = false;
            ServerStatusData.portStatus3.Active = false;
            ServerStatusData.portStatus3.PortNotAchive = false;
            ServerStatusData.portStatus3.CueOrOperAtionfalied = false;
            ServerStatusData.portStatus3.NetWorKError = false;
            ServerStatusData.portStatus3.SystemReBooted = false;

            foreach (EumPortStatus3 status in portStatus3)
            {
                switch (status)
                {
                    case EumPortStatus3.NotInCue: ServerStatusData.portStatus3.NotInCue = false; break;
                    case EumPortStatus3.InitState: ServerStatusData.portStatus3.InitState = false; break;
                    case EumPortStatus3.CueNotDone: ServerStatusData.portStatus3.CueNotDone = false; break;
                    case EumPortStatus3.PortNotIdle: ServerStatusData.portStatus3.PortNotIdle = false; break;
                    case EumPortStatus3.PortPlaying: ServerStatusData.portStatus3.PortPlaying = false; break;
                    case EumPortStatus3.Active: ServerStatusData.portStatus3.Active = false; break;
                    case EumPortStatus3.PortNotAchive: ServerStatusData.portStatus3.PortNotAchive = false; break;
                    case EumPortStatus3.CueOrOperAtionfalied: ServerStatusData.portStatus3.CueOrOperAtionfalied = false; break;
                    case EumPortStatus3.NetWorKError: ServerStatusData.portStatus3.NetWorKError = false; break;
                    case EumPortStatus3.SystemReBooted: ServerStatusData.portStatus3.SystemReBooted = false; break;
                }
            }
        }
        #endregion

        #region 상태정보 설정


        /// <summary>
        /// STOP 인 경우 PORT 초기화 상태
        /// The system is in the IDLE state. The output is black and there is no signal port activity.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_Idle
        {
            get { return ServerStatusData.portMediaStatus.Idle; }
            set
            {
                ServerStatusData.portMediaStatus.Idle = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// OUTPUT PORT  [ PLAY CUE, CUE WITH DATA , ] 신호가 들어 와
        /// PORT에 CUEING 진행중 인 경우 설정
        /// The system is in the cue or record init state (cueing or initializing).
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_CueInit
        {
            get { return ServerStatusData.portMediaStatus.CueInit; }
            set
            {
                ServerStatusData.portMediaStatus.CueInit = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// OUTPUT PORT  [ PLAY ] 신호가 들어 온 경우
        /// PORT의 CUE 영상이 송출이 되면 설정
        /// PLAY/RECORD - The system is in the play or record state.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_PlayRecord
        {
            get { return ServerStatusData.portMediaStatus.PlayRecord; }
            set
            {
                ServerStatusData.portMediaStatus.PlayRecord = value;
                VdcpConnect.PortStatus();
            }
        }



        /// <summary>
        /// OUTPUT PORT  [ STILL ] 신호가 들어 오면 설정
        /// STILL - The system is in the Still state.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_Still
        {
            get { return ServerStatusData.portMediaStatus.Still; }
            set
            {
                ServerStatusData.portMediaStatus.Still = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// OUTPUT PORT  [ JOG ] 신호가 들어 오면 설정
        /// JOG - The system is in the jog state.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_Jog
        {
            get { return ServerStatusData.portMediaStatus.Jog; }
            set
            {
                ServerStatusData.portMediaStatus.Jog = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// OUTPUT PORT  [ VARPLAY ] 신호가 들어 오면 설정
        /// VARIABLE PLAY – Can be set in addition to the PLAY/RECORD bit, but can not be set alone.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_VarPlay
        {
            get { return ServerStatusData.portMediaStatus.VarPlay; }
            set
            {
                ServerStatusData.portMediaStatus.VarPlay = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// 해당 PORT가 다른 일을 진행 중인 경우 설정
        /// The system is in the busy state and can only accept immediate
        /// commands and status requests: 
        /// STOP, PLAY, RECORD, STILL, STEP, CONTINUE, PORT STATUS, SYSTEM STATUS
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_PortBusy
        {
            get { return ServerStatusData.portMediaStatus.PortBusy; }
            set
            {
                ServerStatusData.portMediaStatus.PortBusy = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// INPUT PORT [ RECORD INIT, RECORD INIT WITH DATA  ] 신회가 들어와
        /// PORT가 INIT이 완료 되면 설정
        /// CUE/INIT-DONE - The play cue or record init has been completed.The signal port
        ///                 can now accept a PLAY or RECORD command.
        /// </summary>
        /// <param name="state"></param>
        public bool MediaStatus_CueInitDone
        {
            get { return ServerStatusData.portMediaStatus.CueInitDone; }
            set
            {
                ServerStatusData.portMediaStatus.CueInitDone = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The selected port is inoperative
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_PortDown
        {
            get { return ServerStatusData.portHwMediaStatus.PortDown; }
            set
            {
                ServerStatusData.portHwMediaStatus.PortDown = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        ///  New ID’s have been added to the disk system by recording or transferring from an
        ///  archive system, and the controller of the port has not yet asked for that list of the ID’s
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_IDAdded
        {
            get { return ServerStatusData.portHwMediaStatus.IDAdded; }
            set
            {
                ServerStatusData.portHwMediaStatus.IDAdded = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// ID’s have been deleted from the disk and the controller of the port has not yet asked
        /// for that list of ID’s
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_IDDelete
        {
            get { return ServerStatusData.portHwMediaStatus.IDDelete; }
            set
            {
                ServerStatusData.portHwMediaStatus.IDDelete = value;
                VdcpConnect.PortStatus();
            }
        }


        /// <summary>
        /// ID’s have been deleted from the disk and the controller of the port has not yet asked
        /// for that list of ID’s
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_IDDeleteArch
        {
            get { return ServerStatusData.portHwMediaStatus.IDDeleteArch; }
            set
            {
                ServerStatusData.portHwMediaStatus.IDDeleteArch = value;
                VdcpConnect.PortStatus();
            }
        }


        /// <summary>
        /// New ID’s have been added to an archive system connected to the disk system and the
        /// controller of the port has not yet asked for that list of the ID’s
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_IDAddtoArch
        {
            get { return ServerStatusData.portHwMediaStatus.IDAddtoArch; }
            set
            {
                ServerStatusData.portHwMediaStatus.IDAddtoArch = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The system has no input reference
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_NoRefInput
        {
            get { return ServerStatusData.portHwMediaStatus.NoRefInput; }
            set
            {
                ServerStatusData.portHwMediaStatus.NoRefInput = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The port has no video input (input port only).
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_NoVideoInput
        {
            get { return ServerStatusData.portHwMediaStatus.NoVideoInput; }
            set
            {
                ServerStatusData.portHwMediaStatus.NoVideoInput = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The port has no audio input (input port only).
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_NoAuioInput
        {
            get { return ServerStatusData.portHwMediaStatus.NoAuioInput; }
            set
            {
                ServerStatusData.portHwMediaStatus.NoAuioInput = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The audio level in is beyond limit (input port only).
        /// </summary>
        /// <param name="state"></param>
        public bool HwMediaStatus_AudioOverLoad
        {
            get { return ServerStatusData.portHwMediaStatus.AudioOverLoad; }
            set
            {
                ServerStatusData.portHwMediaStatus.AudioOverLoad = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The system has detected a functional error.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_SystemError
        {
            get { return ServerStatusData.portStatus1.SystemError; }
            set
            {
                ServerStatusData.portStatus1.SystemError = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The system has received a command with an illegal value, e.g. NEW COPY, SORT
        /// MODE, CLOSE PORT, SELECT PORT, RECORD INIT, % TO SIGNAL FULL, VIDEO
        /// COMPRESSION RATE, AUDIO SAMPLE RATE, AUDIO COMPRESSION RATE,
        /// AUDIO IN LEVEL, AUDIO OUT LEVEL, SELECT OUTPUT, SELECT INPUT,
        /// RECORD MODE, SC ADJUST, HORIZONTAL POSITION ADJUST, OPEN PORT. -
        /// Command Ignored
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_IllegalValue
        {
            get { return ServerStatusData.portStatus1.IllegalValue; }
            set
            {
                ServerStatusData.portStatus1.IllegalValue = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The communications port has selected a signal port that it may not open because the
        /// port is already open and locked or it is an invalid port number. - Signal port commands will not be executed.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_InvalidPort
        {
            get { return ServerStatusData.portStatus1.InvalidPort; }
            set
            {
                ServerStatusData.portStatus1.InvalidPort = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The controlling device has issued a command not applicable to the open port , e.g.
        /// RECORD, RECORD INIT, FREEZE, UNFREEZE to a signal output port or PLAY CUE,
        /// CUE WITH DATA, PLAY, STILL STEP CONTINUE JOG, VARIABLE PLAY to a
        /// signal input port. - Command Ignored
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_WrongPortType
        {
            get { return ServerStatusData.portStatus1.WrongPortType; }
            set
            {
                ServerStatusData.portStatus1.WrongPortType = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The disk can not process the command because it has too many commands pending.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_CommandQueueFull
        {
            get { return ServerStatusData.portStatus1.CommandQueueFull; }
            set
            {
                ServerStatusData.portStatus1.CommandQueueFull = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The disk is unable to store any more audio/video data, e.g. RECORD INIT when not
        /// enough storage space to record for duration specified in RECORD INIT command. - Command Ignored
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_DiskFull
        {
            get { return ServerStatusData.portStatus1.DiskFull; }
            set
            {
                ServerStatusData.portStatus1.DiskFull = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command, other than an Immediate Command was issued while the busy bit was  set. - Command Ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_CmdwhileBusy
        {
            get { return ServerStatusData.portStatus1.CmdwhileBusy; }
            set
            {
                ServerStatusData.portStatus1.CmdwhileBusy = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that is not supported by the device. Any optional command. -  Command Ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus1_NotSupport
        {
            get { return ServerStatusData.portStatus1.NotSupport; }
            set
            {
                ServerStatusData.portStatus1.NotSupport = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// An invalid ID was specified in a command with an ID parameter, e.g. PLAY CUE,
        /// CUE WITH DATA, NEW COPY, DELETE, GET FROM ARCHIVE, SEND TO
        /// ARCHIVE, DELETE FROM ARCHIVE, DELETE PROTECT, UNDELETE PROTECT,
        /// ID SIZE REQUEST. - Command Ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_InvalidId
        {
            get { return ServerStatusData.portStatus2.InvalidId; }
            set
            {
                ServerStatusData.portStatus2.InvalidId = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// The ID was not found, e.g. PLAY CUE, CUE WITH DATA, NEW COPY,
        /// DELETE, GET FROM ARCHIVE, SEND TO ARCHIVE, DELETE FROM ARCHIVE,
        /// DELETE PROTECT, UNDELETE PROTECT, ID SIZE REQUEST. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool Status2_IDNotFound
        {
            get { return ServerStatusData.portStatus2.IDNotFound; }
            set
            {
                ServerStatusData.portStatus2.IDNotFound = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// An ID specified in RECORD INIT was already in the system. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDAlreadyExists
        {
            get { return ServerStatusData.portStatus2.IDAlreadyExists; }
            set
            {
                ServerStatusData.portStatus2.IDAlreadyExists = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued for an ID while that ID was still recording that cannot be
        /// performed until that ID is done recording, i.e.PLAY CUE, CUE WITH DATA, DELETE
        /// PROTECT ID, NEW COPY, DELETE, SEND TO ARCHIVE, GET FROM ARCHIVE,
        ///ID SIZE REQUEST. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDStilRecording
        {
            get { return ServerStatusData.portStatus2.IDStilRecording; }
            set
            {
                ServerStatusData.portStatus2.IDStilRecording = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A DELETE command was issued while the ID was playing. - Command Ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDStillPlaying
        {
            get { return ServerStatusData.portStatus2.IDStillPlaying; }
            set
            {
                ServerStatusData.portStatus2.IDStillPlaying = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A PLAY CUE or CUE WITH DATA command was issued before ID has been
        /// transferred from Archive. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDNotTransferredFromaArchive
        {
            get { return ServerStatusData.portStatus2.IDNotTransferredFromaArchive; }
            set
            {
                ServerStatusData.portStatus2.IDNotTransferredFromaArchive = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A GET FROM ARCHIVE command was issued for an ID that is already in the disk. -  Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDNotTransferredToArchive
        {
            get { return ServerStatusData.portStatus2.IDNotTransferredToArchive; }
            set
            {
                ServerStatusData.portStatus2.IDNotTransferredToArchive = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A DELETE command was issued for an ID that is delete protected. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus2_IDDeleteProtected
        {
            get { return ServerStatusData.portStatus2.IDDeleteProtected; }
            set
            {
                ServerStatusData.portStatus2.IDDeleteProtected = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the system to be in the cue state (cueing state -
        /// no commands require the disk to be in this state currently).
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_NotInCue
        {
            get { return ServerStatusData.portStatus3.NotInCue; }
            set
            {
                ServerStatusData.portStatus3.NotInCue = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the system to be in the cue state (cueing state -
        /// no commands require the disk to be in this state currently).
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_InitState
        {
            get { return ServerStatusData.portStatus3.InitState; }
            set
            {
                ServerStatusData.portStatus3.InitState = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the system to be in the cue/init done state, e.g.
        /// PLAY, RECORD, JOG, VARIABLE PLAY, STEP, CONTINUE, FREEZE, UNFREEZE. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_CueNotDone
        {
            get { return ServerStatusData.portStatus3.CueNotDone; }
            set
            {
                ServerStatusData.portStatus3.CueNotDone = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the system to be in the idle state, e.g.RECORD
        /// INIT in some disks. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_PortNotIdle
        {
            get { return ServerStatusData.portStatus3.PortNotIdle; }
            set
            {
                ServerStatusData.portStatus3.PortNotIdle = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        ///A command was issued that required the signal port to not be in the play state. (No
        /// command required, not to be in the play state at this time.)
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_PortPlaying
        {
            get { return ServerStatusData.portStatus3.PortPlaying; }
            set
            {
                ServerStatusData.portStatus3.PortPlaying = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the signal port to not be in the play state. (No
        /// command required, not to be in the play state at this time.)
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_Active
        {
            get { return ServerStatusData.portStatus3.Active; }
            set
            {
                ServerStatusData.portStatus3.Active = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A command was issued that required the signal port to be playing, recording, or active
        /// (not idle), e.g.STILL, STEP, CONTINUE, JOG, VARIABLE PLAY, POSITION
        ///REQUEST, ACTIVE ID REQUEST, PLAY, RECORD, FREEZE, UNFREEZE, STOP. - Command ignored.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_PortNotAchive
        {
            get { return ServerStatusData.portStatus3.PortNotAchive; }
            set
            {
                ServerStatusData.portStatus3.PortNotAchive = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// A CUE command or other command that has been ACKed and started failed for some
        /// unknown reason. - Command will not be executed properly.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_CueOrOperAtionfalied
        {
            get { return ServerStatusData.portStatus3.CueOrOperAtionfalied; }
            set
            {
                ServerStatusData.portStatus3.CueOrOperAtionfalied = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// Indicates a file transfer was not done because of a network error.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_NetWorKError
        {
            get { return ServerStatusData.portStatus3.NetWorKError; }
            set
            {
                ServerStatusData.portStatus3.NetWorKError = value;
                VdcpConnect.PortStatus();
            }
        }

        /// <summary>
        /// Indicates the disk system was rebooted. The controller needs to do a PORT OPEN
        ///  and SELECT command and any other start up command sequence.
        /// </summary>
        /// <param name="state"></param>
        public bool PortStatus3_SystemReBooted
        {
            get { return ServerStatusData.portStatus3.SystemReBooted; }
            set
            {
                ServerStatusData.portStatus3.SystemReBooted = value;
                VdcpConnect.PortStatus();
            }
        }

        #endregion

        #region 시스템 상태 정보 설정
        /// <summary>
        /// Storad ID
        /// </summary>
        /// <param name="storedId"></param>
        public string SystemStatus_StoredId
        {
            get { return ServerStatusData.systemStatusData.StoredId; }
            set { ServerStatusData.systemStatusData.StoredId = value; }
        }

        /// <summary>
        /// 동영상 저장 총 남은시간
        /// </summary>
        /// <param name="timecode"></param>
        public string SystemStatus_TotalRemainTime
        {
            get { return ServerStatusData.systemStatusData.TotalRemainTime; }
            set { ServerStatusData.systemStatusData.TotalRemainTime = value; }

        }

        public string SystemStatus_LargestBlock
        {
            get { return ServerStatusData.systemStatusData.LargestBlock; }
            set { ServerStatusData.systemStatusData.LargestBlock = value; }

        }


        /// <summary>
        /// Signal Full Level
        /// </summary>
        /// <param name="signalfull"></param>
        public int SystemStatus_SignalFullLevel
        {
            get { return ServerStatusData.systemStatusData.SignalFullLevel; }
            set { ServerStatusData.systemStatusData.SignalFullLevel = value; }
        }

        /// <summary>
        /// Standard Time
        /// </summary>
        public string SystemStatus_StandardTime
        {
            get { return ServerStatusData.systemStatusData.StandarTime; }
            set { ServerStatusData.systemStatusData.StandarTime = value; }
        }


        public bool SystemStatus_DiskFull
        {
            get { return ServerStatusData.systemStatusData.DiskFull; }
            set { ServerStatusData.systemStatusData.DiskFull = value; }
        }

        public bool SystemStatus_SystemDown
        {
            get { return ServerStatusData.systemStatusData.SystemDown; }
            set { ServerStatusData.systemStatusData.SystemDown = value; }
        }

        public bool SystemStatus_DiskDown
        {
            get { return ServerStatusData.systemStatusData.DiskDown; }
            set { ServerStatusData.systemStatusData.DiskDown = value; }
        }

        public bool SystemStatus_RemoteControlDisabled
        {
            get { return ServerStatusData.systemStatusData.RemoteControlDisabled; }
            set { ServerStatusData.systemStatusData.RemoteControlDisabled = value; }
        }

        public bool SystemStatus_ArchiveAvailable
        {
            get { return ServerStatusData.systemStatusData.RemoteControlDisabled; }
            set { ServerStatusData.systemStatusData.RemoteControlDisabled = value; }
        }

        public bool SystemStatus_ArchiveFull
        {
            get { return ServerStatusData.systemStatusData.ArchiveFull; }
            set { ServerStatusData.systemStatusData.ArchiveFull = value; }
        }

        public bool SystemStatus_LcoalOffline
        {
            get { return ServerStatusData.systemStatusData.LocalOffline; }
            set { ServerStatusData.systemStatusData.LocalOffline = value; }
        }

        public bool SystemStatus_Systemoffline
        {
            get { return ServerStatusData.systemStatusData.SystemOffline; }
            set { ServerStatusData.systemStatusData.SystemOffline = value; }
        }

        public bool SystemStatus_LocalofflineFull
        {
            get { return ServerStatusData.systemStatusData.LocalOfflineFull; }
            set { ServerStatusData.systemStatusData.LocalOfflineFull = value; }
        }

        public bool SystemStatus_SystemOfflineFull
        {
            get { return ServerStatusData.systemStatusData.SystemOfflineFull; }
            set { ServerStatusData.systemStatusData.SystemOfflineFull = value; }
        }

        #endregion

        #region Vedio Compression 설정
        public bool PortSetting_Off
        {
            get { return ServerStatusData.portSetting.Off; }
            set { ServerStatusData.portSetting.Off = value; }
        }
        public bool PortSetting_Composite
        {
            get { return ServerStatusData.portSetting.Composite; }
            set { ServerStatusData.portSetting.Composite = value; }
        }
        public bool PortSetting_SVidoe
        {
            get { return ServerStatusData.portSetting.SVidoe; }
            set { ServerStatusData.portSetting.SVidoe = value; }
        }
        public bool PortSetting_Yuv
        {
            get { return ServerStatusData.portSetting.Yuv; }
            set { ServerStatusData.portSetting.Yuv = value; }
        }
        public bool PortSetting_D1
        {
            get { return ServerStatusData.portSetting.D1; }
            set { ServerStatusData.portSetting.D1 = value; }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~VdcpConnecterLib() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }

}

