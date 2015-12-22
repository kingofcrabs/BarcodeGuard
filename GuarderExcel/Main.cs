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
using System.Threading;
using GuarderExcel;

namespace Guarder
{
    public partial class GuardForm : Form
    {

        List<ErrorInfo> errorsInfo = new List<ErrorInfo>();
        int sampleCnt = 0;
        bool programModify = false;
        public GuardForm()
        {
            InitializeComponent();
            this.Load += Main_Load;
            this.FormClosing += GuardForm_FormClosing;
            this.FormClosed += GuardForm_FormClosed;
        }

        void GuardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string folder = Folders.GetOutputFolder() + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string file = folder + DateTime.Now.ToString("HHmmss") + ".csv";
            try
            {
                if(sampleCnt != 0)
                    SaveBarcodes(dataGridView, file, sampleCnt);
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

      

        void GuardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Pipeserver.Close();
            Helper.CloseWaiter(strings.NotifierName);
        }

        void Main_Load(object sender, EventArgs e)
        {
            lblVersion.Text = "版本号：" + strings.version;
            CreateNamedPipeServer();
            Helper.WriteRetryOrIgnore(false);
            dataGridView.Visible = false;
            lstMP3Barcode.DrawMode = DrawMode.OwnerDrawFixed;
            lstMP3Barcode.DrawItem += lstMP3Barcode_DrawItem;
            lstMP3Barcode.SelectedIndexChanged += LstMP3Barcode_SelectedIndexChanged;
        }

        private void LstMP3Barcode_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void lstMP3Barcode_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush myBrush = Brushes.Black;
            if (e.Index < 0)
                return;
            string allText = lstMP3Barcode.Items[e.Index].ToString();
            string s2Show = "";
            if(allText != "") 
            {
                string[] contents = allText.Split(':');
                s2Show = contents[0];
                bool isError = contents[1] == "e";
                myBrush = isError ? Brushes.Red : Brushes.Green;
            
            }
           
            e.DrawBackground();
            Brush backGroundBrush = Brushes.White;
            if (e.Index == lstMP3Barcode.SelectedIndex)
            {
                backGroundBrush = Brushes.Wheat;
            }
            e.Graphics.FillRectangle(backGroundBrush, e.Bounds);
            e.DrawFocusRectangle();//焦点框          
            e.Graphics.DrawString(s2Show, e.Font,
               myBrush, e.Bounds, StringFormat.GenericDefault);
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

        private void InitDataGridView(int gridCnt)
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.Columns.Clear();
            int startGrid = GlobalVars.Instance.StartGridID;
            List<string> strs = new List<string>();

            for (int i = 0; i < gridCnt; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}", startGrid + i);
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                strs.Add("");
            }
            dataGridView.RowHeadersWidth = 80;
            for (int i = 0; i < 16; i++)
            {
                dataGridView.Rows.Add(strs.ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("行{0}", i + 1);
            }
            dataGridView.Visible = true;
        }

        public void UpdateDataGridView(int gridID, List<string> barcode)
        {
            for (int i = 0; i < barcode.Count; i++)
            {
                int col = gridID - GlobalVars.Instance.StartGridID;
                var cell = dataGridView.Rows[i].Cells[col];
                cell.Value = barcode[i];
                System.Drawing.Color foreColor = System.Drawing.Color.Green;
                string curBarcode = barcode[i];
                bool bEqual = curBarcode == GlobalVars.Instance.eachGridExpectedBarcodes[gridID][i];
                foreColor = bEqual ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Red;
                cell.Style.ForeColor = foreColor;
            }
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
            Helper.WriteRetryOrIgnore(isRetry);
            Helper.CloseWaiter(strings.NotifierName);
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
            try
            {
                ExecuteCommandImpl(sCommand);
            }
            catch(Exception ex)
            {
                AddErrorInfo("Error happened in execute command: " + ex.Message);
            }
            
            
        }

