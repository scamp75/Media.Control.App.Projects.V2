using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.lib.Api
{
    public class MediaDataInfo
    {
        public string MediaId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Duration { get; set; }
        public int Frame { get; set; }
        public DateTime CreatDate { get; set; }
        public int InPoint { get; set; }
        public string InTimeCode { get; set; }
        public int OutPoint { get; set; }
        public string OutTimeCode { get; set; }
        public string Creator { get; set; }
        public string Type { get; set; }
        public string Proxy { get; set; }
        public string Path { get; set; }
        public int Fps { get; set; }
        public string Des { get; set; }
        public string State { get; set; }
    }

    
}
