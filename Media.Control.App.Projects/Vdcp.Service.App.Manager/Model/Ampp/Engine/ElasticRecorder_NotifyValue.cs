using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model.Engine
{

    public class Ping
    { 
        public string Workload { get;set; }
        public string Application { get; set; }
        public string Success { get; set; }
        public string Time { get; set; }

    }

    public class Prepare
    {
        public string Source { get; set; }
        public string FileName { get; set; }
        public string StartAt { get; set; }
        public int Duration { get; set; }
        public string StartMode { get; set; }
        public string RecorderState { get; set; }
    }


    public class Recordconfig
    {

        public Mode Mode { get; set; }
        public string RecordLocationType { get; set; }
        public bool ChunkingEnabled { get; set; }
        public string ChunkSize { get; set; }
        public string RouterType { get; set; }
        public string Fabric { get; set; }
        public string RouterDestination { get; set; }
        public string RouterSource { get; set; }
        public string Profile { get; set; }
        public List<ProfileParameter> ProfileParameters { get; set; } = new List<ProfileParameter>();
        public string AmSiteId { get; set; }
        public string TimecodeType { get; set; }
        public string StartTimecode { get; set; }
        public string UtcOffset { get; set; }
        public string ProxyQuality { get; set; }
        public string MaxAudioChannels { get; set; }
        public VideoStandard VideoStandard { get; set; }
    }


    public class Mode
    {

        public string Id { get; set; }
        public string Channel { get; set; }
        public string Network { get; set; }
        public string SourceType { get; set; }
        public string Source { get; set; }
    }


    public class ProfileParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class VideoStandard
    {
        public string FrameRate { get; set; }
        public string Resolution { get; set; }
        public string ScanMode { get; set; }
    }

    public class Recordingstate
    {
        public string Source { get; set; }
        public string FileName { get; set; }
        public string RecordingStatus { get; set; }
        public string AssetId { get; set; }
    }

    public class ServiceHealth
    { 
        public string Health { get; set; }
        public string HealthText { get; set; }

    }

    public class RecordInfo
    {
        public ServiceHealth ServiceHealth { get; set; }

        public string State { get; set; }

        public string RecordedFrames { get; set; }
    }

    public class Inputstate
    {
        public int Index { get; set; }
        public bool Program { get; set; }

    }

}
