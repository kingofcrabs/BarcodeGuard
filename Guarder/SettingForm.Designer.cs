namespace Guarder
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.txtSampleCount = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.rdb2Transfer = new System.Windows.Forms.RadioButton();
            this.rdbResult = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDstSlice = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // txtSampleCount
            // 
            this.txtSampleCount.Location = new System.Drawing.Point(67, 6);
            this.txtSampleCount.Name = "txtSampleCount";
            this.txtSampleCount.Size = new System.Drawing.Size(124, 20);
            this.txtSampleCount.TabIndex = 14;
            this.txtSampleCount.Text = "5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "样本数：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 331);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "提示：";
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(15, 347);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(312, 83);
            this.txtInfo.TabIndex = 19;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(252, 453);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 25);
            this.btnConfirm.TabIndex = 20;
            this.btnConfirm.Text = "确定";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // rdb2Transfer
            // 
            this.rdb2Transfer.Location = new System.Drawing.Point(0, 0);
            this.rdb2Transfer.Name = "rdb2Transfer";
            this.rdb2Transfer.Size = new System.Drawing.Size(104, 24);
            this.rdb2Transfer.TabIndex = 0;
            // 
            // rdbResult
            // 
            this.rdbResult.Location = new System.Drawing.Point(0, 0);
            this.rdbResult.Name = "rdbResult";
            this.rdbResult.Size = new System.Drawing.Size(104, 24);
            this.rdbResult.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // txtDstSlice
            // 
            this.txtDstSlice.Location = new System.Drawing.Point(0, 0);
            this.txtDstSlice.Name = "txtDstSlice";
            this.txtDstSlice.Size = new System.Drawing.Size(100, 20);
            this.txtDstSlice.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "模板：";
            // 
            // lstTemplates
            // 
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(15, 59);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(312, 251);
            this.lstTemplates.TabIndex = 22;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 490);
            this.ControlBox = false;
            this.Controls.Add(this.lstTemplates);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSampleCount);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingForm";
            this.Text = "设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSampleCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.RadioButton rdb2Transfer;
        private System.Windows.Forms.RadioButton rdbResult;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDstSlice;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lstTemplates;
    }
}