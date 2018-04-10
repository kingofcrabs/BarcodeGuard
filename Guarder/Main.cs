using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using Biobanking;
using System.Threading;

namespace Guarder
{
    public partial class GuardForm : Form
    {
        int sampleCount = 0;
        bool programModify = false;
        List<int> srcGrids = new List<int>();
        Dictionary<int, List<string>> eachGridBarcodes = new Dictionary<int, List<string>>();
        Dictionary<int, CheckInfo> eachGridCheckInfo = new Dictionary<int, CheckInfo>();
        ErrorsInfo errorsInfo = new ErrorsInfo();
        string dummy = "***";
        public GuardForm(int sampleCnt,string templateFile)
        {
            InitializeComponent();
            lblVersion.Text = strings.version;
            sampleCount = sampleCnt;
            TemplateReader templateReader = new TemplateReader();
            templateReader.GetCheckInfos(templateFile, ref srcGrids, ref eachGridCheckInfo);
            txtTemplate.Text = templateFile;
            
            this.Load += Main_Load;
            this.FormClosing += GuardForm_FormClosing;
            this.FormClosed += GuardForm_FormClosed;
        }

        void GuardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string folder = Helper.GetExeFolder() + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string file = folder + DateTime.Now.ToString("HHmmss") + ".csv";
            try
            {
                SaveBarcodes(dataGridView, file, sampleCount);
            }
            catch(Exception ex)
            {
                txtLog.Text = ex.Message + ex.StackTrace;
                e.Cancel = true;
            }
        }

