using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;
using DeckLinkDirectShowLib;

namespace DeckLinkDirectShowLib
{
    
    public class DeckLinkDirectShow
    {


        private DeckLinkCapture deckLinkCapture;
        private FilterEnumerator filterEnumerator;

        public event EventHandler<VideoFrameEventArgs> VideoFrameReceived;
        public event EventHandler<AudioSampleEventArgs> AudioSampleReceived;

        public string filterName { get; set; }

        public List<string> FilterList { get; set; }
        public List<string> aFilterList { get; set; }

        public DeckLinkDirectShow()
        {
            // DeckLink 장치 필터 검색 및 사용자에게 선택된 포트 전달
            //DeckLinkRegistryFinder.SearchDeckLinkCLSID();
            
            FilterList = new List<string>();
            aFilterList = new List<string>();
            filterEnumerator = new FilterEnumerator();
            filterEnumerator.FindDeckLinkFilter();
            FilterList = filterEnumerator.FilterList;
            aFilterList = filterEnumerator.aFilterList;

            deckLinkCapture = new DeckLinkCapture();
            deckLinkCapture.AudioSampleReceived += DeckLinkCapture_AudioSampleReceived;
            deckLinkCapture.VideoFrameReceived += DeckLinkCapture_VideoFrameReceived;

        }

        private void DeckLinkCapture_VideoFrameReceived(object sender, VideoFrameEventArgs e)
        {
            VideoFrameReceived?.Invoke(this, e);
        }

        private void DeckLinkCapture_AudioSampleReceived(object sender, AudioSampleEventArgs e)
        {
            AudioSampleReceived?.Invoke(this, e);
        }

        public void Start(string guid, string aguid)
        {
            deckLinkCapture.Start(guid, aguid);
        }

        public void Stop()
        {
            deckLinkCapture.Stop();
        }

    }
}
