using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guarder
{
    public struct CheckInfo
    {
        public bool isSrc;
        public bool yearPrefix;
        public BorPDesc BorPDesc;
        public int srcGrid;
        public int dstGrid;
        public string suffix;
        public CheckInfo(bool isSrc,bool yearPrefix, BorPDesc BorPDesc, int srcGrid, int dstGrid, string suffix)
        {
            this.isSrc = isSrc;
            this.yearPrefix = yearPrefix;
            this.BorPDesc = BorPDesc;
            this.srcGrid = srcGrid;
            this.suffix = suffix;
            this.dstGrid = dstGrid;
        }

    }
}
