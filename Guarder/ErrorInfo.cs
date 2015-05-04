using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guarder
{
    public class ErrorsInfo
    {
        public bool IsSource { get; set; }
        public List<SampleDescription> SampleDescriptionCollection{get;set;}
    }

    public class SampleDescription
    {
        public string Barcode { get; set; }
        public string ExpectedBarcode { get; set; }
        public bool IsCorrect { get; set; }
        public string ErrMsg { get; set; }
        public int LineNumber { get; set; }
        public SampleDescription(int lineNumber, string barcode,string expectedBarcode, string errMsg, bool isCorrect = true)
        {
            Barcode = barcode;
            ExpectedBarcode = expectedBarcode;
            ErrMsg = errMsg;
            IsCorrect = isCorrect;
            LineNumber = lineNumber;
        }
    }

}
