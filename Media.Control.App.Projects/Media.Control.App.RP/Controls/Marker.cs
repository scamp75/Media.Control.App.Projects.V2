using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Media.Control.App.RP.Controls
{
    public enum MarkerShape
    {
        /// <summary>
        /// 정삼각형 (clip 시작점)
        /// </summary>
        Equilateral,  
        /// <summary>
        /// 직각삼감형 (IN / OUT Point)
        /// </summary>
        RightTriangle 
    }

    public enum TriangleDirection
    {
        None,
        Left,
        Right
    }

    public class Marker
    {
        public int Index { get; set; }
        public double Value { get; set; }
        public double Duration { get; set; }
        public System.Windows.Controls.Button TriangleButton { get; set; }
        public Line MarkerLine { get; set; }
        public string Shape { get; set; } // "Equilateral", "RightTriangle"
        public string Direction { get; set; } // "Left", "Right" (optional)
    }

}
