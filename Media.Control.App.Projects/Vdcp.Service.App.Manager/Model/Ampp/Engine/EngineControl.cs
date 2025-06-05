using Ampp.Control.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vdcp.Service.App.Manager.Model;
using Ampp.Control.lib.Model;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static Ampp.Control.lib.AmppControlLib;
using System.Diagnostics;
using Newtonsoft.Json;


namespace Vdcp.Service.App.Manager.Model.Engine
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



        public EngineControl(AmppConfig amppConfig, string AmppWorkLoad, string reconKey)
        {   
                amppControl = new AmppControlLib(amppConfig.PlatformUrl, amppConfig.PlatformKey, AmppWorkLoad);
    
                amppControl.ReconKey = reconKey == "" ? ReconKey : reconKey;
                amppControl.OnAmppControlErrorEvent += AmppControl_OnAmppControlErrorEvent;
                amppControl.OnAmppControlNotifyEvent += AmppControl_OnAmppControlNotifyEvent;
                amppControl.OnStateEvent += AmppControl_OnStateEvent;

                EnagineName = reconKey;
            
        }

        public async Task<bool> Connect()
        {
            
            return await amppControl.Connect();
            
        }

        public async Task<bool> Start( string ProducerName, System.Windows.Controls.Image Handle)
        {
            Task<bool> result= null;

            
            //var startResult = await amppControl.StartFrame(SystemConfigDataStatic.ChannelConfigData.AmppConfig.WorkNode ,
            //                                SystemConfigDataStatic.ChannelConfigData.AmppConfig.Fabric,
            //                                ProducerName,
            //                                Handle);

            //if (startResult) result = Task.FromResult(true);
            //else result = Task.FromResult(false);

            return result.Result;
        }

        public void StopFrame()
        {
            //amppControl.StopFrame();
        }

        public bool Stop()
        {
            
            amppControl.OnAmppControlErrorEvent -= AmppControl_OnAmppControlErrorEvent;
            amppControl.OnAmppControlNotifyEvent -= AmppControl_OnAmppControlNotifyEvent;
            amppControl.OnStateEvent -= AmppControl_OnStateEvent;
            StopFrame();
            return true;
            
        }

        public async Task<bool> Recoder( EnmRecoderControl controlType, JObject obj = null )
        {   
            if (obj == null) obj = new JObject();

            var result = await  amppControl.Recorder(controlType, obj);

            return result;
            
        }

        public async Task<bool> Player( EnmPlayerControl controlType, JObject obj = null)
        {
            if(obj == null) obj = new JObject();    

            var  result = await amppControl.Player(controlType, obj);

            return result;
            
        }


        public async Task<bool> CleanCut( EnmCleancut controlType, JObject obj =null)
        {   
            if (obj == null) obj = new JObject();

            var result = await amppControl.Cleancut(controlType, obj);

            return result;
            
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
