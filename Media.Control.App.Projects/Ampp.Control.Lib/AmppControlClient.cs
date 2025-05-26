using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows.Media.Imaging;
using System.Windows;
using Ampp.Control.lib.Model;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace Ampp.Control.lib
{
    /// <summary>
    /// Class for interacting with AMPP Control using HTTP Client and SignalR Client
    /// </summary>
    public class AmppControlClient
    {
        private GVPlatform platform;

        public Image ControlHandle = null;
        public int SUBSCRIPTION_RENEW_TIME_SECONDS { get; set; } = 0;

        private List<string> subscriptions = new List<string>();
        //private readonly string folderPath;
        private readonly CancellationTokenSource frameCacheSubscriptionCancellationToken
            = new CancellationTokenSource();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="platformURL">URL for GV Platform</param>
        /// <param name="apiKey">The JWT API Key</param>
        public AmppControlClient(string platformURL, string apiKey)
        {
            platform = new GVPlatform(platformURL, apiKey);
            
        }

        /// <summary>
        /// Event Fired when an AMPP Control Notification Event is received
        /// </summary>
        public event EventHandler<AmppControlNotificationEventArgs> OnAmppControlNotifyEvent;

        /// <summary>
        /// Event fired when an AMPP Control Error notification is received
        /// </summary>
        public event EventHandler<AmppControlErrorEventArgs> OnAmppControlErrorEvent;

        /// <summary>
        /// Event fired when signalr hubconnection recovered from a connection loss.
        /// </summary>
        public event EventHandler<string> OnSignalRReconnected;


        /// <summary>
        /// Execute a Macro
        /// </summary>
        /// <param name="macroId">Id of Macro</param>
        /// <param name="reconKey">Key that will be sent in all AMPP Control Messages</param>
        /// <returns></returns>
        public async Task<bool> ExecuteMacroAsync(string macroId, string reconKey)
        {
            var url = "/ampp/control/api/v1/macro/execute";

            var macroRequest = new MacroRequest
            {
                ReconKey = reconKey,
                Uuid = macroId,
            };

            var response =  await this.platform.Post(url, macroRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> PutMacroAsyne(string macroid, AmppControlMacro payload)
        {
            var url = "/ampp/control/api/v1/macro";

            var response = await this.platform.Put(url, payload);

            return response.IsSuccessStatusCode;
        }


        /// <summary>
        /// Request that an Application Sends all information about its state
        /// </summary>
        /// <param name="workloadId">The workload</param>
        /// <param name="reconKey">the key to be sent back in all response messages.</param>
        /// <returns></returns>
        public Task<bool> GetStateAsync(string workloadId, string reconKey)
        {
            return this.PushAmppControlMessageAsync(workloadId, "any", "getstate", new JObject(), reconKey);
        }

        /// <summary>
        /// Gets a List all application types
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AmppControlApplication>> GetApplicationTypesAsync()
        {
            var url = "/ampp/control/api/v1/control/application/references";

            var result = await this.platform.Get(url);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var body = result.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<IList<AmppControlApplication>>(body);
                return data;
            }

            return null;
        }

        /// <summary>
        /// List all Macros
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AmppControlMacro>> GetMacrosAsync()
        {
            var url = "/ampp/control/api/v1/macro";
            var result = await this.platform.Get(url).ConfigureAwait(false);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var body = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<IList<AmppControlMacro>>(body);
                return data;

                //var body = result.Content.ReadAsStringAsync().Result;
                //var data = JsonConvert.DeserializeObject<IList<AmppControlMacro>>(body);
                //return data;
            }

            return Enumerable.Empty<AmppControlMacro>(); 
        }

        /// <summary>
        /// List all workloads for an application type
        /// </summary>
        /// <param name="applicationType">The application type (I.e. MiniMixer, AudioMixer, Clip Player etc)</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetWorkloadsForApplicationTypeAsync(string applicationType)
        {
            var url = $"/ampp/control/api/v1/control/application/{applicationType}/workloads";
            var result = await this.platform.Get(url);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var body = result.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<IList<string>>(body);
                return data;
            }

            return null;
        }

        /// <summary>
        /// Connect to GV Platform
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LoginAsync()
        {
            var result = await this.platform.LoginAsync();

            if (result)
            {
                this.platform.OnPushNotifyEvent += Platform_OnPushNotifyEvent;

                return await this.platform.StartNotificationsListenerAsync();

            }

            return result;
        }


        public async void DisConnect()
        {
            StopKeyframesSubscriptionAsync();
        }

        private void Platform_OnPushNotifyEvent(object sender, PushNotification e)
        {
            Console.WriteLine("Received keyframes notification");

            if (e.BinaryContent != null)
            {
                DrawImageFromBytes(e.BinaryContent);
            }
            else if (e.Content == "Removed")
            {
                Console.WriteLine("Notification is removed");
            }
            else
            {
                this.OnNotification(e);
            }
        }

        /// <summary>
        /// Ping an application and wait for a response.
        /// </summary>
        /// <param name="workloadId">The Id of the workload to ping</param>
        /// <param name="timeout">How long to block for response in ms</param>
        /// <returns></returns>
        public async Task<bool> PingAsync(string workloadId, int timeout)
        {
            string pingGuid = System.Guid.NewGuid().ToString();
            ManualResetEvent pingResponse = new ManualResetEvent(false);

            EventHandler<AmppControlNotificationEventArgs> pingHandler = (sender, e) =>
            {
                if (e.Notification.Key == pingGuid)
                {
                    pingResponse.Set();
                }
            };

            OnAmppControlNotifyEvent += pingHandler;

            await PushAmppControlMessageAsync(workloadId, "any", "ping", new JObject(), pingGuid);
            
            bool pingOkay = pingResponse.WaitOne(timeout);

            OnAmppControlNotifyEvent -= pingHandler;

            return pingOkay;
        }

        /// <summary>
        /// Send an AMPP Control message directly using the PushNotifications SignalR connection
        /// </summary>
        /// <param name="workloadId">Workload to send to</param>
        /// <param name="applicationType">Application Type (I.e. MiniMixer)</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="payload">JSON Payload</param>
        /// <param name="reconKey">A key that will be returned in any notify response.</param>
        /// <returns></returns>
        public async Task<bool> PushAmppControlMessageAsync(string workloadId, string applicationType, string command, JObject payload, string reconKey)
        {
            var topic = $"gv.ampp.control.{workloadId}.{command}";

            var content = JObject.FromObject(new
            {
                Key = reconKey,
                Payload = payload,
            });

            return await this.platform.PushNotificationAsync(topic, content);
        }

        /// <summary>
        /// Send an AMPP Control message via an HTTP PUT request to the AMPP Control service
        /// </summary>
        /// <param name="workloadId">Workload to send to</param>
        /// <param name="applicationType">Application Type (I.e. MiniMixer)</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="payload">JSON Payload</param>
        /// <param name="reconKey">A key that will be returned in any notify response.</param>
        /// <returns></returns>
        public async Task<bool> SendAmppControlMessageAsync(string workloadId, string applicationType, string command, JObject payload, string reconKey)
        {
            var url = "/ampp/control/api/v1/control/commit";

            AmppControlRequest controlRequest = new AmppControlRequest
            {
                Application = applicationType,
                Command = command,
                FormData = JsonConvert.SerializeObject(payload),
                Workload = workloadId,
                ReconKey = reconKey,
            };

            var response = await this.platform.Post(url, controlRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// List all ControlGroups for an application type
        /// </summary>
        /// <param name="applicationType">The application type (I.e. MiniMixer)</param>
        /// <returns></returns>
        public async Task<IEnumerable<ControlGroup>> GetControlGroupsForApplicationTypeAsync(string applicationType)
        {
            var url = $"/ampp/control/api/v1/group/application/{applicationType}/groups";
            var result = await this.platform.Get(url);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var body = result.Content.ReadAsStringAsync().Result;
                var data = JsonConvert.DeserializeObject<IList<ControlGroup>>(body);
                return data;
            }

            return null;
        }

        /// <summary>
        /// Subscribe to Notifications for a given workload
        /// </summary>
        /// <param name="workloadId">Id of workload to subscribe to.</param>
        public Task SubscribeToWorkload(string workloadId)
        {
            // Notification topics are of the form:
            // gv.ampp.control.{workload}.{command}.{type}
            string topic = $"gv.ampp.control.{workloadId}.*.*";
            return this.platform.SubscribeToNotification(topic);
        }

        public Task ExSubscribeToWorkload(string workloadId, string mode)
        {
            // Notification topics are of the form:
            // gv.ampp.control.{workload}.{command}.{type}
            string topic = $"gv.ampp.control.{workloadId}.{mode}.notify";
            return this.platform.SubscribeToNotification(topic);
        }



        /// <summary>
        /// PushNotification Received from SignalR Hub
        /// </summary>
        /// <param name="notification"></param>

        private void OnNotification(PushNotification notification)
        {
            // Notification topics are of the form:
            // gv.ampp.control.{workload}.{command}.{type}
            var topicParts = notification.Topic.Split(".");
            var type = topicParts[5];
            var command = topicParts[4];
            var workload = topicParts[3];

            var amppPayload = JObject.Parse(notification.Content).ToObject<AmppControlNotification>();

            // Sometimes AMPP Control Messages are bundled up
            // if the Key is multi message then unbundle them.
            if (amppPayload.Key == "multimessage")
            {
                foreach(var message in amppPayload.Payload)
                {
                    var packagedNotification = message.ToObject<AmppControlNotification>();
                    this.ProcessNotification(type, workload, command, packagedNotification);
                }
                return;
            }

            this.ProcessNotification(type, workload, command, amppPayload); 

        }

        private void ProcessNotification(string type, string workload, string command, AmppControlNotification payload)
        {

            if (type == "notify")
            {
                this.OnAmppControlNotifyEvent?.Invoke(this, new AmppControlNotificationEventArgs()
                {
                    Workload = workload,
                    Command = command,
                    Notification = payload,
                });
            }
            else if (type == "status")
            {
                this.OnAmppControlErrorEvent?.Invoke(this, new AmppControlErrorEventArgs()
                {
                    Workload = workload,
                    Command = command,
                    Status = payload.Status,
                    Details = payload.Details,
                    Error = payload.Error,
                    Key = payload.Key,
                });
            }
        }



        public async Task<Producer> GetProducerAsync(Guid fabricId, string producerName)
        {
            string requestUri = $"/cluster/matrix/api/v1/producer/{fabricId}/{producerName}";
            HttpResponseMessage response = await this.platform.Get(requestUri);

            if (response.IsSuccessStatusCode)
            {
                string body = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Producer>(body);
            }
            return null;
        }

        public void AddKeyframesSubscription(string nodeId, string flowId)
        {
            string subscription = $"gv.ampp.keyframe.{nodeId}.{flowId}.{PreviewSize.Large.ToString().ToLower()}";
            subscriptions.Add(subscription);
        }

        public async Task StartKeyframesSubscriptionAsync()
        {
            foreach (string subscription in subscriptions)
            {
                await platform.SubscribeToNotification(subscription);
                Console.WriteLine($"Subscribing {subscription}");
            }
            await Task.Run(async () =>
            {
                await RenewFrameCacheSubscriptions();
            },
            frameCacheSubscriptionCancellationToken.Token);
        }

        private async Task RenewFrameCacheSubscriptions()
        {
            try
            {
                while (!frameCacheSubscriptionCancellationToken.Token.IsCancellationRequested)
                {
                    foreach (string subscription in subscriptions)
                    {
                        Console.WriteLine($"Renewing subscription: {subscription}");
                        string[] parts = subscription.Split('.');
                        string topic = $"{parts[0]}.{parts[1]}.{parts[2]}.{parts[3]}";
                        string flowId = parts[4];
                        await SendFlowSubscriptionRequest(topic, flowId);
                    }
                    await Task.Delay(30);
                    //await Task.Delay(SUBSCRIPTION_RENEW_TIME_SECONDS * 1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while subscribing to keyframes!", ex);
                throw;
            }
        }

        public async Task StopKeyframesSubscriptionAsync()
        {
            frameCacheSubscriptionCancellationToken.Cancel();
            foreach (string subscription in subscriptions)
            {
                await platform.UnsubscribeFromNotification(subscription);
                Console.WriteLine($"Unsubscribing {subscription}");
            }
        }

        // This method tells the framecache to generate keyframes
        private Task SendFlowSubscriptionRequest(string topic, string flowId)
        {
            KeyframesSubscriptionNotification notification = new KeyframesSubscriptionNotification()
            {
                PreviewSize = PreviewSize.Small,
                FlowId = flowId
            };
            return platform.PushNotificationAsync(topic, JObject.FromObject(notification));
        }



        private async void DrawImageFromBytes(byte[] imageData)
        {
            await Task.Run(() => {
                BitmapImage bitmapImage = ImageConveter.ByteArrayToBitmapImage(imageData);

                Application.Current.Dispatcher.Invoke(() => {

                    ControlHandle.Source = bitmapImage;
                });
            });
        }

    }
}
