using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Service.App.Manager.Model
{ 
    public class LoggerModel
    {
        public Logger logger { get; set; } = null;
        public LoggerModel(string url) 
        {
            logger = new Logger(url);
    
        }

        public void SendLogger(string type, string channel, string title, string message)
        {
            logger.Log(type, channel, title, message);
        }
    }
}
