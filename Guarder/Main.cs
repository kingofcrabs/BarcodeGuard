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
        FirstRoundPackageInfo firstRoundLayoutInfo = new FirstRoundPackageInfo();
        SecondRoundPackageInfo secondRoundLayoutInfo = new SecondRoundPackageInfo();
        PackageInfo packageInfo = new PackageInfo();
        int sampleCount = 0;
        bool is2TransferTubes = false;
        bool programModify = false;
        List<int> srcGrids = new List<int>();
        Dictionary<int, List<string>> eachGridBarcodes = new Dictionary<int, List<string>>();
        //Dictionary<int, List<string>> eachSrcGridRefBarcodes = new Dictionary<int, List<string>>();
        ErrorsInfo errorsInfo = new ErrorsInfo();
        string dummy = "***";
        public GuardForm(int sampleCnt, bool is2Transfer,int plasmaSlices, int productSlices)
        {
            InitializeComponent();
            Helper.WriteResult(false);
            this.is2TransferTubes = is2Transfer;
            lblVersion.Text = strings.version;
            sampleCount = sampleCnt;
            try
            {
                GetSettings(plasmaSlices,productSlices);
            }
            catch (Exception ex)
            {
                AddErrorInfo(ex.Message);
                return;
            }
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
            rdb2Transfer.Checked = is2TransferTubes;
            rdbResult.Checked = !is2TransferTubes;
            CreateNamedPipeServer();
            UpdateDataGridView();
        }

        private void GetSettings(int plasmaSlice,int productSlices)
        {
            string sFolder = ConfigurationManager.AppSettings["biobankingFolder"];
            if (!Directory.Exists(sFolder))
                throw new Exception("biobanking 文件夹找不到！");
            PipettingSettings pipettingSettings = null;
            LabwareSettings labwareSettings = null;
        
            Helper.LoadSettings(sFolder, ref pipettingSettings, ref labwareSettings);
            pipettingSettings.dstPlasmaSlice = plasmaSlice;

            if(is2TransferTubes)
            {
                firstRoundLayoutInfo.slices = pipettingSettings.dstPlasmaSlice;
                firstRoundLayoutInfo.srcStartGrid = labwareSettings.sourceLabwareStartGrid;
                firstRoundLayoutInfo.dstStartGrid = labwareSettings.dstLabwareStartGrid;
            }
            else
            {
                secondRoundLayoutInfo.srcSlices = pipettingSettings.dstPlasmaSlice;
                secondRoundLayoutInfo.dstSlices = productSlices;
                secondRoundLayoutInfo.srcStartGrid = int.Parse(ConfigurationManager.AppSettings["secondRoundSrcStartGrid"]);
                secondRoundLayoutInfo.dstStartGrid = int.Parse(ConfigurationManager.AppSettings["secondRoundDstStartGrid"]);
            }
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

            GetPackageInfo();
          
            for (int j = 0; j < packageInfo.totalColNum; j++)
                strs.Add("");
            
            int srcStartGrid = GetSrcStartGrid();
            ////src samples
            TubeType tubeType = is2TransferTubes ? TubeType.source : TubeType.transfter;

            for (int i = 0; i < packageInfo.srcSampleColNum; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}src", srcStartGrid + i);
                srcGrids.Add(srcStartGrid + i);
                column.HeaderCell.Style.BackColor = Color.LightSeaGreen;
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                eachGridBarcodes.Add(srcStartGrid + i,new List<string>() );
            }

            tubeType = is2TransferTubes ? TubeType.transfter : TubeType.dst;
            int dstStartGrid = GetDstStartGrid();
            for (int i = 0; i < packageInfo.dstColNum; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}dst", dstStartGrid + i);
                column.HeaderCell.Style.BackColor = Color.LightBlue;
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                eachGridBarcodes.Add(dstStartGrid + i,new List<string>());
            }

            dataGridView.RowHeadersWidth = 80;
            for (int i = 0; i < 16; i++)
            {
                dataGridView.Rows.Add(strs.ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("行{0}", i + 1);
            }
            ExportScanGrids(eachGridBarcodes.Keys.Select(x => x.ToString()).ToList());
            //string grids2Scan = Helper.GetExeFolder() + "grids.txt";
            //File.WriteAllLines(grids2Scan,eachGridBarcodes.Keys.Select(x=>x.ToString()).ToArray());
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

        private void GetPackageInfo()
        {
            packageInfo.srcSampleColNum = (int)Math.Ceiling(sampleCount / 16.0);
            packageInfo.srcSlices = is2TransferTubes ? 1 : secondRoundLayoutInfo.srcSlices;
            packageInfo.dstSlices = is2TransferTubes ? firstRoundLayoutInfo.slices : secondRoundLayoutInfo.dstSlices;
            packageInfo.totalColNum = packageInfo.srcSampleColNum * (packageInfo.srcSlices + packageInfo.dstSlices);
            packageInfo.srcSampleColNum *= packageInfo.srcSlices;
            packageInfo.dstColNum = packageInfo.totalColNum - packageInfo.srcSampleColNum;
        }

        //private List<string> CalculateBarcodes(int gridIndex, int slices, TubeType tubeType)
        //{
        //    int correspondingSrcSampleGridIndex = gridIndex / slices;
            
        //    List<string> barcodes = new List<string>();
        //    string prefix = "";
        //    string year = (DateTime.Now.Year % 100).ToString();
        //    switch(tubeType)
        //    {
        //        case TubeType.source:
        //            prefix = string.Format("{0}B", year);
        //            break;
        //        case TubeType.transfter:
        //            prefix = string.Format("{0}T", year);
        //            break;
        //        case TubeType.dst:
        //            prefix = string.Format("{0}P", year);
        //            break;
        //    }
            
        //    //int gridStartID = correspondingSrcSampleGridIndex * 16 + firstID;
        //    //string gridSliceDesc = "";
        //    //if (tubeType != TubeType.source)
        //    //{
        //    //    gridSliceDesc = string.Format("-{0}", gridIndex % slices + 1);
        //    //}
        //    //int remainingSrcSamples = sampleCount - correspondingSrcSampleGridIndex * 16;
        //    //remainingSrcSamples = Math.Min(remainingSrcSamples, 16);
        //    //string suffix = isRedo ? "R" : "";
        //    //for( int i = 0; i< remainingSrcSamples;i++)
        //    //{
        //    //    barcodes.Add(prefix + string.Format("{0:D7}", gridStartID + i) + gridSliceDesc + suffix);
        //    //}
        //    return barcodes;
        //}

        private int GetDstStartGrid()
        {
            return is2TransferTubes ? firstRoundLayoutInfo.dstStartGrid : secondRoundLayoutInfo.dstStartGrid;
        }

        private int GetSrcStartGrid()
        {
            return is2TransferTubes ? firstRoundLayoutInfo.srcStartGrid : secondRoundLayoutInfo.srcStartGrid;
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
                SaveBarcodeThisGrid(grid,barcodes);
                int indexInRegion = (grid - GetSrcStartGrid()) % packageInfo.srcSlices;
                if (indexInRegion == 0) //first slice
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
            int dstSlices = packageInfo.dstSlices;
            int regionIndex = (srcGrid - GetSrcStartGrid()) / packageInfo.srcSlices;
            for( int sliceIndex = 0; sliceIndex < dstSlices; sliceIndex++)
            {
                int dstGrid = GetDstStartGrid() + regionIndex * packageInfo.dstSlices + sliceIndex;
                eachGridBarcodes[dstGrid] = CalculdateCorrespondingBarcodes(sliceIndex, barcodes, bValidList);
                //UpdateThisGridExpectedBarcode(eachGridBarcodes[dstGrid],sliceIndex, barcodes, results);
            }
        }

        private void UpdateCertianDstExpectedBarcode(string barcode,int colIndex, int rowIndex)
        {
            int slices = packageInfo.dstSlices;
            //int regionIndex = colIndex / packageInfo.srcSlices;
            int curRegion = colIndex / packageInfo.srcSlices;
            int indexInRegion = colIndex % packageInfo.srcSlices;
            if (indexInRegion != 0)
                return;
            for (int sliceIndex = 0; sliceIndex < slices; sliceIndex++)
            {
                int dstGrid = GetDstStartGrid() + curRegion * packageInfo.dstSlices + sliceIndex;
                if (!eachGridBarcodes.ContainsKey(dstGrid))
                    continue;
                eachGridBarcodes[dstGrid][rowIndex] = CalculdateCorrespondingBarcode(barcode, true, sliceIndex);
                AddHintInfo(string.Format("刷新行{0}条{1}处的期望条码成{2}", rowIndex + 1, dstGrid, barcode), System.Drawing.Color.Blue);
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

        private List<string> CalculdateCorrespondingBarcodes(int sliceIndex, List<string> barcodes,List<bool> bValidList)
        {
            List<string> correspondingBarcodes = new List<string>();
            
            for (int i = 0; i < barcodes.Count; i++ )
            {
                correspondingBarcodes.Add(CalculdateCorrespondingBarcode(barcodes[i], bValidList[i], sliceIndex));
            }
            return correspondingBarcodes;
        }

        private string CalculdateCorrespondingBarcode(string srcBarcode, bool bValid, int sliceIndex)
        {
            if (!bValid)
            {
                //AddErrorInfo(string.Format("条码设置错误{0}，请重新设置", srcBarcode));
                return dummy;
            }
            string year = (DateTime.Now.Year % 100).ToString();
            string s = srcBarcode;
          
            string sID = s.Substring(3, 7);
            string sExpectedBarcode = string.Format("{0}P{1}", year, sID);
            if (s.Contains("R"))
                sExpectedBarcode += "R";
            if (s.Contains("D"))
                sExpectedBarcode += "D";
            string suffix = is2TransferTubes ? ((char)(sliceIndex + 'A')).ToString() : (sliceIndex + 1).ToString();
            sExpectedBarcode += "-" + suffix;
            return sExpectedBarcode;
        }

        private bool CheckSourceSampleBarcodes(int grid,
            List<string> barcodes,
            List<bool> checkResults,
            ref string errMsg)
        {
            
            bool bok = true;
            int correspondingSrcSampleGridIndex = (grid- GetSrcStartGrid()) / packageInfo.srcSlices;
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

        private bool IsValidSrcBarcode(int grid,int rowIndex, List<string> barcodes, ref string errMsg, bool isFromUI = false)
        {
            //B or P
            string BorP = is2TransferTubes ? "B" : "P";
            string year = (DateTime.Now.Year % 100).ToString();
            string prefixStr = year + BorP;

            int expectedLen = 10;
            string sCurrentBarcode = barcodes[rowIndex];
            if (sCurrentBarcode.Contains('-'))
                expectedLen += 2;
            if (sCurrentBarcode.Contains('R') || sCurrentBarcode.Contains('D'))
                expectedLen += 1;

            int indexInRegion = (grid - GetSrcStartGrid()) % packageInfo.srcSlices;
            if (indexInRegion != 0) //not first in region
            {
                if (eachGridBarcodes.ContainsKey(grid - indexInRegion))//compare to the first in the region
                {
                    string firstInRegionBarcode = eachGridBarcodes[grid - indexInRegion][rowIndex];
                    firstInRegionBarcode = firstInRegionBarcode.Substring(0, firstInRegionBarcode.Length - 1);
                    string removeLastCurrentBarcode = sCurrentBarcode.Substring(0, sCurrentBarcode.Length - 1);
                    if (firstInRegionBarcode != removeLastCurrentBarcode)
                    {
                        errMsg = string.Format("Grid{0}中第{1}个条码:{2}与Grid{3}中的同行的条码不匹配！",
                        grid,
                        rowIndex + 1,
                        sCurrentBarcode,
                        grid - indexInRegion);
                        return false;
                    }
                }
            }

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

            if (!sCurrentBarcode.StartsWith(prefixStr))
            {
                errMsg = string.Format("Grid{0}中第{1}个条码:{2}不符合规则！必须以‘{3}’开始",
                    grid,
                    rowIndex + 1, sCurrentBarcode, prefixStr);
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

            char expectedSuffix = (char)('A' + indexInRegion);
            if(sCurrentBarcode.Contains('P'))
            {
                char last = sCurrentBarcode.Last();
               
                if(last != expectedSuffix)
                {
                    errMsg = string.Format("Grid{0}中第{1}个样品为中转管，但是其条码{2}不以{3}结尾!",
                   grid, rowIndex + 1, sCurrentBarcode,expectedSuffix);
                    return false;
                }
            }
            
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
            for(int i = 0; i < expectedBarcodes.Count; i++)
            {
                string expectedBarcode = "";
                bool isValid = IsValidDstBarcode(barcodes[i], grid, i,ref expectedBarcode, ref errMsg);
                string tmpStr = isValid ? "" : errMsg;
                errorsInfo.SampleDescriptionCollection.Add(new SampleDescription(i + 1, barcodes[i], expectedBarcode,tmpStr, isValid));
                results.Add(isValid);
                bok = bok & isValid;
            }
            return bok;
        }

        private bool IsValidDstBarcode(string curBarcode, int dstGrid, int colIndex,ref string expectedBarcode, ref string errMsg)
        {
            expectedBarcode = dummy;
            if (eachGridBarcodes.ContainsKey(dstGrid) && eachGridBarcodes[dstGrid].Count() > colIndex)
                expectedBarcode = eachGridBarcodes[dstGrid][colIndex];
            if (expectedBarcode == dummy)
            {
                errMsg = string.Format("Grid: {0}第{1}个条码无法找到其原始管条码！", dstGrid, colIndex + 1);
                return false;
            }
            
            if (curBarcode != expectedBarcode)
            {
                errMsg = string.Format("Grid: {0}第{1}个条码为{2},应该为{3}！",
                    dstGrid, colIndex + 1, curBarcode, expectedBarcode);
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
                    UpdateCertianDstExpectedBarcode(actual,cell.ColumnIndex, cell.RowIndex);
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
