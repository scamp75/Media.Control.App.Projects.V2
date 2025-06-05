using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{
    public class VdcpStateThread
    {
        public string Name { get; set; } = string.Empty;

        public bool IsRunning { get; set; } = false;
;
        public string CurrentTimeCode { get; set; } = "00:00:00:00";
        public string RemainingTimeCode { get; set; } = "00:00:00:00";

        private Thread threadVdcp = null;

        public VdcpStateThread() { }
        public VdcpStateThread(string name)
        {
            Name = name;

            threadVdcp = new Thread(new ThreadStart(ThreadStateRun));
        }

        private void ThreadStateRun()
        {
            while(true)
            {
                if(IsRunning)
                {

                    Thread.Sleep(30);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
