using Media.Control.App.RP.Model.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class OnairMediaList
    {
        public CueMediaInfo CueMedia { get; set; } = new CueMediaInfo();
        public PlayMediaInfo PlayMedia { get; set; } = new PlayMediaInfo();

        public void Init()
        {
            CueMedia.Init();
            PlayMedia.Init();
        }

        public void SetCueMediaInfo(EngineControl engine, string path, MediaDataInfo mediaData, int cleancut, double fps)
        {
            CueMedia.Path = path;
            CueMedia.Control = engine;
            CueMedia.MediaData = mediaData;
            CueMedia.Cleancut = cleancut;
            CueMedia.Fps = fps;
        }

        public void SetPlayMediaInfo(EngineControl engine, string path,  MediaDataInfo mediaData, int cleancut, double fps)
        {
            PlayMedia.Path = path;
            PlayMedia.Control = engine;
            PlayMedia.MediaData = mediaData;
            PlayMedia.Cleancut = cleancut;
            PlayMedia.Fps = fps;
        }

        public void ChangePlayMediaInfo()
        {
            PlayMedia.Path = CueMedia.Path;
            PlayMedia.Control = CueMedia.Control;
            PlayMedia.MediaData = CueMedia.MediaData;
            PlayMedia.Cleancut = CueMedia.Cleancut;
            PlayMedia.Fps = CueMedia.Fps;
        }

        public void ChangeEngine()
        {
            EngineControl temp = PlayMedia.Control;

            PlayMedia.Control = CueMedia.Control;
            CueMedia.Control = temp;

            int tempIndex = PlayMedia.Cleancut;
            PlayMedia.Cleancut = CueMedia.Cleancut;
            CueMedia.Cleancut = tempIndex;
        }

    }
}
