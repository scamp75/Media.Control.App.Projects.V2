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

namespace Vdcp.Service.App.Manager.ViewModel
{
    public class VdcpServerViewModel : INotifyPropertyChanged
    {

        #region Properties
        private VdcpService vdcpService = null;
        
        private EngineControl amppEngin1 = null;
        private EngineControl amppEngin2  = null;
        private EngineControl amppEngin = null;
        private MediaApiConnecter ApiConnecter = null;
        
        private bool isThreadStart = false;

        public string CurrentTimeCode { get; set; } = "00:00:00:00";
        public string RemainingTimeCode { get; set; } = "00:00:00:00";


        private List<string> _ClipList { get; set; } = new List<string>();

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
        private bool isIncreasing;

        public AmppConfig AmppConfig
        {
            get { return _amppConfig; }
            set { _amppConfig = value; OnPropertyChanged(nameof(AmppConfig)); }
        }
        #endregion

        #region 초기 생성자 맟 생성자 오버로딩

        public VdcpServerViewModel(PortDataInfo portData)
        {
            Type = portData.Type;
            ServerName = $"Port_{portData.PortName}";
            PortName = portData.PortName;
            SelectPort = Type != "Player" ? -portData.SelectPort : portData.SelectPort;
            Macros1 = portData.Macros1;
            Macros2 = portData.Macros2;
            WorkLoad1 = portData.WorkLoad1;
            WorkLoad2 = portData.WorkLoad2;
            AmppConfig = portData.AmppConfig;

            vdcpService = new VdcpService(PortName);
            vdcpService.VdcpActionEvent += VdcpServer_VdcpServerActionEvent;

            FrameSmoother.FrameUpdated += FrameSmoother_FrameUpdated;

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

            return result;
        }

        public void Close()
        {
            if (vdcpService != null) vdcpService.Close();
        }

