using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Config
{
    public class ChannelConfig
    {
        public EnmControlType ChannelType { get; set; }

        public EnuChannel Channel { get; set; }

        public string Name { get; set; }

        public string WorkLoad1 { get; set; }

        public string WorkLoad2 { get; set; }


        public List<InputConfigData> InPutList { get; set; }  = new List<InputConfigData>();  
    }
}
