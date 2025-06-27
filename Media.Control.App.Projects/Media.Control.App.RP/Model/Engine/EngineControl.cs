using Ampp.Control.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Media.Control.App.RP.Model.Config;
using Ampp.Control.lib.Model;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Ampp.Control.lib.AmppControlLib;
using System.Diagnostics;
using Newtonsoft.Json;
using Vdcp.Control.Client;
using ControlzEx.Standard;
using System.Diagnostics.Metrics;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using System.Collections;


namespace Media.Control.App.RP.Model.Engine
{
    public class EngineControl
    {


        public event EventHandler<AmppControlErrorEventArgs> OnAmppControlErrorEvent;
        public event EventHandler<AmppControlNotificationEventArgs> OnAmppControlNotifyEvent;
        public event EventHandler<string> OnStateEvent;


        public string EnagineName { get; set; } = "";
        private string ReconKey = "AmppControlSdk";

        private AmppControlLib amppControl { get; set; }
        private VdcpControlClient VdcpControl { get; set; } = null;

        public int TIME_SECONDS { get; set; } = 300;

        public EngineControl(EnmEnaginType type, string AmppWorkLoad = "", string reconKey = "" )
        {
            if (type == EnmEnaginType.Ampp)
            {
                amppControl = new AmppControlLib(SystemConfigDataStatic.ChannelConfigData.AmppConfig.Platformurl,
                                                   SystemConfigDataStatic.ChannelConfigData.AmppConfig.ApiKey,
                                                   AmppWorkLoad);

                
                                                  //SystemConfigDataStatic.ChannelConfigData.ChannelList.WorkLoad1);
                //SystemConfigDataStatic.ChannelConfigData.AmppConfig.WorkNode);

                amppControl.ReconKey = reconKey == "" ? ReconKey : reconKey;
                amppControl.OnAmppControlErrorEvent += AmppControl_OnAmppControlErrorEvent;
                amppControl.OnAmppControlNotifyEvent += AmppControl_OnAmppControlNotifyEvent;
                amppControl.OnStateEvent += AmppControl_OnStateEvent;

                EnagineName = reconKey;
            }
            else if(type == EnmEnaginType.Vdcp)
            {
                VdcpControl = new VdcpControlClient();
                VdcpControl.portType = SystemConfigDataStatic.ChannelConfigData.VdcpType == EnuVdcpType.Udp ? PortType.NetWork : PortType.Serial ;
                VdcpControl.PortName = SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.ComPort;
            }
        }

        public async Task<bool> Connect(EnmEnaginType type)
        {
            bool result = false;

            if (type == EnmEnaginType.Ampp)
            {
                // key frame  설정 일단 주석  
                //var startResult = await amppControl.ConnectFrame(TIME_SECONDS);
                return await amppControl.Connect();
            }
            else
            {
                if(VdcpControl.portType == PortType.Serial)
                {
                    if (VdcpControl.OpenPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort, 0))
                    {
                        VdcpControl.Active = true;

                        if(VdcpControl.SelectPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort))
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    if(VdcpControl.SetUpdData(SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.Port))
                    {
                        if(VdcpControl.SetClinetData(SystemConfigDataStatic.ChannelConfigData.IpAddress
                                               , SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.Port))
                        {
                            if(VdcpControl.OpenPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort, 0))
                            {
                                VdcpControl.Active = true;

                                if (VdcpControl.SelectPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort))
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                    
            }

            return result;
        }

        public async Task<bool> Start(EnmEnaginType type, string ProducerName, System.Windows.Controls.Image Handle)
        {
            Task<bool> result= null;

            if (type == EnmEnaginType.Ampp)
            {
                var startResult = await amppControl.StartFrame(SystemConfigDataStatic.ChannelConfigData.AmppConfig.WorkNode ,
                                              SystemConfigDataStatic.ChannelConfigData.AmppConfig.Fabric,
                                              ProducerName,
                                              Handle);

                if (startResult) result = Task.FromResult(true);
                else result = Task.FromResult(false);

            }
            else
            {
                // amp control 구성
                return false;
            }

            return result.Result;
        }

        public void StopFrame()
        {
            amppControl.StopFrame();
        }

        public bool Stop(EnmEnaginType type)
        {
            if (type == EnmEnaginType.Ampp)
            {
                amppControl.OnAmppControlErrorEvent -= AmppControl_OnAmppControlErrorEvent;
                amppControl.OnAmppControlNotifyEvent -= AmppControl_OnAmppControlNotifyEvent;
                amppControl.OnStateEvent -= AmppControl_OnStateEvent;
                StopFrame();
                return true;
            }
            else
            {

                VdcpControl.ClosePort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort);
                VdcpControl.Close();
                // amp control 구성
                return true;
            }
        }

