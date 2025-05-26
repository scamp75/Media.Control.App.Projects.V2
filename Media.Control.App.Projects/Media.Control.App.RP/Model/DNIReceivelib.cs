using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NewTek;
using NewTek.NDI;
using static NewTek.NDIlib;

namespace Media.Control.App.RP.Model
{
    public class DNIReceivelib
    {
        private Finder ndiFinder;
        private IntPtr recvInstance;
        private CancellationTokenSource Tokensource;

        private Thread ndiThread = null;
        private bool isRunning = false;
        private Task receiveTask;
        public event Action<WriteableBitmap> VideoFrameReceived;
        public event Action<(double leftLevel, double rightLevel)> AudioLevelsReceived;

        public System.Windows.Controls.Image VideoImage { get; set; } = null;
        public System.Windows.Controls.ProgressBar AudioLeveLeft { get; set; } = null;
        public System.Windows.Controls.ProgressBar AudioLeveRight { get; set; } = null;

        public List<Source> SourceList { get; private set; }
        public bool Initialize()
        {
            bool result = false;    
            // Initialize the NDI library
            if (NDIlib.initialize())
            {

                SourceList = new List<Source>();
                LoadNDISources();

                
                result = true;
            }

            return result;
        }
        private void LoadNDISources()
        {
            ndiFinder = new Finder(true);
            ndiFinder.NewNdiSourceDiscovered  += NdiFinder_NewNdiSourceDiscovered;
            Thread.Sleep(1000); // Wait for sources to be discovered
        }

        private void NdiFinder_NewNdiSourceDiscovered(Source source)
        {
            SourceList.Add(source);
        }

        public List<string> GetSourecName()
        {
            SourceList = ndiFinder.Sources;

            List<string> sourceNames = new List<string>();
            
            foreach (var source in SourceList)
            {
                sourceNames.Add(source.SourceName);
            }

            return sourceNames;
        }

        public bool Start(string sourceName)
        {
            bool result = false;
            var selectedSource = SourceList.FirstOrDefault(s => s.SourceName == sourceName);
            if (selectedSource == null) return result;

            // Convert NewTek.NDI.Source to NewTek.NDIlib.source_t
            var ndiSource = new NDIlib.source_t
            {
                p_ndi_name = Marshal.StringToHGlobalAnsi(selectedSource.Name),
                p_url_address = Marshal.StringToHGlobalAnsi(selectedSource.Uri.ToString())
            };

            var recvSettings = new recv_create_v3_t()
            {
                source_to_connect_to = ndiSource,
                color_format = recv_color_format_e.recv_color_format_BGRX_BGRA, 
                bandwidth = recv_bandwidth_e.recv_bandwidth_highest,
                allow_video_fields = false
            };

            recvInstance = NDIlib.recv_create_v3(ref recvSettings);
            if (recvInstance == IntPtr.Zero)
            {
                result = false;
            }

            Tokensource = new CancellationTokenSource();
            receiveTask = Task.Run(() => TaskReceive(Tokensource.Token));
            isRunning = true;

            return result;
        }

        public void Stop()
        {
            isRunning = false;
            

            if (Tokensource != null)
            {
                Tokensource.Cancel();
            }

            Thread.Sleep(600); // Wait for the thread to finish
            // ✅ 수신 쓰레드가 완전히 종료되길 기다림
            receiveTask?.Wait(100);  // 최대 1초 대기
            receiveTask = null;

            Tokensource?.Dispose();
            Tokensource = null;


            

            if (recvInstance != IntPtr.Zero)
            {
                NDIlib.recv_destroy(recvInstance);
                recvInstance = IntPtr.Zero;
            }
            
            if (ndiFinder != null)
            {
                ndiFinder.Dispose();
                ndiFinder = null;
            }
            
            NDIlib.destroy();
        }

