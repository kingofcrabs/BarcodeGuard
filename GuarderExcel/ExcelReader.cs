﻿using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Guarder
{
    class ExcelReader
    {
        const int plateBarcodeStartLine = 7;
        const string suffix = ".xlsm";
        static public void ReadBarcodes()
        {
            try
            {
                ReadBarcodesImpl();
            }
            catch(Exception ex)
            {
                throw new Exception("Excel格式错误，可能板子数量不够！");
            }
            
        }

        private static void ReadBarcodesImpl()
        {
            string sFolder = GlobalVars.Instance.ExcelFolder;
            var di = new DirectoryInfo(sFolder);
            var last = di.EnumerateFiles("*" + suffix)
                                .OrderBy(d => d.CreationTime)
                                .Last();
            string sFilePath = last.FullName;
            int plateCnt = GlobalVars.Instance.PlateCnt;
            string csvFilePath = sFilePath.Replace(suffix, ".csv");
            if (File.Exists(csvFilePath))
                File.Delete(csvFilePath);
            SaveAsCSV(new List<string>() { sFilePath });
            List<string> lines = File.ReadAllLines(csvFilePath).ToList();
            GlobalVars.Instance.eachGridExpectedBarcodes.Clear();
            GlobalVars.Instance.eachPlateExpectedBarcodes.Clear();
            int curGrid = GlobalVars.Instance.StartGridID;
            for (int i = 0; i < GlobalVars.Instance.PlateCnt; i++)
            {
                int startLine = i * 10 + plateBarcodeStartLine - 1;

                string barcodeLine = lines[startLine];
                string barcode = ParsePlateBarcode(barcodeLine);
                GlobalVars.Instance.eachPlateExpectedBarcodes.Add(barcode);
                GlobalVars.Instance.PlateBarcodes.Add(barcode);
                List<string> thisPlateBarcodes = ParseWellBarcodes(lines, startLine);
                while (thisPlateBarcodes.Count > 0)
                {
                    var thisGridBarcodes = thisPlateBarcodes.Take(16).ToList();
                    GlobalVars.Instance.eachGridExpectedBarcodes.Add(curGrid++, thisGridBarcodes);
                    thisPlateBarcodes = thisPlateBarcodes.Skip(thisGridBarcodes.Count).ToList();
                }

            }
        }

        private static List<string> ParseWellBarcodes(List<string> lines, int startLine)
        {
            List<string> barcodes = new List<string>();
            Dictionary<int, string> pos_barcodes = new Dictionary<int, string>();
            for(int r = 0; r < 8; r++)
            {
                int lineNum = startLine + r + 2;
                string theLine = lines[lineNum];
                List<string> strs = theLine.Split(',').ToList();
                for(int col = 0; col < 12; col++)
                {
                    string barcode = strs[col + 1];
                    if (barcode == "")
                        break;
                    pos_barcodes.Add(GetWellID(col, r), barcode);
                }
            }
            for(int i = 1; i<= 96; i++)
            {
                if (pos_barcodes.ContainsKey(i))
                    barcodes.Add(pos_barcodes[i]);
                else
                    break;
            }
            return barcodes;
        }

        private static int GetWellID(int x, int y)
        {
            return x * 8 + y + 1;
        }

        private static string ParsePlateBarcode(string barcodeLine)
        {
            string[] strs = barcodeLine.Split(',');
            return strs[2];
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
