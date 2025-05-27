using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VdcpService.lib
{
    public class BooleanProperty
    {
        private int row = 0;
        private int column = 0;
        private bool pValue = false;

        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int Column
        {
            get { return column; }
            set { column = value; }
        }

        public bool PValue
        {
            get { return pValue; }
            set { pValue = value; }
        }

        public BooleanProperty(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public BooleanProperty(int row, int column, bool set)
        {
            this.row = row;
            this.column = column;
            this.pValue = set;
        }
    }
}
