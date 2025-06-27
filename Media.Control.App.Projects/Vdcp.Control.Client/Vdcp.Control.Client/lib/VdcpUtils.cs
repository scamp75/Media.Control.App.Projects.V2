using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vdcp.Control.Client
{
    class VdcpUtils
    {
        public enum TimeType { DF, NDF }

        public static int TCToFrame(TimeType type, string aTC)
        {
            int result = -1;

            if (type == TimeType.DF)
                result = DFTCToFrame(aTC);
            else
                result = NDFTCToFrame(aTC);

            return result;
        }

        public static String FrameToTC(TimeType type, int iFrame)
        {
            string result = "00:00:00:00";

            if (type == TimeType.DF)
                result = FrameToDFTC(iFrame);
            else
                result = FrameToNDFTC(iFrame);

            return result;
        }

        public static String FrameToDFTC(int iFrame)
        {
            int hh, mm, m10, m1, ss, ff;
            hh = iFrame / 107892;
            iFrame = iFrame % 107892;
            m10 = iFrame / 17982;
            iFrame = iFrame % 17982;

            if (iFrame < 1800)
            {
                m1 = 0;
                ss = iFrame / 30;
                ff = iFrame % 30;
            }
            else
            {
                iFrame = iFrame - 1800;
                m1 = iFrame / 1798 + 1;
                iFrame = iFrame % 1798;
                if (iFrame < 28)
                {
                    ss = 0;
                    ff = iFrame + 2;
                }
                else
                {
                    iFrame = iFrame - 28;
                    ss = iFrame / 30 + 1;
                    ff = iFrame % 30;
                }
            }

            mm = m10 * 10 + m1;
            return String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", hh, mm, ss, ff);
        }

        public static String FrameToNDFTC(int iFrame)
        {
            int hh, mm, ss, ff;

            hh = iFrame / 108000;
            iFrame = iFrame % 108000;
            mm = iFrame / 1800;
            iFrame = iFrame % 1800;
            ss = iFrame / 30;
            ff = iFrame % 30;

            return String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", hh, mm, ss, ff);
        }

        public static int DFTCToFrame(string aTC)
        {
            if (aTC.Length != 11) return 0;
            int hh, mm, m10, m1, ss, ff;
            int iFrame;

            hh = Convert.ToInt32(aTC.Substring(0, 2));
            mm = Convert.ToInt32(aTC.Substring(3, 2));
            ss = Convert.ToInt32(aTC.Substring(6, 2));
            ff = Convert.ToInt32(aTC.Substring(9, 2));

            if (hh < 0) hh = 0;
            if (hh > 23) hh = 23;

            if (mm < 0) mm = 0;
            if (mm > 59) mm = 59;

            if (ss < 0) ss = 0;
            if (ss > 59) ss = 59;

            if (ss == 0)
            {
                if (mm == 0 || mm == 10 || mm == 20 || mm == 30 || mm == 40 || mm == 50)
                {
                    if (ff < 0) ff = 0;
                    if (ff > 29) ff = 29;
                }
                else
                {
                    if (ff < 2) ff = 2;
                    if (ff > 29) ff = 29;
                }
            }
            else
            {
                if (ff < 0) ff = 0;
                if (ff > 29) ff = 29;
            }

            aTC = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", hh, mm, ss, ff);

            m10 = mm / 10;
            m1 = mm % 10;
            iFrame = hh * 107892 + m10 * 17982;

            if (m1 == 0)
            {
                iFrame = iFrame + ss * 30 + ff;
            }
            else
            {
                iFrame = iFrame + (m1 - 1) * 1798 + 1800;
                if (ss == 0)
                    iFrame = iFrame + ff - 2;
                else
                    iFrame = iFrame + (ss - 1) * 30 + 28 + ff;
            }

            return iFrame;
        }

        public static int NDFTCToFrame(string aTC)
        {
            int hh, mm, ss, ff;
            int iFrame;

            hh = Convert.ToInt32(aTC.Substring(0, 2));
            mm = Convert.ToInt32(aTC.Substring(3, 2));
            ss = Convert.ToInt32(aTC.Substring(6, 2));
            ff = Convert.ToInt32(aTC.Substring(9, 2));

            iFrame = (hh * 108000) + (mm * 1800) + (ss * 30) + ff;

            return iFrame;
        }

        /// <summary>
        /// 입력받은 값을 byte Arry로 변환 후 순서를 뒤집는다.
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public static byte[] Int32To4BytesL(Int32 aValue)
        {
            byte[] temp1 = BitConverter.GetBytes(aValue);

            byte[] temp2 = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                temp2[i] = temp1[3 - i];
            }

            return temp2;
        }

        /// <summary>
        /// 입력받은 값을 byte Arry로 변환 후 순서를 뒤집는다.
        /// </summary>
        /// <param name="aValue"></param>
        /// <returns></returns>
        /// <summary>        
        public static byte[] Int32To3BytesL(Int32 aValue)
        {
            byte[] temp1 = BitConverter.GetBytes(aValue);

            byte[] temp2 = new byte[3];

            for (int i = 0; i < 3; i++)
            {
                temp2[i] = temp1[2 - i];
            }

            return temp2;
        }



        public static byte[] IDToASCIIBytes(string aValue)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();

            return ascii.GetBytes(aValue);
        }

        public static string ASCIIBytesToID(byte[] aValue, int aStartIndex, int aCount)
        {
            //  return MaxNine.Lib.ConvertString.EncKrToString(aValue, aStartIndex, aCount);
            return string.Empty;
        }

        public static byte[] ByteToCutByte(byte[] aValue, int aStartIndex, int aCount)
        {
            byte[] reByte = new byte[aCount];
            Buffer.BlockCopy(aValue, aStartIndex, reByte, 0, aCount);

            return reByte;
        }


        /// <summary>
        /// 타입코드를 받아서 4byte BCD 값으로 변환.
        /// HH:MM:DD:FF -> FFDDMMHH
        /// </summary>
        /// <param name="aTc"></param>
        /// <returns></returns>
        public static byte[] StrTcToBcdBytes(string aTc)
        {
            byte[] temp = new byte[4];

            temp[3] = Convert.ToByte(aTc.Substring(0, 2), 16);
            temp[2] = Convert.ToByte(aTc.Substring(3, 2), 16);
            temp[1] = Convert.ToByte(aTc.Substring(6, 2), 16);
            temp[0] = Convert.ToByte(aTc.Substring(9, 2), 16);
            return temp;
        }

        /// <summary>
        /// FFDDMMHH -> HH:MM:DD:FF
        /// </summary>
        /// <param name="aBcd"></param>
        /// <returns></returns>
        public static string BcdBytesToStrTc(byte[] aBcd)
        {
            if (aBcd.Length != 4) return "00:00:00:00";
            return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}", aBcd[3], aBcd[2], aBcd[1], aBcd[0]);
        }


        public static string BcdBytesToStrTc(byte[] aBcd, int aStartIndex)
        {
            if (aBcd.Length < aStartIndex + 4) return "00:00:00:00";
            return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}", aBcd[aStartIndex + 3], aBcd[aStartIndex + 2], aBcd[aStartIndex + 1], aBcd[aStartIndex]);
        }

        public static string BcdBytesToRemainTc(byte[] aBcd, int aStartIndex)
        {
            if (aBcd.Length < aStartIndex + 4) return "00:00:00:00";

            if (aBcd.Length >= aStartIndex + 5)
            {
                return string.Format("{0:X}{1:X2}:{2:X2}:{3:X2}:{4:X2}", aBcd[aStartIndex + 4], aBcd[aStartIndex + 3], aBcd[aStartIndex + 2], aBcd[aStartIndex + 1], aBcd[aStartIndex]);
            }
            else
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}", aBcd[aStartIndex + 3], aBcd[aStartIndex + 2], aBcd[aStartIndex + 1], aBcd[aStartIndex]);
            }
        }
    }
}