        public bool Vdcp(EumCommandKey command, out JObject? outPut, JObject? obj = null)
        {
            bool result = false;
            outPut = null;

            switch (command)
            {
                case EumCommandKey.NORMAL: break;
                case EumCommandKey.LOCALDISABLE:
                case EumCommandKey.LOCALENABLE:
                case EumCommandKey.STOP:
                    result = VdcpControl.Stop();
                    break;
                case EumCommandKey.PLAY:
                    result = VdcpControl.Play();
                    break;
                case EumCommandKey.RECORD:
                    result = VdcpControl.Record();
                    break;
                case EumCommandKey.FREEZE:
                case EumCommandKey.STILL:
                    result = VdcpControl.Still();
                    break;
                case EumCommandKey.STEP:
                    result = VdcpControl.Step();
                    break;
                case EumCommandKey.CONTINUE:
                    result = VdcpControl.Continue();
                    break;
                case EumCommandKey.JOG:
                    if (obj != null)
                    {
                        if (obj.ContainsKey("Speed"))
                        {
                            var speed = obj["Speed"].Value<int>();
                            result = VdcpControl.Jog(speed);
                        }
                    }
                    else
                    {
                        result = VdcpControl.Jog(0);
                    }
                    break;
                case EumCommandKey.VARIPLAY:

                    if (obj != null)
                    {
                        var value1 = obj["Value1"].Value<int>();
                        var value2 = obj["Value2"].Value<int>();
                        var value3 = obj["Value3"].Value<int>();

                        result = VdcpControl.VariPlay((byte)value1, (byte)value2, (byte)value3);
                    }

                    break;
                case EumCommandKey.UNFREEZE:
                    break;
                case EumCommandKey.EEMODE:
                    break;
                case EumCommandKey.RENAMEID:
                    break;
                case EumCommandKey.EXRENAMEID:
                    break;
                case EumCommandKey.PRESETTIME:
                    break;
                case EumCommandKey.CLOSEPORT:
                    result= VdcpControl.ClosePort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort);
                    break;
                case EumCommandKey.SELECTPORT:
                    result = VdcpControl.SelectPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort);
                    break;
                case EumCommandKey.RECORDINIT:
                case EumCommandKey.EXRECORDINIT:

                    if (obj != null && obj.ContainsKey("MediaId") && obj.ContainsKey("Duration"))
                    {
                        var media = obj["MediaId"].Value<string>();
                        var dur = obj["Duration"].Value<string>();
                        result = VdcpControl.RecordInitEx(media, dur);
                    }
                        
                    break;
                case EumCommandKey.PLAYCUE:
                case EumCommandKey.EXPLAYCUE:
                    if (obj != null && obj.ContainsKey("MediaId"))
                    {
                        var media = obj["MediaId"].Value<string>();
                        result = VdcpControl.PlayCueEx(media);
                    }
                    break;
                case EumCommandKey.CUEWITHDATA:
                case EumCommandKey.EXCUEWITHDATA:
                    if (obj != null && obj.ContainsKey("MediaId"))
                    {
                        var media = obj["MediaId"].Value<string>();
                        var som = obj["Som"].Value<string>();
                        var eom = obj["Eom"].Value<string>();

                        result = VdcpControl.CueWithDataEx(media, som, eom);
                    }
                    break;
                case EumCommandKey.DELETEID:
                    break;
                case EumCommandKey.EXDELETEID:
                    break;
                case EumCommandKey.CLEAR:
                    break;
                case EumCommandKey.SIGNALFULL:
                    break;
                case EumCommandKey.SELECTINPUT:
                    if (obj != null && obj.ContainsKey("InPut"))
                    {
                        var input = obj["InPut"].Value<string>();
                        result = VdcpControl.SelectInput(input);
                    }
                    break;
                case EumCommandKey.RECODEINITWITHDATA:
                    break;
                case EumCommandKey.EXRECODEINITWITHDATA:
                    break;
                case EumCommandKey.PRESET:

                    break;
                case EumCommandKey.DISKPREROLL:
                    break;
                case EumCommandKey.OPENPORT:
                    result = VdcpControl.OpenPort((byte)SystemConfigDataStatic.ChannelConfigData.VdcpPortConfig.SelectComPort, 0);
                    break;
                case EumCommandKey.NEXT:
                case EumCommandKey.EXNEXT:
                    int outTotal = 0;
                    ArrayList outList = new ArrayList();

                    result = VdcpControl.NextEx(out outTotal, outList);
                    outPut = new JObject
                    {
                        { "Total", outTotal },
                        { "List", JArray.FromObject(outList) }
                    };

                    break;
                
                case EumCommandKey.LIST:
                case EumCommandKey.EXLIST:

                    int outTotal1 = 0;
                    ArrayList outList1 = new ArrayList();

                    result = VdcpControl.ListEx(out outTotal1, outList1);

                    outPut = new JObject
                    {
                        { "Total", outTotal1 },
                        { "List", JArray.FromObject(outList1) }
                    };

