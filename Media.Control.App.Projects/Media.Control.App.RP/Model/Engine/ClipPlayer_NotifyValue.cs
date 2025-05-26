using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Engine
{
    public class TransportState
    {
        public string State { get; set; }
        public string EndBehaviour { get; set; }
        public string StartAt { get; set; }

    }

    public class TransportCommand
    {
        public long Position { get; set; }
        public long InPosition { get; set; }
        public long OutPosition { get; set; }
        public string Rate { get; set; }
        public string EndBehaviour { get; set; }
        public string StartAt { get; set; }
    }


    public class PlayerConfig
    {
        public string ControlGroup { get; set; }
        public bool PreserveResolution { get; set; }
        public string Resolution { get; set; }
        public string ScanMode { get; set; }
        public string FrameRate { get; set; }
        public bool Alpha { get; set; }
        public string MediaPath { get; set; }
        public string BucketName { get; set; }
        public string BucketPrefix { get; set; }
    }


    public class VideoStandard2
    {
        public bool PreserveResolution { get; set; }
        public string Resolution { get; set; }
        public string ScanMode { get; set; }
        public string FrameRate { get; set; }
    }


    public class Clip
    {
        public string File { get; set; }

        public bool Loaded { get; set; }
    }

    public class AutoreCue
    {
        public bool Recue { get; set; }
    }


    public class ClaearAssets
    {
        public bool isCleared
        {   get; set; }

    }

    public class Shuttle
    {
        public double Rate { get; set; } 
    }

    public class Rate
    {
        public double rate { get; set; }
    }
}
