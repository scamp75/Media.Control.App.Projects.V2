using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{

    
    public class VdcpEventArgsDefine 
    {
       
        private string VdcpData = string.Empty;
        private string Name = string.Empty;

        public EumCommandKey CommandKey { get; set; } = EumCommandKey.NORMAL;
        public EumEEMode EEMode { get; set; } = EumEEMode.Off;
        public string TimeCode { get; set; } = "00:00:00:00";

        public string StatTime { get;set; } = "00:00:00:00";
        public string StopTime { get; set; } = "00:00:00:00";

        public string Duration { get; set; } = "00:00:00:00";

        public string NewName { get; set; }
        public string OldName { get; set; }
        public string ClipName { get; set; }

        public string sSom { get; set; }
        public string sEom { get; set; }
        public int Frames { get; set; }
        public double Jog { get; set; } = 0.0;

        public double Shuttle { get; set; } = 0.0;

        public string Input { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;

        public int Seconds { get; set; }
        public int iPortNum { get; set; }
        public int iValue1 { get; set; }
        public int iValue2 { get; set; }
        public float fValue { get; set; }
        public int iTimeCode { get; set; }
        public long iSom { get; set; }
        public long iEom { get; set; }

        public bool bLock { get; set; }

        public System.IO.Ports.SerialError ErrorType { get; set; }

        internal VdcpEventArgsDefine(EumCommandKey key)
        {
            CommandKey = key;
        }

        internal VdcpEventArgsDefine()
        {
         
        }

        internal VdcpEventArgsDefine(EumCommandKey key, string data)
        {
            CommandKey = key;
            ClipName = data;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, System.IO.Ports.SerialError type)
        {
            CommandKey = key;
            ErrorType = type;
        }

        internal VdcpEventArgsDefine(string name, EumCommandKey key, string data)
        {
            Name = name;
            CommandKey = key;
            VdcpData = data;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, EumEEMode mode)
        {
            CommandKey = key;
            EEMode = mode;
        }

        internal VdcpEventArgsDefine(EumCommandKey key , string value1, string value2)
        {
            CommandKey = key;

            switch (key)
            {
                case EumCommandKey.RENAMEID:
                case EumCommandKey.EXRENAMEID:
                    OldName = value1;
                    NewName = value2;
                    break;
                case EumCommandKey.RECORDINIT:
                case EumCommandKey.EXRECORDINIT:
                    ClipName = value1;
                    TimeCode = value2;
                    break;
            }
        }

        internal VdcpEventArgsDefine(EumCommandKey key, string clipName, string som, string eom)
        {
            CommandKey = key;
            ClipName = clipName;
            sSom = som;
            sEom = eom;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, string clipNmae, long som, long eom)
        {
            CommandKey = key;
            ClipName = clipNmae;
            iSom = som;
            iEom = eom;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, int value)
        {
            CommandKey = key;
            iValue1 = value;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, float value)
        {
            CommandKey = key;
            fValue = value;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, int value1, int value2)
        {
            CommandKey = key;
            Frames = value1;
            Seconds = value2;
        }

        internal VdcpEventArgsDefine(EumCommandKey key, int portNum, bool mode)
        {
            CommandKey = key;
            iPortNum = portNum;
            bLock   = mode;
        }

    }
}