        private void ExecuteCommandImpl(string sCommand)
        {
            if (sCommand.Contains("shutdown"))
            {
                this.Close();
                return;
            }

            if (sCommand != "")
            {
                txtLog.AppendText(sCommand + "\r\n");
            }
            
            int grid = 0;
            List<string> barcodes = new List<string>();
            List<bool> results = new List<bool>();
            ReadBarcode(ref grid, barcodes);
            bool bok = false;
            if (grid == GlobalVars.Instance.PlateStartGridID)
            {
                errorsInfo.Clear();
                lstMP3Barcode.Items.Clear();
                for (int i = 0; i < GlobalVars.Instance.PlateCnt; i++)
                {
                    string expectedBarcode = GlobalVars.Instance.eachPlateExpectedBarcodes[i];
                    string actualBarcode = barcodes[i];
                    bok = expectedBarcode == actualBarcode;
                    string errMsg = bok ? "" : "条码不匹配！";
                    errorsInfo.Add(new ErrorInfo(i + 1, actualBarcode, expectedBarcode, errMsg, bok));
                    string suffix = bok ? ":r" : ":e";
                    lstMP3Barcode.Items.Add(actualBarcode+suffix);
                }
                if (lstMP3Barcode.Items.Count > 0)
                    lstMP3Barcode.SelectedIndex = 0;
            }
            else
            {
                //check grid is valid
                if (!GlobalVars.Instance.eachGridExpectedBarcodes.ContainsKey(grid))
                {
                    string errMsg = string.Format("当前扫描Grid{0}位置不在期望的试管载架中！", grid);
                    AddErrorInfo(errMsg);
                    Helper.WriteResult(false);
                }
                //update no check
                UpdateGridCells(grid, barcodes);
                //check ok, if not ok, show concrete information.
                bok = CheckBarcodes(grid, barcodes, results);
                //update ui
                UpdateGridCells(grid, barcodes, results);
            }


            if (bok)
            {
                Helper.CloseWaiter(strings.NotifierName);
                AddHintInfo(string.Format("Grid{0}条码检查通过！", grid), Color.DarkGreen);
            }
            else
            {
                ShowErrorDialog(grid);
                AddErrorInfo(GetLatestError());
            }
            Helper.WriteResult(bok);
        }

        private string GetLatestError()
        {
            ErrorInfo errInfo = errorsInfo.Last(x => !x.IsCorrect);
            if(errInfo == null)
                return "未知错误！";
            return string.Format("位于{0}行条码错误：期望条码{1}，实际条码{2}", errInfo.LineNumber, errInfo.ExpectedBarcode, errInfo.Barcode);
        }

        private bool CheckBarcodes(int grid,List<string> barcodes, List<bool> results)
        {
            errorsInfo.Clear();
            for(int i = 0; i< GlobalVars.Instance.eachGridExpectedBarcodes[grid].Count ; i++)
            {
                bool bok = IsValidBarcode(grid,i);
                results.Add(bok);

                string actualBarcode = barcodes[i];
                string expectedBarcode = GlobalVars.Instance.eachGridExpectedBarcodes[grid][i];
                string errMsg = bok ? "" : "条码不匹配！";
                errorsInfo.Add(new ErrorInfo(i+1,actualBarcode,expectedBarcode,errMsg,bok));
            }
            return !results.Contains(false);
        }

        private void ShowErrorDialog(int grid)
        {
             SourceErrors sourceErrorForm = new SourceErrors(errorsInfo,grid);
             sourceErrorForm.ShowDialog();
        }

        private void UpdateGridCells(int grid, List<string> barcodes, List<bool> checkResults = null)
        {
            programModify = true;
            for (int i = 0; i < barcodes.Count; i++)
            {
                int colIndex = GlobalVars.Instance.eachGridExpectedBarcodes.Keys.ToList().FindIndex(x => x == grid);
                dataGridView.Rows[i].Cells[colIndex].Value = barcodes[i];
                if(checkResults != null && i < checkResults.Count )
                {
                    dataGridView.Rows[i].Cells[colIndex].Style.BackColor = checkResults[i] ? Color.LightGreen : Color.Red;
                    dataGridView.Rows[i].Cells[colIndex].ReadOnly = checkResults[i];
                }
            }
            programModify = false;
        }

