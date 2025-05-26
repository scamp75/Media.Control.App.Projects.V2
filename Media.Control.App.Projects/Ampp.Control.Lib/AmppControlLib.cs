using Ampp.Control.lib.Model;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace Ampp.Control.lib
{
    public enum EnumControlType { ElasticRecorder, ClipPlayer, CleanCut }
    public enum EnmRecoderControl { Prepare, Startprepare, Record, Startat, Stop, Stopat, Recordingstate, Recordconfig, Getstate, Ping, Notify }

    public enum EnmCleancut { Getstate, Inputstate, Videostandard, Inputassignment, Ping }
    public enum EnmPlayerControl
    {
        Transportstate, Clip, Playpause, Loop, Fastforward, Rewind, Startat, Rate,
        Stopat, Gotostart, Gotoend, Stepforward, Stepback, Markin, Markout, Seek, Shuttle,
        Autorecue, Transportcommand, Videostandard, Getstate, Clearassets, Ping
    }


    public class AmppControlLib
    {
        public delegate void ErrorEventHandleAge(string message);

        public event ErrorEventHandleAge OnStateEvent;

        public event EventHandler<AmppControlNotificationEventArgs> OnAmppControlNotifyEvent;

        public event EventHandler<AmppControlErrorEventArgs> OnAmppControlErrorEvent;

        public string ReconKey { get; set; } = "AmppControlSdk";

        public string PlatformUrl { get; set; }

        public string ApiKey { get; set; }

        public string WorkloadId { get; set; }

        public string NodeId { get; set; }
        public string FabricId { get; set; }
        public string ProducerName { get; set; }

        private int framDelay { get; set; } = 60;
        public int FRAME_DELAY_SECOND
        {
            get => framDelay;
            set
            {
                framDelay = value;
                if (amppControl != null)
                {
                    amppControl.SUBSCRIPTION_RENEW_TIME_SECONDS = framDelay;
                }
            }
        }

        private AmppControlClient amppControl = null;
        private AmppKeyframesClient keyframesClient = null;
        
        private Producer producer = null;

        public AmppControlLib(string url, string key, string load)
        {
            PlatformUrl = url;
            ApiKey = key;
            WorkloadId = load;
        }

        #region Connect

        public async Task<bool> Connect()
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(PlatformUrl) && !string.IsNullOrEmpty(ApiKey))
                {
                    amppControl = new AmppControlClient(PlatformUrl, ApiKey);

                    var connected =  await amppControl.LoginAsync();

                    if (connected)
                    {
                        if (!string.IsNullOrEmpty(WorkloadId))
                        {
                            
                            amppControl.OnAmppControlNotifyEvent += AmppControl_OnAmppControlNotifyEvent;
                            amppControl.OnAmppControlErrorEvent  += AmppControl_OnAmppControlErrorEvent;
                            amppControl.OnSignalRReconnected += AmppControl_OnSignalRReconnected;

                            
                            //AMPP-EDGE-01:ELASTIC#1
                            
                            await amppControl.SubscribeToWorkload(WorkloadId);
                            await amppControl.ExSubscribeToWorkload(WorkloadId, "recorderinfo");
                            
                            return true;
                            //bool isOnline = await amppControl.PingAsync(WorkloadId, 1000);
                            //if (isOnline)
                            //{
                            //    Debug.WriteLine($"Ping {isOnline}");
                            //    await amppControl.GetStateAsync(WorkloadId, ReconKey);
                            //    result = true;
                            //}
                        }
                        else { if(OnStateEvent != null) OnStateEvent("Connect [Error] : No WorkLoadId"); return false; }
                    }
                }
                else { if (OnStateEvent != null) OnStateEvent("Connect [Error] : Check ( PlatformUrl, ApiKey)"); return false; }
            }
            catch (Exception ex)
            {
                if (OnStateEvent != null) OnStateEvent($"Connect [Exception] : {ex.Message}");
            }

            return result;
        }

        private void AmppControl_OnSignalRReconnected(object? sender, string e)
        {
            
        }
        #endregion

        public async Task<bool> ConnectFrame(int TIME_SECONDS)
        {
            bool result = false;

            try
            {
                if (!string.IsNullOrEmpty(PlatformUrl) && !string.IsNullOrEmpty(ApiKey))
                {
                    keyframesClient = new AmppKeyframesClient(PlatformUrl, ApiKey);

                    var connected = await keyframesClient.LoginAsync();
                    result = connected;

                    keyframesClient.SUBSCRIPTION_RENEW_TIME_SECONDS = TIME_SECONDS;

                }
                else { if (OnStateEvent != null) OnStateEvent("Connect [Error] : Check ( PlatformUrl, ApiKey)"); return false; }
            }
            catch (Exception ex)
            {
                if (OnStateEvent != null) OnStateEvent($"Connect [Exception] : {ex.Message}");
            }

            return result;
        }


        #region Set Frame

        private string Modelid;
        private string Fabricid;
        public async Task<bool> StartFrame(string nodeld, string fabricId, string producerName, Image controlHandle)
        {
            Task<bool> result = Task.FromResult(false);

            try
            {
                keyframesClient.ControlHandle = controlHandle;

                FabricId = fabricId;
                ProducerName = producerName;

                Guid guid = Guid.Parse(fabricId);

                Producer producer = await keyframesClient.GetProducerAsync(guid, producerName);

                if (producer != null)
                {
                    string flowId = producer.Stream?.Flows?.FirstOrDefault(f => f.DataType == FlowDataType.Pic)?.FlowId;
                    if (!string.IsNullOrEmpty(flowId))
                    {
                        Modelid = nodeld;
                        Fabricid = fabricId;
                        keyframesClient.AddKeyframesSubscription(nodeld, flowId);
                        keyframesClient.StartKeyframesSubscriptionAsync();

                        result = Task.FromResult(true);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result.Result;
        }


        //public async Task<bool> StartFrame(string nodeld, string fabricId, string producerName, Image controlHandle)
        //{
        //    Task<bool> result = Task.FromResult(false);

        //    try
        //    {
        //        amppControl.ControlHandle = controlHandle;

        //        FabricId = fabricId;
        //        ProducerName = producerName;

        //        Guid guid = Guid.Parse(fabricId);

        //        Producer producer = await amppControl.GetProducerAsync(guid, producerName);

        //        if (producer != null)
        //        {
        //            string flowId = producer.Stream?.Flows?.FirstOrDefault(f => f.DataType == FlowDataType.Pic)?.FlowId;
        //            if (!string.IsNullOrEmpty(flowId))
        //            {
        //                amppControl.AddKeyframesSubscription(nodeld, flowId);
        //                amppControl.StartKeyframesSubscriptionAsync();

        //                result = Task.FromResult(true);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return result.Result;
        //}

        public void StopFrame()
        {

            //amppControl.StopKeyframesSubscriptionAsync();

            keyframesClient?.StopKeyframesSubscriptionAsync();
            keyframesClient?.RemoveKeyframesSubscription(Modelid, Fabricid);
            
        }
        #endregion

        #region Send,Push

        public async Task<bool> Recorder(EnmRecoderControl recoderControl, JObject controlData )
        {

            bool result = false;

            if (WorkloadId != null)
            {
                //result = await amppControl.SendAmppControlMessageAsync(WorkloadId, EnumControlType.ElasticRecorder.ToString(),
                //                            recoderControl.ToString().ToLower(), controlData, ReconKey);


                amppControl.SendAmppControlMessageAsync(WorkloadId, EnumControlType.ElasticRecorder.ToString(),
                                            recoderControl.ToString().ToLower(), controlData, ReconKey);


                result = true;
            }

            return result;
        }

        public async Task<bool> Player(EnmPlayerControl playerControl, JObject controlData)
        {
            bool result = false;

            if(WorkloadId != null)
            {

                //result = await  amppControl.SendAmppControlMessageAsync(WorkloadId, EnumControlType.ClipPlayer.ToString(),
                //             playerControl.ToString().ToLower(), controlData, ReconKey);
                amppControl.SendAmppControlMessageAsync(WorkloadId, EnumControlType.ClipPlayer.ToString(),
                         playerControl.ToString().ToLower(), controlData, ReconKey);

                result = true;
            }

            return result;
        }

        public async Task<bool> Cleancut(EnmCleancut cleancutControl, JObject controlData)
        {
            bool result = false;
            if (WorkloadId != null)
            {
                amppControl.SendAmppControlMessageAsync(WorkloadId, EnumControlType.CleanCut.ToString(),
                         cleancutControl.ToString().ToLower(), controlData, ReconKey);
                result = true;
            }
            return result;
        }

        public async Task<IEnumerable<AmppControlApplication>> GetApplicationTypes()
        {
            return await amppControl?.GetApplicationTypesAsync();
        }

        public async Task<IEnumerable<AmppControlMacro>> GetMacros()
        {
            return await amppControl?.GetMacrosAsync();
        }

        public  async Task<bool> PutMacroAsyne(string id , AmppControlMacro payload)
        {
            return await amppControl?.PutMacroAsyne(id, payload);
        }

        public List<string> GetWorkloadsForApplicationType(string appType)
        {
            return amppControl.GetWorkloadsForApplicationTypeAsync(appType).Result.ToList();
        }


        #endregion


        #region Event

        private void AmppControl_OnAmppControlErrorEvent(object sender, AmppControlErrorEventArgs e)
        {
            if(OnAmppControlErrorEvent != null) OnAmppControlErrorEvent(sender, e);
        }

        private void AmppControl_OnAmppControlNotifyEvent(object sender, AmppControlNotificationEventArgs e)
        {
            OnAmppControlNotifyEvent?.Invoke(sender, e);
            //if (OnAmppControlNotifyEvent != null) OnAmppControlNotifyEvent(sender, e);
        }
        #endregion
    }
}
