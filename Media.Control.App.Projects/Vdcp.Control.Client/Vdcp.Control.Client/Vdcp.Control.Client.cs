using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Vdcp.Control.Client
{

    public enum PortType { Serial, NetWork }


    public enum EPOSTIONTYPE : byte
    {
        REMAIN = 0x00,
        SOMBASE = 0x01,
        ZEROBASE = 0x02
    }

    public enum EVDCPSTATE : byte
    {
        IDLE = 0x01,
        CUEINIT = 0x02,
        PLAYREC = 0x04,
        STILL = 0x08,
        JOG = 0x10,
        VARPLAY = 0x20,
        PORTBUSY = 0x40,
        CUEDONE = 0x80,
        CUEDONESTILL = 0x8C,
        CUEDONEPLAYREC = 0x84,
    }

    public enum SYSTEMSTATUSREQTYPE : byte
    {
        REMAINING = 0x41
    }
    public class VdcpControlClient : SvrSerialVdcp
    {
        
        
        public bool Connected { get; set; }

        private static Int32 DataIndex = 4;

        public PortType portType { get; set; }

        private VdcpUdpAdapter udpAdapter = null;
        private ManualResetEvent m_Event = new ManualResetEvent(false);
        //public CMxVdcp()
        //    : this()
        //{
        //    //////////////
        //    BaudRate = 38400;
        //    ByteSize = 8;
        //    StopBits = StopBits.One;
        //    Parity = Parity.Odd;
        //    InBuffSize = 1024;
        //    OutBuffSize = 1024;
        //}

        public VdcpControlClient()
            : base("VDCP")
        {
            BaudRate = 38400;
            ByteSize = 8;
            StopBits = StopBits.One;
            Parity = Parity.Odd;
            InBuffSize = 1024;
            OutBuffSize = 1024;
            Connected = false;

        }

        public void Close()
        {
            udpAdapter.Close();
        }


        public bool SetUpdData(int port)
        {
            bool result = false;
            try
            {
                //udpAdapter = null;
                udpAdapter = new VdcpUdpAdapter();
                udpAdapter.ReciveData += UdpAdapter_ReciveData;
                udpAdapter.Start(port);
                result = true;
            }
#pragma warning disable CS0168 // 'ex' 변수가 선언되었지만 사용되지 않았습니다.
            catch (Exception ex)
#pragma warning restore CS0168 // 'ex' 변수가 선언되었지만 사용되지 않았습니다.
            {

            }

            return result;
        }

        public bool SetClinetData(string address, int port)
        {

            bool result = false;
            try
            {
                udpAdapter.Client(address, port);
                result = true;
            }
            catch
            {

            }

            return result;
        }

        private byte[] _Tempbuff;
        private Int32 _TempSize;

        private void UdpAdapter_ReciveData(byte[] recData, int count)
        {
            if (recData.Length != 0)
            {
                _Tempbuff = new byte[count];
                Array.Copy(recData, 0, _Tempbuff, 0, count);
                _TempSize = count;

                m_Event.Set();

            }
        }

        private bool CheckSum(byte[] aBuf, int aBufSize)
        {
            byte iCRC = 0;
            for (int i = 2; i < aBufSize - 1; i++)
            {
                iCRC += aBuf[i];
            }

            iCRC = (byte)(~iCRC + 1);
            if (iCRC == aBuf[aBufSize - 1]) return true;
            else return false;
        }


        /// <summary>
        /// 8자 미만이면 $0x20을 뒤에 채우고
        /// 이상이면 시작 byte에 길이를 채운다.
        /// </summary>
        /// <param name="aId"></param>
        /// <returns></returns>
        private byte[] CovertStardID(string aId)
        {
            byte[] sTemp = StringToUtf8Array(aId);//VdcpUtils.IDToASCIIBytes(aId);

            byte[] sBuff = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

            Array.Copy(sTemp, 0, sBuff, 0, sTemp.Length);

            return sBuff;
        }

        private byte[] CovertExtendID(string aId)
        {
            byte[] sTemp = StringToUtf8Array(aId);//VdcpUtils.IDToASCIIBytes(aId);

            byte[] sBuff = new byte[sTemp.Length + 1];

            //sBuff[0] = (byte)aId.Length;
            sBuff[0] = (byte)sTemp.Length;

            Array.Copy(sTemp, 0, sBuff, 1, sTemp.Length);

            return sBuff;
        }


        /// <summary>
        /// VDCP 응답에 대해서 결과를 파싱후 이벤트를 Set 시킨다.
        /// </summary>
        /// <param name="e"></param>
        ///         
        protected override ERECVSTAT OnDataReceived(byte[] aRxBuff, int aRxSize)
        {

            try
            {

                if (aRxSize == 0) return ERECVSTAT.CONTINUE;


                if (aRxBuff[0] == 0x04)
                {
                    return ERECVSTAT.SUCCESS;
                }
                else if (aRxBuff[0] == 0x05)
                {

                    switch (aRxBuff[1])
                    {
                        case 0x01:
                            LastError = "Undefined error";
                            break;
                        case 0x04:
                            LastError = "CheckSum error";
                            break;
                        case 0x10:
                            LastError = "Parity error";
                            break;
                        case 0x20:
                            LastError = "Overrun error";
                            break;
                        case 0x40:
                            LastError = "Framing error";
                            break;
                        case 0x80:
                            LastError = "Timeout";
                            break;
                        default:
                            LastError = string.Format("UnKown Error : {0:2X}", aRxBuff[1]);
                            break;

                    }
                    return ERECVSTAT.FAIL;
                }
                else
                {
                    Int32 iStxIndex = -1;
                    for (int i = 0; i < aRxSize; i++)
                    {
                        // STX byte를 구하자.
                        if (aRxBuff[i] == 0x02)
                        {
                            iStxIndex = i;
                            break;
                        }
                    }

                    if (iStxIndex == -1)
                    {
                        aRxSize = 0;
                        return ERECVSTAT.CONTINUE;
                    }

                    if (iStxIndex > 0)
                    {
                        for (int i = 0; i < aRxSize - iStxIndex; i++)
                        {
                            aRxBuff[i] = aRxBuff[iStxIndex + i];
                        }
                        aRxSize -= iStxIndex;
                    }

                    Int32 iBC = aRxBuff[1];
                    if (aRxSize >= (iBC + 3))
                    {
                        if (CheckSum(aRxBuff, aRxSize))
                        {
                            return ERECVSTAT.SUCCESS;
                        }
                        else
                        {
                            return ERECVSTAT.FAIL;
                        }
                    }
                    else
                    {
                        return ERECVSTAT.CONTINUE;
                    }
                }
            }
            catch (Exception er)
            {
                LastError = er.Message;
                return ERECVSTAT.FAIL;
            }

        }

        /// <summary>
        /// 데이터를 보낸 후 응답을 대기한다.
        /// 여기서는 파싱한다.
        /// </summary>        
        private bool TransmitCommand(byte aCmd1, byte aCmd2, byte[] aDataBuf, int aDataSize, out byte[] aRecvData, out int aIRecvSize)
        {
            bool result = false;
            byte iCRC = 0;
            byte[] btBuffs = new byte[aDataSize + 5];

            iCRC = (byte)(aCmd1 + aCmd2);

            btBuffs[0] = 0x02;    // STX
            btBuffs[1] = (byte)(0x02 + aDataSize);   // BC
            btBuffs[2] = aCmd1;
            btBuffs[3] = aCmd2;

            int iCrcPos = 4;
            for (int i = 0; i < aDataSize; i++)
            {
                btBuffs[iCrcPos] = aDataBuf[i];
                iCRC += aDataBuf[i];
                iCrcPos++;
            }
            btBuffs[iCrcPos] = (byte)(~iCRC + 1);


            if (portType == PortType.Serial)
                result = SendRecveData(btBuffs, aDataSize + 5, out aRecvData, out aIRecvSize);
            else
            {
                m_Event.Reset();

                result = udpAdapter.Send(btBuffs);

                if (m_Event.WaitOne(RecveTimeOutMSec, false))
                {
                    aRecvData = new byte[_TempSize];
                    aIRecvSize = _TempSize;
                    Array.Copy(_Tempbuff, 0, aRecvData, 0, _Tempbuff.Length);

                }
                else
                {
                    aRecvData = new byte[0];
                    aIRecvSize = 0;
                    LastError = "Receive TimeOut";
                }
            }


            return result;
        }




        private bool TransmitCommand(byte aCmd1, byte aCmd2, byte[] aDataBuf, int aDataSize)
        {


            bool result = false;
            byte iCRC = 0;
            byte[] btBuffs = new byte[aDataSize + 5];

            iCRC = (byte)(aCmd1 + aCmd2);

            btBuffs[0] = 0x02;    // STX
            btBuffs[1] = (byte)(0x02 + aDataSize);   // BC
            btBuffs[2] = aCmd1;
            btBuffs[3] = aCmd2;

            int iCrcPos = 4;
            for (int i = 0; i < aDataSize; i++)
            {
                btBuffs[iCrcPos] = aDataBuf[i];
                iCRC += aDataBuf[i];
                iCrcPos++;
            }
            btBuffs[iCrcPos] = (byte)(~iCRC + 1);



            return result;
        }


        #region "System Commands"

        /// <summary>
        /// 0X.0C 0X.0D 0X.14 0x.15 0X.16
        /// 거의 사용되지 않는 명령
        /// 클립 락 및 언락 , 포트 락 및 언락 
        /// </summary>
        /// <returns></returns>
        #endregion


        #region "Immdiate Commands"

        public bool Stop()
        {
            byte[] recvData;
            Int32 iRecvSize;

            return TransmitCommand(0x10, 0x00, null, 0, out recvData, out iRecvSize);
        }

        public bool Play()
        {
            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x01, null, 0, out recvData, out iRecvSize);
        }

        public bool Play(byte aHandle)
        {
            byte[] temp1 = new byte[1];
            temp1[0] = aHandle;
            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x01, temp1, 1, out recvData, out iRecvSize);
        }



        public bool Still()
        {
            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x04, null, 0, out recvData, out iRecvSize);

        }

        public bool Step()
        {
            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x05, null, 0, out recvData, out iRecvSize);

        }

        public bool Continue()
        {
            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x06, null, 0, out recvData, out iRecvSize);
        }


        public bool SelectInput(string aInput)
        {
            byte[] temp1 = CovertExtendID(aInput);
            byte[] recvData;
            Int32  iRecvSize;

            return TransmitCommand(0x20, 0x39, temp1, temp1.Length, out recvData, out iRecvSize);
        }

        /// <summary>
        /// 5Frame Latency        
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public bool Jog(Int32 aValue)
        {
            byte[] buff;
            Int32 nCnt;
            if ((aValue < 128) && (aValue > -127))
            {
                buff = new byte[1];
                buff[0] = (byte)aValue;
                nCnt = 1;

                Debug.WriteLine(buff[0]);
            }
            else
            {
                buff = VdcpUtils.Int32To4BytesL(aValue);
                nCnt = 4;

                // Debug.WriteLine(buff[0] + "." + buff[1] + "." + buff[2] + "." + buff[3]);
            }
            byte[] recvData;
            Int32 iRecvSize;

            return TransmitCommand(0x10, 0x07, buff, nCnt, out recvData, out iRecvSize);
        }

        /// <summary>
        /// 5Frame latency
        /// </summary>
        /// <param name="aValue"> 
        /// 0x000000 = still
        /// 0x010000 = std play forward
        /// 0x7F0000 = 127 times std play forward
        /// 0xFF0000 = std play reverse
        /// 0x800000 = 128 times play reverse
        /// </param>
        /// <returns></returns>
        public bool VariPlay(byte aValue1, byte aValue2, byte aValue3)
        {
            byte[] buff = new byte[3];

            buff[0] = aValue1;
            buff[1] = aValue2;
            buff[2] = aValue3;

            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0x10, 0x08, buff, 3, out recvData, out iRecvSize);

        }


        /// <summary>
        /// 5Frame latency
        /// </summary>
        /// <param name="aValue"> 
        /// 0x000000 = still
        /// 0x010000 = std play forward
        /// 0x7F0000 = 127 times std play forward
        /// 0xFF0000 = std play reverse
        /// 0x800000 = 128 times play reverse
        /// </param>
        /// <returns></returns>
        /// 

        private const float BasePlayValue = 0x010000;

        public bool VariPlay(float value)
        {

            int speedValue = 0;

            if (value >= 0)
            {
                if (value >= 127)
                    speedValue = 0x7F0000;
                else
                    speedValue = (int)(BasePlayValue * value);

            }
            else
            {
                if (value <= -128)
                    speedValue = 0x800000;
                else
                    speedValue = 0x1000000 + (int)(BasePlayValue * value);
            }



            byte[] buff = VdcpUtils.Int32To3BytesL(speedValue);
            byte[] recvData;
            Int32 iRecvSize;

            // Debug.WriteLine(buff[0] + "." + buff[1] + "." + buff[2]);

            Debug.WriteLine(-(256 - BitConverter.ToInt16(buff, 0)));


            return TransmitCommand(0x10, 0x08, buff, 3, out recvData, out iRecvSize);
        }

        public bool First()
        {
            return Jog(-2592000);
        }

        public bool End()
        {
            return Jog(2592000);
        }
        public bool FF()
        {
            return VariPlay(-262144);
            //return VariPlay(262144);
        }

        public bool REW()
        {
            return VariPlay(262144);
            //return VariPlay(-262144);
        }

        public bool Record()
        {
            byte[] recvData;
            Int32 iRecvSize;

            return TransmitCommand(0x10, 0x02, null, 0, out recvData, out iRecvSize);
        }

        public bool RecordInitEx(string aID, string aDur)
        {

            byte[] temp1 = CovertExtendID(aID);
            byte[] temp2 = VdcpUtils.StrTcToBcdBytes(aDur);

            byte[] temp3 = new byte[temp1.Length + temp2.Length];
            Array.Copy(temp1, 0, temp3, 0, temp1.Length);
            Array.Copy(temp2, 0, temp3, temp1.Length, temp2.Length);

            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0xA0, 0x23, temp3, temp3.Length, out recvData, out iRecvSize);
            
        }

        public bool PlayCueEx(string aID)
        {
            byte[] temp1 = CovertExtendID(aID);

            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0xA0, 0x24, temp1, temp1.Length, out recvData, out iRecvSize);
            
        }

        public bool CueWithDataEx(string aId, string sSom, string sDur)
        {
            byte[] temp1 = CovertExtendID(aId);
            byte[] temp2 = VdcpUtils.StrTcToBcdBytes(sSom);
            byte[] temp3 = VdcpUtils.StrTcToBcdBytes(sDur);


            byte[] temp4 = new byte[temp1.Length + temp2.Length + temp3.Length];
            Array.Copy(temp1, 0, temp4, 0, temp1.Length);
            Array.Copy(temp2, 0, temp4, temp1.Length, temp2.Length);
            Array.Copy(temp3, 0, temp4, temp1.Length + temp2.Length, temp3.Length);

            byte[] recvData;
            Int32 iRecvSize;
            return TransmitCommand(0xA0, 0x25, temp4, temp4.Length, out recvData, out iRecvSize);
            
        }

        public bool CueWithDataEx(string aId)
        {
            byte[] temp1 = CovertExtendID(aId);

            byte[] recvData;
            Int32 iRecvSize;
         
            return TransmitCommand(0xA0, 0x24, temp1, temp1.Length, out recvData, out iRecvSize);
        }


        #endregion


        #region "Preset/select Commands 2x.xx"





        public bool ClosePort(byte aPortId)
        {
            byte[] buff = new byte[1];
            buff[0] = aPortId;

            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x20, 0x21, buff, 1, out recvData, out iRecvSize))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SelectPort(int aPortId)
        //public bool SelectPort(byte aPortId)
        {
            byte[] buff = new byte[1];
            buff[0] = (byte)aPortId;

            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x20, 0x22, buff, 1, out recvData, out iRecvSize))
            {
                if (recvData.Length != 0)
                {
                    Connected = true;
                    return true;
                }
                else return false;
            }
            else
            {
                return false;
            }
        }

        public bool CueWithDataEx(string aId)
        {
            byte[] temp1 = CovertExtendID(aId);



            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xA0, 0x24, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion

        #region "Sense Requests Commands 3x.xx"

        public bool OpenPort(byte aPortId, byte aLockode)
        {
            bool result = false;
            byte[] buff = new byte[2];
            buff[0] = aPortId;
            buff[1] = aLockode;

            byte[] recvData;
            Int32 iRecvSize;

            if (TransmitCommand(0x30, 0x01, buff, 2, out recvData, out iRecvSize))
            {
                /// 데이터가 있는 경우에는 STX, BC, Cmd1, cmd2, data 
                if (recvData.Length > 2 && recvData[DataIndex] == 1) return true;
                else
                    result = false;
            }
            else
            {
                result = false;
            }


            return result;
        }


        private byte[] StringToUtf8Array(string aClip)
        {
            UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(aClip);
        }

        private string Utf8ArryToString(byte[] aClip, int aIndex, int aLength)
        {
            //byte[] temp = new byte[aLength];
            UTF8Encoding encoding = new System.Text.UTF8Encoding();

            //Array.Copy(aClip, aIndex, temp, 0, aLength);

            return encoding.GetString(aClip, aIndex, aLength);
        }



        public bool Next(out int aTotalCount, ArrayList aClips)
        {
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x02, null, 0, out recvData, out iRecvSize))
            {
                byte[] buff = new byte[2];

                buff[1] = recvData[DataIndex];
                buff[0] = recvData[DataIndex + 1];
                aTotalCount = BitConverter.ToInt16(buff, 0);

                int iClipIndex = DataIndex + 2;
                int iClipLen;
                int j = 0;
                while (iClipIndex < iRecvSize - 1)
                {
                    iClipLen = recvData[iClipIndex];
                    iClipIndex++;


                    //     aClips.Add(VdcpUtils.Utf8ArryToString(_RxBuff, iClipIndex, iClipLen));
                    aClips.Add(Utf8ArryToString(recvData, iClipIndex, iClipLen));
                    iClipIndex += iClipLen;
                    j++;
                }
                return true;
            }
            else
            {
                aTotalCount = 0;
                return false;
            }
        }


        public bool NextEx(out int aTotalCount, ArrayList aClips)
        {
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xB0, 0x02, null, 0, out recvData, out iRecvSize))
            {
                byte[] buff = new byte[2];

                buff[1] = recvData[DataIndex];
                buff[0] = recvData[DataIndex + 1];
                aTotalCount = BitConverter.ToInt16(buff, 0);

                int iClipIndex = DataIndex + 2;
                int iClipLen;
                int j = 0;
                while (iClipIndex < iRecvSize - 1)
                {
                    iClipLen = recvData[iClipIndex];
                    iClipIndex++;


                    //     aClips.Add(VdcpUtils.Utf8ArryToString(_RxBuff, iClipIndex, iClipLen));
                    aClips.Add(Utf8ArryToString(recvData, iClipIndex, iClipLen));
                    iClipIndex += iClipLen;
                    j++;
                }
                return true;
            }
            else
            {
                aTotalCount = 0;
                return false;
            }
        }


        public bool LastEx(out string aId)
        {

            aId = "";
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xB0, 0x03, null, 0, out recvData, out iRecvSize))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool PortStatus(out List<string> aStatus, out byte PortId)
        {
            byte Status = 0;
            aStatus = new List<string>();

            bool result = PortStatus(out Status, out PortId);

            //   0        1            2       3      4       5        6          7
            // CUEDONE | PORTBUSY  | VARPLAY  |JOG  | STILL | PLAY | CUE/ INIT  | IDLE
            string statePort = string.Empty;
            char[] byteArray = HexStringToBinary(Status.ToString("X2")).ToArray();

            for (int i = 0; i < 8; ++i)
            {
                if (byteArray[i].ToString() == "1")
                {
                    switch (i)
                    {
                        case 0: statePort = EVDCPSTATE.CUEDONE.ToString(); break;
                        case 1: statePort = EVDCPSTATE.PORTBUSY.ToString(); break;
                        case 2: statePort = EVDCPSTATE.VARPLAY.ToString(); break;
                        case 3: statePort = EVDCPSTATE.JOG.ToString(); break;
                        case 4: statePort = EVDCPSTATE.STILL.ToString(); break;
                        case 5: statePort = EVDCPSTATE.PLAYREC.ToString(); break;
                        case 6: statePort = EVDCPSTATE.CUEINIT.ToString(); break;
                        case 7: statePort = EVDCPSTATE.IDLE.ToString(); break;
                    }

                    aStatus.Add(statePort);
                }
            }

            if (PortId == 0)
            {

            }


            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aStatus"> 0x01</param>
        /// <returns></returns>
        public bool PortStatus(out byte aStatus, out byte aPortId)
        {

            byte[] temp1 = new byte[1];
            temp1[0] = 0x01;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x05, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                if (DataIndex > iRecvSize)
                {
                    aStatus = 0;
                    aPortId = 0;
                    return false;
                }
                else
                {
                    string sTemp = "";

                    for (int i = 5; i < recvData.Length; i++)
                    {

                        if (i == 6)
                            sTemp = recvData[i].ToString(); //Encoding.Default.GetString(recvData[i]);
                        else
                            sTemp = Convert.ToString(recvData[i], 2).PadLeft(8, '0');

                        // System.Diagnostics.Debug.WriteLine(sTemp);
                    }

                    aStatus = recvData[DataIndex + 1];
                    aPortId = recvData[DataIndex + 2];
                    return true;
                }
            }
            else
            {
                aStatus = 0;
                aPortId = 0;
                return false;
            }

        }

        public bool PortStatus(out byte[] aStatus)
        {

            byte[] temp1 = new byte[1];
            temp1[0] = 0x01;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x05, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                if (DataIndex > iRecvSize)
                {
                    aStatus = null;
                    return false;
                }
                else
                {
                    byte[] recArray = new byte[6];

                    if (recvData.Length != 13)
                    {

                    }
                    else
                    {
                        for (int i = 0; i < 6; ++i)
                        {
                            recArray.SetValue(recvData[DataIndex + i + 1], i);
                        }

                    }

                    aStatus = recArray;

                    return true;
                }
            }
            else
            {
                aStatus = null;
                return false;
            }

        }


        /// <summary>
        /// 0 remain tc, 1 som-base current tc, 2 zero-base current tc
        /// </summary>
        /// <param name="aMode"></param>
        /// <returns></returns>
        public string PositionRequest(EPOSTIONTYPE aType)
        {
            byte[] temp1 = new byte[1];
            temp1[0] = (byte)aType;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x06, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                // if (_iRxBuffSize < DataIndex + 1 + 4 + 1) return "00:00:00:00";                
                return VdcpUtils.BcdBytesToStrTc(recvData, DataIndex + 1);
            }
            else
            {
                return "00:00:00:00";
            }
        }

        private static readonly Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
    { '0', "0000" },
    { '1', "0001" },
    { '2', "0010" },
    { '3', "0011" },
    { '4', "0100" },
    { '5', "0101" },
    { '6', "0110" },
    { '7', "0111" },
    { '8', "1000" },
    { '9', "1001" },
    { 'a', "1010" },
    { 'b', "1011" },
    { 'c', "1100" },
    { 'd', "1101" },
    { 'e', "1110" },
    { 'f', "1111" }
};

        public string HexStringToBinary(string hex)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in hex)
            {
                // This will crash for non-hex characters. You might want to handle that differently.
                result.Append(hexCharacterToBinary[char.ToLower(c)]);
            }
            return result.ToString();
        }

        public bool SystemStatusRequst(out SystemStatus sys)
        {
            // aRemain = "00:00:00:00";
            //aRemain = new byte[1];
            sys = new SystemStatus();
            string statndtime = string.Empty;

            byte[] temp1 = new byte[1];
            temp1[0] = (byte)SYSTEMSTATUSREQTYPE.REMAINING;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x10, temp1, temp1.Length, out recvData, out iRecvSize))
            {

                //            4                   5            6           7
                // REMOTE CONTROL DISABLED / DISK DOWN / SYSTEM DOWN  / DISK FULL

                string statePort = string.Empty;
                char[] byteArray = HexStringToBinary(recvData[10].ToString("X2")).ToArray();

                DiskStatusData diksData = new DiskStatusData();

                for (int i = 0; i < 8; ++i)
                {
                    if (byteArray[i].ToString() == "1")
                    {
                        switch (i)
                        {
                            case 0: break;
                            case 1: break;
                            case 2: break;
                            case 3: break;
                            case 4: diksData.DisRemoteControl = true; break;
                            case 5: diksData.DiskDown = true; break;
                            case 6: diksData.SystemDown = true; break;
                            case 7: diksData.DiskFull = true; break;
                        }

                    }
                }

                sys.StoredID = recvData[9].ToString();
                sys.TotalRemainTime = VdcpUtils.BcdBytesToStrTc(recvData, 5);
                sys.DiskStatus = diksData;
                sys.StandardTime = VdcpUtils.BcdBytesToStrTc(recvData, 11);
                sys.SignalFullLevel = (int)recvData[15];

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SystemStatusRequstEx(out string aTotal, out string aRemain)
        {
            aTotal = "";
            aRemain = "";
            return true;
        }

        public bool List(out int aTotalCount, ArrayList aClips)
        {
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x11, null, 0, out recvData, out iRecvSize))
            {
                byte[] buff = new byte[2];
                buff[1] = recvData[DataIndex];
                buff[0] = recvData[DataIndex + 1];
                aTotalCount = BitConverter.ToInt16(buff, 0);

                int iClipIndex = DataIndex + 2;
                int iClipLen;
                int j = 0;
                while (iClipIndex < iRecvSize - 1)
                {
                    iClipLen = recvData[iClipIndex];
                    iClipIndex++;

                    //aClips.Add(VdcpUtils.ASCIIBytesToID(_RxBuff, iClipIndex, iClipLen));
                    aClips.Add(Utf8ArryToString(recvData, iClipIndex, iClipLen));
                    iClipIndex += iClipLen;
                    j++;
                }
                return true;
            }
            else
            {
                aTotalCount = 0;
                return false;
            }
        }

        public bool ListEx(out int aTotalCount, ArrayList aClips)
        {
            bool result = false;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xB0, 0x11, null, 0, out recvData, out iRecvSize))
            {
                if (recvData.Length != 0)
                {
                    byte[] buff = new byte[2];
                    buff[1] = recvData[DataIndex];
                    buff[0] = recvData[DataIndex + 1];
                    aTotalCount = BitConverter.ToInt16(buff, 0);

                    int iClipIndex = DataIndex + 2;
                    int iClipLen;
                    int j = 0;
                    while (iClipIndex < iRecvSize - 1)
                    {
                        iClipLen = recvData[iClipIndex];
                        iClipIndex++;

                        //aClips.Add(VdcpUtils.ASCIIBytesToID(_RxBuff, iClipIndex, iClipLen));
                        aClips.Add(Utf8ArryToString(recvData, iClipIndex, iClipLen));
                        iClipIndex += iClipLen;
                        j++;
                    }
                }
                else aTotalCount = 0;

                result = true;
            }
            else
            {
                aTotalCount = 0;
                result = false;
            }

            return result;
        }

        public string IDSizeRequest(string aId)
        {
            byte[] recvData;
            Int32 iRecvSize;
            byte[] temp1 = CovertStardID(aId);
            if (TransmitCommand(0x30, 0x14, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                return VdcpUtils.BcdBytesToStrTc(recvData, DataIndex);
            }
            else
            {
                return "00:00:00:00";
            }
        }

        public bool ActiveIDRequest(out string clip)
        {
            bool result = false;
            byte[] recvData;
            Int32 iRecvSize;
            clip = string.Empty;
            if (TransmitCommand(0x30, 0x07, null, 0, out recvData, out iRecvSize))
            {
                if (recvData.Length != 0)
                {
                    if (recvData[DataIndex] == 1) result = true;

                    if (result)
                    {
                        int iClipIndex = DataIndex + 2;
                        int iClipLen = recvData[5];
                        clip = Utf8ArryToString(recvData, iClipIndex, iClipLen);
                    }

                }
            }

            return result;
        }

        public bool ActiveIDRequestEx(out string clip)
        {
            bool result = false;
            byte[] recvData;
            Int32 iRecvSize;
            clip = string.Empty;
            if (TransmitCommand(0xB0, 0x07, null, 0, out recvData, out iRecvSize))
            {
                if (recvData.Length != 0)
                {
                    if (recvData[DataIndex] == 1) result = true;

                    if (result)
                    {
                        int iClipIndex = DataIndex + 2;
                        int iClipLen = recvData[5];
                        clip = Utf8ArryToString(recvData, iClipIndex, iClipLen);
                    }

                }
            }

            return result;
        }

        public string IDSizeRequestEx(string aId)
        {
            byte[] recvData;
            Int32 iRecvSize;
            byte[] temp1 = CovertExtendID(aId);
            if (TransmitCommand(0xB0, 0x14, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                if (iRecvSize == 9)
                {
                    return VdcpUtils.BcdBytesToStrTc(recvData, DataIndex);
                }
                else
                {
                    return "00:00:00:00";
                }
            }
            else
            {
                return "00:00:00:00";
            }
        }

        public bool IDSizeRequestEx(string aId, out string aSom, out string aDur)
        {
            aDur = "00:00:00:00";
            aSom = "00:00:00:00";
            byte[] temp1 = CovertExtendID(aId);
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xB0, 0x14, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                if (iRecvSize == 9)
                {

                    aDur = VdcpUtils.BcdBytesToStrTc(recvData, DataIndex);
                    aSom = "00:00:00:00";
                    return true;
                }
                else if (iRecvSize == 13)
                {
                    aDur = VdcpUtils.BcdBytesToStrTc(recvData, DataIndex);
                    aSom = VdcpUtils.BcdBytesToStrTc(recvData, DataIndex + 4);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool IDRequest(string aId, out bool IsExist)
        {
            byte[] temp1 = CovertStardID(aId);
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x30, 0x16, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                //if(_RxBuff[DataIndex+1] == 0x01


                if (recvData[4] == 0x01)
                    IsExist = true;
                else
                    IsExist = false;

                return true;
            }
            else
            {
                IsExist = false;
                return false;
            }
        }


        public bool IDRequestEx(string aId, out bool IsExist)
        {
            byte[] temp1 = CovertExtendID(aId);
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0xB0, 0x16, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                IsExist = false;
                if (iRecvSize == 1)
                {
                    IsExist = true;
                }
                else
                {
                    if ((recvData[DataIndex] & 0x01) == 0x01) IsExist = true;
                    else
                    {
                        string vs = string.Empty;
                        foreach (byte b in recvData)
                        {
                            vs += $".{b.ToString()}";
                        }

                        Console.WriteLine(vs);

                        return false;
                    }
                }



                return true;
            }
            else
            {
                IsExist = false;
                return false;
            }
        }

        public bool EncAutoStop(byte aMode)
        {
            byte[] temp1 = new byte[1];
            temp1[0] = aMode;
            byte[] recvData;
            Int32 iRecvSize;
            if (TransmitCommand(0x20, 0x73, temp1, temp1.Length, out recvData, out iRecvSize))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
    
}
