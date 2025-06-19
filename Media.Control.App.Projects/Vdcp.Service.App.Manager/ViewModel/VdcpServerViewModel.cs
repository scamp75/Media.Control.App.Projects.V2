using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ampp.Control.lib;
using Vdcp.Service.App.Manager.Model;
using Vdcp.Service.App.Manager.Model.Engine;
using VdcpService.lib;
using System.Windows.Forms;
using System.DirectoryServices;
using Microsoft.AspNetCore.SignalR.Protocol;
using Ampp.Control.lib.Model;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Media.Media3D;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class VdcpServerViewModel :  INotifyPropertyChanged
    {

        #region Properties

        private VdcpService vdcpService = null;
        
        private EngineControl amppEngin1 = null;
        private EngineControl amppEngin2  = null;
        private EngineControl amppEngin = null;
        private MediaApiConnecter ApiConnecter = null;
        private AmppControlMacro amppMacro = null;
        public bool isThreadStart = false;
        private bool isActive = false;
        private Thread AmppStateThread = null;
        private FrameSmoother FrameSmoother { get; set; }

        public event Action<string, string> OnVdcpStateChange;
        

        public MainWindowsViewModel MainWindow { get; set; } = null;

        public string CurrentTimeCode { get; set; } = "00:00:00;00";
        public string RemainingTimeCode { get; set; } = "00:00:00;00";

        public double FPS { get; set; } = 30.0;

        public TimecodeCalculator Calculator { get; set; } 

        public string SelectInput { get; set; } = "Defult";

        public double Duration { get; set; }

        public string RecoderStateus { get;set; } = "Prepared";

        public string PlayerStateus { get; set; } = "Idle";

        public string CueClipName { get; set; } = string.Empty;

        private List<string> _ClipList { get; set; } = new List<string>();

        public List<MediaDataInfo> mediaDataInfos = new List<MediaDataInfo>();

        public List<string> ClipList
        {
            get { return _ClipList; }
            set { _ClipList = value; OnPropertyChanged(nameof(ClipList)); }
        }

        public string IpAddress { get; set; } = string.Empty;

        private string _type = "Player";
        public string Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; OnPropertyChanged(nameof(ServerName)); }
        }

        private string _PortName;
        public string PortName
        {
            get { return _PortName; }
            set { _PortName = value; OnPropertyChanged(nameof(PortName)); }
        }

        private string _Macros1 { get; set; } = string.Empty;
        public string Macros1
        {
            get { return _Macros1; }
            set { _Macros1 = value; OnPropertyChanged(nameof(Macros1)); }
        }

        private string _Macros2 { get; set; } = string.Empty;
        public string Macros2
        {
            get { return _Macros2; }
            set { _Macros2 = value; OnPropertyChanged(nameof(Macros2)); }
        }

        private int _selectPort = 0;
        public int SelectPort
        {
            get { return _selectPort; }
            set { _selectPort = value; OnPropertyChanged(nameof(SelectPort)); }
        }

        private string _workLoad1 = string.Empty;

        public string WorkLoad1
        {
            get { return _workLoad1; }
            set { _workLoad1 = value; OnPropertyChanged(nameof(WorkLoad1)); }
        }

        private string _workLoad2 = string.Empty;
        public string WorkLoad2
        {
            get { return _workLoad2; }
            set { _workLoad2 = value; OnPropertyChanged(nameof(WorkLoad2)); }
        }

        private AmppConfig _amppConfig = null;
        private bool isIncreasing = true; // rewind 인경우 false 로 변경

        #endregion

        #region 초기 생성자 맟 생성자 오버로딩

        // Update the constructor of VdcpServerViewModel to explicitly call the base class constructor
        // with the required 'MainWindow' parameter.

        public VdcpServerViewModel(PortDataInfo portData) 
        {
            Type = portData.Type;
            ServerName = $"{StaticSystemCofnig.AmppConfig.ServerName}_{portData.PortName}";
            PortName = portData.PortName;
            SelectPort = Type != "Player" ? -portData.SelectPort : portData.SelectPort;
            Macros1 = portData.Macros1;
            Macros2 = portData.Macros2;
            WorkLoad1 = portData.WorkLoad1;
            WorkLoad2 = portData.WorkLoad2;
          //  AmppConfig = portData.AmppConfig;

            

            vdcpService = new VdcpService(PortName);
            vdcpService.VdcpActionEvent += VdcpServer_VdcpServerActionEvent;
            
            FrameSmoother = new FrameSmoother();
            FrameSmoother.FrameUpdated += FrameSmoother_FrameUpdated;

        }

        private void SendLog(LogType type, string message, string details = "")
        {
            MainWindow?.LoggerConnecter.Log(type.ToString(), ServerName, message, details);
        }


        private void VdcpServer_VdcpServerActionEvent(VdcpEventArgsDefine commandData)
        {
            ControlAmpp(commandData);
        }

        public bool Open( EnuPortType type, string address, int port, bool b)
        {
            bool result = false;
            IpAddress = address;

            if (vdcpService != null)
                result = vdcpService.Open(type, address, port, b);

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Open Vdcp Server {ServerName} {PortName} {type} {address}:{port} {b}", "");
            return result;
        }

        public void Close()
        {
            if (vdcpService != null) vdcpService.Close();
        }

        public async Task<bool> Start()
        {
            var result = await AmppConnect();
            isThreadStart = true;

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Start Vdcp Server {ServerName} {PortName} {Type}", "");
            return result;
        }

        public void Puase()
        {

            isThreadStart = false;
            isActive = false;
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] Pause Vdcp Server {ServerName} {PortName} {Type}", "");
        }

        public void Stop()
        {
            isThreadStart = false;
            AmppDisconnect();
        }
        #endregion

        
        /// Vdcp Event Control Methods //////////////////////////////////////////////////////////////////////////////////////
        #region Vdcp Event Control Methods
        public async Task ControlAmpp(VdcpEventArgsDefine commandData)
        {
            object objData = null;

      //      if (amppEngin == null) return;

            switch (commandData.CommandKey)
            {
                case EumCommandKey.NORMAL: break;
                case EumCommandKey.LOCALDISABLE:
                case EumCommandKey.LOCALENABLE:
                case EumCommandKey.STOP:
                    Stop(commandData.StopTime);
                    break;
                case EumCommandKey.CONTINUE:
                    Continue();
                    break;
                case EumCommandKey.PLAY:
                    Play(commandData.StatTime);
                    break;
                case EumCommandKey.RECORD:
                    Record(commandData.StatTime);
                    break;
                case EumCommandKey.FREEZE:
                case EumCommandKey.STILL:
                    Still();
                    break;
                case EumCommandKey.STEP:
                    Seek(commandData.Frames);
                    break;
                case EumCommandKey.JOG:
                    Jog(commandData.Jog);
                    break;
                case EumCommandKey.VARIPLAY:
                    Shuttle(commandData.Shuttle);
                    break;
                case EumCommandKey.UNFREEZE:
                    break;
                case EumCommandKey.EEMODE:
                    break;
                case EumCommandKey.EXRENAMEID:
                case EumCommandKey.RENAMEID:
                    ReName(commandData.OldName, commandData.ClipName);
                    break;
                case EumCommandKey.PRESETTIME:
                    break;
                case EumCommandKey.CLOSEPORT:
                    ClosePort(commandData.iPortNum);
                    break;
                case EumCommandKey.SELECTPORT:
                    SetSelectPort(commandData.iPortNum);
                    break;
                case EumCommandKey.SELECTINPUT:
                    SelectInput = commandData.Input;
                    break;
                case EumCommandKey.RECORDINIT:
                case EumCommandKey.EXRECORDINIT:
                    RecordInit( commandData.ClipName);
                    break;
                case EumCommandKey.EXPLAYCUE:
                case EumCommandKey.PLAYCUE:
                    Cue(commandData.ClipName);
                    break;
                case EumCommandKey.CUEWITHDATA:
                case EumCommandKey.EXCUEWITHDATA:
                    CueWithData(commandData.ClipName, commandData.iSom, commandData.iEom);
                    break;
                case EumCommandKey.EXDELETEID:
                case EumCommandKey.DELETEID:
                    Delete(commandData.ClipName);
                    break;
                case EumCommandKey.CLEAR:
                    break;
                case EumCommandKey.SIGNALFULL:
                    SingalFull(commandData.iValue1);
                    break;
                case EumCommandKey.RECODEINITWITHDATA:
                case EumCommandKey.EXRECODEINITWITHDATA:
                    ReCodeInitWithData( commandData.FileName, commandData.sSom, commandData.sEom);
                    break;
                case EumCommandKey.PRESET:
                    break;
                case EumCommandKey.DISKPREROLL:
                    DiskPreRoll();
                    break;
                case EumCommandKey.OPENPORT:
                    OpenPort(commandData.iPortNum);
                    break;
                case EumCommandKey.NEXT:
                    Next( SendType.Normal);break;
                case EumCommandKey.EXNEXT:
                    Next(SendType.Expansion); break;
                case EumCommandKey.LIST:
                    List(SendType.Normal); break;
                case EumCommandKey.EXLIST:
                    List(SendType.Expansion); break;
                case EumCommandKey.LAST: break;
                case EumCommandKey.PORTSTATUS:
                    break;
                case EumCommandKey.POSTIONREQUEST:
                    break;
                case EumCommandKey.SYSTEMSTATUS:
                    break;
                case EumCommandKey.EXSIZEREQUEST:
                    SizeRequest(SendType.Expansion, commandData.ClipName);
                    break;
                case EumCommandKey.SIZEREQUEST:
                    SizeRequest(SendType.Normal, commandData.ClipName);
                    break;
                case EumCommandKey.IDREQUEST:
                    IdRequest(SendType.Normal, commandData.ClipName);
                    break;
                case EumCommandKey.EXIDREQUEST:
                    IdRequest(SendType.Expansion,commandData.ClipName);
                    break;
                case EumCommandKey.ACTIVEIDREQUEST:
                    break;
                case EumCommandKey.EXACTIVEIDREQUEST:
                    break;
            }
        }
        #endregion
        /// Vdcp Command ////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Vdpc Command Methods

        private async void IdRequest(SendType type, string fileName)
        {
            bool status = ClipList?.Contains(fileName) ?? false;
            vdcpService.IDRequest(status, type);

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} ID Request File = {fileName} Status = {status}"
                            , $"{fileName}");
        }

        private async void SizeRequest(SendType type, string fileName)
        {
            var clip = mediaDataInfos.Where(x => x.Name == fileName).FirstOrDefault();

            string duration = clip != null ? clip.Duration : "00:00:00;00";
            vdcpService.IDSizeRequest(duration, type );

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Size Request File = {fileName} Duration is {duration}", $"{fileName} {duration}");
        }

        private async void Next (SendType type)
        {
            vdcpService.Next(ClipList, type);
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Get Next ClipList", $"{ClipList.Count}");
        }

        private async void List(SendType type)
        {
            vdcpService.List(ClipList, type);
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Get ClipList", $"{ClipList.Count}");
        }

        private async void OpenPort(int Port)
        {
            if (Port == SelectPort)
            {
                isActive = true;
            }
        }
        private async void DiskPreRoll()
        {
            
        }

        private async void ReCodeInitWithData( string filePath, string Som, string Eom)
        {
            RecordInit(filePath);
        }

        private async void SingalFull(int value)
        {
            //var objData = new { Value = value };
            //await amppEngin.Player(EnmPlayerControl.Signalfull, JObject.FromObject(objData));
        }

        private async void Delete(string fileName)
        {
            
        }

        private async void CueWithData(string FileName , long Som, long Eom)
        {
            Cue(FileName);
            
            var engCueControl = GetNextCueControl();

            if (engCueControl == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Cue File = {FileName} Ampp Engine is null");
                return;
            }

            var transport = new
            {
                start = "$now",
                position = Som,
                inPosition = Som,
                outPosition = Eom,
                rate = 0.0,
                endBehaviour = "repeat"
            };

            await engCueControl.Player(EnmPlayerControl.Transportcommand, JObject.FromObject(transport));
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Cue File = {FileName} Som = {Som} Eom = {Eom}"
                , $"{FileName} Som = {Som} Eom = {Eom}");
                
        }

        private async void Cue(string fileName)
        {
            if(vdcpService.MediaStatus_Idle)
                SetVdcpMedaiState("CueInit");
            else
                SetVdcpMedaiState("CueInit", "Play");

            var engCueControl = GetNextCueControl();
            
            if(engCueControl == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Cue File = {fileName} Ampp Engine is null");
                return;
            }

            var media = mediaDataInfos.Where(c => c.Name == fileName).FirstOrDefault();

            var objData = new { file = $"{media?.Path}" };
            await engCueControl.Player(EnmPlayerControl.Clip, JObject.FromObject(objData));
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Cue File = {fileName}");

            isLoadMedia = false;
            isSetFps = false;

            CueClipName = media?.Path;
            
            
        }

        private async void RecordInit(string filePath )
        {
            SetVdcpMedaiState("CueInit");

            var RecorderPrepare = new
            {
                Source = SelectInput == "Defult" 
                                ? StaticSystemCofnig.RecordeConfig.DefultInput : SelectInput,
                filename = filePath,
                Profile = StaticSystemCofnig.RecordeConfig.RecordProfile
            };

            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Prepare Source = {SelectInput} fileName {filePath} Ampp Engine is null");
                return;
            }
            else
            {
                await amppEngin?.Recoder(EnmRecoderControl.Prepare,
                    JObject.FromObject(RecorderPrepare));

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Prepare Source = {SelectInput} fileName {filePath} ");
            }
        }

        private async void SetSelectPort(int Port)
        {
            if(isActive && Port != SelectPort)
            {
                isActive = false;
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Set Select Port {Port}");
            }
        }

        private async void ClosePort(int Port)
        {
            isActive = false;

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Close Port {Port}", $"{Port}");
        }

        private async void Shuttle(double value)
        {
            SetVdcpMedaiState("Busy");
            var objData = new { Rate = value };

            if(amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Shuttle Rate = {value} Ampp Engine is null");
                return;
            }
            else
            {
                await amppEngin.Player(EnmPlayerControl.Shuttle, JObject.FromObject(objData));
            }
        }

        private async void Jog(double value)
        {
            SetVdcpMedaiState("Busy");
            var objData = new { Rate = value };
            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Jog Rate = {value} Ampp Engine is null");
                return;
            }
            else
            {
                await amppEngin.Player(EnmPlayerControl.Rate, JObject.FromObject(objData));
            }
                
        }

        private async void Seek(int Frame )
        {
            SetVdcpMedaiState("Busy");

            var objData = new { frame = Frame };
            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Seek Frame = {Frame} Ampp Engine is null");
                return;
            }
            else
            {
                await amppEngin.Player(EnmPlayerControl.Seek, JObject.FromObject(objData));
                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Seek Frame = {Frame}");
            }
        }
        private async void Continue()
        {
            SetVdcpMedaiState("Busy");

            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Continue Ampp Engine is null");
                return;
            }
            else
            {
                if (PlayerStateus == "Still")
                    await amppEngin.Player(EnmPlayerControl.Playpause);

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Continue");
            }
        }

        private async void Still()
        {
            SetVdcpMedaiState("Busy");

            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Still Ampp Engine is null");
                return;
            }
            else
            {
                if (PlayerStateus == "Play")
                    await amppEngin.Player(EnmPlayerControl.Playpause);

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Still");
            }
        }

        private async void Record(string StartTime = "")
        {
            SetVdcpMedaiState("Busy");

            if (amppEngin == null)
            {
                SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Start Ampp Engine is null");
                return;
            }
            else
            {
                string startTime = "2018-11-13T20:20:39.000Z";
               // if (StartTime != "") startTime = StartTime;

                await amppEngin.Recoder(EnmRecoderControl.Startat, JObject.FromObject(
                     new
                     {
                         Start = $"{startTime}"
                     }));

                SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Start");
            }
        }

        private async void Play(string StartTime ="")
        {
            SetVdcpMedaiState("Busy");

            ChangeEngine();

            await SendMacro(amppEngin.WorkLoad, amppEngin.Index);


            //if (amppEngin == null)
            //{
            //    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Start Ampp Engine is null");
            //    return;
            //}
            //else
            //{
            //    var objData = new { Start = $"{StartTime}" };
            //    await amppEngin.Player(EnmPlayerControl.Startat, JObject.FromObject(objData));

            //    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Start");

            //    isThreadStart = true;
            //}
        }

        private async void Stop(string stopTime = "")
        {
            var objData = new { Stop = $"2022-01-10T20:00:00.000Z" };
            if (stopTime != string.Empty)
                objData = new { Stop = $"{stopTime}" };


            SetVdcpMedaiState("Busy");
            if (Type == "Player")
            {
                var eject = new
                {
                    isCleared = true
                };

                if (amppEngin == null)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Eject Ampp Engine is null");
                    return;
                }
                else
                {
                    await amppEngin.Player(EnmPlayerControl.Clearassets, JObject.FromObject(eject));
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Eject");
                }
            }
            else
            {
                if (amppEngin == null)
                {
                    SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Stop Ampp Engine is null", $"{stopTime}");
                    return;
                }
                else
                {
                    await amppEngin.Recoder(EnmRecoderControl.Stopat, JObject.FromObject(objData));
                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Stop");
                }
            }
        }

        private void ReName(string oldName, string newName)
        {
            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} ReName OldName = {oldName} NewName = {newName}", $"{oldName}  {newName}");
        }

        private void SetVdcpMedaiState(string state1, string state2 = "")
        {
            vdcpService.MediaStatus_PortBusy = false;
            vdcpService.MediaStatus_CueInit = false;
            vdcpService.MediaStatus_CueInitDone = false;
            vdcpService.MediaStatus_Still = false;
            vdcpService.MediaStatus_PlayRecord = false;
            vdcpService.MediaStatus_Idle = false;
            vdcpService.MediaStatus_Jog = false;
            vdcpService.MediaStatus_VarPlay  = false;
           
            if( state1 == "Busy")
            {
                vdcpService.MediaStatus_PortBusy = true;
            }
            else if (state1 == "CueInit")
            {
                vdcpService.MediaStatus_PortBusy = true;
                vdcpService.MediaStatus_CueInit = true;

                if (state2 == "Play")
                    vdcpService.MediaStatus_PlayRecord = true;
            }
            else if (state1 == "CueDone")
            {
                vdcpService.MediaStatus_CueInitDone = true;

                if(state2 == "Play")
                    vdcpService.MediaStatus_PlayRecord = true;
                else 
                    vdcpService.MediaStatus_Still = true;
            }
            else if (state1 == "Record" || state1 == "Play")
            {
                vdcpService.MediaStatus_PlayRecord = true;
            }
            else if (state1 == "Still")
            {
                vdcpService.MediaStatus_PlayRecord = true;
                vdcpService.MediaStatus_Still = true;
            }
            else if (state1 == "Jog")
            {
                vdcpService.MediaStatus_Jog = true;
                vdcpService.MediaStatus_PlayRecord = true;
            }
            else if (state1 == "VarPlay")
            {
                vdcpService.MediaStatus_VarPlay = true;
                vdcpService.MediaStatus_PlayRecord = true;
            }
            else if (state1 == "Idle")
            {
                vdcpService.MediaStatus_Idle = true;
            }

            SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Media Status {state1} or {state2}");
        }

        #endregion

        /// Ampp Control////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Ampp Control Methods
        private async Task<bool> AmppConnect()
        {
            Task<bool> result = Task.FromResult(false); 

            try
            {
                if (Type == "Player")
                {
                    amppEngin1 = new EngineControl(StaticSystemCofnig.AmppConfig, WorkLoad1, ServerName, 1);
                    var connect = await amppEngin1.Connect();

                    if(connect)
                    {
                        amppEngin1.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                        amppEngin1.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                        amppEngin1.OnStateEvent += AmppEngin_OnStateEvent;
                     
                        
                        amppEngin2 = new EngineControl(StaticSystemCofnig.AmppConfig, WorkLoad2, ServerName, 2);
                        connect = await amppEngin2.Connect();

                        if (connect)
                        {
                            amppEngin2.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                            amppEngin2.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                            amppEngin2.OnStateEvent += AmppEngin_OnStateEvent;
                        }
                        else
                        {
                            SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Mode : Ampp Connect Failed WorkLoad2 = {WorkLoad2}", $"{WorkLoad2}  SelectPort = {SelectPort}");
                        }


                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Mode : Ampp Connect WorkLoad1 = {WorkLoad1}", $"{WorkLoad1}  SelectPort = {SelectPort}");

                        amppEngin = amppEngin1;


                        // 사전 macro 설정이 필요. Clip Player , Clena Cut
                        if (amppMacro == null)
                        {
                            var macros = await amppEngin.GetControlMacro();

                            foreach (var macro in macros)
                            {
                                if (macro.Name.ToLower() == ServerName.ToLower())
                                {
                                    amppMacro = macro;

                                    SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Ampp Macro Found = {macro.Name}", $"{macro.Name}");
                                    break;
                                }
                            }
                        }

                       
                        AmppStateThread = new Thread(new ThreadStart(DoAmppWork));
                        AmppStateThread.Start();

                        result = Task.FromResult(true);
                    }
                    else
                    {
                        SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Player Mode : Ampp Connect Failed WorkLoad1 = {WorkLoad1}", $"{WorkLoad1}  SelectPort = {SelectPort}");
                    }
                }
                else
                {
                    amppEngin = new EngineControl(StaticSystemCofnig.AmppConfig, WorkLoad1, ServerName, 0 );
                    var connect = await amppEngin.Connect();

                    if (connect)
                    {
                        amppEngin.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                        amppEngin.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                        amppEngin.OnStateEvent += AmppEngin_OnStateEvent;

                        SendLog(LogType.Info, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Mode : Ampp Connect WorkLoad1 = {WorkLoad1}", $"{WorkLoad1}  SelectPort = {SelectPort}");

                        result = Task.FromResult(true);
                    }
                    else
                    {
                        SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] {PortName} Recoder Mode : Ampp Connect Failed WorkLoad1 = {WorkLoad1}", $"{WorkLoad1}  SelectPort = {SelectPort}");
                    } 
                }
            }
            catch
            { 
                result = Task.FromResult(false);
            }

            return result.Result;
        }

        private async void DoAmppWork()
        {

            while (true)
            {
                if (isThreadStart)
                {
                    await amppEngin.Player(EnmPlayerControl.Getstate);
                    ////////////////////////////////////////////////////////
                }

                Thread.Sleep(500);
            }
        }
        private void AmppEngin_OnStateEvent(object? sender, string e)
        {

        }

        private void ChangeEngine()
        {
            if(vdcpService.MediaStatus_CueInitDone 
                && vdcpService.MediaStatus_PlayRecord)
            {
                amppEngin = amppEngin.EnagineName == amppEngin1.EnagineName ? amppEngin2 : amppEngin1;
            }
            else
            {
                amppEngin = amppEngin1;
            }
        }

        private EngineControl GetNextCueControl()
        {
            EngineControl Result = null;

            if (vdcpService.MediaStatus_Idle)
            {
                Result = amppEngin;
            }
            else if(vdcpService.MediaStatus_PlayRecord || vdcpService.MediaStatus_CueInitDone)
            {
                Result = amppEngin.EnagineName == amppEngin1.EnagineName ? amppEngin2 : amppEngin1;
            }
   
            return Result;
        }

        private EngineControl GetEnginControl(string enginName)
        {
            if (amppEngin1 != null && amppEngin1.EnagineName == enginName)
                return amppEngin1;
            else if (amppEngin2 != null && amppEngin2.EnagineName == enginName)
                return amppEngin2;
            return null;

        }

        public async Task SendMacro(string worklorad, int index)
        {
            amppMacro.Commands[0].Workloads.Add(worklorad);

            var payloads = new Dictionary<string, object>
                { 
                    { "Program", "true" },
                    { "Index" , index} 
                };

            amppMacro.Commands[1].Payload = payloads;

            await amppEngin.PutMacro(amppMacro.Uuid, amppMacro);
        }

        private bool isLoadMedia = false;
        private bool isSetFps = false;  
        private DateTime oldDateTime = DateTime.Now;
        private long oldFrame = 0;
        private void AmppEngin_OnAmppControlNotifyEvent(object? sender, Ampp.Control.lib.Model.AmppControlNotificationEventArgs e)
        {
            var payload = JsonConvert.SerializeObject(e.Notification.Payload);

            if (Type == "Recoder" && e.Workload == WorkLoad1)
            {
                switch (e.Command)
                {
                    case "recordconfig":

                        var config = JsonConvert.DeserializeObject<Recordconfig>(payload);

                        if (config.VideoStandard != null)
                        {
                            if (isSetFps) return;

                            switch (config.VideoStandard?.FrameRate)
                            {
                                case "24000x1001":
                                    FPS = 23.976; break;
                                case "24x1":
                                    FPS = 24.0; break;
                                case "25x1":
                                    FPS = 25; break;
                                case "30x1":
                                    FPS = 30; break;
                                case "50x1":
                                    FPS = 50; break;
                                case "30000x1001":
                                    FPS = 29.97; break;
                                case "60000x1001":
                                    FPS = 59.94; break;
                                case "60x1":
                                    FPS = 60; break;
                            }

                            Calculator = new TimecodeCalculator((decimal)FPS);
                            FrameSmoother.Initialize(0, (int)FPS);
                            isSetFps = true;
                        }

                        break;
                    case "recordingstate":
                        var state = JsonConvert.DeserializeObject<Recordingstate>(payload);

                        if (state.Source != null)
                        {
                            SelectInput = state.Source;
                        }
                        break;
                    case "recorderinfo":

                        var info = JsonConvert.DeserializeObject<RecordInfo>(payload);

                        if (info != null)
                        {
                            FrameSmoother.UpdateExternalFrame((int)Convert.ToInt32(info.RecordedFrames), 1, true);

                            if (info.State == "started")
                            {
                                if (RecoderStateus == "Prepared")
                                {
                                    RecoderStateus = "Record";
                                    FrameSmoother.Start();

                                    SetVdcpMedaiState("Record");
                                    OnVdcpStateChange(ServerName, "Recoding");

                                //    ApiConnecter
                                }

                            }
                            else if (info.State == "prepared")
                            {
                                RecoderStateus = "Prepared";
                                SetVdcpMedaiState("CueDone");
                                OnVdcpStateChange(ServerName, "Prepared");
                            }
                            else if (info.State == "ready")
                            {
                                if (RecoderStateus == "Record")
                                {
                                    //CurrentTimeCode = Calculator?.FrameNumberToTimecode((ulong)Convert.ToInt32(info.RecordedFrames));
                                    CurrentTimeCode = TimecodeConverter.FrameToTimecode((int)Convert.ToInt32(info.RecordedFrames), FPS);
                                    vdcpService.CurrentTimeCode =  CurrentTimeCode;
                                    FrameSmoother.Stop();
                                }

                                SetVdcpMedaiState("Still");
                                RecoderStateus = "Pause";
                                OnVdcpStateChange(ServerName, "Done");
                            }
                            else if (info.State == "Error")
                            {
                                RecoderStateus = "Error";
                                OnVdcpStateChange(ServerName, "Error");
                            }
                        }
                        break;
                }
            }
            else // player
            {
                if(e.Notification.Key.Contains(ServerName))
                {
                    switch (e.Command)
                    {
                        case "ping":
                            break;
                        case "clearassets":
                            var clear = JsonConvert.DeserializeObject<Model.Engine.ClaearAssets>(payload);
                            
                            if (clear.isCleared)
                            {
                                if(PlayerStateus != "Idle")
                                {
                                    SetVdcpMedaiState("Idle");
                                    PlayerStateus = "Idle";
                                }

                                FrameSmoother.Stop();
                                FrameSmoother.ResetFrame(0);

                                isLoadMedia = false;
                                isSetFps = false;
                            }

                            break;
                        case "transportcommand": // 진행 정보   2..
                            var command = JsonConvert.DeserializeObject<Model.Engine.TransportCommand>(payload);
                            FrameSmoother.UpdateExternalFrame(Convert.ToInt32(command.Position), Tick: 1, isIncreasing);

                            TimeSpan diff = DateTime.Now - oldDateTime;

                            var timecode = TimecodeConverter.FrameToTimecode((int)command.Position, FPS);

                            Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}]"
                                + $" =================>  Frame : {command.Position} " +
                                 $"Currnt : {timecode} [{strcount - command.Position}] ---->") ;
                                // $"  FrameCount  {command.Position - oldFrame}"
                                //+ $"[ {diff.Milliseconds} ]" ) ;

                            oldDateTime = DateTime.Now;
                            oldFrame = command.Position;

                            Duration = command.OutPosition - command.InPosition - 1;

                            if (double.TryParse(command.Rate, out double temp))
                            {
                                if (temp == 0)    // stop 상태
                                {
                                    SetTimecode((int)command.Position);

                                    if (command.Position != 0)
                                    {
                                        if(PlayerStateus != "Still")
                                        {
                                            //SetTimecode((int)command.Position);
                                            PlayerStateus = "Still";
                                            SetVdcpMedaiState("Still");
                                        }
                                    }
                                        
                                    if (FrameSmoother.IsRunning == true) FrameSmoother.Stop();
                                }
                                else if (temp > 0 && temp < 1) // jog 상태
                                {

                                    if(PlayerStateus != "Jog")
                                    {
                                        PlayerStateus = "Jog";
                                        SetVdcpMedaiState("Jog");
                                        FrameSmoother.isFrameSmoother = false;
                                    }

                                    //DisPlayTimcode((int)command.Position);
                                    Debug.WriteLine($"타임코드 직정입력 {command.Position}");
                                }
                                else if (temp == 1 || temp > 1)  // play 상태
                                {

                                    if(PlayerStateus != "Play")
                                    {
                                        PlayerStateus = "Play";
                                        if (temp == 1) SetVdcpMedaiState("Play");
                                        else if (temp > 1) SetVdcpMedaiState("VarPlay");
                                    }

                                    FrameSmoother.isFrameSmoother = true;
                                    if (FrameSmoother.IsRunning == false)
                                        FrameSmoother.Start();
                                }
                            }

                            break;
                        case "playerconfig": // Player 설정 정보  3...

                            if (isSetFps) return; // FPS 설정은 한번만
                            var config = JsonConvert.DeserializeObject<Model.Engine.PlayerConfig>(payload);

                            switch (config.FrameRate)
                            {
                                case "24000x1001":
                                    FPS = 23.976; break;
                                case "24x1":
                                    FPS = 24.0; break;
                                case "25x1":
                                    FPS = 25; break;
                                case "30x1":
                                    FPS = 30; break;
                                case "50x1": 
                                    FPS = 50; break;
                                case "30000x1001":
                                    FPS = 29.97; break;
                                case "60000x1001":
                                    FPS = 59.94; break;
                                case "60x1":
                                    FPS = 60; break;
                            }

                            Calculator = new TimecodeCalculator((decimal)FPS);
                            FrameSmoother.Initialize(0, (int)FPS);
                            isSetFps = true;

                            break;
                        case "clip":  //4...
                            if(isLoadMedia) return; // Clip 정보는 한번만

                            var clip = JsonConvert.DeserializeObject<Model.Engine.Clip>(payload);

                            if (clip.Loaded && !isLoadMedia && clip.File == CueClipName)
                            {
                                SetVdcpMedaiState("CueDone");
                                PlayerStateus = "CueDone";
                                isLoadMedia = true;
                            }
                            
                            break;
                        case "transportstate":
                        case "videostandard":
                        case "shuttle":
                        case "rate":
                        case "recue":
                            break;
                    }
                }
            }

            //Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] -------------------------------------------");
            //Debug.WriteLine($"Notification {e.Notification.Key}");
            //Debug.WriteLine($"Workload:\t{e.Workload}");
            //Debug.WriteLine($"Command:\t{e.Command}");
            //Debug.WriteLine($"Details:\t{e.Notification.Status}");
            //Debug.WriteLine($"Payload:\t{JsonConvert.SerializeObject(e.Notification.Payload)}");
        }

        int strcount = 0;
        private void FrameSmoother_FrameUpdated(object? sender, int e)
        {
            SetTimecode(e);
        }

        private void SetTimecode(int frame)
        {
            var timecode = TimecodeConverter.FrameToTimecode((int)frame, FPS);

            strcount = frame;

            if (timecode != null)
            {
                CurrentTimeCode = timecode;
                vdcpService.CurrentTimeCode = timecode;

                Debug.WriteLine($"[{DateTime.Now.ToString("hh:MM:ss:fff")}] <=================  " +
                      $"Frame : {frame} Currnt : {timecode} ");

                if (Duration > 0)
                {
                    RemainingTimeCode = TimecodeConverter.FrameToTimecode(Convert.ToInt32(Duration - frame), FPS);
                    vdcpService.RemainingTimeCode = RemainingTimeCode;
                }
            }
        }

        private void AmppEngin_OnAmppControlErrorEvent(object? sender, Ampp.Control.lib.AmppControlErrorEventArgs e)
        {
            SendLog(LogType.Error, $"[OnAmppControlErrorEvent] Workload : {e.Key} [{e.Workload}] Command : {e.Command}", e.Details);

            Debug.WriteLine("************Error Notification**************");
            Debug.WriteLine($"Workload:\t{e.Workload}");
            Debug.WriteLine($"Command:\t{e.Command}");
            Debug.WriteLine($"Status:\t{e.Status}");
            Debug.WriteLine($"Error:\t{e.Error}");
            Debug.WriteLine($"Details:\t{e.Details}");
            Debug.WriteLine("**************************************");
        }

        private void AmppDisconnect()
        {
            if (amppEngin != null)
            {
                if(Type == "Player")
                {
                    if (amppEngin1 != null)
                    {
                        amppEngin1.Stop();
                        amppEngin1 = null;
                    }
                    if (amppEngin2 != null)
                    {
                        amppEngin2.Stop();
                        amppEngin2 = null;
                    }

                    if(AmppStateThread != null)
                    {
                        isThreadStart = false;
                        AmppStateThread?.Join(100);
                        AmppStateThread = null;
                    }
                }
                else
                {
                    amppEngin.Stop();
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