                    break;
                case EumCommandKey.LAST: break;
                case EumCommandKey.PORTSTATUS:
                    List<string> state = new List<string>();
                    byte portName = 0x00;

                    VdcpControl.PortStatus(out state, out portName );

                    outPut = new JObject
                    {
                        { "PortName", portName },
                        { "State", JArray.FromObject(state) }
                    };

                    result = true;

                    break;
                case EumCommandKey.POSTIONREQUEST:

                    string tiemcode = string.Empty;

                    if (obj != null && obj.ContainsKey("Postiontype"))
                    {
                        var postionType = obj["Postiontype"].Value<byte>();

                        if (postionType == 0x00)
                        {
                            tiemcode = VdcpControl.PositionRequest(EPOSTIONTYPE.REMAIN);
                        }
                        else if (postionType == 0x01)
                        {
                            tiemcode = VdcpControl.PositionRequest(EPOSTIONTYPE.SOMBASE);
                        }
                        else if (postionType == 0x02)
                        {
                            tiemcode = VdcpControl.PositionRequest(EPOSTIONTYPE.ZEROBASE);
                        }
                    }

                    outPut = new JObject
                    {
                        { "Postion", tiemcode }
                    };
                    result = true;

                    break;
                case EumCommandKey.SYSTEMSTATUS:

                    SystemStatus sys = new SystemStatus();
                    VdcpControl.SystemStatusRequst(out sys);

                    outPut = JObject.FromObject(sys);
                    result = true;

                    break;
                case EumCommandKey.SIZEREQUEST:
                case EumCommandKey.EXSIZEREQUEST:
                    if (obj != null && obj.ContainsKey("ClipName"))
                    {
                        var clipName = obj["ClipName"].Value<string>();
                        var timecode = VdcpControl.IDSizeRequest(clipName);

                        outPut = new JObject
                        {
                            { "ClipName", clipName },
                            { "Size", timecode }
                        };

                        result = true;
                    }
                    
                    break;
                case EumCommandKey.IDREQUEST:
                case EumCommandKey.EXIDREQUEST:
                    if (obj != null && obj.ContainsKey("ClipName"))
                    {
                        var clipName = obj["ClipName"].Value<string>();

                        bool isExist = false;
                        result= VdcpControl.IDRequestEx(clipName, out isExist);

                        outPut = new JObject
                        {
                            { "ClipName", clipName },
                            { "IsExist", isExist }
                        };
                    }

                    break;
                case EumCommandKey.EXACTIVEIDREQUEST:
                case EumCommandKey.ACTIVEIDREQUEST:
                    string ClipName = string.Empty;
                    result = VdcpControl.ActiveIDRequestEx(out ClipName);

                    outPut = new JObject
                    {
                        { "ClipName", ClipName }
                    };
                    
                    break;
            }

            
            return result;
        }

        public async Task<bool> Recoder(EnmEnaginType type, EnmRecoderControl controlType = EnmRecoderControl.None,  JObject obj = null )
        {
            if (type == EnmEnaginType.Ampp)
            {
                if (obj == null) obj = new JObject();

                var result = await  amppControl.Recorder(controlType, obj);

                return result;
            }
            else
            {
                // amp control 구성
                return false;
            }
        }

        public async Task<bool> Player(EnmEnaginType type, EnmPlayerControl controlType = EnmPlayerControl.None, JObject obj = null)
        {
            if (type == EnmEnaginType.Ampp)
            {
                if(obj == null) obj = new JObject();    

                var  result = await amppControl.Player(controlType, obj);

                return result;
            }
            else
            {
                // amp control 구성
                return false;
            }

            return false;
        }


        public async Task<bool> CleanCut(EnmEnaginType type, EnmCleancut controlType, JObject obj =null)
        {
            if (type == EnmEnaginType.Ampp)
            {
                if (obj == null) obj = new JObject();

                var result = await amppControl.Cleancut(controlType, obj);

                return result;
            }
            else
            {
                // amp control 구성
                return false;
            }

            return false;
        }

        public Task<IEnumerable<AmppControlMacro>> GetControlMacro()
        { 
            return amppControl?.GetMacros(); 
        }


        public Task<bool> PutMacro(string uid, AmppControlMacro payload)
        {
            return amppControl?.PutMacroAsyne(uid, payload);
        }

        private void AmppControl_OnStateEvent(string message)
        {
            OnStateEvent(null, message);

            Debug.WriteLine($"State : \t{message}");
        }

        private void AmppControl_OnAmppControlNotifyEvent(object? sender, AmppControlNotificationEventArgs e)
        {   
            OnAmppControlNotifyEvent(sender, e);
 
        }

        private void AmppControl_OnAmppControlErrorEvent(object? sender, AmppControlErrorEventArgs e)
        {
            OnAmppControlErrorEvent(sender, e);
        }


    }
}
