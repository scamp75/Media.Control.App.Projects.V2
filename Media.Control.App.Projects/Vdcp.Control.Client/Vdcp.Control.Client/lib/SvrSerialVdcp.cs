using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Vdcp.Control.Client
{
    
    public enum ERECVSTAT
    {
        NONE,
        CONTINUE,
        FAIL,  // LastError 메세지를 확인한다. Nack에 대한 에러 메세지도 여기서 처리.
        SUCCESS
    }

    public abstract class SvrSerialVdcp : IDisposable
    {
        private SerialPort _SerialPort;

        /// <summary>
        /// 명령 응답 대기용 WaitHandle
        /// </summary>
        private ManualResetEvent m_Event = new ManualResetEvent(false);

        //   private ILogManager _LogMgr;

        private string _sDevName;

        private Int32 _iRecveTimeOutMSec = 1000;

        private string _LastError;

        /// <summary>
        /// 수신 데이터 최대 길이는 1024로 하자.     
        /// </summary>
        public static Int32 MaxRxBuffSize = 1024 * 2;

        /// <summary>
        /// 수신 버퍼.
        /// </summary>
        private byte[] _RxBuff;

        /// <summary>
        /// 수신 데이터 갯수.
        /// </summary>
        private Int32 _iRxBuffSize;

        private byte[] _Tempbuff;
        private Int32 _TempSize;

        private ERECVSTAT _RecvStat;

        public SvrSerialVdcp(string aDevName)
        {
            _SerialPort = new SerialPort();
            _sDevName = aDevName;
            //   _LogMgr = aLogMgr;
            _RxBuff = new byte[MaxRxBuffSize];
            _RecvStat = ERECVSTAT.NONE;

            _Tempbuff = new byte[MaxRxBuffSize];
            _TempSize = 0;

            _SerialPort.DataReceived += new SerialDataReceivedEventHandler(_DataReceived);
        }

        /// <summary>
        /// Finalize
        /// </summary>
        public void Dispose()
        {
            Active = false;
        }

        public bool Active
        {
            set
            {
                try
                {
                    if (value)
                    {
                        if (_SerialPort.IsOpen)
                        {
                            _SerialPort.Close();
                        }

                        _SerialPort.Open();
                    }
                    else
                    {
                        if (_SerialPort.IsOpen)
                        {
                            _SerialPort.Close();
                        }
                    }
                }
                catch (Exception er)
                {
                    LastError = er.Message;
                }
            }
            get
            {
                return _SerialPort.IsOpen;
            }
        }

        public string LastError
        {
            set
            {
                _LastError = value;
            }
            get
            {
                string temp = _LastError;
                _LastError = "";
                return temp;
            }
        }

        //protected void WriteLog(LogType aType, string aMsg, LogLevel aLev)
        //{
        //    if(_LogMgr != null)
        //    {
        //        _LogMgr.WriteLogEx(aType, _sDevName + "["+ _SerialPort.PortName +"]", aMsg, aLev);
        //    }
        //}

        private void _DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!_SerialPort.IsOpen) return;

                try
                {
                    _TempSize = _SerialPort.Read(_Tempbuff, 0, _Tempbuff.Length);
                }
                catch
                {
                    return;
                }

                if (_RecvStat != ERECVSTAT.CONTINUE || _TempSize <= 0)
                {
                    return;
                }

                if (_TempSize + _iRxBuffSize > MaxRxBuffSize)
                {
                    _RecvStat = ERECVSTAT.FAIL;
                    _SerialPort.DiscardInBuffer();
                    LastError = "Over Flow";
                    _iRxBuffSize = 0;
                    m_Event.Set();
                }
                else
                {
                    Array.Copy(_Tempbuff, 0, _RxBuff, _iRxBuffSize, _TempSize);

                    _iRxBuffSize += _TempSize;

                    _RecvStat = OnDataReceived(_RxBuff, _iRxBuffSize);

                    if (_RecvStat != ERECVSTAT.CONTINUE)
                    {
                        // WriteLog(LogType.I, "RECV [ " + ConvertString.BytesToHexStr(_RxBuff, 0, _iRxBuffSize) + " ]", LogLevel.Lev5);
                        m_Event.Set();
                    }
                }
            }
            catch (Exception ec)
            {
                _RecvStat = ERECVSTAT.FAIL;
                LastError = ec.Message;

                //WriteLog(LogType.E, ec.Message, LogLevel.Lev1);
                //  수신 데이터 비우기
                try
                {
                    if (_SerialPort.BytesToRead > 0)
                    {
                        _SerialPort.DiscardInBuffer();
                    }
                }
                catch { }
                m_Event.Set();
            }
        }

        /// <summary>
        // Data를 받아서 Pasing까지한 후 결과를 리턴한다.     
        // 프로토콜에 따른 수신 데이터 처리를 여기서 하자..
        /// </summary>
        /// <param name="aRxBuff"></param>
        /// <param name="aRxSize"></param>
        /// <returns></returns>
        protected abstract ERECVSTAT OnDataReceived(byte[] aRxBuff, int aRxSize);

        /// <summary>
        /// 사용할 포트의 이름 확인/설정
        /// </summary>
        public string PortName
        {
            get { return _SerialPort.PortName; }
            set { _SerialPort.PortName = value; }
        }

        /// <summary>
        /// 포트의 전송 속도 확인
        /// </summary>
        public Int32 BaudRate
        {
            get { return _SerialPort.BaudRate; }
            protected set { _SerialPort.BaudRate = value; }
        }

        /// <summary>
        /// 포트의 바이트당 비트수 확인
        /// </summary>
        public Int32 ByteSize
        {
            get { return _SerialPort.DataBits; }
            protected set { _SerialPort.DataBits = value; }
        }

        /// <summary>
        /// 포트의 정지 비트수 확인
        /// </summary>
        public StopBits StopBits
        {
            get { return _SerialPort.StopBits; }
            protected set { _SerialPort.StopBits = value; }
        }

        public Int32 InBuffSize
        {
            get { return _SerialPort.ReadBufferSize; }
            protected set { _SerialPort.ReadBufferSize = value; }
        }

        public Int32 OutBuffSize
        {
            get { return _SerialPort.WriteBufferSize; }
            protected set { _SerialPort.WriteBufferSize = value; }
        }

        /// <summary>
        /// 포트의 패리티 검사 방법 확인
        /// </summary>
        public Parity Parity
        {
            get { return _SerialPort.Parity; }
            protected set { _SerialPort.Parity = value; }
        }

        public int RecveTimeOutMSec
        {
            set
            {
                _iRecveTimeOutMSec = value;
            }
            get
            {
                return _iRecveTimeOutMSec;
            }
        }

        protected void InitStatus()
        {
            if (!_SerialPort.IsOpen) return;
            _RecvStat = ERECVSTAT.CONTINUE;
            _iRxBuffSize = 0;
            _LastError = "";
            //_SerialPort.DiscardOutBuffer();
            m_Event.Reset();
        }

        /// <summary>
        /// 데이터를 보내구 그 그 결과를 반환한다.
        /// 외부 콜에 대해서 동기화 처리를 한다.
        /// 인자가 필요한 경우에 쓴다.
        /// </summary>
        /// <param name="aTxBuffer"></param>
        /// <param name="aCount"></param>
        /// <param name="aRxBuff"></param>
        /// <param name="aRxCount"></param>
        /// <returns></returns>
        protected bool SendRecveData(byte[] aTxBuffer, int aCount, out byte[] aRxBuff, out int aRxCount)
        {
            lock (this)
            {
                InitStatus();
                aRxBuff = null;
                aRxCount = 0;

                if (!_SerialPort.IsOpen) return false;
                _SerialPort.Write(aTxBuffer, 0, aCount);

                if (m_Event.WaitOne(RecveTimeOutMSec, false))
                {
                    if (_RecvStat == ERECVSTAT.SUCCESS)
                    {
                        aRxCount = _iRxBuffSize;
                        aRxBuff = new byte[aRxCount];
                        Array.Copy(_RxBuff, 0, aRxBuff, 0, aRxCount);
                        return true;
                    }
                    else return false;
                }
                else
                {
                    LastError = "Receive TimeOut";
                    return false;
                }
            }
        }

        /// <summary>
        /// 데이터를 보내구 그 그 결과를 반환한다.
        /// 외부 콜에 대해서 동기화 처리를 한다.
        /// 인자가 필요 없는 경우에 쓴다.
        /// </summary>
        /// <param name="aTxBuffer"></param>
        /// <param name="aCount"></param>
        /// <param name="aRxBuff"></param>
        /// <param name="aRxCount"></param>
        /// <returns></returns>
        protected bool SendRecveData(byte[] aTxBuffer, int aCount)
        {
            lock (this)
            {
                if (!_SerialPort.IsOpen) return false;
                //  InitStatus();
                _SerialPort.Write(aTxBuffer, 0, aCount);
                //  WriteLog(LogType.I, "SEND [ " + ConvertString.BytesToHexStr(aTxBuffer) + " ]", LogLevel.Lev5);
                if (m_Event.WaitOne(RecveTimeOutMSec, false))
                {
                    if (_RecvStat == ERECVSTAT.SUCCESS)
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    LastError = "Receive TimeOut";
                    return false;
                }
            }
        }
    }
}

