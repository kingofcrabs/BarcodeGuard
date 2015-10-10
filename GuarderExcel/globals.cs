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
        private int plateStartGridID = int.Parse(ConfigurationManager.AppSettings["plateStartGrid"]);
        #endregion
        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        public GlobalVars()
        {
            PlateBarcodes = new List<string>();
            WaiterName = "FeedMe";
        }

        public Dictionary<int, List<string>> eachGridExpectedBarcodes = new Dictionary<int, List<string>>();
        public List<string> eachPlateExpectedBarcodes = new List<string>();


        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public int PlateStartGridID
        {
            get
            {
                return plateStartGridID;
            }
        }

        public int StartGridID
        {
            get
            {
                return startGridID;
            }
        }

        public List<string> PlateBarcodes { get; set; }

        public int PlateCnt { get; set; }



        public string ExcelFolder
        {
            get
            {
                return GetSetting("excelFolder");
            }
        }

        public string WaiterName { get; set; }
    }
}
