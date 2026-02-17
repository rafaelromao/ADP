using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    public partial class ADPServerMonitorForm : Form {
        public ADPServerMonitorForm() {
            InitializeComponent();
        }

        private void openADPServerMonitorToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!ADPServer.GetDebugModeEnabled()) {
                MessageBox.Show("The ADPServer tracing is not enabled!");
                return;
            }
            ADPFileMonitor monitor = null;
            string logFileName = ADPServer.GetServerAddress() + ADPServer.GetLogFileName();
            Process[] processes = Process.GetProcessesByName(ADPServer.GetProcessName());
            if (processes.Length > 0) {
                monitor = new ADPFileMonitor(logFileName, false);
            }
            if (monitor != null) {
                monitor.Show();
            } else { 
                MessageBox.Show("The ADPServer is not running!");
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }
    }
}