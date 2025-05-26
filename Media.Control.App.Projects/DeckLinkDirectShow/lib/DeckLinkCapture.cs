using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;

namespace DeckLinkDirectShowLib
{
    internal class DeckLinkCapture
    {
        private IGraphBuilder graphBuilder;
        private ICaptureGraphBuilder2 captureGraphBuilder;
        private IMediaControl mediaControl;

        private IBaseFilter deckLinkFilter;
        private IBaseFilter adeckLinkFilter;
        private IBaseFilter videoSampleGrabberFilter;
        private IBaseFilter audioSampleGrabberFilter;
        private ISampleGrabber videoSampleGrabber;
        private ISampleGrabber audioSampleGrabber;
        private VideoFrameConverter videoFrameConverter = new VideoFrameConverter();

        // 사용자에게 전달할 이벤트 (영상, 오디오 콜백 이벤트는 별도 구현)
        public event EventHandler<VideoFrameEventArgs> VideoFrameReceived;
        public event EventHandler<AudioSampleEventArgs> AudioSampleReceived;

        public void Start(string vguid,string aguid )
        {

            try
            {
                int hr;

                // 1. IGraphBuilder 생성
                graphBuilder = (IGraphBuilder)new FilterGraph();

                // 1-1. ICaptureGraphBuilder2 생성 후 FilterGraph 설정
                captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
                hr = captureGraphBuilder.SetFiltergraph(graphBuilder);
                DsError.ThrowExceptionForHR(hr);

                // 2. 선택된 DeckLink 장치 필터 생성
                // (아래 CLSID는 예시입니다. 실제 DeckLink 필터의 CLSID로 변경하세요.)
                Guid clsidDeckLink = new Guid(vguid);
                Type deckLinkType = Type.GetTypeFromCLSID(clsidDeckLink);
                deckLinkFilter = (IBaseFilter)Activator.CreateInstance(deckLinkType);

                Guid aclsidDeckLink = new Guid(aguid);
                Type adeckLinkType = Type.GetTypeFromCLSID(aclsidDeckLink);
                adeckLinkFilter = (IBaseFilter)Activator.CreateInstance(adeckLinkType);

                hr = graphBuilder.AddFilter(deckLinkFilter, "DeckLink Video Capture");
                DsError.ThrowExceptionForHR(hr);
                hr = graphBuilder.AddFilter(adeckLinkFilter, "DeckLink Audio Capture");
                DsError.ThrowExceptionForHR(hr);

                // 3. 영상용 Sample Grabber 필터 생성 및 추가
                videoSampleGrabberFilter = (IBaseFilter)new SampleGrabber(); // DirectShowLib.SampleGrabber
                videoSampleGrabber = (ISampleGrabber)videoSampleGrabberFilter;
                hr = graphBuilder.AddFilter(videoSampleGrabberFilter, "Video Sample Grabber");
                DsError.ThrowExceptionForHR(hr);

                object stramConfig = new object();//(IAMStreamConfig) new IAMStreamConfig();
                Guid iid = typeof(IAMStreamConfig).GUID;
                captureGraphBuilder.FindInterface(PinCategory.Capture, MediaType.Video
                                                    , deckLinkFilter, typeof(IAMStreamConfig).GUID, out stramConfig);
                IAMStreamConfig videoStreamConfig = stramConfig as IAMStreamConfig;

                //// 영상 미디어 타입 설정 (예: RGB24 포맷)
                AMMediaType videoMediaType = new AMMediaType
                {
                    majorType = MediaType.Video,
                    subType = MediaSubType.RGB24,
                    formatType = FormatType.VideoInfo
                };
                hr = videoSampleGrabber.SetMediaType(videoMediaType);
                DsError.ThrowExceptionForHR(hr);
                DsUtils.FreeAMMediaType(videoMediaType);

                //AMMediaType videoMediaType = GetCustomVideoMediaType();

                //// 3. 오디오용 Sample Grabber 필터 생성 및 추가
                audioSampleGrabberFilter = (IBaseFilter)new SampleGrabber();
                audioSampleGrabber = (ISampleGrabber)audioSampleGrabberFilter;
                hr = graphBuilder.AddFilter(audioSampleGrabberFilter, "Audio Sample Grabber");
                DsError.ThrowExceptionForHR(hr);

                // 오디오 미디어 타입 설정 (예: PCM)
                //AMMediaType audioMediaType = new AMMediaType
                //{
                //    majorType = MediaType.Audio,
                //    subType = MediaSubType.PCM,
                //    formatType = FormatType.WaveEx
                //};

                AMMediaType audioMediaType = GetDefaultAudioMediaType();

                hr = audioSampleGrabber.SetMediaType(audioMediaType);
                DsError.ThrowExceptionForHR(hr);
                DsUtils.FreeAMMediaType(audioMediaType);

                IBaseFilter audioNullRenderer = (IBaseFilter)new NullRenderer();
                hr = this.graphBuilder.AddFilter(audioNullRenderer, "Audio Null Renderer");
                DsError.ThrowExceptionForHR(hr);

                // Color Space Converter 필터 생성 및 추가
                IBaseFilter colorConverter = (IBaseFilter)new Colour(); // DirectShowLib의 Colour 필터
                hr = this.graphBuilder.AddFilter(colorConverter, "Color Space Converter");
                DsError.ThrowExceptionForHR(hr);

                // 4. 필터 연결 (영상 및 오디오 경로)
                // 영상 스트림 연결 : DeckLink -> Video Sample Grabber
                hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, deckLinkFilter, videoSampleGrabberFilter, null);
                DsError.ThrowExceptionForHR(hr);

                // 오디오 스트림 연결 : DeckLink -> Audio Sample Grabber
                hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Audio, adeckLinkFilter, audioSampleGrabberFilter, null);
                DsError.ThrowExceptionForHR(hr);

