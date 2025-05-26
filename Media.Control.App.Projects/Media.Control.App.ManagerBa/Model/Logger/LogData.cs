using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.Logger
{
    public class LogData
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; }
        public DateTime CreateDate { get; set; }

        public string Time { get; set; }
    }
}