         public void SaveBarcodes(DataGridView dataGridView,string sPath, int sampleCount)
        {
            string strLine = "";
            char comma = ',';
            List<string> contents = new List<string>();
            //Write in the headers of the columns.  
            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                if (i > 0)
                    strLine += comma;
                strLine += dataGridView.Columns[i].HeaderText;
            }
            strLine.Remove(strLine.Length - 1);
            contents.Add(string.Format("{0}{1}",comma,strLine));
            strLine = "";
            //Write in the content of the columns.  
            int lastColumnRows = sampleCount % 16;
            if (lastColumnRows == 0)
                lastColumnRows = 16;
            for (int j = 0; j < dataGridView.Rows.Count; j++)
            {
                strLine = string.Format("行{0},",j+1);
                for (int k = 0; k < dataGridView.Columns.Count; k++)
                {

                    if (k == dataGridView.Columns.Count - 1 && j >= lastColumnRows)
                        break;

                    if (k > 0)
                        strLine += comma;
                    if (dataGridView.Rows[j].Cells[k].Value == null)
                        strLine += "";
                    else
                    {
                        string m = dataGridView.Rows[j].Cells[k].Value.ToString().Trim();
                        strLine += m;
                    }
                }
                if (strLine != string.Empty)
                {
                    strLine.Remove(strLine.Length - 1);
                    contents.Add(strLine);
                }
            }
            File.WriteAllLines(sPath, contents.ToArray());
        }

        private int ParseID(string firstID)
        {
            string sID = firstID.Substring(3);
            sID = sID.TrimEnd('R');
            return int.Parse(sID);
        }

        void GuardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Pipeserver.Close();
            Helper.CloseWaiter(strings.NotifierName);
        }

        void Main_Load(object sender, EventArgs e)
        {
            txtSampleCount.Text = sampleCount.ToString();
         
            CreateNamedPipeServer();
            UpdateDataGridView();
            
        }

    

        private void AddErrorInfo(string txt)
        {
            richTextInfo.SelectionColor = Color.Red;
            richTextInfo.AppendText(txt+"\r\n");
        }

        private void AddInfo(string txt)
        {
            richTextInfo.SelectionColor = Color.Black;
            richTextInfo.AppendText(txt);
        }

        private void UpdateDataGridView()
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.Columns.Clear();
            List<string> strs = new List<string>();
            int neededSrcGrids = (sampleCount + 15) / 16;
            int colIndex = 0;
            List<int> dstGrids = new List<int>();
            srcGrids = srcGrids.Take(neededSrcGrids).ToList();
            Dictionary<int,int> dstGridID_SrcID = new Dictionary<int,int>();
            foreach (var gridID in srcGrids)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}src", gridID);
                var correspondingDsts = eachGridCheckInfo.Where(x=>x.Value.srcGrid == gridID && !x.Value.isSrc).Select(x=>x.Key).ToList();
                correspondingDsts.ForEach(x=>dstGridID_SrcID.Add(x,gridID));
                dstGrids.AddRange(correspondingDsts);
                column.HeaderCell.Style.BackColor = Color.LightSeaGreen;
                dataGridView.Columns.Add(column);
                dataGridView.Columns[colIndex++].SortMode = DataGridViewColumnSortMode.Programmatic;
                eachGridBarcodes.Add(gridID,new List<string>() );
                if (colIndex == neededSrcGrids)
                    break;
            }
            foreach(var dstGrid in dstGrids)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}dst[条{1}src]", dstGrid, dstGridID_SrcID[dstGrid]);
                column.HeaderCell.Style.BackColor = Color.LightBlue;
                dataGridView.Columns.Add(column);
                eachGridBarcodes.Add(dstGrid, new List<string>());
            }

            int totalColNum = dstGrids.Count + srcGrids.Count;
            for (int j = 0; j < totalColNum; j++)
                strs.Add("");
            int totalColumns = dstGrids.Count + srcGrids.Count;
            for (int i = 0; i < totalColumns; i++)
            {
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            dataGridView.RowHeadersWidth = 80;
            for (int i = 0; i < 16; i++)
            {
                dataGridView.Rows.Add(strs.ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("行{0}", i + 1);
            }
            ExportScanGrids(eachGridBarcodes.Keys.Select(x => x.ToString()).ToList());
        }

        private void ExportScanGrids(List<string> list)
        {
            string folder = Helper.GetExeFolder() + "Grids\\";
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string sGridsCount = folder + "gridsCount.txt";
            File.WriteAllText(sGridsCount, list.Count.ToString());
            for( int i = 0; i< list.Count; i++)
            {
                string sFile = folder + string.Format("{0}.txt", i + 1);
                File.WriteAllText(sFile, list[i].ToString());
            }
            Helper.CloseWaiter(strings.NotifierName);
        }

     


        private void btnClear_Click(object sender, EventArgs e)
        {
            richTextInfo.Clear();
        }

        private void RetryOrIgnore(bool isRetry)
        {
            string keyInfo = isRetry ? "按下重试键\r\n" : "按下忽略键\r\n";
            richTextInfo.SelectionColor = Color.Blue;
            richTextInfo.AppendText(keyInfo);
            Helper.CloseWaiter(strings.NotifierName);
            Helper.WriteRetryOrIgnore(isRetry);
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            RetryOrIgnore(false);
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            RetryOrIgnore(true);           
        }

        #region namedpipe
        private void CreateNamedPipeServer()
        {
            try
            {
                Pipeserver.owner = this;
                Pipeserver.ownerInvoker = new Invoker(this);
                ThreadStart pipeThread = new ThreadStart(Pipeserver.createPipeServer);
                Thread listenerThread = new Thread(pipeThread);
                listenerThread.SetApartmentState(ApartmentState.STA);
                listenerThread.IsBackground = true;
                listenerThread.Start();
            }catch(Exception ex)
            {
                AddErrorInfo(ex.Message);
            }
        }

        internal void ExecuteCommand(string sCommand)
        {
            if (sCommand.Contains("shutdown"))
            {
                this.Close();
                return;
            }
            if (sCommand != "")
            {
                //txtLog.AppendText(sCommand + "\r\n");
            }


            int grid = 0;
            List<string> barcodes = new List<string>();
            List<bool> results = new List<bool>();
            ReadBarcode(ref grid, barcodes);
            string errMsg = "";
            bool bok = false;
            try
            {
                bok = CheckBarcodes(grid, barcodes, results, ref errMsg);
                if(!bok)
                {
                    ShowErrorDialog(grid);
                }
            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
                bok = false;
                Helper.WriteResult(bok);
                return;
            }
            barcodes = barcodes.Take(results.Count).ToList();
            if (IsSourceSample(grid))
            {
                //if (!is2TransferTubes) //ref barcode only works with from transfer to dst
                //    eachSrcGridRefBarcodes[grid] = barcodes;
                if(srcGrids.Contains(grid))
                    UpdateAllDstExpectedBarcodes(grid,barcodes, results);
            }
            UpdateGridCells(grid, barcodes, results);
           
            if (bok)
            {
                Helper.CloseWaiter(strings.NotifierName);
                AddHintInfo(string.Format("Grid{0}条码检查通过！", grid), Color.DarkGreen);
            }
            else
                AddErrorInfo(errMsg);
            Helper.WriteResult(bok);
        }

        private void ShowErrorDialog(int grid)
        {
             SourceErrors sourceErrorForm = new SourceErrors(errorsInfo,grid);
             sourceErrorForm.ShowDialog();
        }

        private void SaveBarcodeThisGrid(int grid, List<string> barcodes)
        {
            if (eachGridBarcodes.ContainsKey(grid))
                eachGridBarcodes[grid] = barcodes;
            else
                eachGridBarcodes.Add(grid, barcodes);
        }

        private void UpdateAllDstExpectedBarcodes(
            int srcGrid,
            List<string> barcodes, 
            List<bool> bValidList)
        {
            List<CheckInfo> sameSrcGridCheckInfos = new List<CheckInfo>();
            foreach(var thisGridCheckInfo in eachGridCheckInfo)
            {
                if (thisGridCheckInfo.Value.isSrc)
                    continue;
                CheckInfo checkInfo = thisGridCheckInfo.Value;
                if(checkInfo.srcGrid == srcGrid)
                {
                    sameSrcGridCheckInfos.Add(checkInfo);
                }
            }

            foreach(var checkInfo in sameSrcGridCheckInfos)
            {
                int dstGrid = checkInfo.dstGrid;
                eachGridBarcodes[dstGrid] = CalculdateCorrespondingBarcodes(checkInfo, barcodes, bValidList);
            }
        }

        private void UpdateCertianDstExpectedBarcode(string barcode,int srcGrid, int rowIndex)
        {
            foreach(var checkInfo in eachGridCheckInfo.Values)
            {
                if(checkInfo.srcGrid == srcGrid)
                {
                    eachGridBarcodes[checkInfo.dstGrid][rowIndex] = CalculdateCorrespondingBarcode(checkInfo, barcode, true);
                }
                AddHintInfo(string.Format("刷新行{0}条{1}处的期望条码成{2}", rowIndex + 1, checkInfo.dstGrid, barcode), System.Drawing.Color.Blue);
            }
        }

        private bool IsSourceSample(int grid)
        {
            return srcGrids.Contains(grid);
        }
        private bool CheckBarcodes(int grid,List<string> barcodes,List<bool> results,ref string errMsg)
        {
            if(!eachGridBarcodes.ContainsKey(grid))
                throw new Exception(string.Format("当前扫描Grid{0}位置不在期望的试管载架中！", grid));

            bool bSourceSample = IsSourceSample(grid);
            errorsInfo.IsSource = bSourceSample;
            errorsInfo.SampleDescriptionCollection = new List<SampleDescription>();
         
            if (bSourceSample)
            {
                return CheckSourceSampleBarcodes(grid, barcodes, results, ref errMsg);
            }
            else
            {
                return CheckDstSampleBarcodes(grid, barcodes, results, ref errMsg);
            }
        }

        

        private void UpdateGridCells(int grid, List<string> barcodes, List<bool> checkResults)
        {
            programModify = true;
            for (int i = 0; i < barcodes.Count; i++)
            {
                int colIndex = eachGridBarcodes.Keys.ToList().FindIndex(x => x == grid);
                dataGridView.Rows[i].Cells[colIndex].Value = barcodes[i];
                dataGridView.Rows[i].Cells[colIndex].Style.BackColor = checkResults[i] ? Color.LightGreen : Color.Red;
                dataGridView.Rows[i].Cells[colIndex].ReadOnly = checkResults[i];
            }
            programModify = false;
        }

        private List<string> CalculdateCorrespondingBarcodes(CheckInfo checkInfo, List<string> barcodes,List<bool> bValidList)
        {
            List<string> correspondingBarcodes = new List<string>();
            
            for (int i = 0; i < barcodes.Count; i++ )
            {
                correspondingBarcodes.Add(CalculdateCorrespondingBarcode(checkInfo,barcodes[i], bValidList[i]));
            }
            return correspondingBarcodes;
        }

        private string CalculdateCorrespondingBarcode(CheckInfo checkInfo,string srcBarcode, bool bValid)
        {
            if (!bValid)
            {
                return dummy;
            }
            string s = srcBarcode;
            string year = s.Substring(0, 2);
            string sID = GetCompactBarcode(s,checkInfo); //can be changed later
            string bOrP = GetBOrP(checkInfo);
            string sExpectedBarcode = string.Format("{0}{1}{2}", year,bOrP, sID);
            sExpectedBarcode += checkInfo.suffix;
            return sExpectedBarcode;
        }

        private string GetCompactBarcode(string s, CheckInfo checkInfo)
        {
            string sID = s.Substring(3);
            if(checkInfo.suffix != "")
                sID = sID.Replace(checkInfo.suffix, "");
            return sID;
        }

        private string GetBOrP(CheckInfo checkInfo)
        {
            if (checkInfo.BorPDesc == BorPDesc.Nothing)
                return "";
            if (checkInfo.BorPDesc == BorPDesc.Blood)
                return "B";
            return "P";
        }

        private bool CheckSourceSampleBarcodes(int grid,
            List<string> barcodes,
            List<bool> checkResults,
            ref string errMsg)
        {
            
            bool bok = true;
            int correspondingSrcSampleGridIndex = FindGridIndex(grid); // (grid - GetSrcStartGrid()) / packageInfo.srcSlices;
            
            int remainingSrcSamples = sampleCount - correspondingSrcSampleGridIndex * 16;
            int thisGridSample = Math.Min(remainingSrcSamples, 16);
             
            for (int i = 0; i < thisGridSample; i++)
            {
                bool bValidSrcBarcode = IsValidSrcBarcode(grid,i,barcodes,ref errMsg);
                checkResults.Add(bValidSrcBarcode);
                string tmpStr = bValidSrcBarcode ? "" : errMsg;
                errorsInfo.SampleDescriptionCollection.Add(new SampleDescription(i + 1, barcodes[i],"", tmpStr, bValidSrcBarcode));
                bok = bok & bValidSrcBarcode;
            }
            return bok;
        }

        private int FindGridIndex(int grid)
        {
            for(int i = 0; i< srcGrids.Count; i++)
            {
                if (srcGrids[i] == grid)
                    return i;
            }
            throw new Exception(string.Format("找不到Grid{0}!", grid));
        }

        private bool IsValidSrcBarcode(int grid,int rowIndex, List<string> barcodes, ref string errMsg, bool isFromUI = false)
        {
            CheckInfo checkInfo = eachGridCheckInfo.Where(x => x.Value.srcGrid == grid && x.Value.isSrc).First().Value;
            //B or P
            string BorP = "";
            if (checkInfo.BorPDesc == BorPDesc.Blood)
                BorP = "B";
            else if (checkInfo.BorPDesc == BorPDesc.Plasma)
                BorP = "P";
            
            string year = (DateTime.Now.Year % 100).ToString();
            string prefixStr = year + BorP;
            string prefixStrLast = ((DateTime.Now.Year-1) % 100).ToString() + BorP;

            int expectedLen = 10;
            string sCurrentBarcode = barcodes[rowIndex];
            if (sCurrentBarcode.Contains('-'))
                expectedLen += 2;
            if (sCurrentBarcode.Contains('R') || sCurrentBarcode.Contains('D'))
                expectedLen += 1;


            if(isFromUI) //check all barcodes
            {
                for(int i = 0;  i< barcodes.Count; i++)
                {
                    if (i == rowIndex)
                        continue;
                    if (barcodes[i] == sCurrentBarcode)
                    {
                        errMsg = string.Format("Grid{0}中第{1}个条码:{2}重复！",
                            grid,
                            rowIndex + 1, sCurrentBarcode);
                        return false;
                    }
                }

            }

            List<string> aheadBarcodes = barcodes.Take(rowIndex).ToList();
            foreach(var pair in eachGridBarcodes)
            {
                int tmpGrid = pair.Key;
                var tmpBarcode = pair.Value;
                if (tmpGrid >= grid)
                    continue;
                if(tmpBarcode.Contains(sCurrentBarcode))
                {
                    errMsg = string.Format("Grid{0}中第{1}个条码:{2}在Grid{3}中已经存在！",
                    grid,
                    rowIndex + 1,
                    sCurrentBarcode,
                    tmpGrid);
                    return false;
                }
            }

            if (aheadBarcodes.Contains(sCurrentBarcode))
            {
                errMsg = string.Format("Grid{0}中第{1}个条码:{2}重复！",
                    grid,
                    rowIndex + 1, sCurrentBarcode);
                return false;
            }

            //var tstPrefix = sCurrentBarcode.Substring(2);
            if (!sCurrentBarcode.StartsWith(prefixStr) && (!sCurrentBarcode.StartsWith(prefixStrLast)))
            {
                errMsg = string.Format("Grid{0}中第{1}个条码:{2}不符合规则！必须以‘{3}’或‘{4}’开始",
                    grid,
                    rowIndex + 1, sCurrentBarcode, prefixStr,prefixStrLast);
                return false;
            }
          

            if (sCurrentBarcode.Length != expectedLen)
            {
                errMsg = string.Format("Grid{0}中第{1}个条码:{2}长度为{3}，应为{4}，",
                    grid, rowIndex + 1, sCurrentBarcode, sCurrentBarcode.Length, expectedLen);
                return false;
            }

            if (sCurrentBarcode.Contains('P') && !sCurrentBarcode.Contains('-'))
            {
                errMsg = string.Format("Grid{0}中第{1}个样品为中转管，但是其条码{2}不含有'-'!",
                    grid, rowIndex + 1, sCurrentBarcode);
                return false;
            }

            //char expectedSuffix = (char)('A' + indexInRegion);
            //if(sCurrentBarcode.Contains('P'))
            //{
            //    char last = sCurrentBarcode.Last();
               
            //    if(last != expectedSuffix)
            //    {
            //        errMsg = string.Format("Grid{0}中第{1}个样品为中转管，但是其条码{2}不以{3}结尾!",
            //       grid, rowIndex + 1, sCurrentBarcode,expectedSuffix);
            //        return false;
            //    }
            //}
            
            string sub = sCurrentBarcode.Substring(3, 7);
            int digitalCount = sub.Count(x => Char.IsDigit(x));
            if (digitalCount != 7)
            {
                errMsg = string.Format("Grid{0}中第{0}个条码:{1}不符合规则，没有7位ID！",
                    grid,
                    rowIndex + 1, sCurrentBarcode);
                return false;
            }

            //int regionIndex = (grid - GetSrcStartGrid()) / packageInfo.srcSlices;
            //int refGrid = GetSrcStartGrid() + regionIndex * packageInfo.srcSlices;
            //if (eachSrcGridRefBarcodes.ContainsKey(refGrid))
            //{
            //    var tmpRef = eachSrcGridRefBarcodes[refGrid][rowIndex];
            //    var refBarcodeWithoutSuffix = tmpRef.Remove(tmpRef.Length - 1);
            //    string expectedBarcode = string.Format("{0}{1}", refBarcodeWithoutSuffix, expectedSuffix);
            //    bool bok = expectedBarcode == barcodes[rowIndex];
            //    if (!bok)
            //        errMsg = string.Format("Grid{0}中第{1}个条码希望是{2}，实际却是{3}", grid, rowIndex + 1, expectedBarcode, barcodes[rowIndex]);
            //    return bok;
            //}
            return true;
        }

        private bool CheckDstSampleBarcodes(int grid, List<string> barcodes, List<bool> results, ref string errMsg)
        {
            List<string> expectedBarcodes = eachGridBarcodes[grid];
            if (expectedBarcodes.Count == 0)
                throw new Exception(string.Format("无法找到Grid:{0}期望的条码！", grid));
            bool bok = true;
            var index = barcodes.IndexOf(dummy);
            if( index != -1)
            {
                string tmpExpectedBarcode = "";
                string tmpErrMsg = "";
                bool isValid = IsValidDstBarcode(barcodes[index], grid, index, ref tmpExpectedBarcode, ref tmpErrMsg);
                AddHintInfo("*** string found, look at its errMessage: " + tmpErrMsg, Color.DarkRed);
            }


            for(int i = 0; i < expectedBarcodes.Count; i++)
            {
                string expectedBarcode = "";
                bool isValid = IsValidDstBarcode(barcodes[i], grid, i,ref expectedBarcode, ref errMsg);
                string tmpStr = isValid ? "" : errMsg;
                errorsInfo.SampleDescriptionCollection.Add(new SampleDescription(i + 1, barcodes[i], expectedBarcode,tmpStr, isValid));
                results.Add(isValid);
                if (!isValid)
                    bok = false;
            }
            return bok;
        }

        private bool IsValidDstBarcode(string curBarcode, int dstGrid, int rowIndex,ref string expectedBarcode, ref string errMsg)
        {
            expectedBarcode = dummy;
            if (eachGridBarcodes.ContainsKey(dstGrid) && eachGridBarcodes[dstGrid].Count() > rowIndex)
                expectedBarcode = eachGridBarcodes[dstGrid][rowIndex];
            if (expectedBarcode == dummy)
            {
                errMsg = string.Format("Grid: {0}第{1}个条码无法找到其原始管条码！", dstGrid, rowIndex + 1);
                return false;
            }
            
            if (curBarcode != expectedBarcode)
            {
                errMsg = string.Format("Grid: {0}第{1}个条码为{2},应该为{3}！",
                    dstGrid, rowIndex + 1, curBarcode, expectedBarcode);
                return false;
            }
            return true;
        }

      

       
        private void UpdateGridCell(int gridID, int i, string expectedBarcode, string actualBarcode)
        {
            programModify = true;
            bool isEqual = expectedBarcode == actualBarcode;
            int colIndex = eachGridBarcodes.Keys.ToList().FindIndex(x => x == gridID);
            dataGridView.Rows[i].Cells[colIndex].Value = actualBarcode;
            dataGridView.Rows[i].Cells[colIndex].Style.BackColor = isEqual ? Color.LightGreen : Color.Red;
            if (isEqual)
                dataGridView.Rows[i].Cells[colIndex].ReadOnly = true;
            programModify = false;
        }

        private void ReadBarcode(ref int grid, List<string> barcodes)
        {
            string posIDFile = ConfigurationManager.AppSettings["posIDFile"];
            List<string> contents = File.ReadAllLines(posIDFile).ToList();
            contents = contents.Where(x => x != "").ToList();
            if (contents.Count() != 17)
                throw new Exception("条码文件行数不是17！");
            string firstLine = contents[1];
            string[] strs = firstLine.Split(';');
            grid = int.Parse(strs[0]);
            barcodes.Clear();
            contents.RemoveAt(0);
            foreach(string s in contents)
            {
                barcodes.Add(Parse(s));
            }
            txtLog.AppendText(string.Format("{0} Scan grid: {1}\r\n", DateTime.Now.ToLongTimeString(), grid));
        }

        private string Parse(string s)
        {
            string[] strs = s.Split(';');
            return strs.Last();
        }
        #endregion
        private void dataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            if (eachGridBarcodes == null
                || eachGridBarcodes.Count == 0)
                return;
            var curCell = dataGridView.CurrentCell;
            int grid = eachGridBarcodes.Keys.ElementAt(curCell.ColumnIndex);
            bool isSourceGrid = IsSourceSample(grid);
            string errMsg = "";
            if (eachGridBarcodes[grid].Count == 0)
                return;
            bool bok = true;
            try
            {
                bok = IsValidBarcode(grid, curCell.RowIndex, eachGridBarcodes[grid], ref errMsg, isSourceGrid,true);
            }
            catch(Exception ex)
            {
                return;
            }
            if (!bok)
            {
                dataGridView.BeginEdit(false);
            }
            else
            {
                dataGridView.EndEdit();
            }
        }
        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (programModify)
                return;
            if (e.ColumnIndex == -1)
                return;
            if (e.ColumnIndex >= eachGridBarcodes.Keys.Count)
                return;
            int grid = eachGridBarcodes.Keys.ElementAt(e.ColumnIndex);
            var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string actual = cell.Value.ToString();
            List<string> curColBarcodes = new List<string>(eachGridBarcodes[grid]);
            SaveBarcodeThisCell(actual, cell.RowIndex, curColBarcodes);
            //SaveBarcodeThisCell(actual, cell.RowIndex, eachGridBarcodes[grid]);
            bool bok = false;
            string errMsg = "";
            bool isSourceGrid = IsSourceSample(grid);
            bok = IsValidBarcode(grid, e.RowIndex, curColBarcodes, ref errMsg, isSourceGrid,true);
            if(bok)
            {
                cell.Style.BackColor = Color.Orange;
                cell.ReadOnly = true;
                string hint = string.Format("Grid:{0} 第{1}个样品管的条码得到修复。"
                    , grid, e.RowIndex + 1);
                if (isSourceGrid)
                {
                    UpdateCertianDstExpectedBarcode(actual, grid, cell.RowIndex);
                }
                SaveBarcodeThisCell(actual, cell.RowIndex, eachGridBarcodes[grid]);
                AddHintInfo(hint, Color.Orange);
            }
            else
            {
                AddErrorInfo("修复失败，原因是： " + errMsg);
            }
        }

        private void SaveBarcodeThisCell(string actual, int rowIndex, List<string> list)
        {
            list[rowIndex] = actual;
        }



        bool IsValidBarcode(int grid, int rowIndex, List<string>barcodes, ref string errMsg,bool isSourceGrid, bool isFromUI = false)
        {
            bool bok = false;
            string actual = barcodes[rowIndex];
 	        if (isSourceGrid)
            {
                bok = IsValidSrcBarcode(grid, rowIndex, barcodes, ref errMsg, isFromUI);
            }
            else
            {
                string expectedBarcode = "";
                bok = IsValidDstBarcode(actual, grid, rowIndex,ref expectedBarcode, ref errMsg);
            }
            return bok;

        } 
        
        void AddHintInfo(string hint, Color color)
        {
            richTextInfo.SelectionColor = color;
            richTextInfo.AppendText(hint+"\r\n");
        }

      
    }

    class LayoutInfoBase
    {
        public int srcStartGrid;
        public int dstStartGrid;
    }
    enum TubeType
    {
        source = 0,
        transfter = 1,
        dst = 2
    }
    class FirstRoundPackageInfo : LayoutInfoBase
    {
        public int slices;
    }

    class SecondRoundPackageInfo : LayoutInfoBase
    {
        public int srcSlices;
        public int dstSlices;
    }

    class PackageInfo
    {
         public int srcSampleColNum;
         public int srcSlices;
         public int dstSlices;
         public int totalColNum;
         public int dstColNum;
    }
}
