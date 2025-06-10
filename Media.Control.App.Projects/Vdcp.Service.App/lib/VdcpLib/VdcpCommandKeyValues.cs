using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
     class VdcpCommandKeyValues
    {
        public static readonly byte[] RECIVE_LOCALDISABLE          = new byte[4] { 0x02, 0x02, 0x00, 0x0C };
        public static readonly byte[] RECIVE_LOCALENABLE           = new byte[4] { 0x02, 0x02, 0x00, 0x0D };
        public static readonly byte[] RECIVE_STOP                  = new byte[4] { 0x02, 0x02, 0x10, 0x00 };
        public static readonly byte[] RECIVE_PLAY                  = new byte[4] { 0x02, 0x02, 0x10, 0x01 };
        public static readonly byte[] RECIVE_RECORD                = new byte[4] { 0x02, 0x02, 0x10, 0x02 };
        public static readonly byte[] RECIVE_FREEZE                = new byte[4] { 0x02, 0x02, 0x10, 0x03 };
        public static readonly byte[] RECIVE_STILL                 = new byte[4] { 0x02, 0x02, 0x10, 0x04 };
        public static readonly byte[] RECIVE_STEP                  = new byte[4] { 0x02, 0x02, 0x10, 0x05 };
        public static readonly byte[] RECIVE_CONTINUE              = new byte[4] { 0x02, 0x02, 0x10, 0x06 };
        public static readonly byte[] RECIVE_JOG                   = new byte[4] { 0x02, 0x03, 0x10, 0x07 };
        public static readonly byte[] RECIVE_VARIPLAY              = new byte[4] { 0x02, 0x05, 0x10, 0x08 };
        public static readonly byte[] RECIVE_UNFREEZE              = new byte[4] { 0x02, 0x02, 0x10, 0x09 };
        public static readonly byte[] RECIVE_EEMODE                = new byte[4] { 0x02, 0x03, 0x10, 0x0A };
        public static readonly byte[] RECIVE_RENAMEID              = new byte[4] { 0x02, 0x12, 0x20, 0x1D };
        public static readonly byte[] RECIVE_EXRENAMEID            = new byte[4] { 0x02, 0x12, 0xA0, 0x1D };
        public static readonly byte[] RECIVE_PRESETTIME            = new byte[4] { 0x02, 0x07, 0x20, 0x1E };
        public static readonly byte[] RECIVE_CLOSEPORT             = new byte[4] { 0x02, 0x03, 0x20, 0x21 };
        public static readonly byte[] RECIVE_SELECTPORT            = new byte[4] { 0x02, 0x03, 0x20, 0x22 };
        public static readonly byte[] RECIVE_RECORDINIT            = new byte[4] { 0x02, 0x0E, 0x20, 0x23 };
        public static readonly byte[] RECIVE_EXRECORDINIT          = new byte[4] { 0x02, 0x0E, 0xA0, 0x23 };

        public static readonly byte[] RECIVE_SELECTINPUT           = new byte[4] { 0x02, 0x0A, 0x20, 0x39 };

        public static readonly byte[] RECIVE_PLAYCUE               = new byte[4] { 0x02, 0x0A, 0x20, 0x24 };
        public static readonly byte[] RECIVE_EXPLAYCUE             = new byte[4] { 0x02, 0x0A, 0xA0, 0x24 };
        public static readonly byte[] RECIVE_CUEWITHDATA           = new byte[4] { 0x02, 0x12, 0x20, 0x25 };
        public static readonly byte[] RECIVE_EXCUEWITHDATA         = new byte[4] { 0x02, 0x12, 0xA0, 0x25 };
        public static readonly byte[] RECIVE_DELETEID              = new byte[4] { 0x02, 0x0A, 0x20, 0x26 };
        public static readonly byte[] RECIVE_EXDELETEID            = new byte[4] { 0x02, 0x0A, 0xA0, 0x26 };
        public static readonly byte[] RECIVE_CLEAR                 = new byte[4] { 0x02, 0x02, 0x20, 0x29 };
        public static readonly byte[] RECIVE_SIGNALFULL            = new byte[4] { 0x02, 0x03, 0x20, 0x2B };
        public static readonly byte[] RECIVE_RECODEINITWITHDATA    = new byte[4] { 0x02, 0x12, 0x20, 0x2C };
        public static readonly byte[] RECIVE_EXRECODEINITWITHDATA  = new byte[4] { 0x02, 0x12, 0xA0, 0x2C };
        public static readonly byte[] RECIVE_PRESET                = new byte[4] { 0x02, 0x02, 0x20, 0x30 };
        public static readonly byte[] RECIVE_DISKPREROLL           = new byte[4] { 0x02, 0x04, 0x20, 0x43 };
        public static readonly byte[] RECIVE_OPENPORT              = new byte[4] { 0x02, 0x04, 0x30, 0x01 };
        public static readonly byte[] RECIVE_NEXT                  = new byte[4] { 0x02, 0x02, 0x30, 0x02 };
        public static readonly byte[] RECIVE_EXNEXT                = new byte[4] { 0x02, 0x02, 0xB0, 0x02 };
        public static readonly byte[] RECIVE_LAST                  = new byte[4] { 0x02, 0x02, 0x30, 0x03 };
        public static readonly byte[] RECIVE_EXLAST                = new byte[4] { 0x02, 0x02, 0xB0, 0x03 };
        public static readonly byte[] RECIVE_PORTSTATUS            = new byte[4] { 0x02, 0x03, 0x30, 0x05 };
        public static readonly byte[] RECIVE_POSTIONREQUEST        = new byte[4] { 0x02, 0x03, 0x30, 0x06 };
        public static readonly byte[] RECIVE_SYSTEMSTATUS          = new byte[4] { 0x02, 0x03, 0x30, 0x10 };
        public static readonly byte[] RECIVE_LIST                  = new byte[4] { 0x02, 0x02, 0x30, 0x11 };
        public static readonly byte[] RECIVE_EXLIST                = new byte[4] { 0x02, 0x02, 0xB0, 0x11 };
        public static readonly byte[] RECIVE_SIZEREQUEST           = new byte[4] { 0x02, 0x0A, 0x30, 0x14 };
        public static readonly byte[] RECIVE_EXSIZEREQUEST         = new byte[4] { 0x02, 0x0A, 0xB0, 0x14 };
        public static readonly byte[] RECIVE_IDREQUEST             = new byte[4] { 0x02, 0x0A, 0x30, 0x16 };
        public static readonly byte[] RECIVE_EXIDREQUEST           = new byte[4] { 0x02, 0x0A, 0xB0, 0x16 };

        public static readonly byte[] RECIVE_ACTIVEIDREQUEST       = new byte[4] { 0x02, 0x02, 0x30, 0x07 };
        public static readonly byte[] RECIVE_EXACTIVEIDREQUEST     = new byte[4] { 0x02, 0x02, 0xB0, 0x07 };


        public static readonly byte[] SEND_LOCALDISABLE           = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_LOCALENABLE            = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_STOP                   = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_PLAY                   = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_RECORD                 = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_FREEZE                 = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_STILL                  = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_STEP                   = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_CONTINUE               = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_JOG                    = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_VARIPLAY               = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_UNFREEZE               = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EEMODE                 = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_RENAMEID               = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXRENAMEID             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_PRESETTIME             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_CLOSEPORT              = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_SELECTPORT             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_RECORDINIT             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXRECORDINIT           = new byte[4] { 0x00, 0x00, 0x04, 0x00 };

        public static readonly byte[] SEND_SELECTINPUT            = new byte[4] { 0x00, 0x00, 0x04, 0x00 };

        public static readonly byte[] SEND_PLAYCUE                = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXPLAYCUE              = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_CUEWITHDATA            = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXCUEWITHDATA          = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_DELETEID               = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXDELETEID             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_CLEAR                  = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_SIGNALFULL             = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_RECODEINITWITHDATA     = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_EXRECODEINITWITHDATA   = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_PRESET                 = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_DISKPREROLL            = new byte[4] { 0x00, 0x00, 0x04, 0x00 };
        public static readonly byte[] SEND_OPENPORT               = new byte[4] { 0x00, 0x03, 0x30, 0x81 };
        public static readonly byte[] SEND_NEXT                   = new byte[4] { 0x00, 0xff, 0x30, 0x82 };
        public static readonly byte[] SEND_EXNEXT                 = new byte[4] { 0x00, 0xff, 0x30, 0x82 };
        public static readonly byte[] SEND_LAST                   = new byte[4] { 0x00, 0xff, 0x30, 0x82 };
        public static readonly byte[] SEND_EXLAST                 = new byte[4] { 0x00, 0xff, 0x30, 0x82 };
        public static readonly byte[] SEND_PORTSTATUS             = new byte[4] { 0x00, 0xff, 0x30, 0x85 };
        public static readonly byte[] SEND_POSTIONREQUEST         = new byte[4] { 0x00, 0x07, 0x30, 0x86 };
        public static readonly byte[] SEND_SYSTEMSTATUS           = new byte[4] { 0x00, 0xff, 0x30, 0x90 };
        public static readonly byte[] SEND_LIST                   = new byte[4] { 0x00, 0xff, 0x30, 0x91 };
        public static readonly byte[] SEND_EXLIST                 = new byte[4] { 0x00, 0xff, 0x30, 0x91 };
        public static readonly byte[] SEND_SIZEREQUEST            = new byte[4] { 0x00, 0x06, 0x30, 0x94 };
        public static readonly byte[] SEND_EXSIZEREQUEST          = new byte[4] { 0x00, 0x06, 0x30, 0x94 };
        public static readonly byte[] SEND_IDREQUEST              = new byte[4] { 0x00, 0x03, 0x30, 0x96 };
        public static readonly byte[] SEND_EXIDREQUEST            = new byte[4] { 0x00, 0x03, 0x30, 0x96 };

        public static readonly byte[] SEND_ACTIVEIDREQUEST = new byte[4] { 0x02, 0x11, 0x30, 0x87 };
        public static readonly byte[] SEND_EXACTIVEIDREQUEST = new byte[4] { 0x02, 0x11, 0x30, 0x87 };

        public static readonly byte ACK = 0x04;
        public static readonly byte NAk = 0x05;

    }
}
