using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class PortStatusDefine
    {
        #region private
        private BooleanProperty idle = new BooleanProperty(0, 1);
        private BooleanProperty cueInit = new BooleanProperty(1, 1);
        
        private BooleanProperty playRecord = new BooleanProperty(2, 1);
        
        private BooleanProperty still = new BooleanProperty(3, 1);
        private BooleanProperty jog = new BooleanProperty(4, 1);
        private BooleanProperty varPlay = new BooleanProperty(5, 1);
        private BooleanProperty portBusy = new BooleanProperty(6, 1);
        private BooleanProperty cueInitDone = new BooleanProperty(7, 1);
        

        private BooleanProperty portDown = new BooleanProperty(0, 3);
        private BooleanProperty iDAdded = new BooleanProperty(1, 3);
        private BooleanProperty iDDelete = new BooleanProperty(2, 3);
        private BooleanProperty iDAddtoArch = new BooleanProperty(3, 3);
        private BooleanProperty noRefInput = new BooleanProperty(4, 3);
        private BooleanProperty noVideoInput = new BooleanProperty(5, 3);
        private BooleanProperty noAuioInput = new BooleanProperty(6, 3);
        private BooleanProperty audioOverLoad = new BooleanProperty(7, 3);

        private BooleanProperty systemError = new BooleanProperty(0, 4);
        private BooleanProperty illegalValue = new BooleanProperty(1, 4);
        private BooleanProperty invalidPort = new BooleanProperty(2, 4);
        private BooleanProperty wrongPortType = new BooleanProperty(3, 4);
        private BooleanProperty commandQueueFull = new BooleanProperty(4, 4);
        private BooleanProperty diskFull = new BooleanProperty(5, 4);
        private BooleanProperty cmdwhileBusy = new BooleanProperty(6, 4);
        private BooleanProperty notSupport = new BooleanProperty(7, 4);

        private BooleanProperty invalidId = new BooleanProperty(0, 5);
        private BooleanProperty iDNotFound = new BooleanProperty(1, 5);
        private BooleanProperty iDAlreadyExists = new BooleanProperty(2, 5);
        private BooleanProperty iDStilRecording = new BooleanProperty(3, 5);
        private BooleanProperty iDStillPlaying = new BooleanProperty(4, 5);
        private BooleanProperty iDNotTransferredFromaArchive = new BooleanProperty(5, 5);
        private BooleanProperty iDNotTransferredToArchive = new BooleanProperty(6, 5);
        private BooleanProperty iDDeleteProtected = new BooleanProperty(7, 5);

        private BooleanProperty notInCue = new BooleanProperty(0, 6);
        private BooleanProperty initState = new BooleanProperty(0, 6);
        private BooleanProperty cueNotDone = new BooleanProperty(1, 6);
        private BooleanProperty portNotIdle = new BooleanProperty(2, 6);
        private BooleanProperty portPlaying = new BooleanProperty(3, 6);
        private BooleanProperty active = new BooleanProperty(3, 6);
        private BooleanProperty portNotAchive = new BooleanProperty(4, 6);
        private BooleanProperty cueOrOperAtionfalied = new BooleanProperty(5, 6);
        private BooleanProperty netWorKError = new BooleanProperty(6, 6);
        private BooleanProperty systemReBooted = new BooleanProperty(7, 6);

        private BooleanProperty off = new BooleanProperty(0, 7);
        private BooleanProperty composite = new BooleanProperty(1, 7);
        private BooleanProperty sVidoe = new BooleanProperty(2, 7);
        private BooleanProperty yuv = new BooleanProperty(3, 7);
        private BooleanProperty d1 = new BooleanProperty(4, 7);
        #endregion

        public int PortNum { get; set; }
        public BooleanProperty Idle { get { return idle; } set { idle = value; } }
        public BooleanProperty CueInit { get { return cueInit; } set { cueInit = value; } }
        public BooleanProperty PlayRecord { get { return playRecord; } set { playRecord = value; } }
        
        public BooleanProperty Still { get { return still; } set { still = value; } }
        public BooleanProperty Jog { get { return jog; } set { jog = value; } }
        public BooleanProperty VarPlay { get { return varPlay; } set { varPlay = value; } }
        public BooleanProperty PortBusy { get { return portBusy ; } set { portBusy = value; } }
        public BooleanProperty CueInitDone { get { return cueInitDone; } set { cueInitDone = value; } }
        

        public BooleanProperty PortDown { get { return portDown; } set { portDown = value; } }
        public BooleanProperty IDAdded { get { return iDAdded; } set { iDAdded = value; } }
        public BooleanProperty IDDelete { get { return iDDelete; } set { iDDelete = value; } }
        public BooleanProperty IDAddtoArch { get { return iDAddtoArch; } set { iDAddtoArch = value; } }
        public BooleanProperty NoRefInput { get { return noRefInput; } set { noRefInput = value; } }
        public BooleanProperty NoVideoInput { get { return noVideoInput; } set { noVideoInput = value; } }
        public BooleanProperty NoAuioInput { get { return noAuioInput; } set { noAuioInput = value; } }
        public BooleanProperty AudioOverLoad { get { return audioOverLoad; } set { audioOverLoad = value; } }

        public BooleanProperty SystemError { get { return systemError; } set { systemError = value; } }
        public BooleanProperty IllegalValue { get { return illegalValue; } set { illegalValue = value; } }
        public BooleanProperty InvalidPort { get { return invalidPort; } set { invalidPort = value; } }
        public BooleanProperty WrongPortType { get { return wrongPortType; } set { wrongPortType = value; } }
        public BooleanProperty CommandQueueFull { get { return commandQueueFull; } set { commandQueueFull = value; } }
        public BooleanProperty DiskFull { get { return diskFull; } set { diskFull = value; } }
        public BooleanProperty CmdwhileBusy { get { return cmdwhileBusy; } set { cmdwhileBusy = value; } }
        public BooleanProperty NotSupport { get { return notSupport; } set { notSupport = value; } }

        public BooleanProperty InvalidId { get { return invalidId; } set { invalidId = value; } }
        public BooleanProperty IDNotFound { get { return iDNotFound; } set { iDNotFound = value; } }
        public BooleanProperty IDAlreadyExists { get { return iDAlreadyExists; } set { iDAlreadyExists = value; } }
        public BooleanProperty IDStilRecording { get { return iDStilRecording; } set { iDStilRecording = value; } }
        public BooleanProperty IDStillPlaying { get { return iDStillPlaying; } set { iDStillPlaying = value; } }
        public BooleanProperty IDNotTransferredFromaArchive { get { return iDNotTransferredFromaArchive; } set { iDNotTransferredFromaArchive = value; } }
        public BooleanProperty IDNotTransferredToArchive { get { return iDNotTransferredToArchive; } set { iDNotTransferredToArchive = value; } }
        public BooleanProperty IDDeleteProtected { get { return iDDeleteProtected; } set { iDDeleteProtected = value; } }

        public BooleanProperty NotInCue { get { return notInCue; } set { notInCue = value; } }
        public BooleanProperty InitState { get { return initState; } set { initState = value; } }
        public BooleanProperty CueNotDone { get { return cueNotDone; } set { cueNotDone = value; } }
        public BooleanProperty PortNotIdle { get { return portNotIdle; } set { portNotIdle = value; } }
        public BooleanProperty PortPlaying { get { return portPlaying; } set { portPlaying = value; } }
        public BooleanProperty Active { get { return active; } set { active = value; } }
        public BooleanProperty PortNotAchive { get { return portNotAchive; } set { portNotAchive = value; } }
        public BooleanProperty CueOrOperAtionfalied { get { return cueOrOperAtionfalied; } set { cueOrOperAtionfalied = value; } }
        public BooleanProperty NetWorKError { get { return netWorKError; } set { netWorKError = value; } }
        public BooleanProperty SystemReBooted { get { return systemReBooted; } set { systemReBooted = value; } }

        public BooleanProperty Off { get { return off; } set { off = value; } }
        public BooleanProperty Composite { get { return composite; } set { composite = value; } }
        public BooleanProperty SVidoe { get { return sVidoe; } set { sVidoe = value; } }
        public BooleanProperty Yuv { get { return yuv; } set { yuv = value; } }
        public BooleanProperty D1 { get { return d1; } set { d1 = value; } }


    }
}