        private void TaskReceive(CancellationToken token)
        {
            video_frame_v2_t videoFrame = new video_frame_v2_t();
            audio_frame_v3_t audioFrame = new audio_frame_v3_t();
            metadata_frame_t metadataFrame = new metadata_frame_t();

            while (!token.IsCancellationRequested)
            {
                var frameType = NDIlib.recv_capture_v3(recvInstance, ref videoFrame, ref audioFrame, ref metadataFrame, 1000);

                if (!isRunning || recvInstance == IntPtr.Zero || token.IsCancellationRequested)
                {
                    Debug.WriteLine($"recvInstance = {recvInstance} | token.IsCancellationRequested  = {token.IsCancellationRequested} | isRuning = {isRunning}");
                    break;
                }


                if (frameType == frame_type_e.frame_type_video)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => // Use Application.Current.Dispatcher
                    {
                        if (VideoImage != null)
                        {
                            var bitmap = ConvertToWriteableBitmap(videoFrame);
                            VideoImage.Source = bitmap;
                        }
                    });

                    NDIlib.recv_free_video_v2(recvInstance, ref videoFrame);
                }
                else if (frameType == frame_type_e.frame_type_audio)
                {
                    (double leftLevel, double rightLevel) = CalculateStereoLevels(audioFrame);

                    var Left = Math.Min(leftLevel * 5, 1.0);
                    var Right = Math.Min(rightLevel * 5, 1.0);


                    System.Windows.Application.Current.Dispatcher.Invoke(() => // Use Application.Current.Dispatcher
                    {
                        AudioLevelsReceived((Left, Right));

                        if (AudioLeveLeft != null && AudioLeveRight != null)
                        {
                            AudioLeveLeft.Value = Left;  // 최대 1.0으로 클램핑
                            AudioLeveRight.Value = Right;
                        }
                    });

                   // AudioLevelsReceived((AudioLevelLeft, AudioLevelRight));

                   // Debug.WriteLine($"Audio Levels - Left: {Left}, Right: {Right}");

                    NDIlib.recv_free_audio_v3(recvInstance, ref audioFrame);
                }
                else if (frameType == frame_type_e.frame_type_metadata)
                {
                    NDIlib.recv_free_metadata(recvInstance, ref metadataFrame);
                }

            }
        }
        private void SaveBitmapToPng(string filePath, WriteableBitmap latestBitmap)
        {
            if (latestBitmap == null) return;

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(latestBitmap));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private WriteableBitmap ConvertToWriteableBitmap(video_frame_v2_t frame)
        {
            try
            {
                int width = frame.xres;
                int height = frame.yres;
                int stride = frame.line_stride_in_bytes;
                int expectedBufferSize = stride * height;

                // Allocate managed buffer and copy memory safely
                WriteableBitmap bitmap = null;
                if (frame.p_data != IntPtr.Zero)
                {
                    if (expectedBufferSize <= 0)
                    {
                        Debug.WriteLine("Invalid buffer size calculated.");
                        return null;
                    }

                    byte[] buffer = new byte[expectedBufferSize];

                    try
                    {
                        Marshal.Copy(frame.p_data, buffer, 0, expectedBufferSize);
                    }
                    catch (TaskCanceledException) { }
                    catch (ObjectDisposedException) { }
                    

                    bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

                    // Ensure the buffer passed matches what WritePixels expects
                    Int32Rect rect = new Int32Rect(0, 0, width, height);
                    bitmap.Lock();
                    bitmap.WritePixels(rect, buffer, stride, 0);
                    bitmap.Unlock();
                }
                else
                {
                    Debug.WriteLine("frame.p_data is null.");
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error in ConvertToWriteableBitmap: {ex.Message}");
                Debug.WriteLine($"Frame Info - Width: {frame.xres}, Height: {frame.yres}, Stride: {frame.line_stride_in_bytes}");
                return null;
            }
        }

        private (double leftLevel, double rightLevel) CalculateStereoLevels(audio_frame_v3_t audioFrame)
        {

            try
            {
                int samplesPerChannel = audioFrame.no_samples;
                int totalSamples = samplesPerChannel * audioFrame.no_channels;

                if (audioFrame.no_channels < 2 || totalSamples == 0)
                    return (0f, 0f);

                float[] samples = new float[totalSamples];
                Marshal.Copy(audioFrame.p_data, samples, 0, totalSamples);

                double sumLeft = 0, sumRight = 0;
                for (int i = 0; i < samplesPerChannel; i++)
                {
                    sumLeft += samples[i * audioFrame.no_channels + 0] * samples[i * audioFrame.no_channels + 0];
                    sumRight += samples[i * audioFrame.no_channels + 1] * samples[i * audioFrame.no_channels + 1];
                }

                double rmsLeft = Math.Sqrt(sumLeft / samplesPerChannel);
                double rmsRight = Math.Sqrt(sumRight / samplesPerChannel);

                return (rmsLeft, rmsRight);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calculating stereo levels: {ex.Message}");
                return (0f, 0f);
            }

        }

        public void Destroy()
        {
            if (ndiFinder != null) ndiFinder = null;

            // Destroy the NDI library
            NDIlib.destroy();
        }

    }
}
