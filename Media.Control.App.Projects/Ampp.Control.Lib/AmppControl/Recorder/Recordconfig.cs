using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ampp.Control.Lib.AmppControl
{

    public enum EnmType { Local, S3_Bucket}
    internal class Recordconfig
    {
        public EnmType RecordLocationType { get; set; }

        public string RecordLocation { get; set; }
    }
}
