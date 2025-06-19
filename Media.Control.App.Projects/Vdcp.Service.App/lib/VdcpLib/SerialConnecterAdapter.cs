using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VdcpService.lib
{
    public enum SendType { Normal, Expansion }
    public enum LoggerType { Send, Recive }
    public enum EnuPortType { Serial, Udp }

    public delegate void VdcpActionEventDelegate(VdcpEventArgsDefine commandData);

    public class SerialConnecterAdapter : VdcpCommandAdapter
    {
        public event VdcpActionEventDelegate EventActionCallbacks;

        private EnuPortType PortType { get; set; } = EnuPortType.Serial;
        private bool FisrtList = false;
        private int LockMode = 0;
        private string messageLog = string.Empty;
        private object obj = new object();
        private SerialPort serialPort = null;
        private VdcpUdpAdapter ServerAdapter = null;
        //   private VdcpUdpAdapter ClientAdapter = null;

        private EumCommandKey ReKey = EumCommandKey.NORMAL;

        public bool StatusLogOn { get; set; }
        public bool SelectPortConnect { get; set; }
        public bool OpenPortConnect { get; set; }
        public string SerialErrerMessage { get; set; }
        public string PortName { get; private set; }
        public int OpenPortNumber { get; private set; }
        public int SelectPortNumber
        {
            get { return PortNubmer; }
            private set { PortNubmer = value; }
        }

        public SerialConnecterAdapter()
        {

        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            EventActionCallbacks(new VdcpEventArgsDefine(EumCommandKey.ERROR, e.EventType));
            Console.WriteLine(e.EventType.ToString());
        }

        private byte[] SumRecvCmd = null;

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (obj)
            {
                int RecSize = serialPort.BytesToRead;
                if (RecSize == 0) return;

                byte[] buffer = new byte[1024];
                int cnt = serialPort.Read(buffer, 0, 1024);

                byte[] recvcmd = new byte[cnt];

                System.Buffer.BlockCopy(buffer, 0, recvcmd, 0, cnt);

                if (SumRecvCmd == null && !CheckByteArray(recvcmd))
                {
                    SumRecvCmd = new byte[cnt];
                    SumRecvCmd = recvcmd;

                    //System.Buffer.BlockCopy(SumRecvCmd, 0, recvcmd, 0, cnt);
                    return;
                }
                else if (SumRecvCmd != null)
                {
                    if (!CheckByteArray(SumRecvCmd))
                    {
                        SumRecvCmd = Combine(SumRecvCmd, recvcmd);

                        if (CheckByteArray(SumRecvCmd))
                        {
                            if (CheckSum(SumRecvCmd))
                                RecviveCommandWork(ParserCommand(SumRecvCmd), SumRecvCmd);
                            else
                                SendNak();

                            SumRecvCmd = null;
                        }
                    }

                    return;
                }

                if (CheckSum(recvcmd))
                    RecviveCommandWork(ParserCommand(recvcmd), recvcmd);
                else
                    SendNak();

            }
        }

        private bool CheckByteArray(byte[] recvecmd)
        {
            bool result = false;

            if (recvecmd[1] == recvecmd.Count() - 3)
                result = true;

            return result;

        }

        private string SetReceiveWrite(byte[] buffer)
        {
            string reviceString = string.Empty;

            StringBuilder sb = new StringBuilder(1024);
            for (int i = 0; i < buffer.Count(); i++)
            {
                sb.Append(Convert.ToInt32(buffer[i]).ToString("x2"));
                sb.Append(".");
            }

            sb.AppendLine();

            return sb.ToString().Replace("\r\n", string.Empty);

        }
        private void WiriteLog(EumCommandKey key, LoggerType loggerType, byte[] buffer)
        {
            Action LogWrite = () =>
            {
                Logger.WriteLine(TraceEventType.Information,
                              $" [ {(loggerType == LoggerType.Recive ? "R" : "S") } ]"
                            + $"  [ {key.ToString().PadRight(20, ' ')} ]"
                            + $"  {SetReceiveWrite(buffer)}"
                            );
            };

            if (Logger.LoggerLevel != EnuLoggerLevel.Off)
            {
                if (Logger.LoggerLevel == EnuLoggerLevel.Detail)
                {
                    if (key != EumCommandKey.POSTIONREQUEST
                        && key != EumCommandKey.PORTSTATUS
                        && key != EumCommandKey.JOG
                        )
                        LogWrite();
                }
                else
                    LogWrite();

            }
        }
        private void WiriteLog(string message)
        {
            Logger.WriteLine(TraceEventType.Information,
                 $" [ D ]"
                 + $"  [ { "DATA >>>".PadRight(20, ' ')} ]"
                 + $"  {message}"
                 );
        }
        void SendNak()
        {
            try
            {
                byte[] aRecvData = new byte[1];
                aRecvData[0] = NAK;

                if (PortType == EnuPortType.Serial)
                {
                    serialPort.Write(aRecvData, 0, aRecvData.Length);

                    if (Logger.LoggerLevel != EnuLoggerLevel.Off)
                    {
                        if (Logger.LoggerLevel == EnuLoggerLevel.Detail)
                        {
                            if (ReKey != EumCommandKey.POSTIONREQUEST
                                && ReKey != EumCommandKey.PORTSTATUS
                                && ReKey != EumCommandKey.JOG
                                && ReKey != EumCommandKey.VARIPLAY)
                                Task.Run(() => WiriteLog(EumCommandKey.NAK, LoggerType.Send, aRecvData));
                        }
                        else
                            Task.Run(() => WiriteLog(EumCommandKey.NAK, LoggerType.Send, aRecvData));

                    }
                }
                else
                {
                    Task.Run(() => ServerAdapter.Send(aRecvData));
                    Task.Run(() => WiriteLog(EumCommandKey.NAK, LoggerType.Send, aRecvData));
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"---------------> SendNack Exception : {ex.Message}");
            }
        }
        void SendAck()
        {
            try
            {
                byte[] aRecvData = new byte[1];
                aRecvData[0] = ACK;

                if (PortType == EnuPortType.Serial)
                {
                    serialPort.Write(aRecvData, 0, aRecvData.Length);

                    if (Logger.LoggerLevel != EnuLoggerLevel.Off)
                    {
                        if (Logger.LoggerLevel == EnuLoggerLevel.Detail)
                        {
                            if (ReKey != EumCommandKey.POSTIONREQUEST
                                && ReKey != EumCommandKey.PORTSTATUS
                                && ReKey != EumCommandKey.JOG
                                && ReKey != EumCommandKey.VARIPLAY)
                                Task.Run(() => WiriteLog(EumCommandKey.ACK, LoggerType.Send, aRecvData));
                        }
                        else
                            Task.Run(() => WiriteLog(EumCommandKey.ACK, LoggerType.Send, aRecvData));

                    }
                }
                else
                {
                    Task.Run(() => ServerAdapter.Send(aRecvData));
                    Task.Run(() => WiriteLog(EumCommandKey.ACK, LoggerType.Send, aRecvData));
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"---------------> SendAck Exception : {ex.Message}");
            }
        }
        void RecviveCommandWork(EumCommandKey key, byte[] RecData)
        {
            try
            {
                int position = 0;
                int count = 0;
                int recCount = 0;
                int recCount1 = 0;
                int PortNum = 0;

                string sClipName = string.Empty;
                string sDruation = string.Empty;
                string sSom = string.Empty;
                string sEom = string.Empty;
                string sInput = string.Empty;   

                byte[] clipName = null;
                byte[] input = null;
                byte[] value = null;
                byte[] oldName = null;
                byte[] newName = null;
                byte[] duration = null;
                byte[] bySom = null;
                byte[] byEom = null;
                byte[] sendData = null;
                byte[] RecvData = null;
                ReKey = key;

                Task.Run(() => WiriteLog(key, LoggerType.Recive, RecData));

                switch (key)
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
                        SendAck();

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key)));
                        break;
                    case EumCommandKey.JOG:

                        SendAck();
                        position = Convert.ToInt32(RecData[1]) - 2;
                        value = new byte[position];

                        System.Buffer.BlockCopy(RecData, 4, value, 0, position);

                        count = 0;
                        if (value.Count() == 1)
                        {
                            count = value[0];
                            if (129 <= count && count < 256)
                                count = -(256 - count);
                        }
                        else
                            count = BytesToInt(value);

                        // EventActionCallbacks(new VdcpEventArgsDefine(key, count));
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, count)));

                        break;
                    case EumCommandKey.VARIPLAY:
                        // 0x000000 = still
                        // 0x010000 = std play forward
                        // 0x7F0000 = 127 times std play forward
                        // 0xFF0000 = std play reverse
                        // 0x800000 = 128 times play reverse
                        //////////////////////////////////////////////////////////////////////////
                        SendAck();
                        position = Convert.ToInt32(RecData[1]) - 2;
                        value = new byte[position];
                        System.Buffer.BlockCopy(RecData, 4, value, 0, position);
                        int count1 = BitConverter.ToInt16(value, 0);

                        //Console.WriteLine($" value = {value[0]} {value[1]} {value[2]}");
                        //Console.WriteLine($" value = {string.Format("{0:x2}", value[0])} " +
                        //                         $"{string.Format("{0:X2}", value[1])} " +
                        //                         $"{string.Format("{0:X2}", value[2])}");

                        int ivalue = Bit24ToInt32(value);
                        float speedValue = 0;
                        float BasePlayValue = 0x010000;

                        if (value[1] == 0)
                        {
                            if (count1 >= 128)
                                speedValue = -(256 - count1);
                            else
                                speedValue = count1;
                        }
                        else
                        {
                            speedValue = (float)(ivalue / BasePlayValue);
                            speedValue = (float)Math.Round(speedValue * 100, 1) / 100;
                        }

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, speedValue)));

                        Console.WriteLine($"---------------> Vari.Play [{speedValue}]");


                        break;
                    case EumCommandKey.UNFREEZE:
                        SendAck();
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key)));
                        Console.WriteLine("---------------> Unfreeze");
                        break;
                    case EumCommandKey.EEMODE:
                        SendAck();
                        EumEEMode mode = (EumEEMode)Convert.ToInt32(RecData[4]);
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, mode)));
                        Console.WriteLine("---------------> EEMode");
                        break;
                    case EumCommandKey.RENAMEID:
                        SendAck();
                        oldName = new byte[8];
                        newName = new byte[8];

                        System.Buffer.BlockCopy(RecData, 4, oldName, 0, 8);
                        System.Buffer.BlockCopy(RecData, 12, newName, 0, 8);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, BytesToString(oldName).Trim()
                                                                                    , BytesToString(newName).Trim())));

                        Console.WriteLine($"------------> Rename ID  [{BytesToString(oldName)} -> {BytesToString(newName)}]");
                        break;
                    case EumCommandKey.EXRENAMEID:
                        recCount1 = Convert.ToInt32(RecData[4]);
                        oldName = new byte[recCount1];

                        System.Buffer.BlockCopy(RecData, 5, oldName, 0, recCount1);

                        recCount = Convert.ToInt32(RecData[5 + recCount1]);
                        newName = new byte[recCount];

                        System.Buffer.BlockCopy(RecData, 6 + recCount1, newName, 0, recCount);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, BytesToString(oldName).Trim()
                                                                                        , BytesToString(newName).Trim())));

                        Console.WriteLine($"------------> ExRename ID  [{BytesToString(oldName)} -> {BytesToString(oldName)}] ");
                        break;
                    case EumCommandKey.PRESETTIME:
                        break;
                    case EumCommandKey.CLOSEPORT:
                        PortNum = Convert.ToInt16(RecData[4]);
                        //if (PortNum >= 128)
                        //    PortNum = (128 - PortNum);
                        var port = PortNum > 127 ? (sbyte)PortNum : PortNum;

                        if (OpenPortNumber == port)
                        {
                            SendAck();
                            OpenPortConnect = false;
                            Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, port)));
                        }
                        else
                            SendNak();

                        break;
                    case EumCommandKey.SELECTPORT:

                        PortNum = Convert.ToInt16(RecData[4]);
                        //if (PortNum >= 128)
                        //    PortNum = (128 - PortNum);

                        var port1 = PortNum > 127 ? (sbyte)PortNum : PortNum;

                        if (OpenPortConnect && OpenPortNumber == port1)
                        {
                            SendAck();
                            SelectPortNumber = port1;
                            Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, port1, false)));
                        }
                        else
                            SendNak();

                        Console.WriteLine($"------------> PortNum :[{port1}] ");

                        break;
                    case EumCommandKey.RECORDINIT:
                        SendAck();
                        clipName = new byte[8];
                        duration = new byte[4];

                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);
                        System.Buffer.BlockCopy(RecData, 14, duration, 0, 4);

                        sDruation = ConvertByteArrayToTcString(duration);
                        sClipName = BytesToString(clipName);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sDruation)));

                        Console.WriteLine($"------------> RecordInit :[{sClipName} {sDruation}] ");
                        break;
                    case EumCommandKey.EXRECORDINIT:
                        SendAck();
                        recCount = Convert.ToInt32(RecData[4]);

                        clipName = new byte[recCount];
                        duration = new byte[4];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);
                        System.Buffer.BlockCopy(RecData, 5 + recCount, duration, 0, 4);

                        sDruation = ConvertByteArrayToTcString(duration);
                        sClipName = BytesToString(clipName);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sDruation)));

                        Console.WriteLine($"------------> ExRecordInit :[{sClipName} {sDruation}] ");

                        break;
                    case EumCommandKey.PLAYCUE:
                        SendAck();
                        clipName = new byte[8];
                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);

                        sClipName = BytesToString(clipName);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, 0, 0)));

                        Console.WriteLine($"------------> PlayCue :[{sClipName}] ");
                        break;
                    case EumCommandKey.EXPLAYCUE:
                        SendAck();
                        recCount = Convert.ToInt32(RecData[4]);
                        clipName = new byte[recCount];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);

                        sClipName = BytesToString(clipName);
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, 0, 0)));

                        Console.WriteLine($"------------> ExPlayCue :[{sClipName}] ");
                        break;
                    case EumCommandKey.CUEWITHDATA:
                        SendAck();
                        clipName = new byte[8];
                        bySom = new byte[4];
                        byEom = new byte[4];

                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);
                        sClipName = BytesToString(clipName);

                        System.Buffer.BlockCopy(RecData, 14, bySom, 0, 4);
                        sSom = ConvertByteArrayToTcString(bySom);

                        System.Buffer.BlockCopy(RecData, 18, byEom, 0, 4);
                        sEom = ConvertByteArrayToTcString(byEom);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sSom, sEom)));

                        Console.WriteLine($"------------> CueWithData :[{sClipName} {sSom} {sEom}] ");
                        break;
                    case EumCommandKey.EXCUEWITHDATA:
                        SendAck();
                        recCount = Convert.ToInt32(RecData[4]);

                        clipName = new byte[recCount];
                        bySom = new byte[4];
                        byEom = new byte[4];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);
                        sClipName = BytesToString(clipName);

                        System.Buffer.BlockCopy(RecData, 5 + recCount, bySom, 0, 4);
                        sSom = ConvertByteArrayToTcString(bySom);

                        System.Buffer.BlockCopy(RecData, 9 + recCount, byEom, 0, 4);
                        sEom = ConvertByteArrayToTcString(byEom);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sSom, sEom)));

                        Console.WriteLine($"------------> ExCueWithData :[{sClipName} {sSom} {sEom}] ");
                        break;
                    case EumCommandKey.DELETEID:
                        SendAck();
                        clipName = new byte[8];

                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);
                        sClipName = BytesToString(clipName);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName)));

                        Console.WriteLine($"------------> DeleteId :[{sClipName}] ");
                        break;
                    case EumCommandKey.EXDELETEID:
                        SendAck();
                        recCount = Convert.ToInt32(RecData[4]);
                        clipName = new byte[recCount];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);
                        sClipName = BytesToString(clipName);

                        EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName));
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName)));

                        Console.WriteLine($"------------> ExDeleteId :[{sClipName}] ");
                        break;
                    case EumCommandKey.CLEAR:
                        break;
                    case EumCommandKey.SIGNALFULL:
                        SendAck();
                        int SignalNum = 0;
                        SignalNum = Convert.ToInt32(RecData[4]);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, SignalNum)));

                        Console.WriteLine($"------------> SingnalFull :[{SignalNum}]");
                        break;
                    case EumCommandKey.SELECTINPUT:
                        SendAck();

                        recCount = Convert.ToInt32(RecData[4]);
                        input = new byte[recCount];

                        System.Buffer.BlockCopy(RecData, 5, input, 0, recCount);
                        sInput = BytesToString(input);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine()
                        {
                            CommandKey = key,
                            Input = sInput
                        }));

                        break;
                    case EumCommandKey.RECODEINITWITHDATA:
                        SendAck();
                        clipName = new byte[8];
                        bySom = new byte[4];
                        byEom = new byte[4];

                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);
                        sClipName = BytesToString(clipName);

                        System.Buffer.BlockCopy(RecData, 14, bySom, 0, 4);
                        sSom = ConvertByteArrayToTcString(bySom);

                        System.Buffer.BlockCopy(RecData, 18, byEom, 0, 4);
                        sEom = ConvertByteArrayToTcString(byEom);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sSom, sEom)));

                        Console.WriteLine($"------------> RecodInitWitData :[{sClipName} {sSom} {sEom}] ");
                        break;
                    case EumCommandKey.EXRECODEINITWITHDATA:
                        SendAck();
                        recCount = Convert.ToInt32(RecData[4]);

                        clipName = new byte[recCount];
                        bySom = new byte[4];
                        byEom = new byte[4];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);
                        sClipName = BytesToString(clipName);

                        System.Buffer.BlockCopy(RecData, 5 + recCount, bySom, 0, 4);
                        sSom = ConvertByteArrayToTcString(bySom);

                        System.Buffer.BlockCopy(RecData, 9 + recCount, byEom, 0, 4);
                        sEom = ConvertByteArrayToTcString(byEom);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName, sSom, sEom)));

                        Console.WriteLine($"------------> ExRecodInitWitData :[{sClipName} {sSom} {sEom}] ");
                        break;
                    case EumCommandKey.PRESET:

                        break;
                    case EumCommandKey.DISKPREROLL:
                        SendAck();
                        int Frames = 0;
                        int Seconds = 0;
                        Frames = Convert.ToInt32(RecData[4]);
                        Seconds = Convert.ToInt32(RecData[5]);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, Frames, Seconds)));

                        Console.WriteLine($"------------> DiskPreroll :[{Frames} {Seconds}] ");
                        break;
                    case EumCommandKey.OPENPORT:

                        PortNum = Convert.ToInt16(RecData[4]);

                        //if (PortNum >= 128)
                        //    PortNum = (128 - PortNum);

                        LockMode = Convert.ToInt32(RecData[5]);
                        OpenPortNumber = PortNum > 127 ? (sbyte)PortNum : PortNum;

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, PortNum, LockMode == 0 ? false : true)));

                        if (!OpenPortConnect)
                        {
                            OpenPortConnect = true;
                            OpenPort(1);
                        }
                        else if (OpenPortConnect && LockMode == 1) OpenPort(0);
                        else OpenPort(1);

                        Console.WriteLine($"------------> Open Port :[{PortNum} {LockMode}] ");
                        break;
                    case EumCommandKey.NEXT:
                    case EumCommandKey.EXNEXT:
                    case EumCommandKey.LIST:
                    case EumCommandKey.EXLIST:

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key)));
                        break;
                    case EumCommandKey.LAST: break;
                    case EumCommandKey.PORTSTATUS:
                        sendData = ServerStatusData.GetPortStatusData();
                        SendCommand(sendData);

                        Console.WriteLine($"------------> PORTSTATUS :[] ");

                        break;
                    case EumCommandKey.POSTIONREQUEST:
                        int TimeType = 0;
                        TimeType = Convert.ToInt32(RecData[4]);

                        sendData = new byte[5];
                        duration = StrTcToBcdBytes(TimeType == 0 ? ServerStatusData.RemainingTimeCode
                                                                 : ServerStatusData.CurrentTimeCode);

                        sendData[0] = Convert.ToByte(TimeType);
                        sendData[1] = duration[0];
                        sendData[2] = duration[1];
                        sendData[3] = duration[2];
                        sendData[4] = duration[3];

                        RecvData = MakeCommand(EumCommandKey.POSTIONREQUEST, sendData, sendData.Length);
                        SendCommand(RecvData);

                        //Console.WriteLine($"---------------> Timecode: { duration}");
                        break;
                    case EumCommandKey.SYSTEMSTATUS:
                        sendData = ServerStatusData.GetSystemStatusData();
                        // RecvData = MakeCommand(EumCommandKey.SYSTEMSTATUS, sendData, sendData.Length);
                        SendCommand(sendData);
                        break;
                    case EumCommandKey.SIZEREQUEST:
                    case EumCommandKey.IDREQUEST:
                        clipName = new byte[8];
                        System.Buffer.BlockCopy(RecData, 4, clipName, 0, 8);

                        sClipName = BytesToString(clipName);
                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName)));

                        break;
                    case EumCommandKey.EXSIZEREQUEST:
                    case EumCommandKey.EXIDREQUEST:
                        recCount = Convert.ToInt32(RecData[4]);
                        clipName = new byte[recCount];

                        System.Buffer.BlockCopy(RecData, 5, clipName, 0, recCount);
                        sClipName = BytesToString(clipName);

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName)));
                        break;
                    case EumCommandKey.EXACTIVEIDREQUEST:
                    case EumCommandKey.ACTIVEIDREQUEST:

                        Task.Run(() => EventActionCallbacks(new VdcpEventArgsDefine(key, sClipName)));

                        break;

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"---------------> RecviveCommandWork Exception : {ex.Message}");
            }
        }
        public bool Open(EnuPortType portType, string Name, int portNum, bool statusLog)
        {
            bool result = false;

            try
            {
                StatusLogOn = statusLog;
                PortName = Name;
                PortNubmer = portNum;
                PortType = portType;

                if (portType == EnuPortType.Serial)
                {

                    if (serialPort == null)
                    {
                        serialPort = new SerialPort();

                        serialPort.BaudRate = 38400;
                        serialPort.DataBits = 8;
                        serialPort.StopBits = StopBits.One;
                        serialPort.Parity = Parity.Odd;
                        serialPort.ReadBufferSize = 1024;
                        serialPort.WriteBufferSize = 1024;
                        serialPort.Handshake = System.IO.Ports.Handshake.None;
                        serialPort.ReadTimeout = System.IO.Ports.SerialPort.InfiniteTimeout;
                        serialPort.WriteTimeout = System.IO.Ports.SerialPort.InfiniteTimeout;
                        serialPort.ReadBufferSize = 2048;
                        serialPort.PortName = Name;

                        serialPort.DataReceived += SerialPort_DataReceived;
                        serialPort.ErrorReceived += SerialPort_ErrorReceived;
                        serialPort.Open();
                    }
                }
                else
                {
                    ServerAdapter = new VdcpUdpAdapter();
                    ServerAdapter.ReciveData += UdpAdapter_ReciveData;
                    ServerAdapter.Start(portNum);
                }


                Logger.File_Listener.Location = $"ComLogger";
                Logger.File_Listener.BaseFileName = Name;

                // 이어쓰기, 파일크기 롤링, 하루 단위 롤링 설정 (값이 0이면 롤링 하지 않음)
                Logger.File_Listener.StartAppend = true;
                Logger.File_Listener.MaxFileSize = 30 * 1024; // 30 KB
                Logger.File_Listener.DailyRolling = true;

                // 로그 파일 최대 개수 설정 (초과 시 오래된 로그부터 자동 삭제)
                Logger.File_Listener.MaxFileCount = 10;

                Logger.Add_File_Listener();
                Logger.WriteLine_Information($"Create {Name} [{portNum}] Serial Port ");

                result = true;


            }
            catch (SystemException ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> Create Exception : {ex.Message}");

            }

            return result;
        }

        private void UdpAdapter_ReciveData(byte[] recData, int count)
        {
            if (recData.Length != 0)
            {
                byte[] recvcmd = new byte[count];

                System.Buffer.BlockCopy(recData, 0, recvcmd, 0, count);

                string write = string.Empty;
                foreach (byte b in recData)
                {
                    write += $".{b.ToString()}";
                }

                Console.WriteLine($"DataCount : {count} [ {write} ]");

                if (CheckSum(recvcmd))
                    RecviveCommandWork(ParserCommand(recvcmd), recvcmd);
                else
                    SendNak();
            }

            // Console.WriteLine(recData);
        }

        public void Close()
        {
            try
            {
                if (PortType == EnuPortType.Serial)
                {
                    if(serialPort == null) return;

                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.ErrorReceived -= SerialPort_ErrorReceived;

                    System.Threading.Thread.Sleep(100);
                    Logger.WriteLine(TraceEventType.Information, $"PortName : {PortName} Close");

                    serialPort = null;
                }
                else
                {
                    if (ServerAdapter == null) return;
                    ServerAdapter.ReciveData -= UdpAdapter_ReciveData;
                    ServerAdapter.Close();

                    ServerAdapter = null;
                    Logger.WriteLine(TraceEventType.Information, $"Name : {PortName} Close");
                }
            }
            catch
            {

            }
        }

        public void AddActionEventDelegate(VdcpActionEventDelegate callback)
        {
            EventActionCallbacks = Delegate.Combine(EventActionCallbacks, callback)
                                                    as VdcpActionEventDelegate;
        }

        public void RemoveActionEventDelegate(VdcpActionEventDelegate callbakc)
        {
            EventActionCallbacks = Delegate.Remove(EventActionCallbacks, callbakc)
                                                    as VdcpActionEventDelegate;
        }


        private bool SendCommand(byte[] sendData)
        {
            bool result = false;

            if (sendData == null) return result;

            try
            {
                if (PortType == EnuPortType.Serial)
                    serialPort.Write(sendData, 0, sendData.Length);
                else
                    ServerAdapter.Send(sendData);

                result = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> SendCommand Exception : {ex.Message}");
            }

            return result;
        }

        private int sendDataCount = 2;
        private int MediaTotalCount = 0;

        private byte[] sendData = null;
        private byte[] aRecvData = null;

        public bool OpenPort(int Mode)
        {
            bool result = false;
            try
            {

                sendDataCount = 1; sendData = null;
                sendData = new byte[sendDataCount];
                sendData[0] = Convert.ToByte(Mode);

                aRecvData = new byte[sendDataCount + 5];
                aRecvData = MakeCommand(EumCommandKey.OPENPORT, sendData, sendData.Length);

                result = SendCommand(aRecvData);



                messageLog = string.Empty;
                messageLog = $"Mode = {Mode}";
                WiriteLog(EumCommandKey.OPENPORT, LoggerType.Send, aRecvData);
                WiriteLog(messageLog);
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> OpenPort Exception : {ex.Message}");
            }

            return result;
        }

        public bool List(List<string> ClipList, SendType sendType)
        {
            bool result = false;

            try
            {
                MediaTotalCount = 0;
                sendData = new byte[2];
                messageLog = string.Empty;

                List<byte[]> clipByteList = new List<byte[]>();

                for (int i = 0; i < ClipList.Count; i++)
                {
                    byte[] clipName = Encoding.UTF8.GetBytes(ClipList[i]);

                    if (sendType == SendType.Normal)
                    {
                        if (clipName.Length < 9)
                        {
                            clipByteList.Add(clipName);
                            ++MediaTotalCount;
                        }
                    }
                    else
                    {
                        clipByteList.Add(clipName);
                        ++MediaTotalCount;
                    }

                    if (MediaTotalCount == 10) break;
                }

                // 남을 영상 갯수 정의 하기
                RemainCount(ClipList.Count - MediaTotalCount, ref sendData);

                for (int i = 0; i < clipByteList.Count(); i++)
                {
                    byte[] clipName = clipByteList[i];

                    if (sendType == SendType.Normal)
                    {
                        if (clipName.Length <= 8)
                        {
                            sendData = Combine(sendData, clipName);

                            if (clipName.Length < 8)
                            {
                                for (int j = clipName.Length; j < 8; j++)
                                    sendData = Combine(sendData, Convert.ToByte(' '));
                            }
                        }
                    }
                    else
                    {
                        sendData = Combine(sendData, Convert.ToByte(clipName.Length));
                        sendData = Combine(sendData, clipName);
                    }

                    messageLog += ClipList[i] + "\r\n".PadRight(49, ' ');
                    if (i == 9) break;
                }

                aRecvData = new byte[sendData.Length + 5];

                aRecvData = MakeCommand(sendType == SendType.Normal
                    ? EumCommandKey.LIST : EumCommandKey.EXLIST
                    , sendData, sendData.Length);

                result = SendCommand(aRecvData);
                FisrtList = result;

                Task.Run(() => WiriteLog(sendType == SendType.Normal
                        ? EumCommandKey.LIST : EumCommandKey.EXLIST
                        , LoggerType.Send, aRecvData));

                Task.Run(() => WiriteLog(messageLog));

                Console.WriteLine(messageLog);
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> List Exception : {ex.Message}");
            }

            return result;
        }

        public bool Next(List<string> ClipList, SendType sendType)
        {
            bool result = false;

            try
            {
                sendData = new byte[2];
                messageLog = string.Empty;

                List<byte[]> clipByteList = new List<byte[]>();

                if (FisrtList)
                {
                    int IndexCount = 0;
                    for (int i = MediaTotalCount; i < ClipList.Count; i++)
                    {
                        byte[] clipName = Encoding.UTF8.GetBytes(ClipList[i]);

                        if (sendType == SendType.Normal)
                        {
                            if (clipName.Length < 9)
                            {
                                clipByteList.Add(clipName);
                                ++MediaTotalCount;
                            }
                        }
                        else
                        {
                            clipByteList.Add(clipName);
                            ++MediaTotalCount;
                        }

                        ++IndexCount;
                        if (IndexCount == 10) break;
                    }
                }

                RemainCount(ClipList.Count - MediaTotalCount, ref sendData);

                for (int i = 0; i < clipByteList.Count(); i++)
                {
                    byte[] clipName = clipByteList[i];

                    if (sendType == SendType.Normal)
                    {
                        if (clipName.Length <= 8)
                        {
                            sendData = Combine(sendData, clipName);

                            if (clipName.Length < 8)
                            {
                                for (int j = clipName.Length; j < 8; j++)
                                    sendData = Combine(sendData, Convert.ToByte(' '));
                            }
                        }
                    }
                    else
                    {
                        sendData = Combine(sendData, Convert.ToByte(clipName.Length));
                        sendData = Combine(sendData, clipName);
                    }

                    messageLog += ClipList[i] + "\r\n".PadRight(49, ' ');
                    if (i == 9) break;
                }

                aRecvData = new byte[sendDataCount + 5];
                aRecvData = MakeCommand(sendType == SendType.Normal
                                        ? EumCommandKey.NEXT : EumCommandKey.EXNEXT
                                        , sendData, sendData.Length);

                result = SendCommand(aRecvData);

                Task.Run(() => WiriteLog(sendType == SendType.Normal
                                        ? EumCommandKey.NEXT : EumCommandKey.EXNEXT
                                        , LoggerType.Send, aRecvData));

                Task.Run(() => WiriteLog(messageLog));

                Console.WriteLine(messageLog);

            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> Next Exception : {ex.Message}");
            }

            return result;
        }

        public bool IDRequest(bool status, SendType sendType)
        {
            bool result = false;

            try
            {
                sendData = null;
                sendDataCount = 1;
                sendData = new byte[sendDataCount];

                if (status)
                    sendData[0] = 0x01;
                else
                    sendData[0] = 0x00;
                //sendData[1] = 0x00;
                //sendData[2] = 0x00;

                messageLog = string.Empty;

                messageLog += $"IDRequest = {status}";
                aRecvData = new byte[sendDataCount + 5];
                aRecvData = MakeCommand(sendType == SendType.Normal
                                       ? EumCommandKey.IDREQUEST : EumCommandKey.EXIDREQUEST
                                       , sendData, sendData.Length);

                result = SendCommand(aRecvData);

                Task.Run(() => WiriteLog(sendType == SendType.Normal
                     ? EumCommandKey.IDREQUEST : EumCommandKey.EXIDREQUEST
                     , LoggerType.Send, aRecvData));
                Task.Run(() => WiriteLog(messageLog));
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> IDRequest Exception : {ex.Message}");
            }

            return result;
        }

        public bool IDSizeRequest(string timeCode, SendType sendType)
        {
            bool result = false;

            try
            {
                sendDataCount = 4;
                sendData = null;
                sendData = new byte[sendDataCount];

                byte[] duration = StrTcToBcdBytes(timeCode);

                sendData[0] = duration[0];
                sendData[1] = duration[1];
                sendData[2] = duration[2];
                sendData[3] = duration[3];

                messageLog = string.Empty;
                messageLog += $"IDSizeRequest = {timeCode}";

                aRecvData = new byte[sendDataCount + 5];
                aRecvData = MakeCommand(sendType == SendType.Normal
                    ? EumCommandKey.SIZEREQUEST : EumCommandKey.EXSIZEREQUEST
                    , sendData, sendData.Length);

                result = SendCommand(aRecvData);

                Task.Run(() => WiriteLog(sendType == SendType.Normal
                   ? EumCommandKey.SIZEREQUEST : EumCommandKey.EXSIZEREQUEST
                   , LoggerType.Send, aRecvData));
                Task.Run(() => WiriteLog(messageLog));
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> IDSizeRequest Exception : {ex.Message}");
            }

            return result;
        }

        public bool ActiveIDRequest(string clipName, SendType sendType)
        {
            bool result = false;

            try
            {
                sendData = new byte[1];
                //sendData = new byte[sendDataCount];
                
               if (clipName != string.Empty)
                    sendData[0] = 0x01;
                else
                    sendData[0] = 0x00;


                if (clipName != string.Empty)
                {
                    byte[] byclipName = Encoding.UTF8.GetBytes(clipName);
                    sendData = Combine(sendData, Convert.ToByte(byclipName.Length));
                    sendData = Combine(sendData, byclipName);

                    messageLog = string.Empty;
                    messageLog += $"ActiveIDRequest = {clipName}";
                }

                aRecvData = new byte[sendDataCount + 5];
                aRecvData = MakeCommand(sendType == SendType.Normal
                    ? EumCommandKey.ACTIVEIDREQUEST : EumCommandKey.EXACTIVEIDREQUEST
                    , sendData, sendData.Length);

                result = SendCommand(aRecvData);

                Task.Run(() => WiriteLog(sendType == SendType.Normal
                   ? EumCommandKey.ACTIVEIDREQUEST : EumCommandKey.EXACTIVEIDREQUEST
                   , LoggerType.Send, aRecvData));
                Task.Run(() => WiriteLog(messageLog));
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> ActiveIDRequest Exception : {ex.Message}");
            }

            return result;
        }


        public bool SystemStatus()
        {
            if (!StatusLogOn) return true;

            bool result = false;

            try
            {
                byte[] data = ServerStatusData.GetSystemStatusData();
                Task.Run(() => WiriteLog(SetReceiveWrite(data)));
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> SystemStatus Exception : {ex.Message}");
            }

            return result;
        }
        public bool PortStatus()
        {
            if (!StatusLogOn) return true;

            bool result = false;

            try
            {
                byte[] data = ServerStatusData.GetPortStatusData();
                Task.Run(() => WiriteLog(SetReceiveWrite(data)));
            }
            catch (Exception ex)
            {
                Logger.WriteLine_Critical(ex.Message);
                Console.WriteLine($"---------------> PortStatus Exception : {ex.Message}");
            }

            return result;
        }


    }
}
