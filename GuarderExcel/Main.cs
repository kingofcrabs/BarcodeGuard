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

namespace Guarder
{
    public partial class GuardForm : Form
    {

        List<ErrorInfo> errorsInfo = new List<ErrorInfo>();
        string dummy = "***";
        int sampleCnt = 0;
        bool programModify = false;
        public GuardForm()
        {
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
            CreateNamedPipeServer();
            dataGridView.Visible = false;
          
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
                if (curBarcode == "***")
                {
                    foreColor = System.Drawing.Color.Red;
                }
                else if (curBarcode == "$$$")
                {
                    foreColor = System.Drawing.Color.Orange;
                }
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
            //check grid is valid
            if (!GlobalVars.Instance.eachGridExpectedBarcodes.ContainsKey(grid))
            {
                string  errMsg = string.Format("当前扫描Grid{0}位置不在期望的试管载架中！", grid);
                AddErrorInfo(errMsg);
                Helper.WriteResult(false);
            }
            //update ui
            UpdateGridCells(grid, barcodes, results);

            //check ok, if not ok, show concrete information.
            bool bok = CheckBarcodes(grid, results);
            if(bok)
            {
                Helper.CloseWaiter(strings.NotifierName);
                AddHintInfo(string.Format("Grid{0}条码检查通过！", grid), Color.DarkGreen);
            }
            else
                ShowErrorDialog(grid);
            Helper.WriteResult(bok);
        }

        private bool CheckBarcodes(int grid, List<bool> results)
        {
            for(int i = 0; i< GlobalVars.Instance.eachGridExpectedBarcodes[grid].Count ; i++)
            {
                bool bok = IsValidBarcode(grid,i);
                results.Add(bok);
            }
            return results.Contains(false);
        }

        private void ShowErrorDialog(int grid)
        {
             SourceErrors sourceErrorForm = new SourceErrors(errorsInfo,grid);
             sourceErrorForm.ShowDialog();
        }

    

        private void UpdateGridCells(int grid, List<string> barcodes, List<bool> checkResults)
        {
            programModify = true;
            for (int i = 0; i < barcodes.Count; i++)
            {
                int colIndex = GlobalVars.Instance.eachGridExpectedBarcodes.Keys.ToList().FindIndex(x => x == grid);
                dataGridView.Rows[i].Cells[colIndex].Value = barcodes[i];
                dataGridView.Rows[i].Cells[colIndex].Style.BackColor = checkResults[i] ? Color.LightGreen : Color.Red;
                dataGridView.Rows[i].Cells[colIndex].ReadOnly = checkResults[i];
            }
            programModify = false;
        }

        private void UpdateGridCell(int gridID, int i, string expectedBarcode, string actualBarcode)
        {
            programModify = true;
            bool isEqual = expectedBarcode == actualBarcode;
            int colIndex = gridID - GlobalVars.Instance.StartGridID;
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


        bool IsValidBarcode(int grid, int row)
        {
           return  dataGridView.Rows[row].Cells[grid].Value.ToString() == GlobalVars.Instance.eachGridExpectedBarcodes[grid][row];
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
                SetErrorInfo("样本数必须在1~3之间！");
                return;
            }
            GlobalVars.Instance.PlateCnt = plateCnt;
            var eachGridBarcodes = ExcelReader.ReadBarcodes();
            InitDataGridView(eachGridBarcodes.Count);
        }
    }
}
