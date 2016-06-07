using Biobanking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Guarder
{
    public partial class SettingForm : Form
    {
        public int smpCnt;
        public bool b2Transfer = false;
        public int plasmaSlices = -1;
        public int productSlices = -1;

        public SettingForm()
        {
            InitializeComponent();
            txtDstSlice.Enabled = false;
            
            if(ConfigurationManager.AppSettings["transferSlice"] != null)
            {
                txtSlices.Text = ConfigurationManager.AppSettings["transferSlice"];
            }
            if (ConfigurationManager.AppSettings["productSlice"] != null)
            {
                txtDstSlice.Text = ConfigurationManager.AppSettings["productSlice"];
            }
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
           

            bInt = int.TryParse(txtSlices.Text, out plasmaSlices);
            if (!bInt)
            {
                SetErrorInfo("中转份数必须为数字！");
                return;
            }
            if(plasmaSlices <1 || plasmaSlices > 6)
            {
                SetErrorInfo("中转份数必须在1~6之间！");
                return;
            }


            bInt = int.TryParse(txtDstSlice.Text, out productSlices);
            if (!bInt)
            {
                SetErrorInfo("产物份数必须为数字！");
                return;
            }
            if (productSlices < 1)
            {
                SetErrorInfo("产物份数必须在大于1！");
                return;
            }

            
            b2Transfer = rdb2Transfer.Checked;
            
            this.Close();
        }

        private void SetErrorInfo(string info)
        {
            txtInfo.Text = info;
            txtInfo.ForeColor = Color.Red;
            txtInfo.BackColor = Color.White;
        }

        private void rdb2Transfer_CheckedChanged(object sender, EventArgs e)
        {
            txtDstSlice.Enabled = !rdb2Transfer.Checked;
        }

    }
}
