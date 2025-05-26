using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ampp.Control.lib;
using System.Windows.Interop;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Ampp.Control.lib.Test.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AmppControlLib amppControl = null;

        private string PlatformUrl = "https://apac1.gvampp.com";
        private string ApiKey = "NjQ2OGQzMWY2OTgzNDIxMDg4ZGI3NjUxMTlmMDJmNzY6N25VMy95L1dTWGNXQitNeG0vbEtmVlNKMzJFaS9kWDFoQVZUbGE5NDVMdzV2MUhMVDlMb3BrOHh1bHFQSFJnQk5VNGdZcHozM0IyT3IzT3FIN0p3Wmc9PQ";
        private string NodeId = "8426db68-b578-488a-822e-1d4884912cc8";
        private string FabricId = "7a6be586-3fa8-452e-a76a-33078238226d";
        //private string WorkLoad = "ac7e5448-7bda-4f9d-bc77-28daea67bd48"; // Recoder
        private string WorkLoad = "2020fe34-f117-4ba3-90cb-70884a053e0f"; //player
        // AMPP-EDGE-01 : CP - Clip Player Engine
        private string reconKey = "AmppControlSdk";
        private string ProducerName = "AMPP-EDGE-01: TSG";//"AMPP-EDGE-01 : CP";
        // AMPP-EDGE-01 : CP

        public MainWindow()
        {
            InitializeComponent();

            amppControl = new AmppControlLib(PlatformUrl, ApiKey, WorkLoad);
            amppControl.ReconKey = reconKey;
            amppControl.OnAmppControlErrorEvent += AmppControl_OnAmppControlErrorEvent;
            amppControl.OnAmppControlNotifyEvent += AmppControl_OnAmppControlNotifyEvent;
            amppControl.OnStateEvent += AmppControl_OnStateEvent;
        }

        private void AmppControl_OnStateEvent(string message)
        {
            txtState.Text = message;
        }

        private void AmppControl_OnAmppControlNotifyEvent(object? sender, Model.AmppControlNotificationEventArgs e)
        {

            if (e.Notification.Key == reconKey)
            {
                txtNotif.Dispatcher.Invoke(() =>
                {
                    txtNotif.Text = $"Workload:\t{e.Workload}\n" +
                                    $"Command:\t{e.Command}\n" +
                                    $"Payload:\t{JsonConvert.SerializeObject(e.Notification.Payload)}";
                });

                Debug.WriteLine($"Workload:\t{e.Workload}");
                Debug.WriteLine($"Command:\t{e.Command}");
                Debug.WriteLine($"Payload:\t{JsonConvert.SerializeObject(e.Notification.Payload)}");
            }
            else
            {
                Console.WriteLine("Application Status Update From Other: " + e.Notification.Key);
            }

        }

        private void AmppControl_OnAmppControlErrorEvent(object? sender, AmppControlErrorEventArgs e)
        {
            //if (e.Key == reconKey)
            //{

            //}

            //string errormessage = $"Workload:\t{e.Workload} Command:\t{e.Command}\n" +
            //                      $"Error:\t{e.Error} Details:\t{e.Details}";

            txtState.Dispatcher.Invoke(() => {
                txtState.Text = $"Workload:\t{e.Workload}\n" +
                                $"Command:\t{e.Command}\n" +
                                $"Error:\t{e.Error}\n" +
                                $"Details:\t{e.Details}";
            });


            Debug.WriteLine("************Error Notification**************");
            Debug.WriteLine($"Workload:\t{e.Workload}");
            Debug.WriteLine($"Command:\t{e.Command}");
            Debug.WriteLine($"Error:\t{e.Error}");
            Debug.WriteLine($"Details:\t{e.Details}");
            Debug.WriteLine("**************************************");
        }

        private async void butConnect_Click(object sender, RoutedEventArgs e)
        {
            var connected = await amppControl.Connect();

            if (connected)
            { 
                txtState.Text = "Connect Ampp Control...";

                // Image Control의 핸들 얻기
                IntPtr controlHandle = GetImageControlHandle(this.PreviewImage);

                //   amppControl.FRAME_DELAY_SECOND = 1;
                var result = await amppControl.StartFrame(NodeId, FabricId, ProducerName, this.PreviewImage);
            }
            else
            { txtState.Text = "No Connect Ampp Control..."; }
        }

        private IntPtr GetImageControlHandle(System.Windows.Controls.Image imageControl)
        {
            // Image Control의 부모 Window 핸들을 얻기
            HwndSource source = PresentationSource.FromVisual(imageControl) as HwndSource;

            if (source != null)
            {
                return source.Handle; // Image Control의 핸들을 반환
            }
            return IntPtr.Zero; // 핸들을 얻지 못한 경우
        }

        private async void butStartFrame_Click(object sender, RoutedEventArgs e)
        {
            

            
        }

        private void butPlay_Click(object sender, RoutedEventArgs e)
        {
            amppControl.Recorder(EnmRecoderControl.Record, new JObject());
            // amppControl.Player(AmppControlLib.EnmPlayerControl.Playpause, new JObject());


            amppControl.Recorder(EnmRecoderControl.Getstate, new JObject());
            //Task task = Task.Run(async () =>
            //{
            //    while(true)
            //    {
            //        amppControl.Recorder(EnmRecoderControl.Getstate, new JObject());


            //        if(isStop)
            //        {
            //            amppControl.Recorder(EnmRecoderControl.Stop, new JObject());
            //            break;
            //        }

            //        Thread.Sleep(300);
            //    }


            //});

        }

        bool isStop = false;

        private void butStop_Click(object sender, RoutedEventArgs e)
        {
            //isStop = true;
            amppControl.Recorder(EnmRecoderControl.Stop, new JObject());
        }

        private void butPrepaer_Click(object sender, RoutedEventArgs e)
        {
            var RecorderPrepare = new
            {
                filename = $"{this.MediaId.Text}"

            };

            amppControl.Recorder(EnmRecoderControl.Startprepare, JObject.FromObject(RecorderPrepare));

            amppControl.Recorder(EnmRecoderControl.Getstate, new JObject());
        }
    }
}