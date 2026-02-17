namespace Cati.ADP.Server {
    partial class ADPServerMonitorForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ADPServerMonitorForm));
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TrayIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openADPServerMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayIconContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.TrayIconContextMenuStrip;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "ADPServer";
            this.TrayIcon.Visible = true;
            // 
            // TrayIconContextMenuStrip
            // 
            this.TrayIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openADPServerMonitorToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.TrayIconContextMenuStrip.Name = "TrayIconContextMenuStrip";
            this.TrayIconContextMenuStrip.Size = new System.Drawing.Size(206, 48);
            // 
            // openADPServerMonitorToolStripMenuItem
            // 
            this.openADPServerMonitorToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.openADPServerMonitorToolStripMenuItem.Name = "openADPServerMonitorToolStripMenuItem";
            this.openADPServerMonitorToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.openADPServerMonitorToolStripMenuItem.Text = "Open ADPServer monitor";
            this.openADPServerMonitorToolStripMenuItem.Click += new System.EventHandler(this.openADPServerMonitorToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // ADPServerMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Name = "ADPServerMonitorForm";
            this.ShowInTaskbar = false;
            this.Text = "ADP Server Monitor";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.TrayIconContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip TrayIconContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openADPServerMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    }
}