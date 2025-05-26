using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ampp.Control.Lib.AmppControl
{
    public enum EnmState { Prepare, Record, Stop }
    internal class Recordstate
    {
        public string Filename { get; set; }

        public EnmState State { get; set; }
    }
}