                // 5. 콜백 등록 : 각 Sample Grabber에 ISampleGrabberCB 구현 객체 등록
                // SampleGrabberCallback는 ISampleGrabberCB 인터페이스를 구현하며, 각각의 콜백 메서드 내부에서
                // VideoFrameReceived, AudioSampleReceived 이벤트를 호출하도록 구현합니다.
                SampleGrabberCallback videoCallback = new SampleGrabberCallback(OnVideoSampleReceived);
                hr = videoSampleGrabber.SetCallback(videoCallback, 1); // 1: Buffer callback 방식 사용
                DsError.ThrowExceptionForHR(hr);

                SampleGrabberCallback audioCallback = new SampleGrabberCallback(OnAudioSampleReceived);
                hr = audioSampleGrabber.SetCallback(audioCallback, 1);
                DsError.ThrowExceptionForHR(hr);

                //6.MediaControl.Run() 호출하여 캡처 시작
                mediaControl = (IMediaControl)graphBuilder;

                // IVideoWindow 인터페이스를 얻어 AutoShow를 false로 설정
                IVideoWindow videoWindow = graphBuilder as IVideoWindow;
                if (videoWindow != null)
                {
                    videoWindow.put_AutoShow(OABool.False);
                }

                hr = mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            
        }

        public void Stop()
        {
            // Stop() 호출 전에 콜백 해제
            if (videoSampleGrabber != null)
                videoSampleGrabber.SetCallback(null, 1);
            if (audioSampleGrabber != null)
                audioSampleGrabber.SetCallback(null, 1);


            if(mediaControl != null)
            {
                mediaControl.Pause();
              //  int hr = mediaControl.Stop();
              //  DsError.ThrowExceptionForHR(hr);

            }

            // 5. 가비지 컬렉션 강제 호출 (선택사항)
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // 영상 샘플 콜백 (SampleGrabberCallback에서 호출)
        private int OnVideoSampleReceived(IntPtr sampleBuffer, int bufferSize)
        {
            // 버퍼 데이터를 Bitmap 등 영상 객체로 변환 후 이벤트 전달
            var frame = videoFrameConverter.ConvertBufferToBitmap(sampleBuffer, bufferSize , 720, 480, false);
            VideoFrameEventArgs args = new VideoFrameEventArgs(frame);
            VideoFrameReceived?.Invoke(this, args);
            return 0;
        }

        // 오디오 샘플 콜백 (SampleGrabberCallback에서 호출)
        private int OnAudioSampleReceived(IntPtr sampleBuffer, int bufferSize)
        {
            // 예: 오디오 데이터를 byte[]로 복사하여 이벤트 전달
            byte[] audioData = new byte[bufferSize];
            Marshal.Copy(sampleBuffer, audioData, 0, bufferSize);
            AudioSampleEventArgs args = new AudioSampleEventArgs(audioData);
            AudioSampleReceived?.Invoke(this, args);
            return 0;
        }
        public static AMMediaType GetDefaultAudioMediaType()
        {
            // 1. WaveFormatEx 구조체 준비
            WaveFormatEx wfx = new WaveFormatEx
            {
                wFormatTag = 1,               // WAVE_FORMAT_PCM
                nChannels = 2,               // 스테레오
                nSamplesPerSec = 48000,       // 48kHz
                wBitsPerSample = 16,         // 16비트
                nBlockAlign = (short)(2 * 16 / 8), // (채널 * 비트수/8)
            };
            wfx.nAvgBytesPerSec = wfx.nSamplesPerSec * wfx.nBlockAlign;
            wfx.cbSize = 0; // 확장 정보 없음

            // 2. AMMediaType 생성
            AMMediaType audioMediaType = new AMMediaType
            {
                majorType = MediaType.Audio,
                subType = MediaSubType.PCM,
                formatType = FormatType.WaveEx,
                formatSize = Marshal.SizeOf(typeof(WaveFormatEx)),
                formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WaveFormatEx)))
            };

            // 3. 구조체 -> 메모리 복사
            Marshal.StructureToPtr(wfx, audioMediaType.formatPtr, false);

            return audioMediaType;
        }

        public class SampleGrabberCallback : ISampleGrabberCB
        {
            private readonly Func<IntPtr, int, int> callbackFunc;

            public SampleGrabberCallback(Func<IntPtr, int, int> callbackFunc)
            {
                this.callbackFunc = callbackFunc;
            }

            // Buffer callback
            public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
            {
                return callbackFunc(pBuffer, BufferLen);
            }

            // Not used in this 예시
            public int SampleCB(double SampleTime, IMediaSample pSample)
            {
                return 0;
            }
        }

    }
}
