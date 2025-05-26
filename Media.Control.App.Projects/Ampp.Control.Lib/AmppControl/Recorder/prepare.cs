using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ampp.Control.Lib.AmppControl
{
    /// <summary>
    /// Command used to prepare a recording
    /// </summary>
    internal class Prepare
    {
        public string Source { get; set; }
        public string FileName { get; set; }
        public string  Profile { get; set; }    

    }
}
