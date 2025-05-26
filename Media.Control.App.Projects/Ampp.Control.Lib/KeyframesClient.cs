using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ampp.Control.lib.Model;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;

namespace Ampp.Control.lib
{
    public class AmppKeyframesClient
    {
        private GVPlatform gvPlatform;

        private List<string> subscriptions = new List<string>();
        //private readonly string folderPath;
        private readonly CancellationTokenSource frameCacheSubscriptionCancellationToken
            = new CancellationTokenSource();

        public Image ControlHandle = null;
        public int SUBSCRIPTION_RENEW_TIME_SECONDS { get; set; } = 30;

        public AmppKeyframesClient(string baseURL, string apiKey)
        {
            gvPlatform = new GVPlatform(baseURL, apiKey);
        }


        public async Task<bool> LoginAsync()
        {
            var connected = await this.gvPlatform.LoginAsync();
            if (connected)
            {
                gvPlatform.OnPushNotifyEvent += OnNotification;
                return await this.gvPlatform.StartNotificationsListenerAsync();
            }
            return connected;
        }


        public async Task<Producer> GetProducerAsync(Guid fabricId, string producerName)
        {
            string requestUri = $"/cluster/matrix/api/v1/producer/{fabricId}/{producerName}";
            HttpResponseMessage response = await this.gvPlatform.Get(requestUri);

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

        public void RemoveKeyframesSubscription(string nodeId, string flowId)
        {
            //string subscription = $"gv.ampp.keyframe.{nodeId}.{flowId}.{PreviewSize.Large.ToString().ToLower()}";
            //subscriptions.Remove(subscription);
            subscriptions.Clear();
        }

        public async Task StartKeyframesSubscriptionAsync()
        {
            foreach (string subscription in subscriptions)
            {
                await gvPlatform.SubscribeToNotification(subscription);
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
                    await Task.Delay(SUBSCRIPTION_RENEW_TIME_SECONDS);
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
                await gvPlatform.UnsubscribeFromNotification(subscription);
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
            return gvPlatform.PushNotificationAsync(topic, JObject.FromObject(notification));
        }

        private void OnNotification(object sender, PushNotification e)
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
                Console.WriteLine("Error handling keyframes notification");
                //throw new Exception("Error handling keyframes notification");
            }
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
