using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Cati.ADP.Common {
    internal partial class ADPFileMonitorForm : Form {
        private string fileName = "";
        public ADPFileMonitorForm(string logFileName) {
            InitializeComponent();
            fileName = logFileName;
        }
        private void RefreshButton_Click(object sender, EventArgs e) {
            if (File.Exists(fileName)) {
                FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(stream);
                reader.BaseStream.Position = 0;
                LogTextBox.Text = reader.ReadToEnd();
                LogTextBox.SelectionStart = LogTextBox.Text.Length;
                LogTextBox.ScrollToCaret();
            }
        }
        private void RefreshTimer_Tick(object sender, EventArgs e) {
            if (AutoRefreshCheckBox.Checked) {
                RefreshButton_Click(null, null);
            }
        }

        private void ADPServerMonitorForm_FormClosed(object sender, FormClosedEventArgs e) {
            RefreshTimer.Enabled = false;
        }
    }
}