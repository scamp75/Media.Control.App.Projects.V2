using Media.Control.App.RP.Model.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class MediaPlayList
    {
        public PlayListDefine SetPlayListItem(MediaDataInfo media)
        {
            PlayListDefine playListDefine = new PlayListDefine();

            playListDefine.Index = media.Index;
            playListDefine.Duration = media.Frame;
            playListDefine.MediaName = media.Name;
            playListDefine.State = media.State;
            //playListDefine.Macro = media.Name;
           // playListDefine.Control = engineControl;
            playListDefine.Timecode = media.InTimeCode;
            playListDefine.IsPlaying = false;


            //if((media.Index % 2) == 0) // 짝수
            //    playListDefine.Macro = 2;
            //else
            //    playListDefine.Macro = 1;

            //PlayListDefines.Add(playListDefine);

            return playListDefine;

        }
    }
}
