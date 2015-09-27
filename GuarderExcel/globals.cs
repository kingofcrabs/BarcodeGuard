using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Guarder
{
    class GlobalVars
    {
        static GlobalVars instance = null;
        #region configures
        private int startGridID = int.Parse(ConfigurationManager.AppSettings["startGrid"]);

        #endregion

        private int batchID = 0;
        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        public Dictionary<int, List<string>> eachGridExpectedBarcodes = new Dictionary<int, List<string>>();


        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }



        public int StartGridID
        {
            get
            {
                return startGridID;
            }
        }



        public int PlateCnt { get; set; }



        public string ExcelFolder
        {
            get
            {
                return GetSetting("excelFolder");
            }
        }
    }
}
