using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mpv.Player.App
{
    public class Timecode : IComparable, IComparable<Timecode>, IEquatable<Timecode>
    {
        public static readonly Timecode Zero = new Timecode();

        private List<ulong> _TimeList; // Hour, Minute, Second, Frame 순서

        /// <summary>
        /// 시간
        /// </summary>
        public ulong Hour { get => _TimeList[0]; set => _TimeList[0] = value; }
        /// <summary>
        /// 분
        /// </summary>
        public ulong Minute { get => _TimeList[1]; set => _TimeList[1] = value; }
        /// <summary>
        /// 초
        /// </summary>
        public ulong Second { get => _TimeList[2]; set => _TimeList[2] = value; }
        /// <summary>
        /// 프레임 개수
        /// </summary>
        public ulong Frame { get => _TimeList[3]; set => _TimeList[3] = value; }

        /// <summary>
        /// 2자릿수 문자열 형식의 시간
        /// </summary>
        public string HH { get => $"{_TimeList[0]:D2}"; set => _TimeList[0] = Convert.ToUInt64(value); }
        /// <summary>
        /// 2자릿수 문자열 형식의 분
        /// </summary>
        public string MM { get => $"{_TimeList[1]:D2}"; set => _TimeList[1] = Convert.ToUInt64(value); }
        /// <summary>
        /// 2자릿수 문자열 형식의 초
        /// </summary>
        public string SS { get => $"{_TimeList[2]:D2}"; set => _TimeList[2] = Convert.ToUInt64(value); }
        /// <summary>
        /// 2자릿수 문자열 형식의 프레임 개수
        /// </summary>
        public string FF { get => $"{_TimeList[3]:D2}"; set => _TimeList[3] = Convert.ToUInt64(value); }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public Timecode() : this(0, 0, 0, 0) { }

        /// <summary>
        /// 숫자 4개를 받는 생성자
        /// </summary>
        /// <param name="hours">시간</param>
        /// <param name="minutes">분</param>
        /// <param name="seconds">초</param>
        /// <param name="frames">프레임 개수</param>
        public Timecode(ulong hours, ulong minutes, ulong seconds, ulong frames) : this(new List<ulong> { hours, minutes, seconds, frames }) { }

        /// <summary>
        /// 문자열을 받는 생성자
        /// </summary>
        /// <param name="s">HH:mm:ss:ff 혹은 HHmmssff (24시간 기준: HH)</param>
        public Timecode(string s) : this(s.Contains(':') ? parseFormat1(s.Replace(';', ':')) : parseFormat2(s)) { }

        private Timecode(List<ulong> timeList)
        {
            if (timeList.Count < 3)
            {
                timeList.Add(0ul);
            }

            if (timeList.Count < 4)
            {
                timeList.Add(0ul);
            }

            _TimeList = timeList;
        }

        public static Timecode Parse(string s) => new Timecode(s);

        public static bool TryParse(string s, out Timecode result)
        {
            try
            {
                result = new Timecode(s);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static List<ulong> parseFormat1(string format1) => format1.Split(':').Select(UInt64.Parse).ToList();

        private static List<ulong> parseFormat2(string format2) => new List<ulong> { Convert.ToUInt64(format2.Substring(0, 2)), Convert.ToUInt64(format2.Substring(2, 2)), Convert.ToUInt64(format2.Substring(4, 2)), Convert.ToUInt64(format2.Substring(6, 2)) };

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case Timecode tc:
                    return CompareTo(tc);
                case null:
                    return 1; // 내가 더 큼
                default:
                    throw new ArgumentException();
            }
        }

        public int CompareTo(Timecode other) => other == null ? 1 : Hour == other.Hour ? (Minute == other.Minute ? (Second == other.Second ? Frame.CompareTo(other.Frame) : Second.CompareTo(other.Second)) : Minute.CompareTo(other.Minute)) : Hour.CompareTo(other.Hour);

        public bool Equals(Timecode other) => CompareTo(other) == 0 ? true : false;

        public override string ToString() => string.Format("{0:D2}:{1:D2}:{2:D2};{3:D2}", Hour, Minute, Second, Frame);

        public override bool Equals(object o) => Equals((Timecode)o);

        public override int GetHashCode()
        {
            var hashCode = -1350608522;
            hashCode = hashCode * -1521134295 + EqualityComparer<List<ulong>>.Default.GetHashCode(_TimeList);
            hashCode = hashCode * -1521134295 + Hour.GetHashCode();
            hashCode = hashCode * -1521134295 + Minute.GetHashCode();
            hashCode = hashCode * -1521134295 + Second.GetHashCode();
            hashCode = hashCode * -1521134295 + Frame.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HH);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MM);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SS);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FF);
            return hashCode;
        }

        public static bool operator <(Timecode tc1, Timecode tc2) => tc1.CompareTo(tc2) < 0;

        public static bool operator >(Timecode tc1, Timecode tc2) => tc1.CompareTo(tc2) > 0;

        public static bool operator <=(Timecode tc1, Timecode tc2) => tc1.CompareTo(tc2) <= 0;

        public static bool operator >=(Timecode tc1, Timecode tc2) => tc1.CompareTo(tc2) >= 0;

        // Equals 사용할 경우 tc2에 대한 null 검사를 수행함.
        public static bool operator ==(Timecode tc1, Timecode tc2) => EqualityComparer<Timecode>.Default.Equals(tc1, tc2);

        public static bool operator !=(Timecode tc1, Timecode tc2) => !(tc1 == tc2);

        public static implicit operator string(Timecode timecode) => timecode.ToString();

        public static implicit operator Timecode(string timecode)
        {
            try
            {
                return Timecode.Parse(timecode);
            }
            catch
            {
                return Timecode.Zero;
            }
        }
    }

}