        private void ReadBarcode(ref int grid, List<string> barcodes)
        {
            string posIDFile = ConfigurationManager.AppSettings["posIDFile"];
            if(!File.Exists(posIDFile))
            {
                throw new Exception("载架扫描失败！");
            }

            List<string> contents = File.ReadAllLines(posIDFile).ToList();
            contents = contents.Where(x => x != "").ToList();
            
            string firstLine = contents[1];
            string[] strs = firstLine.Split(';');
            grid = int.Parse(strs[0]);
            barcodes.Clear();
            contents.RemoveAt(0);
            bool isPlateGrid = GlobalVars.Instance.PlateStartGridID == grid;
            if(isPlateGrid)
            {
                if (contents.Count > 3)
                    throw new Exception(string.Format("条码行数为{0}，不等于3！", contents.Count));
            }
            else
            {
              if (contents.Count() != 16)
                throw new Exception(string.Format("条码行数为{0},不等于16！",contents.Count));      
             }
            
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


        bool IsValidBarcode(int gridID, int row)
        {
            int gridIndex = gridID - GlobalVars.Instance.StartGridID;
            string curCellText = dataGridView.Rows[row].Cells[gridIndex].Value.ToString().Trim();
            string expectedBarcode = GlobalVars.Instance.eachGridExpectedBarcodes[gridID][row];
            return curCellText == expectedBarcode;
        }
        private void dataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            bool bok = false;
            try
            {
                var curCell = dataGridView.CurrentCell;
                int grid = GlobalVars.Instance.eachGridExpectedBarcodes.Keys.ElementAt(curCell.ColumnIndex);
                bok = IsValidBarcode(grid, curCell.RowIndex);
            }
            catch (Exception ex)
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
            if (e.ColumnIndex >= GlobalVars.Instance.eachGridExpectedBarcodes.Keys.Count)
                return;
            int grid = GlobalVars.Instance.eachGridExpectedBarcodes.Keys.ElementAt(e.ColumnIndex);
            var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string actual = cell.Value.ToString();
            bool bok = IsValidBarcode(grid, e.RowIndex);
            if (bok)
            {
                cell.Style.BackColor = Color.Orange;
                cell.ReadOnly = true;
                string hint = string.Format("Grid:{0} 第{1}个样品管的条码得到修复。"
                    , grid, e.RowIndex + 1);
                AddHintInfo(hint, Color.Orange);
            }
            else
            {
                AddErrorInfo(string.Format("修复失败，期望条码为：{0}, 实际条码为{1}。",
                    GlobalVars.Instance.eachGridExpectedBarcodes[grid][e.RowIndex],
                    cell.Value.ToString()));
            }
        }

        bool IsValidBarcode(int grid, int rowIndex, List<string>barcodes, ref string errMsg)
        {
            bool bok = false;
            string actual = barcodes[rowIndex];
 	        return bok;

        } 
        
        void AddHintInfo(string hint, Color color)
        {
            richTextInfo.SelectionColor = color;
            richTextInfo.AppendText(hint+"\r\n");
        }
        private void SetErrorInfo(string s)
        {
            richTextInfo.ForeColor = System.Drawing.Color.Red;
            richTextInfo.Text = s;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            string sPlateCnt = txtPlateCount.Text;
            int plateCnt = 0;
            if( sPlateCnt == "" )
            {
                SetErrorInfo("板数不得为空！");
                return;
            }
            bool bInt = int.TryParse(sPlateCnt, out plateCnt);
            if(!bInt)
            {
                SetErrorInfo("板数必须为数字！");
                return;
            }
            if( plateCnt <1 || plateCnt > 3)
            {
                SetErrorInfo("板数必须在1~3之间！");
                return;
            }
            try
            {
                EnableControls(false);
                GlobalVars.Instance.PlateCnt = plateCnt;
                ExcelReader.ReadBarcodes();
                int gridCnt = GlobalVars.Instance.eachGridExpectedBarcodes.Count;
                sampleCnt = GlobalVars.Instance.eachGridExpectedBarcodes.Sum(x => x.Value.Count);
                txtSampleCount.Text = sampleCnt.ToString();
                Helper.CloseWaiter(strings.NotifierName);
                Helper.WriteGridCnt(gridCnt);
                InitDataGridView(gridCnt);
            }
            catch(Exception ex)
            {
                AddErrorInfo(ex.Message);
            }
           
        }

        private void EnableControls(bool enableControls)
        {
            btnSet.Enabled = false;
            txtPlateCount.Enabled = false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            int selIndex = lstMP3Barcode.SelectedIndex;
            if(selIndex == -1)
            {
                MessageBox.Show("请选中一个微孔板条码");
                return;
            }
            var expectedBarcode = GlobalVars.Instance.eachPlateExpectedBarcodes[selIndex];
            bool isEqual = (txtCurBarcode.Text == expectedBarcode);
            string suffix = isEqual ? ":r" : ":e";
            lstMP3Barcode.Items[selIndex] = txtCurBarcode.Text + suffix;
            string hint = isEqual ? string.Format("第{0}个微孔板条码已经修复。", selIndex+1) : string.Format("微孔板条码仍然错误,期望条码：{0}！", expectedBarcode);
            AddHintInfo(hint, isEqual ? Color.Green : Color.Red);
        }
    }
}
