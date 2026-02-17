using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Cati.ADP.Common {
    /// <summary>
    /// Monitor a text file and display its content in a window
    /// </summary>
    public class ADPFileMonitor {
        /// <summary>
        /// Creates a new ADPFileMonitor
        /// </summary>
        /// <param name="logFileName">
        /// Name of the file to be monitored
        /// </param>
        /// <param name="show">
        /// If true, shows the form
        /// </param>
        public ADPFileMonitor(string logFileName, bool show) {
            fileName = logFileName;
            if (show) {
                Show();
            }
        }
        /// <summary>
        /// Window Form used to display the file content
        /// </summary>
        ADPFileMonitorForm form;
        /// <summary>
        /// Name of the file to be monitored
        /// </summary>
        string fileName = "";
        /// <summary>
        /// Shows the monitoring form
        /// </summary>
        public void Show() {
            form = new ADPFileMonitorForm(fileName);
            form.RefreshTimer.Enabled = true;
            form.Show();
        }
    }
}
