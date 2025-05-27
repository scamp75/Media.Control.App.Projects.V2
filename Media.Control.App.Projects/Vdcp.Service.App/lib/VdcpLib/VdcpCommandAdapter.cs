using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class VdcpCommandAdapter
    {
        protected Dictionary<EumCommandKey, strVdcpCommand> CommandDictionary = null;
        protected readonly int CommandLength = 2;

        public readonly byte ACK = 0x04;
        public readonly byte NAK = 0x05;
        public static int PortNubmer = 0;

        public PortStatusDefine portStatus { get; set; } = new PortStatusDefine();

        public struct strVdcpCommand
        {
            public EumCommandKey Key { get; set; }
            public string Name { get; set; }
            public byte Stx { get; set; }
            public byte Bc { get; set; }
            public byte Cmd1 { get; set; }
            public byte Cmd2 { get; set; }
            public byte[] Data { get; set; }
            public byte ChkSum { get; set; }
            public byte ReBc { get; set; }
            public byte ReCmd1 { get; set; }
            public byte ReCmd2 { get; set; }
        }

        private BitArray[] PortStatusArrays = new BitArray[8]
        {
            new BitArray(8),     //
			new BitArray(8),     // 7:CUE/INTI DONE      , 6:PORT BUSY        , 5:VAR.PAY           , 4:JOG            , 3:STILL           , 2:PLAY/RECORD      , 1:CUE/INIT     , 0:IDLE
			new BitArray(8),     // 7 ~ 0 : PORT ID
			new BitArray(8),     // 7:AUDIOOVERLOAD      , 6:NOAUDIOINPUT     , 5:NOVIDOEINPUT      , 4:NOREFINPUT     , 3:IDADDTOARCH     , 2:IDDELETE         , 1:IDADDED      , 0:PORTDOWN 
			new BitArray(8),     // 7:NOTSUPPORT         , 6:CMDWHILEBUSY     , 5:DISKFULL          , 4:CMMANDFULL     , 3:WRONGPORTTYPE   , 2:INVALIDPORT      , 1:ILLEGALVALUE , 0:SYSTEMERROR
			new BitArray(8),     // 7:IDELETEPROTECTED   , 6:IDNOTTRANSFERTO  , 5:IDNOTTRANSFERFROM , 4:IDSTILLPLAYING , 3:IDSTILLRECORDING, 2:IDALREADYEXISTS  , 1:IDNOTFUND    , 0:INVALIDID
			new BitArray(8),     // 7:SYSTEMREBOOTED     , 6:NETWORKERROR     , 5:ACTIVE            , 4:PORTPLAYING    , 3:PORTNOTIDLE     , 2:CUENOTDONE       , 1:INITSTATE    , 0:NOTINCUE
			new BitArray(8)      // 7:                   , 6:                 , 5:                  , 4:D1             , 3:YUV             , 2:S-VIDEO          , 1:COMPSITE     , 0:OFF
        };

        private BitArray[] SystemStatusArrays = new BitArray[15]
        {
            new BitArray(8),     // 
			new BitArray(8),     // TOTAL TIME REMAINING (frames)
			new BitArray(8),     // TOTAL TIME REMAINING (Seconds)
			new BitArray(8),     // TOTAL TIME REMAINING (Minutes)
			new BitArray(8),     // TOTAL TIME REMAINING (Hours)
			new BitArray(8),     // NUMBER OF ID’S STORED - MS BYTE
			new BitArray(8),     // NUMBER OF ID’S STORED - LS BYTE
            new BitArray(8),     // 7:              , 6:                 , 5:           , 4:              , 3:REMOTE CONTROL , 2:DISK DOWN      , 1:SYSTEM DOWN   , 0:DISK FULL
            new BitArray(8),     // 7:              , 6:                 , 5:           , 4:              , 3:               , 2:               , 1:ARCHIVE FULL  , 0:ARCHIVE AVALABLE
			new BitArray(8),     // 7:              , 6:                 , 5:           , 4:              , 3:SYSTEM OFFLINE , 2: LOCALOFFLINE  , 1:SYSTEMOFFLINE , 0:LOCALOFFLINE
            new BitArray(8),     // STANDARD TIME (frames)
			new BitArray(8),     // STANDARD TIME (Seconds)
            new BitArray(8),     // STANDARD TIME (Minutes)
            new BitArray(8),     // STANDARD TIME (Hours)
            new BitArray(8)      // % SIGNAL FULL LEVEL


        };

        public VdcpCommandAdapter()
        {
            SetCommandKey();
        }

        internal string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data).Trim();
        }

        internal byte[] StringToBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        internal int BytesToInt(byte[] data)
        {
            int s1 = data[0] & 0xFF;
            int s2 = data[1] & 0xFF;
            int s3 = data[2] & 0xFF;
            int s4 = data[3] & 0xFF;

            return ((s1 << 24) + (s2 << 16) + (s3 << 8) + (s4 << 0));
        }

        internal byte[] SetSystemStatusDataProperty(SystemStatusDefine status)
        {
            byte[] systemStatusData = new byte[15];


            SystemStatusArrays[status.DiskFull.Column].Set(status.DiskFull.Row, status.DiskFull.PValue);
            SystemStatusArrays[status.SystemDown.Column].Set(status.SystemDown.Row, status.SystemDown.PValue);
            SystemStatusArrays[status.DiskDown.Column].Set(status.DiskDown.Row, status.DiskDown.PValue);
            SystemStatusArrays[status.RemoteControlDisabled.Column].Set(status.RemoteControlDisabled.Row, status.RemoteControlDisabled.PValue);


            SystemStatusArrays[status.ArchiveAvailable.Column].Set(status.ArchiveAvailable.Row, status.ArchiveAvailable.PValue);
            SystemStatusArrays[status.ArchiveFull.Column].Set(status.ArchiveFull.Row, status.ArchiveFull.PValue);

            SystemStatusArrays[status.LocalOffline.Column].Set(status.LocalOffline.Row, status.LocalOffline.PValue);
            SystemStatusArrays[status.SystemOffline.Column].Set(status.SystemOffline.Row, status.SystemOffline.PValue);

            SystemStatusArrays[status.LocalOfflineFull.Column].Set(status.LocalOfflineFull.Row, status.LocalOfflineFull.PValue);
            SystemStatusArrays[status.SystemOfflineFull.Column].Set(status.SystemOfflineFull.Row, status.SystemOfflineFull.PValue);


            byte[] totalRemTime = new byte[4];
            totalRemTime = StrTcToBcdBytes(status.TotalRemainTime);

            byte[] standardTime = new byte[4];
            standardTime = StrTcToBcdBytes(status.StandarTime);

            for (int i = 0; i < systemStatusData.Length; i++)
            {
                if (i == 0)
                    systemStatusData[i] = 0x0f;
                else if (i == 1)
                    systemStatusData[i] = totalRemTime[0];
                else if (i == 2)
                    systemStatusData[i] = totalRemTime[1];
                else if (i == 3)
                    systemStatusData[i] = totalRemTime[2];
                else if (i == 4)
                {
                    systemStatusData[i] = totalRemTime[3];
                    Console.WriteLine($"totalRemTime : {totalRemTime[0]}:{totalRemTime[1]}:{totalRemTime[2]}:{totalRemTime[3]}");

                }
                else if (i == 5)
                    systemStatusData[i] = 0x00;
                else if (i == 6)
                    systemStatusData[i] = 0x01;
                else if (i == 10)
                    systemStatusData[i] = standardTime[0];
                else if (i == 11)
                    systemStatusData[i] = standardTime[1];
                else if (i == 12)
                    systemStatusData[i] = standardTime[2];
                else if (i == 13)
                {
                    systemStatusData[i] = standardTime[3];
                    Console.WriteLine($"standardTime : {standardTime[0]}:{standardTime[1]}:{standardTime[2]}:{standardTime[3]}");
                }
                else if (i == 14)
                    systemStatusData[i] = Convert.ToByte(status.SignalFullLevel);
                else
                {
                    SystemStatusArrays[i].CopyTo(systemStatusData, i);
                    Console.WriteLine($" ArrNum {i} : {Convert.ToString(systemStatusData[i], 2).PadLeft(8, '0')}");
                }
            }

            return systemStatusData;
        }

        internal byte[] SetPortStatusDataProperty(PortStatusDefine status)
        {
            byte[] portStstusData = new byte[8];

            PortStatusArrays[status.Idle.Column].Set(status.Idle.Row, status.Idle.PValue);
            PortStatusArrays[status.CueInit.Column].Set(status.CueInit.Row, status.CueInit.PValue);
            PortStatusArrays[status.PlayRecord.Column].Set(status.PlayRecord.Row, status.PlayRecord.PValue);
            PortStatusArrays[status.Still.Column].Set(status.Still.Row, status.Still.PValue);
            PortStatusArrays[status.Jog.Column].Set(status.Jog.Row, status.Jog.PValue);
            PortStatusArrays[status.VarPlay.Column].Set(status.VarPlay.Row, status.VarPlay.PValue);
            PortStatusArrays[status.PortBusy.Column].Set(status.PortBusy.Row, status.PortBusy.PValue);

            PortStatusArrays[status.CueInitDone.Column].Set(status.CueInitDone.Row, status.CueInitDone.PValue);
         
            // 1
            // port id

            // 2
            PortStatusArrays[status.PortDown.Column].Set(status.PortDown.Row, status.PortDown.PValue);
            PortStatusArrays[status.IDAdded.Column].Set(status.IDAdded.Row, status.IDAdded.PValue);
            PortStatusArrays[status.IDDelete.Column].Set(status.IDDelete.Row, status.IDDelete.PValue);
            PortStatusArrays[status.IDAddtoArch.Column].Set(status.IDAddtoArch.Row, status.IDAddtoArch.PValue);
            PortStatusArrays[status.NoRefInput.Column].Set(status.NoRefInput.Row, status.NoRefInput.PValue);
            PortStatusArrays[status.NoVideoInput.Column].Set(status.NoVideoInput.Row, status.NoVideoInput.PValue);
            PortStatusArrays[status.NoAuioInput.Column].Set(status.NoAuioInput.Row, status.NoAuioInput.PValue);
            PortStatusArrays[status.AudioOverLoad.Column].Set(status.AudioOverLoad.Row, status.AudioOverLoad.PValue);

            // 3
            PortStatusArrays[status.SystemError.Column].Set(status.SystemError.Row, status.SystemError.PValue);
            PortStatusArrays[status.IllegalValue.Column].Set(status.IllegalValue.Row, status.IllegalValue.PValue);
            PortStatusArrays[status.InvalidPort.Column].Set(status.InvalidPort.Row, status.InvalidPort.PValue);
            PortStatusArrays[status.WrongPortType.Column].Set(status.WrongPortType.Row, status.WrongPortType.PValue);
            PortStatusArrays[status.CommandQueueFull.Column].Set(status.CommandQueueFull.Row, status.CommandQueueFull.PValue);
            PortStatusArrays[status.DiskFull.Column].Set(status.DiskFull.Row, status.DiskFull.PValue);
            PortStatusArrays[status.CmdwhileBusy.Column].Set(status.CmdwhileBusy.Row, status.CmdwhileBusy.PValue);
            PortStatusArrays[status.NotSupport.Column].Set(status.NotSupport.Row, status.NotSupport.PValue);

            // 4
            PortStatusArrays[status.InvalidId.Column].Set(status.InvalidId.Row, status.InvalidId.PValue);
            PortStatusArrays[status.IDNotFound.Column].Set(status.IDNotFound.Row, status.IDNotFound.PValue);
            PortStatusArrays[status.IDAlreadyExists.Column].Set(status.IDAlreadyExists.Row, status.IDAlreadyExists.PValue);
            PortStatusArrays[status.IDStilRecording.Column].Set(status.IDStilRecording.Row, status.IDStilRecording.PValue);
            PortStatusArrays[status.IDStillPlaying.Column].Set(status.IDStillPlaying.Row, status.IDStillPlaying.PValue);
            PortStatusArrays[status.IDNotTransferredFromaArchive.Column].Set(status.IDNotTransferredFromaArchive.Row, status.IDNotTransferredFromaArchive.PValue);
            PortStatusArrays[status.IDNotTransferredToArchive.Column].Set(status.IDNotTransferredToArchive.Row, status.IDNotTransferredToArchive.PValue);
            PortStatusArrays[status.IDDeleteProtected.Column].Set(status.IDDeleteProtected.Row, status.IDDeleteProtected.PValue);

            // 5
            if(status.NotInCue.PValue || status.InitState.PValue)
                PortStatusArrays[status.NotInCue.Column].Set(status.NotInCue.Row, true);
            else if (!status.NotInCue.PValue && !status.InitState.PValue)
                PortStatusArrays[status.InitState.Column].Set(status.InitState.Row, false);

            PortStatusArrays[status.CueNotDone.Column].Set(status.CueNotDone.Row, status.CueNotDone.PValue);
            PortStatusArrays[status.PortNotIdle.Column].Set(status.PortNotIdle.Row, status.PortNotIdle.PValue);

            if(status.PortPlaying.PValue || status.Active.PValue)
                PortStatusArrays[status.PortPlaying.Column].Set(status.PortPlaying.Row, true);
            else if (!status.PortPlaying.PValue && !status.Active.PValue)
                PortStatusArrays[status.Active.Column].Set(status.Active.Row, false);

            PortStatusArrays[status.PortNotAchive.Column].Set(status.PortNotAchive.Row, status.PortNotAchive.PValue);
            PortStatusArrays[status.CueOrOperAtionfalied.Column].Set(status.CueOrOperAtionfalied.Row, status.CueOrOperAtionfalied.PValue);
            PortStatusArrays[status.NetWorKError.Column].Set(status.NetWorKError.Row, status.NetWorKError.PValue);
            PortStatusArrays[status.SystemReBooted.Column].Set(status.SystemReBooted.Row, status.SystemReBooted.PValue);

            // 6
            PortStatusArrays[status.Off.Column].Set(status.Off.Row, status.Off.PValue);
            PortStatusArrays[status.Composite.Column].Set(status.Composite.Row, status.Composite.PValue);
            PortStatusArrays[status.SVidoe.Column].Set(status.SVidoe.Row, status.SVidoe.PValue);
            PortStatusArrays[status.Yuv.Column].Set(status.Yuv.Row, status.Yuv.PValue);
            PortStatusArrays[status.D1.Column].Set(status.D1.Row, status.D1.PValue);


            for (int i = 0; i < portStstusData.Length ; i++)
            {
                if(i == 0)
                    portStstusData[0] = 0x0f;
                else if (i == 1)
                    PortStatusArrays[i].CopyTo(portStstusData, i);
                else if (i == 2)
                {
                    int selectPort = 0;
                    if (PortNubmer < 0)
                        selectPort = 255 + PortNubmer + 1;
                    else
                        selectPort = PortNubmer;

                    portStstusData[i] = Convert.ToByte(selectPort);
                }
                else
                    PortStatusArrays[i].CopyTo(portStstusData, i);

                //Console.WriteLine($" ArrNum {i} : {Convert.ToString(portStstusData[i], 2).PadLeft(8, '0')}");
            }

            return portStstusData;
        }

        internal EumCommandKey ParserCommand(byte[] data)
        {
            byte Cmd1 = data[2];
            byte Cmd2 = data[3];

            EumCommandKey key = EumCommandKey.NORMAL;
            var Qry = CommandDictionary.Where(c => c.Value.Cmd1 == Cmd1
                                                && c.Value.Cmd2 == Cmd2)
                                                .ToList();
            if (Qry.Count != 0)
                key = Qry[0].Key;
            
            return key;
        }

        internal strVdcpCommand GetVdcpCommand(EumCommandKey key)
        {
            strVdcpCommand strCommand = new strVdcpCommand();

            if (CommandDictionary.ContainsKey(key))
            {
                strCommand.Key = CommandDictionary[key].Key;
                strCommand.Name = CommandDictionary[key].Name;
                strCommand.Stx = CommandDictionary[key].Stx;
                strCommand.Bc = CommandDictionary[key].Bc;
                strCommand.Cmd1 = CommandDictionary[key].Cmd1;
                strCommand.Cmd2 = CommandDictionary[key].Cmd2;
                strCommand.Data = CommandDictionary[key].Data;
                strCommand.ChkSum = CommandDictionary[key].ChkSum;
                strCommand.ReBc = CommandDictionary[key].ReBc;
                strCommand.ReCmd1 = CommandDictionary[key].ReCmd1;
                strCommand.ReCmd2 = CommandDictionary[key].ReCmd2;
            }

            return strCommand;
        }

        private strVdcpCommand SetCommandData(EumCommandKey key, string name
         , byte[] reciveByte, byte[] data, byte chksum, byte[] sendbyte)
        {
            strVdcpCommand strCommand = new strVdcpCommand();

            strCommand.Key = key;
            strCommand.Name = name;
            strCommand.Stx = reciveByte[0];
            strCommand.Bc = reciveByte[1];
            strCommand.Cmd1 = reciveByte[2];
            strCommand.Cmd2 = reciveByte[3];
            strCommand.Data = data;
            strCommand.ChkSum = chksum;
            strCommand.ReBc = sendbyte[1];
            strCommand.ReCmd1 = sendbyte[2];
            strCommand.ReCmd2 = sendbyte[3];

            return strCommand;
        }


        private bool MakeReturnCommand(strVdcpCommand cmd, byte[] aDataBuf, int aDataSize
         , out byte[] aRecvData, out int aIRecvSize)
        {
            byte iCRC = 0;
            byte[] btBuffs = new byte[aDataSize + 5];

            aRecvData = null;
            aIRecvSize = 0;

            iCRC = (byte)(cmd.ReCmd1 + cmd.ReCmd2);

            btBuffs[0] = 0x02; // STX
            if (cmd.ReBc == 0xff)
            {
                //DATA 길이가 정해지지 않는것은 데이터 갯수를 카운트해서 넣는다.
                btBuffs[1] = (byte)(0x02 + aDataSize); // BC
            }
            else
            {
                btBuffs[1] = cmd.ReBc; // BC - 테스트
            }

            btBuffs[2] = cmd.ReCmd1;
            btBuffs[3] = cmd.ReCmd2;

            int iCrcPos = 4;
            for (int i = 0; i < aDataSize; i++)
            {
                btBuffs[iCrcPos] = aDataBuf[i];
                Convert.ToInt32(btBuffs[iCrcPos]).ToString("x2");
                iCRC += aDataBuf[i];
                iCrcPos++;
            }

            btBuffs[iCrcPos] = (byte)(~iCRC + 1);

            //요기까지
            aRecvData = btBuffs;
            aIRecvSize = btBuffs.Length;

            return true;

        }


        internal byte[] MakeCommand(EumCommandKey commandKey, byte[] data, int dataCount)
        {
            byte[] reByte = new byte[ dataCount + 5 ];
            int byteCount = 0;

            if (MakeReturnCommand(GetVdcpCommand(commandKey), data, dataCount, out reByte, out byteCount))
                return reByte;
            else 
                return null;
        }

        internal bool UpDataCommandData(EumCommandKey key, byte[] data)
        {
            bool reulst = false;

            if (CommandDictionary.ContainsKey(key)                                                                                                                                                                    )
                CommandDictionary[key].Data.SetValue(data, 0);

            return reulst;
        }

        internal int Bit24ToInt32(byte[] byteArray)
        {
            int result = (
                 ((0xFF & byteArray[0]) << 16) |
                 ((0xFF & byteArray[1]) << 8) |
                 (0xFF & byteArray[2])
               );

            if ((result & 0x00800000) > 0)
                result = (int)((uint)result | (uint)0xFF000000);
            else
                result = (int)((uint)result & (uint)0x00FFFFFF);

            return result;
        }


        internal int GetStringLength(string str)
        {
            int iresult = 0;
            foreach (char s in str)
            {
                iresult += IsKorean(s) ? 2 : 1;
            }

            return iresult;
        }

        internal bool IsKorean(char ch)
        {
            bool result = false;

            if ((0xAC00 <= ch && ch <= 0xD7A3) || (0x3131 <= ch && ch <= 0x318E))
                result =  true;

            return result;
        }

        private void SetCommandKey()
        {
            CommandDictionary = new Dictionary<EumCommandKey, strVdcpCommand>();

            CommandDictionary.Add(EumCommandKey.LOCALDISABLE
                , SetCommandData(EumCommandKey.LOCALDISABLE, "LOCAL DISABLE", VdcpCommandKeyValues.RECIVE_LOCALDISABLE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_LOCALDISABLE));
            CommandDictionary.Add(EumCommandKey.LOCALENABLE
                , SetCommandData(EumCommandKey.LOCALENABLE, "LOCAL ENABLE", VdcpCommandKeyValues.RECIVE_LOCALENABLE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_LOCALENABLE));
            CommandDictionary.Add(EumCommandKey.STOP
                , SetCommandData(EumCommandKey.LOCALDISABLE, "Stop", VdcpCommandKeyValues.RECIVE_STOP
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_STOP));
            CommandDictionary.Add(EumCommandKey.PLAY
                , SetCommandData(EumCommandKey.PLAY, "PLAY", VdcpCommandKeyValues.RECIVE_PLAY
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_PLAY));
            CommandDictionary.Add(EumCommandKey.RECORD
                , SetCommandData(EumCommandKey.RECORD, "RECORD", VdcpCommandKeyValues.RECIVE_RECORD
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_RECORD));
            CommandDictionary.Add(EumCommandKey.FREEZE
                , SetCommandData(EumCommandKey.FREEZE, "FREEZE", VdcpCommandKeyValues.RECIVE_FREEZE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_FREEZE));
            CommandDictionary.Add(EumCommandKey.STILL
                , SetCommandData(EumCommandKey.STILL, "STILL", VdcpCommandKeyValues.RECIVE_STILL
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_STILL));
            CommandDictionary.Add(EumCommandKey.STEP
                ,SetCommandData(EumCommandKey.STEP, "STEP", VdcpCommandKeyValues.RECIVE_STEP
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_STEP));
            CommandDictionary.Add(EumCommandKey.CONTINUE
                , SetCommandData(EumCommandKey.CONTINUE, "CONTINUE", VdcpCommandKeyValues.RECIVE_CONTINUE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_CONTINUE));
            CommandDictionary.Add(EumCommandKey.JOG
                , SetCommandData(EumCommandKey.JOG, "JOG", VdcpCommandKeyValues.RECIVE_JOG
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_JOG));
            CommandDictionary.Add(EumCommandKey.VARIPLAY
                , SetCommandData(EumCommandKey.VARIPLAY, "VARIPLAY", VdcpCommandKeyValues.RECIVE_VARIPLAY
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_VARIPLAY));
            CommandDictionary.Add(EumCommandKey.UNFREEZE
                , SetCommandData(EumCommandKey.UNFREEZE, "UNFREEZE", VdcpCommandKeyValues.RECIVE_UNFREEZE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_UNFREEZE));
            CommandDictionary.Add(EumCommandKey.EEMODE
                , SetCommandData(EumCommandKey.EEMODE, "EEMODE", VdcpCommandKeyValues.RECIVE_EEMODE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EEMODE));
            CommandDictionary.Add(EumCommandKey.RENAMEID
                , SetCommandData(EumCommandKey.RENAMEID, "RENAMEID", VdcpCommandKeyValues.RECIVE_RENAMEID
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_RENAMEID));
            CommandDictionary.Add(EumCommandKey.EXRENAMEID
                 , SetCommandData(EumCommandKey.EXRENAMEID, "EXRENAMEID", VdcpCommandKeyValues.RECIVE_EXRENAMEID
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXRENAMEID));
            CommandDictionary.Add(EumCommandKey.PRESETTIME
                , SetCommandData(EumCommandKey.PRESETTIME, "PRESETTIME", VdcpCommandKeyValues.RECIVE_PRESETTIME
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_PRESETTIME));
            CommandDictionary.Add(EumCommandKey.CLOSEPORT
                , SetCommandData(EumCommandKey.CLOSEPORT, "CLOSEPORT", VdcpCommandKeyValues.RECIVE_CLOSEPORT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_CLOSEPORT));
            CommandDictionary.Add(EumCommandKey.SELECTPORT
                , SetCommandData(EumCommandKey.SELECTPORT, "SELECTPORT", VdcpCommandKeyValues.RECIVE_SELECTPORT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_SELECTPORT));
            CommandDictionary.Add(EumCommandKey.RECORDINIT
                , SetCommandData(EumCommandKey.RECORDINIT, "RECORDINIT", VdcpCommandKeyValues.RECIVE_RECORDINIT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_RECORDINIT));
            CommandDictionary.Add(EumCommandKey.EXRECORDINIT
                , SetCommandData(EumCommandKey.EXRECORDINIT, "EXRECORDINIT", VdcpCommandKeyValues.RECIVE_EXRECORDINIT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXRECORDINIT));
            CommandDictionary.Add(EumCommandKey.PLAYCUE
                , SetCommandData(EumCommandKey.PLAYCUE, "PLAYCUE", VdcpCommandKeyValues.RECIVE_PLAYCUE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_PLAYCUE));
            CommandDictionary.Add(EumCommandKey.EXPLAYCUE
                , SetCommandData(EumCommandKey.EXPLAYCUE, "EXPLAYCUE", VdcpCommandKeyValues.RECIVE_EXPLAYCUE
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXPLAYCUE));
            CommandDictionary.Add(EumCommandKey.CUEWITHDATA
                , SetCommandData(EumCommandKey.CUEWITHDATA, "CUEWITHDATA", VdcpCommandKeyValues.RECIVE_CUEWITHDATA
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_CUEWITHDATA));
            CommandDictionary.Add(EumCommandKey.EXCUEWITHDATA
                , SetCommandData(EumCommandKey.EXCUEWITHDATA, "EXCUEWITHDATA", VdcpCommandKeyValues.RECIVE_EXCUEWITHDATA
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXCUEWITHDATA));
            CommandDictionary.Add(EumCommandKey.DELETEID
                , SetCommandData(EumCommandKey.DELETEID, "DELETEID", VdcpCommandKeyValues.RECIVE_DELETEID
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_DELETEID));
            CommandDictionary.Add(EumCommandKey.EXDELETEID
                , SetCommandData(EumCommandKey.EXDELETEID, "EXDELETEID", VdcpCommandKeyValues.RECIVE_EXDELETEID
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXDELETEID));
            CommandDictionary.Add(EumCommandKey.CLEAR
                , SetCommandData(EumCommandKey.CLEAR, "CLEAR", VdcpCommandKeyValues.RECIVE_CLEAR
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_CLEAR));
            CommandDictionary.Add(EumCommandKey.SIGNALFULL
                , SetCommandData(EumCommandKey.SIGNALFULL, "SIGNALFULL", VdcpCommandKeyValues.RECIVE_SIGNALFULL
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_SIGNALFULL));
            CommandDictionary.Add(EumCommandKey.RECODEINITWITHDATA
                , SetCommandData(EumCommandKey.RECODEINITWITHDATA, "RECODEINITWITHDATA", VdcpCommandKeyValues.RECIVE_RECODEINITWITHDATA
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_RECODEINITWITHDATA));
            CommandDictionary.Add(EumCommandKey.EXRECODEINITWITHDATA
                , SetCommandData(EumCommandKey.EXRECODEINITWITHDATA, "EXRECODEINITWITHDATA", VdcpCommandKeyValues.RECIVE_EXRECODEINITWITHDATA
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXRECODEINITWITHDATA));
            CommandDictionary.Add(EumCommandKey.PRESET
                , SetCommandData(EumCommandKey.PRESET, "PRESET", VdcpCommandKeyValues.RECIVE_PRESET
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_PRESET));
            CommandDictionary.Add(EumCommandKey.DISKPREROLL
                , SetCommandData(EumCommandKey.DISKPREROLL, "DISKPREROLL", VdcpCommandKeyValues.RECIVE_DISKPREROLL
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_DISKPREROLL));
            CommandDictionary.Add(EumCommandKey.OPENPORT
                , SetCommandData(EumCommandKey.OPENPORT, "OPENPORT", VdcpCommandKeyValues.RECIVE_OPENPORT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_OPENPORT));
            CommandDictionary.Add(EumCommandKey.NEXT
                , SetCommandData(EumCommandKey.NEXT, "NEXT", VdcpCommandKeyValues.RECIVE_NEXT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_NEXT));
            CommandDictionary.Add(EumCommandKey.EXNEXT
                , SetCommandData(EumCommandKey.EXNEXT, "EXNEXT", VdcpCommandKeyValues.RECIVE_EXNEXT
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXNEXT));
            CommandDictionary.Add(EumCommandKey.LAST
                , SetCommandData(EumCommandKey.LAST, "LAST", VdcpCommandKeyValues.RECIVE_LAST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_LAST));
            CommandDictionary.Add(EumCommandKey.PORTSTATUS
                , SetCommandData(EumCommandKey.PORTSTATUS, "PORTSTATUS", VdcpCommandKeyValues.RECIVE_PORTSTATUS
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_PORTSTATUS));
            CommandDictionary.Add(EumCommandKey.POSTIONREQUEST
                , SetCommandData(EumCommandKey.POSTIONREQUEST, "POSTIONREQUEST", VdcpCommandKeyValues.RECIVE_POSTIONREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_POSTIONREQUEST));
            CommandDictionary.Add(EumCommandKey.SYSTEMSTATUS
                , SetCommandData(EumCommandKey.SYSTEMSTATUS, "SYSTEMSTATUS", VdcpCommandKeyValues.RECIVE_SYSTEMSTATUS
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_SYSTEMSTATUS));
            CommandDictionary.Add(EumCommandKey.LIST
                , SetCommandData(EumCommandKey.LIST, "LIST", VdcpCommandKeyValues.RECIVE_LIST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_LIST));
            CommandDictionary.Add(EumCommandKey.EXLIST
                , SetCommandData(EumCommandKey.EXLIST, "EXLIST", VdcpCommandKeyValues.RECIVE_EXLIST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXLIST));
            CommandDictionary.Add(EumCommandKey.SIZEREQUEST
                , SetCommandData(EumCommandKey.SIZEREQUEST, "SIZEREQUEST", VdcpCommandKeyValues.RECIVE_SIZEREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_SIZEREQUEST));
            CommandDictionary.Add(EumCommandKey.EXSIZEREQUEST
                , SetCommandData(EumCommandKey.EXSIZEREQUEST, "EXSIZEREQUEST", VdcpCommandKeyValues.RECIVE_EXSIZEREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXSIZEREQUEST));
            CommandDictionary.Add(EumCommandKey.IDREQUEST
                , SetCommandData(EumCommandKey.IDREQUEST, "IDREQUEST", VdcpCommandKeyValues.RECIVE_IDREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_IDREQUEST));
            CommandDictionary.Add(EumCommandKey.EXIDREQUEST
                , SetCommandData(EumCommandKey.EXIDREQUEST, "EXIDREQUEST", VdcpCommandKeyValues.RECIVE_EXIDREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.SEND_EXIDREQUEST));

            CommandDictionary.Add(EumCommandKey.ACTIVEIDREQUEST
               , SetCommandData(EumCommandKey.ACTIVEIDREQUEST, "ACTIVEIDREQUEST", VdcpCommandKeyValues.RECIVE_ACTIVEIDREQUEST
                                               , new byte[] { }, 0x00, VdcpCommandKeyValues.RECIVE_ACTIVEIDREQUEST));

            CommandDictionary.Add(EumCommandKey.EXACTIVEIDREQUEST
                , SetCommandData(EumCommandKey.EXACTIVEIDREQUEST, "EXACTIVEIDREQUEST", VdcpCommandKeyValues.RECIVE_EXACTIVEIDREQUEST
                                                , new byte[] { }, 0x00, VdcpCommandKeyValues.RECIVE_EXACTIVEIDREQUEST));

        }


        #region 함수 정의
        /// <summary>
        /// byte의 MSB를 Nibble로 변환하여 시간 값을 가져온다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string LeftNibble(byte data)
        {
            return Convert.ToString((data & 0xF0) >> 4);
        }

        /// <summary>
        /// byte의 LSB를 Nibble로 변환하여 시간 값을 가져온다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string RightNibble(byte data)
        {
            return Convert.ToString(data & 0x0F);
        }

        internal bool CheckSum(byte[] data)
        {
            bool result = false;
            byte iCRC = 0;

            if (data.Length >= 5)
            {
                int DataSize = data.Length - 5;//STX,BC,CMD1,CMD2,CHECKSUM 뺀것이 데이터사이즈

                iCRC = (byte)(data[2] + data[3]);

                int iCrcPos = 4;
                for (int i = 0; i < DataSize; i++)
                {
                    iCRC += data[i + 4];
                    iCrcPos++;
                }

                byte CalcCheckSum = (byte)(~iCRC + 1);
                result = CalcCheckSum == data[data.Length - 1] ? true : false;
            }

            return result;
        }

        internal byte[] AddCheckSum(byte[] values)
        {
            return Combine(values, CreateChecksum(values));
        }
        public byte CreateChecksum(byte[] values)
        {
            return CreateChecksum(values, values.Length);
        }

        public byte CreateChecksum(byte[] values, int count)
        {
            byte checksum = 0x00;

            for (int i = 0; i < count; i++)
            {
                checksum += values[i];
            }

            return checksum;
        }

        public bool RemainCount(int remainCount, ref byte[] sendData)
        {
            bool result = false;

            try
            {
                if (remainCount <= 0)
                {
                    sendData[0] = 0x00;
                    sendData[1] = 0x00;
                }
                else if (remainCount > 0 && remainCount <= 255)
                {
                    int nLSB = remainCount % 256;
                    sendData[0] = 0x00;
                    sendData[1] = Convert.ToByte(nLSB);
                }
                else if (remainCount > 255)
                {
                    int nMSB = remainCount / 256;
                    int nLSB = remainCount % 256;

                    sendData[0] = Convert.ToByte(nMSB);
                    sendData[1] = Convert.ToByte(nLSB);
                }
            }
            catch { result = false; }

            return result;
        }

        public byte[] Combine(byte[] original, byte additional)
        {
            byte[] sum = new byte[original.Length + 1];
            Buffer.BlockCopy(original, 0, sum, 0, original.Length);
            sum[sum.Length - 1] = additional;
            return sum;
        }

        public byte[] Combine(byte[] original, byte[] additional)
        {
            byte[] sum = new byte[original.Length + additional.Length];
            Buffer.BlockCopy(original, 0, sum, 0, original.Length);
            Buffer.BlockCopy(additional, 0, sum, original.Length, additional.Length);
            return sum;
        }

        public byte[] Combine(byte[] original, byte[] additional, int additionalOffSet)
        {
            byte[] sum = new byte[original.Length + additionalOffSet];
            Buffer.BlockCopy(original, 0, sum, 0, original.Length);
            Buffer.BlockCopy(additional, 0, sum, original.Length, additionalOffSet);
            return sum;
        }

        public byte[] Combine(byte[] original, byte add, int index)
        {
            for (int i = 0; i < original.Length; ++i)
            {
                if (i == index)
                {
                    original[i] = add;
                }
            }

            return original;
        }

        public byte[] CombineByNull(byte[] original, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                original = Combine(original, new byte[] { 0x00 });
            }

            return original;
        }

        public byte[] ConvertStringToTCByteArray(string value)
        {
            return ConvertStringArrayToTCByteArray(value.Split(':'));
        }
        public byte[] ConvertStringToUpSideDownTCByteArray(string value)
        {
            return ConvertStringArrayToUpSideDownTCByteArray(value.Split(':'));
        }

        private byte[] ConvertStringArrayToUpSideDownTCByteArray(string[] values)
        {
            return new byte[]{
                ConvertToTCByte(values[3].Substring(0,1), values[3].Substring(1,1)),
                ConvertToTCByte(values[2].Substring(0,1), values[2].Substring(1,1)),
                ConvertToTCByte(values[1].Substring(0,1), values[1].Substring(1,1)),
                ConvertToTCByte(values[0].Substring(0,1), values[0].Substring(1,1))
            };
        }

        private byte[] ConvertStringArrayToTCByteArray(string[] values)
        {
            return new byte[]{
                ConvertToTCByte(values[0].Substring(0,1), values[0].Substring(1,1)),
                ConvertToTCByte(values[1].Substring(0,1), values[1].Substring(1,1)),
                ConvertToTCByte(values[2].Substring(0,1), values[2].Substring(1,1)),
                ConvertToTCByte(values[3].Substring(0,1), values[3].Substring(1,1))
            };
        }

        private byte ConvertToTCByte(string msbString, string lsbString)
        {
            return (byte)((Convert.ToInt32(int.Parse(msbString) << 4))
                    + Convert.ToInt32(int.Parse(lsbString)));
        }

        /// <summary>
        /// TC 문자 배열을 하나의 문자로 만들어 반환한다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal string ConvertStringArrayToTCString(string[] data)
        {
            string time = "00:00:00:00";

            if (data == null || data.Length != 4)
                return time;

            time = "";

            time += data[0] + ":";
            time += data[1] + ":";
            time += data[2] + ":";
            time += data[3];

            return time;
        }

        /// <summary>
        /// TC byte 배열을 string 배열로 변환하여 반환한다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal string[] ConvertByteArrayToTCStringArray(byte[] data)
        {
            string[] time = null;

            if (data != null && 3 < data.Length)
            {
                time = new string[4];

                time[0] = LeftNibble(data[3]) + RightNibble(data[3]);
                time[1] = LeftNibble(data[2]) + RightNibble(data[2]);
                time[2] = LeftNibble(data[1]) + RightNibble(data[1]);
                time[3] = LeftNibble(data[0]) + RightNibble(data[0]);
            }

            return time;
        }

        /// <summary>
        /// TC byte 배열을 string 배열로 변환하여 반환한다.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal string[] ConvertByteArrayToTCStringArray1(byte[] data)
        {
            string[] time = null;

            if (data != null && 3 < data.Length)
            {
                time = new string[4];

                time[0] = LeftNibble(data[0]) + RightNibble(data[0]);
                time[1] = LeftNibble(data[1]) + RightNibble(data[1]);
                time[2] = LeftNibble(data[2]) + RightNibble(data[2]);
                time[3] = LeftNibble(data[3]) + RightNibble(data[3]);
            }

            return time;
        }

        internal string ConvertByteArrayToTcString(byte[] data)
        {
            string ff = Convert.ToInt32(data[0]).ToString("x2");
            string ss = Convert.ToInt32(data[1]).ToString("x2");
            string mm = Convert.ToInt32(data[2]).ToString("x2");
            string hh = Convert.ToInt32(data[3]).ToString("x2");

            return string.Format("{0}:{1}:{2}:{3}", hh, mm, ss, ff);
        }

        /// <summary>
        /// 타입코드를 받아서 4byte BCD 값으로 변환.
        /// HH:MM:DD:FF -> FFDDMMHH
        /// </summary>
        /// <param name="Tc"></param>
        /// <returns></returns>
        internal byte[] StrTcToBcdBytes(string Tc)
        {
            byte[] temp = new byte[4];

            temp[3] = Convert.ToByte(Tc.Substring(0, 2), 16);
            temp[2] = Convert.ToByte(Tc.Substring(3, 2), 16);
            temp[1] = Convert.ToByte(Tc.Substring(6, 2), 16);
            temp[0] = Convert.ToByte(Tc.Substring(9, 2), 16);

            return temp;
        }

        #endregion
    }
}
