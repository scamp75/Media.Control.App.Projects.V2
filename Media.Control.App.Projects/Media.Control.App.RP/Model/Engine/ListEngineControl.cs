using Media.Control.App.RP.Model.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model
{
    public class ListEngineControl
    {
        public int ActionCleancut { get; set; }

        public int NextCleancut { get; set; }

        public EngineControl ActionControl { get; set; }

        public EngineControl NextControl { get; set; }


        public string ActionControlName
        {
            get
            {
                if (ActionControl != null)
                {
                    return ActionControl.EnagineName;
                }

                return string.Empty;
            }
        }

        public string NextControlName
        {
            get
            {
                if (NextControl != null)
                {
                    return NextControl.EnagineName;
                }
                return string.Empty;
            }
        }

        public ListEngineControl(int actionCleancut, int nextCleancut, EngineControl actionControl, EngineControl nextControl)
        {
            ActionCleancut = actionCleancut;
            NextCleancut = nextCleancut;
            ActionControl = actionControl;
            NextControl = nextControl;
        }

        public void ChangeEngine()
        {
            EngineControl temp = ActionControl;

            ActionControl = NextControl;
            NextControl = temp;

            int tempIndex = ActionCleancut;
            ActionCleancut = NextCleancut;
            NextCleancut = tempIndex;
        }


        
    }
}
