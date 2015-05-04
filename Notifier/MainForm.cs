using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Notifier
{
    public partial class MainForm : Form
    {
        public MainForm(string s)
        {
            InitializeComponent();
            SendCommand(s);
        }

        private void SendCommand(string sContent)
        {

            string sProgramName = ConfigurationManager.AppSettings["dstProgramName"];
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", sProgramName,
                                                                           PipeDirection.Out,
                                                                        PipeOptions.None))
            {

                txtInfo.AppendText("Attempting to connect to pipe...\r\n");
                try
                {
                    pipeClient.Connect(1000);
                }
                catch(Exception ex)
                {
                    txtInfo.AppendText(string.Format("Cannot connect to server, reason: {0}.\r\n",ex.Message));
                    return;
                }
                txtInfo.AppendText("Connected to pipe.\r\n");

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.Write(sContent);
                }
            }
        }
    }
}
