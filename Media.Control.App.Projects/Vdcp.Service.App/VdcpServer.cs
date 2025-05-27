using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VdcpService.lib;
using VdcpService;

namespace Vdcp.Service.App
{
    public class VdcpServer
    {
        private Thread thread = null;
        private bool thrStart = false;
        public string PortNmae { get; set; }
        public VdcpService vdcpService = null;
        List<string> clipList = null;

        public VdcpServer(string portName)
        {
            this.PortNmae = portName;
            this.vdcpService = new VdcpService(portName);
            this.clipList = new List<string>();

            vdcpService.VdcpActionEvent += VdcpService_VdcpActionEvent;

            thread = new Thread(new ThreadStart(DoVdcpWork));
            thread.Start();
            thrStart = true;
        }

        private void VdcpService_VdcpActionEvent(VdcpEventArgsDefine commandData)
        {
            switch (commandData.CommandKey)
            {
                case EumCommandKey.NORMAL: break;
                case EumCommandKey.LOCALDISABLE:
                case EumCommandKey.LOCALENABLE:
                case EumCommandKey.STOP:
                case EumCommandKey.PLAY:
                case EumCommandKey.RECORD:
                case EumCommandKey.FREEZE:
                case EumCommandKey.STILL:
                case EumCommandKey.STEP:
                case EumCommandKey.CONTINUE:
                    break;
                case EumCommandKey.JOG:
                    break;
                case EumCommandKey.VARIPLAY:
                    break;
                case EumCommandKey.UNFREEZE:
                    break;
                case EumCommandKey.EEMODE:

                    break;
                case EumCommandKey.RENAMEID:
                    break;
                case EumCommandKey.EXRENAMEID:
                    break;
                case EumCommandKey.PRESETTIME:
                    break;
                case EumCommandKey.CLOSEPORT:
                    break;
                case EumCommandKey.SELECTPORT:
                    break;
                case EumCommandKey.RECORDINIT:
                    break;
                case EumCommandKey.EXRECORDINIT:
                    break;
                case EumCommandKey.PLAYCUE:
                    break;
                case EumCommandKey.EXPLAYCUE:
                    break;
                case EumCommandKey.CUEWITHDATA:
                    break;
                case EumCommandKey.EXCUEWITHDATA:
                    break;
                case EumCommandKey.DELETEID:
                    break;
                case EumCommandKey.EXDELETEID:
                    break;
                case EumCommandKey.CLEAR:
                    break;
                case EumCommandKey.SIGNALFULL:
                    break;
                case EumCommandKey.RECODEINITWITHDATA:
                    break;
                case EumCommandKey.EXRECODEINITWITHDATA:
                    break;
                case EumCommandKey.PRESET:
                    break;
                case EumCommandKey.DISKPREROLL:
                    break;
                case EumCommandKey.OPENPORT:
                    break;
                case EumCommandKey.NEXT:
                    Next();
                    break;
                case EumCommandKey.EXNEXT:
                    ExNext();
                    break;
                case EumCommandKey.LIST:
                    List();
                    break;
                case EumCommandKey.EXLIST:
                    ExList();
                    break;
                case EumCommandKey.LAST: break;
                case EumCommandKey.PORTSTATUS:
                    //Console.WriteLine($"------------> PORTSTATUS :[] ");
                    break;
                case EumCommandKey.POSTIONREQUEST:
                    //Console.WriteLine($"---------------> Timecode: { duration}");
                    break;
                case EumCommandKey.SYSTEMSTATUS:
                    SystemStatus();
                    break;
                case EumCommandKey.SIZEREQUEST:
                    IDSizeRequest(); break;
                case EumCommandKey.IDREQUEST:
                    IDRequest(); break;
                case EumCommandKey.EXSIZEREQUEST:
                    ExIDSizeRequest(); break;
                case EumCommandKey.EXIDREQUEST:
                    ExIDRequest(); break;
                case EumCommandKey.ACTIVEIDREQUEST:
                    AtiveIDRequest(); break;
                case EumCommandKey.EXACTIVEIDREQUEST:
                    ExAtiveIDRequest(); break;
            }

        }

        public bool Open(EnuPortType enuPortType, string address, int port, bool b)
        {
            bool result = false;
            if (vdcpService.Open(enuPortType, address, port, b))
                result = true;

            return result;
        }

        public void Close()
        {
            vdcpService.Close();
        }


        private void List()
        {


            vdcpService.List(clipList, SendType.Normal);
        }

        private void ExList()
        {
            vdcpService.List(clipList, SendType.Expansion);
        }

        private void Next()
        {
            vdcpService.Next(clipList, SendType.Normal);
        }

        private void ExNext()
        {
            vdcpService.Next(clipList, SendType.Expansion);
        }

        private void IDSizeRequest()
        {
            vdcpService.IDSizeRequest("", SendType.Normal);
        }

        private void ExIDSizeRequest()
        {
            vdcpService.IDSizeRequest("", SendType.Expansion);
        }


        private void IDRequest()
        {
            vdcpService.IDRequest(true, SendType.Normal);
        }

        private void ExIDRequest()
        {
            vdcpService.IDRequest(true, SendType.Expansion);
        }

        private void AtiveIDRequest()
        {
            //vdcp.ActiveIDRequest("00001234", SendType.Normal);
            vdcpService.ActiveIDRequest("", SendType.Normal);
        }

        private void ExAtiveIDRequest()
        {
            vdcpService.ActiveIDRequest("test000000001", SendType.Expansion);
        }

        private void SystemStatus()
        {
            vdcpService.SystemStatus();
        }

        private void DoVdcpWork()
        {
            while (true)
            {
                if (thrStart)
                {
                    //++timecode;
                    //--retimecode;
                    //vdcp.CurrentTimeCode = FrameToDFTC(timecode);
                    //vdcp.RemainingTimeCode = FrameToDFTC(retimecode);
                }

                Thread.Sleep(30);
            }
        }

    }
}