        public bool Start()
        {
            var result = AmppConnect();
            isThreadStart = true;

            return result.Result;
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

            if (amppEngin == null) return;

            switch (commandData.CommandKey)
            {
                case EumCommandKey.NORMAL: break;
                case EumCommandKey.LOCALDISABLE:
                case EumCommandKey.LOCALENABLE:
                case EumCommandKey.STOP:
                    Stop(commandData.StopTime);
                    break;
                case EumCommandKey.CONTINUE:
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
                case EumCommandKey.RECORDINIT:
                case EumCommandKey.EXRECORDINIT:
                    RecordInit(commandData.Input, commandData.ClipName);
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
                    ReCodeInitWithData(commandData.Input, commandData.FileName, commandData.sSom, commandData.sEom);
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
                case EumCommandKey.SIZEREQUEST:
                    SizeRequest(commandData.ClipName);
                    break;
                case EumCommandKey.IDREQUEST:
                    IdRequest(SendType.Normal, commandData.ClipName); break;
                case EumCommandKey.EXIDREQUEST:
                    IdRequest(SendType.Expansion,commandData.ClipName); break;
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
        }

        private async void SizeRequest(string fileName)
        {
            //var objData = new { file = fileName };
            //await amppEngin.Player(EnmPlayerControl.Sizerequest, JObject.FromObject(objData));
        }

        private async void Next (SendType type)
        {
            vdcpService.Next(ClipList, type);
        }

        private async void List(SendType type)
        {
            vdcpService.List(ClipList, type);
        }

        private async void OpenPort(int Port)
        {
            if (Port == SelectPort)
            {

            }
        }
        private async void DiskPreRoll()
        {
            
        }

        private async void ReCodeInitWithData(string Input, string filePath, string Som, string Eom)
        {
           
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

            Thread.Sleep(300);
            var transport = new
            {
                start = "$now",
                position = Som,
                inPosition = Som,
                outPosition = Eom,
                rate = 0.0,
                endBehaviour = "repeat"
            };

            await amppEngin.Player(EnmPlayerControl.Transportcommand, JObject.FromObject(transport));
        }

        private async void Cue(string fileName)
        {
            var objData = new { file = fileName };
            await amppEngin.Player(EnmPlayerControl.Clip, JObject.FromObject(objData));
        }


        private async void RecordInit(string Input, string filePath)
        {
            var RecorderPrepare = new
            {
                Source = Input,
                filename = filePath
            };

           await amppEngin.Recoder(EnmRecoderControl.Prepare,
                    JObject.FromObject(RecorderPrepare));

        }

        private async void SetSelectPort(int Port)
        {
         
        }

        private async void ClosePort(int Port)
        {
          
        }

        private async void Shuttle(double value)
        {
            var objData = new { Rate = value };
            await amppEngin.Player(EnmPlayerControl.Shuttle, JObject.FromObject(objData));

        }

        private async void Jog(double value)
        {
            var objData = new { Rate = value };
            await amppEngin.Player(EnmPlayerControl.Rate, JObject.FromObject(objData));
        }

        private async void Seek(int Frame )
        {
            var objData = new { frame = Frame };
            await amppEngin.Player(EnmPlayerControl.Seek, JObject.FromObject(objData));

        }

        private async void Still()
        {
            var objData = new { Start = $"$now" };
            await amppEngin.Player(EnmPlayerControl.Stopat, JObject.FromObject(objData));
        }

        private async void Record(string StartTime = "")
        {
            string startTime = "2018-11-13T20:20:39.000Z";
            if (StartTime != "") startTime = StartTime;
            
            await amppEngin.Recoder(EnmRecoderControl.Stopat, JObject.FromObject(
                 new
                 {
                     Start = $"{startTime}"
                 } ));
        }

        private async void Play(string StartTime ="")
        {
            var objData = new { Start = $"{StartTime}" };
            await amppEngin.Player(EnmPlayerControl.Startat, JObject.FromObject(objData));

            isThreadStart = true;
        }

        private async void Stop(string stopTime = "")
        {
            var objData = new { Stop = $"2022-01-10T20:00:00.000Z" };
            if (stopTime != string.Empty)
                objData = new { Stop = $"{stopTime}" };

            if (Type == "Player")
            {
                var eject = new
                {
                    isCleared = true
                };

                await amppEngin.Player(EnmPlayerControl.Clearassets, JObject.FromObject(eject));
            }
            else
                await amppEngin.Recoder(EnmRecoderControl.Stopat, JObject.FromObject(objData));
        }

        private void ReName(string oldName, string newName)
        {

        }
        #endregion

        /// Ampp Control////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Ampp Control Methods
        private Task<bool> AmppConnect()
        {
            Task<bool> result = Task.FromResult(false); 

            try
            {
                if (Type == "Player")
                {
                    amppEngin1 = new EngineControl(AmppConfig, WorkLoad1, ServerName);

                    amppEngin1.Connect().ContinueWith(task =>
                    {
                        amppEngin1.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                        amppEngin1.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                        amppEngin1.OnStateEvent += AmppEngin_OnStateEvent;

                        result = new Task<bool>(() => task.Result);
                    });

                    amppEngin2 = new EngineControl(AmppConfig, WorkLoad2, ServerName);

                    amppEngin2.Connect().ContinueWith(task =>
                    {
                        amppEngin2.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                        amppEngin2.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                        amppEngin2.OnStateEvent += AmppEngin_OnStateEvent;

                        result = new Task<bool>(() => task.Result);
                    });

                    amppEngin = amppEngin1;

                    Task.Run(async () =>
                    {
                        while (isThreadStart)
                        {
                            await amppEngin.Player(EnmPlayerControl.Getstate);
                            Thread.Sleep(600);
                        }
                    });
                }
                else
                {
                    amppEngin = new EngineControl(AmppConfig, WorkLoad1, ServerName);

                    amppEngin.Connect().ContinueWith(task =>
                    {
                        amppEngin.OnAmppControlErrorEvent += AmppEngin_OnAmppControlErrorEvent;
                        amppEngin.OnAmppControlNotifyEvent += AmppEngin_OnAmppControlNotifyEvent;
                        amppEngin.OnStateEvent += AmppEngin_OnStateEvent;

                        result = new Task<bool>(() => task.Result);
                    });
                }
            }
            catch
            { 
            }

            return result ?? Task.FromResult(false);
        }

        private void ChangeEngine()
        {
            amppEngin = amppEngin.EnagineName == amppEngin1.EnagineName ? amppEngin2 : amppEngin1;
        }

        private void AmppEngin_OnStateEvent(object? sender, string e)
        {
            
        }

        private void AmppEngin_OnAmppControlNotifyEvent(object? sender, Ampp.Control.lib.Model.AmppControlNotificationEventArgs e)
        {
            var payload = JsonConvert.SerializeObject(e.Notification.Payload);

            if (Type == "Recoder")
            {
                switch (e.Command)
                {
                    case "recordconfig":

                        var config = JsonConvert.DeserializeObject<Recordconfig>(payload);

                        if (config != null)
                        {

                        }

                        break;
                    case "recordingstate":
                        var state = JsonConvert.DeserializeObject<Recordingstate>(payload);

                        if (state.Source != null)
                        {

                        }
                        break;
                    case "recorderinfo":

                        var info = JsonConvert.DeserializeObject<RecordInfo>(payload);

                        if (info != null)
                        {

                        }

                        break;
                }
            }
            else // player
            {

                if(e.Notification.Key == ServerName)
                {
                    switch (e.Command)
                    {
                        case "ping":
                            break;
                        case "clearassets":
                            var clear = JsonConvert.DeserializeObject<Model.Engine.ClaearAssets>(payload);
                            
                            if (clear.isCleared)
                            {
                                FrameSmoother.Stop();
                                FrameSmoother.ResetFrame(0);
                            }

                            break;
                        case "transportcommand": // 진행 정보   1..
                            var command = JsonConvert.DeserializeObject<Model.Engine.TransportCommand>(payload);
                            FrameSmoother.UpdateExternalFrame(Convert.ToInt32(command.Position), 1, isIncreasing);
                            break;
                        case "playerconfig": // Player 설정 정보  2...

                            break;
                        case "clip":  //4...
                            var clip = JsonConvert.DeserializeObject<Model.Engine.Clip>(payload);
                            if (clip.Loaded)
                            {

                            }

                            break;
                        case "transportstate":  // 상태 정보
                        case "videostandard":  //3...
                        case "shuttle":
                        case "rate":
                        case "recue":
                            break;
                    }
                }
            }
        }

        private void FrameSmoother_FrameUpdated(object? sender, int e)
        {

        }

        private void AmppEngin_OnAmppControlErrorEvent(object? sender, Ampp.Control.lib.AmppControlErrorEventArgs e)
        {
            //SendLog(LogType.Error, $"[{MethodBase.GetCurrentMethod().Name}] Workload : {e.Key} [{e.Workload}] Command : {e.Command}", e.Details);

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
