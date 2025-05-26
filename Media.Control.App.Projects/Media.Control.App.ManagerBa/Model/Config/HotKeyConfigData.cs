using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.ManagerBa.Model.Config
{
    public  class HotKeyConfigData
    {
        public List<HotKeyDefin> HotKeys { get; set; }

        public HotKeyConfigData()
        {
            HotKeys = new List<HotKeyDefin>();  
        }
    }
}
