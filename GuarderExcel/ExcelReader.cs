using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Guarder
{
    class ExcelReader
    {
        public static Dictionary<int,List<string>> ReadBarcodes()
        {
            string sFolder = GlobalVars.Instance.ExcelFolder;
            var di = new DirectoryInfo(sFolder);
            var last = di.EnumerateDirectories()
                                .OrderBy(d => d.CreationTime)
                                .Last();
            string sFilePath = last.FullName;
            int plateCnt = GlobalVars.Instance.PlateCnt;
            SaveAsCSV(new List<string>() { sFilePath });
            string csvFilePath = sFilePath.Replace("xlsx", "csv");

            return new Dictionary<int, List<string>>();

        }

        private static void SaveAsCSV(List<string> sheetPaths)
        {
            Application app = new Application();
            app.Visible = false;
            app.DisplayAlerts = false;
            foreach (string sheetPath in sheetPaths)
            {

                string sWithoutSuffix = "";
                int pos = sheetPath.IndexOf(".xls");
                if (pos == -1)
                    throw new Exception("Cannot find xls in file name!");
                sWithoutSuffix = sheetPath.Substring(0, pos);
                string sCSVFile = sWithoutSuffix + ".csv";
                if (File.Exists(sCSVFile))
                    continue;
                sCSVFile = sCSVFile.Replace("\\\\", "\\");
                Workbook wbWorkbook = app.Workbooks.Open(sheetPath);
                wbWorkbook.SaveAs(sCSVFile, XlFileFormat.xlCSV);
                wbWorkbook.Close();
                Console.WriteLine(sCSVFile);
            }
            app.Quit();
        }
    }
}
