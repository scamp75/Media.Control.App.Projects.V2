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

        public int TIME_SECONDS { get; set; } = 300;


        public EngineControl(EnuEnaginType type, string AmppWorkLoad = "", string reconKey = "" )
        {
            if (type == EnuEnaginType.Ampp)
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
            else
            {

            }
        }

        public async Task<bool> Connect(EnuEnaginType type)
        {
            if (type == EnuEnaginType.Ampp)
            {
                // key frame  설정 일단 주석  
                //var startResult = await amppControl.ConnectFrame(TIME_SECONDS);
                return await amppControl.Connect();
            }
            else
            {
                // amp control 구성
                return false;
            }
        }

        public async Task<bool> Start(EnuEnaginType type, string ProducerName, System.Windows.Controls.Image Handle)
        {
            Task<bool> result= null;

            if (type == EnuEnaginType.Ampp)
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

        public bool Stop(EnuEnaginType type)
        {
            if (type == EnuEnaginType.Ampp)
            {
                amppControl.OnAmppControlErrorEvent -= AmppControl_OnAmppControlErrorEvent;
                amppControl.OnAmppControlNotifyEvent -= AmppControl_OnAmppControlNotifyEvent;
                amppControl.OnStateEvent -= AmppControl_OnStateEvent;
                StopFrame();
                return true;
            }
            else
            {
                // amp control 구성
                return false;
            }
        }

        public async Task<bool> Recoder(EnuEnaginType type, EnmRecoderControl controlType, JObject obj = null )
        {
            if (type == EnuEnaginType.Ampp)
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

        public async Task<bool> Player(EnuEnaginType type, EnmPlayerControl controlType, JObject obj = null)
        {
            if (type == EnuEnaginType.Ampp)
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


        public async Task<bool> CleanCut(EnuEnaginType type, EnmCleancut controlType, JObject obj =null)
        {
            if (type == EnuEnaginType.Ampp)
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
