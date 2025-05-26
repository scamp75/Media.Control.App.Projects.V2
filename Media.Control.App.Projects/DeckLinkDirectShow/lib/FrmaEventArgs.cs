using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DeckLinkDirectShowLib
{
    public class VideoFrameEventArgs : EventArgs
    {
        /// <summary>
        /// 캡처된 영상 프레임 (예: Bitmap)
        /// </summary>
        public Bitmap VideoFrame { get; }

        public VideoFrameEventArgs(Bitmap videoFrame)
        {
            VideoFrame = videoFrame;
        }
    }

    public class AudioSampleEventArgs : EventArgs
    {
        /// <summary>
        /// 캡처된 오디오 데이터 (예: PCM 데이터)
        /// </summary>
        public byte[] AudioData { get; }

        public AudioSampleEventArgs(byte[] audioData)
        {
            AudioData = audioData;
        }
    }
}
