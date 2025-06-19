using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class RecordeConfig
    {
        public string DefultPath { get; set; } 
        public string DefultInput { get; set; }
        
        public string RecordProfile { get; set; } = "Default";
    }
}

