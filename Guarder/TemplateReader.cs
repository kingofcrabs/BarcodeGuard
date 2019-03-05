using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Guarder
{
    class TemplateReader
    {

        HashSet<string> foundBarcodes = new HashSet<string>();
        Dictionary<string, CheckInfo> barcode_SrcGrid = new Dictionary<string, CheckInfo>();
        public void GetCheckInfos(string file, ref List<int> srcGrids, ref Dictionary<int, CheckInfo> eachGridCheckInfo)
        {
            HashSet<int> srcHashGrids = new HashSet<int>();
            eachGridCheckInfo = ReadEachGridCheckInfo(file);
            foreach (var thisGridCheckInfo in eachGridCheckInfo)
            {
                if (thisGridCheckInfo.Value.isSrc)
                {
                    srcHashGrids.Add(thisGridCheckInfo.Key);
                }
            }
            srcGrids = srcHashGrids.ToList();
        }
        private Dictionary<int, CheckInfo> ReadEachGridCheckInfo(string file)
        {
            List<string> strs = File.ReadAllLines(file).ToList();
            Dictionary<int,int> eachColumnGridID = ParseEachColumnGridID(strs[0]);
            Dictionary<Position, CheckInfo> eachPositionCheckInfo = new Dictionary<Position, CheckInfo>();
            barcode_SrcGrid.Clear();

            string sGridInfo = strs[0];
            string sCheckInfo = strs[1];
            Dictionary<int, int> eachCol_Grid = GetGridInfo(sGridInfo);
            Dictionary<int, CheckInfo> eachGridCheckInfo = new Dictionary<int, CheckInfo>();
            string[] subStrs = sCheckInfo.Split(',');
            for (int i = 1; i < subStrs.Length; i++ )
            {
                int gridID = eachCol_Grid[i];
                CheckInfo checkInfo = ParseStr(subStrs[i], gridID);
                eachGridCheckInfo.Add(gridID,checkInfo);
            }
            return eachGridCheckInfo;
        }

        private CheckInfo ParseStr(string s,int thisGridID)
        {
            string srcStr = string.Format("grid{0}", thisGridID);
            bool isSrcGrid = s.Contains(srcStr);
            s = s.ToLower();
            bool hasYearPrefix = s.Contains("yy");
            BorPDesc bOrPDesc = BorPDesc.Nothing;
            if(s.Contains("bp"))
                bOrPDesc = BorPDesc.BloodOrPlasma;
            else if (s.Contains("b"))
                bOrPDesc = BorPDesc.Blood;
            else if(s.Contains("p"))
                bOrPDesc = BorPDesc.Plasma;
            int dashPos = s.IndexOf("-");
            string suffix = "";
            if(dashPos != -1)
                suffix = s.Substring(dashPos);
            string sSrcGrid = s.Replace("yybgrid","");
            sSrcGrid = sSrcGrid.Replace("yypgrid", "");
            if(suffix != "")
                sSrcGrid = sSrcGrid.Replace(suffix, "");
            int srcGrid = int.Parse(sSrcGrid);
            return new CheckInfo(isSrcGrid, hasYearPrefix, bOrPDesc,srcGrid, thisGridID, suffix.ToUpper());
        }

        private Dictionary<int, int> GetGridInfo(string sGridInfo)
        {
            Dictionary<int, int> eachColGrid = new Dictionary<int, int>();
            string[] subStrs = sGridInfo.Split(',');
            for(int i = 1; i < subStrs.Count(); i++)
            {
                if (subStrs[i] == "")
                    continue;
                else
                {
                    string tempS = subStrs[i];
                    tempS = tempS.Replace("grid", "");
                    int gridID = int.Parse(tempS);
                    eachColGrid.Add(i, gridID);
                }
            }
            return eachColGrid;
        }

        private string GetSuffix(string content)
        {
            int index = content.IndexOf("-");
            if(index != -1)
                return content.Substring(index);
            return "";
        }

        private bool IsSrcGrid(string content)
        {
            content = content.Replace("yyB", "");
            content = content.Replace("yyP", "");
            content = content.Replace("grid", "");
            int val;
            bool bok = int.TryParse(content, out val);
            if (!bok)
                return false;
            else
            {
                bool bfound = foundBarcodes.Contains(content);
                foundBarcodes.Add(content);
                return !bfound;
            }
        }

        
        private Dictionary<int, int> ParseEachColumnGridID(string s)
        {
            Dictionary<int, int> eachColumnGridID = new Dictionary<int, int>();
            string[] strs = s.Split(',');
            for (int i = 0; i < strs.Length; i++ )
            {
                if (strs[i] == "")
                {
                    continue;
                }
                else
                {
                    string sGrid = strs[i].ToLower().Replace("grid", "");
                    int gridID = int.Parse(sGrid);
                    eachColumnGridID.Add(i, gridID);
                }

            }
            return eachColumnGridID;
        }

    }

 

    public enum BorPDesc
    {
        Blood,
        Plasma,
        BloodOrPlasma,
        Nothing
    }

    public struct Position
    {
        public int gridID;
        public int rowID;
        public Position(int gridID, int rowID)
        {
            this.gridID = gridID;
            this.rowID = rowID;
        }
    }
}
