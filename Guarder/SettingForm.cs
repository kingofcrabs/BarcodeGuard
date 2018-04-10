using Biobanking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Guarder
{
    public partial class SettingForm : Form
    {
        public int smpCnt;
        public string templateFile;
        List<FileInfo> fileInfos = new List<FileInfo>();
        public SettingForm()
        {
            InitializeComponent();
            this.Load += SettingForm_Load;
        }

        void SettingForm_Load(object sender, EventArgs e)
        {
            Helper.WriteResult(false);
            string templateFolder = ConfigurationManager.AppSettings["TemplateFolder"];
            List<string> files = Directory.EnumerateFiles(templateFolder, "*.csv").ToList();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                fileInfos.Add(fileInfo);
                lstTemplates.Items.Add(fileInfo.Name.Replace(".csv", ""));
            }
            if(lstTemplates.Items.Count == 0)
            {
                SetErrorInfo("找不到比对模板！");
                btnConfirm.Enabled = false;
                return;
            }
            lstTemplates.SelectedIndex = 0;
            
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string sSmpCnt = txtSampleCount.Text;
            smpCnt = 0;
            txtInfo.Text = "";
            bool bInt = int.TryParse(sSmpCnt, out smpCnt);
            if( sSmpCnt == "" )
            {
                SetErrorInfo("样本数不得为空！");
                return;
            }

            if(!bInt)
            {
                SetErrorInfo("样本数必须为数字！");
                return;
            }
            if( smpCnt <1 || smpCnt >200)
            {
                SetErrorInfo("样本数必须在1~200之间！");
                return;
            }
            if(lstTemplates.SelectedIndex == -1)
            {
                SetErrorInfo("请选中一个模板！");
                return;
            }
            templateFile = fileInfos[lstTemplates.SelectedIndex].FullName;

            TemplateReader templateReader = new TemplateReader();
            List<int> srcGrids = new List<int>();
            Dictionary<int, CheckInfo> eachGridCheckInfo = new Dictionary<int, CheckInfo>();
            templateReader.GetCheckInfos(templateFile, ref srcGrids, ref eachGridCheckInfo);
            if (smpCnt > srcGrids.Count * 16)
            {
                SetErrorInfo(string.Format("共有{0}条src，最大允许样品：{1}", srcGrids.Count, srcGrids.Count * 16));
                return;
            }
            this.Close();
        }

        private void SetErrorInfo(string info)
        {
            txtInfo.Text = info;
            txtInfo.ForeColor = Color.Red;
            txtInfo.BackColor = Color.White;
        }

     
    }
}
