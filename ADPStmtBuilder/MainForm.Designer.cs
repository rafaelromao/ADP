namespace ADPStmtBuilder {
    partial class MainForm {
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.SaveAsSchemaButon = new System.Windows.Forms.Button();
            this.SaveSchemaButton = new System.Windows.Forms.Button();
            this.LoadSchemaButton = new System.Windows.Forms.Button();
            this.NewSchemaButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.DataTableStatementsGridView = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.CommandStatementsGridView = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.Panel();
            this.ActiveStatementTextBox = new System.Windows.Forms.TextBox();
            this.CommandRadio = new System.Windows.Forms.RadioButton();
            this.DataTableRadio = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.StatementTextTextBox = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.LanguageTextBox = new System.Windows.Forms.TextBox();
            this.LanguageMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.button6 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataTableStatementsGridView)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CommandStatementsGridView)).BeginInit();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.SaveAsSchemaButon);
            this.panel1.Controls.Add(this.SaveSchemaButton);
            this.panel1.Controls.Add(this.LoadSchemaButton);
            this.panel1.Controls.Add(this.NewSchemaButton);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(930, 64);
            this.panel1.TabIndex = 0;
            // 
            // SaveAsSchemaButon
            // 
            this.SaveAsSchemaButon.Location = new System.Drawing.Point(248, 4);
            this.SaveAsSchemaButon.Name = "SaveAsSchemaButon";
            this.SaveAsSchemaButon.Size = new System.Drawing.Size(75, 55);
            this.SaveAsSchemaButon.TabIndex = 3;
            this.SaveAsSchemaButon.Text = "Save As ...";
            this.SaveAsSchemaButon.UseVisualStyleBackColor = true;
            this.SaveAsSchemaButon.Click += new System.EventHandler(this.SaveAsSchemaButon_Click);
            // 
            // SaveSchemaButton
            // 
            this.SaveSchemaButton.Location = new System.Drawing.Point(167, 4);
            this.SaveSchemaButton.Name = "SaveSchemaButton";
            this.SaveSchemaButton.Size = new System.Drawing.Size(75, 55);
            this.SaveSchemaButton.TabIndex = 2;
            this.SaveSchemaButton.Text = "Save";
            this.SaveSchemaButton.UseVisualStyleBackColor = true;
            this.SaveSchemaButton.Click += new System.EventHandler(this.SaveSchemaButton_Click);
            // 
            // LoadSchemaButton
            // 
            this.LoadSchemaButton.Location = new System.Drawing.Point(85, 4);
            this.LoadSchemaButton.Name = "LoadSchemaButton";
            this.LoadSchemaButton.Size = new System.Drawing.Size(75, 55);
            this.LoadSchemaButton.TabIndex = 1;
            this.LoadSchemaButton.Text = "Load";
            this.LoadSchemaButton.UseVisualStyleBackColor = true;
            this.LoadSchemaButton.Click += new System.EventHandler(this.LoadSchemaButton_Click);
            // 
            // NewSchemaButton
            // 
            this.NewSchemaButton.Location = new System.Drawing.Point(3, 3);
            this.NewSchemaButton.Name = "NewSchemaButton";
            this.NewSchemaButton.Size = new System.Drawing.Size(75, 56);
            this.NewSchemaButton.TabIndex = 0;
            this.NewSchemaButton.Text = "New";
            this.NewSchemaButton.UseVisualStyleBackColor = true;
            this.NewSchemaButton.Click += new System.EventHandler(this.NewSchemaButton_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.DataTableStatementsGridView);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(0, 70);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(204, 475);
            this.panel2.TabIndex = 1;
            // 
            // DataTableStatementsGridView
            // 
            this.DataTableStatementsGridView.AllowUserToResizeRows = false;
            this.DataTableStatementsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DataTableStatementsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataTableStatementsGridView.Location = new System.Drawing.Point(7, 22);
            this.DataTableStatementsGridView.MultiSelect = false;
            this.DataTableStatementsGridView.Name = "DataTableStatementsGridView";
            this.DataTableStatementsGridView.RowHeadersVisible = false;
            this.DataTableStatementsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DataTableStatementsGridView.Size = new System.Drawing.Size(183, 440);
            this.DataTableStatementsGridView.TabIndex = 2;
            this.DataTableStatementsGridView.Enter += new System.EventHandler(this.DataTableStatementsGridView_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "DataTable Statements:";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.CommandStatementsGridView);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Location = new System.Drawing.Point(726, 70);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(204, 475);
            this.panel3.TabIndex = 2;
            // 
            // CommandStatementsGridView
            // 
            this.CommandStatementsGridView.AllowUserToResizeRows = false;
            this.CommandStatementsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandStatementsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CommandStatementsGridView.Location = new System.Drawing.Point(7, 20);
            this.CommandStatementsGridView.Name = "CommandStatementsGridView";
            this.CommandStatementsGridView.RowHeadersVisible = false;
            this.CommandStatementsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.CommandStatementsGridView.Size = new System.Drawing.Size(183, 442);
            this.CommandStatementsGridView.TabIndex = 1;
            this.CommandStatementsGridView.Enter += new System.EventHandler(this.DataTableStatementsGridView_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Command Statements:";
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.button6);
            this.panel4.Controls.Add(this.LanguageTextBox);
            this.panel4.Controls.Add(this.button5);
            this.panel4.Controls.Add(this.button3);
            this.panel4.Controls.Add(this.button4);
            this.panel4.Controls.Add(this.button2);
            this.panel4.Controls.Add(this.button1);
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Controls.Add(this.label5);
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Controls.Add(this.StatementTextTextBox);
            this.panel4.Location = new System.Drawing.Point(211, 70);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(509, 475);
            this.panel4.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(466, 52);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(25, 23);
            this.button3.TabIndex = 14;
            this.button3.Text = "-";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(440, 52);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(25, 23);
            this.button4.TabIndex = 13;
            this.button4.Text = "+";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(395, 52);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(25, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = ">";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(370, 52);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(25, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "<";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.ActiveStatementTextBox);
            this.panel5.Controls.Add(this.CommandRadio);
            this.panel5.Controls.Add(this.DataTableRadio);
            this.panel5.Enabled = false;
            this.panel5.Location = new System.Drawing.Point(18, 20);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(340, 56);
            this.panel5.TabIndex = 8;
            // 
            // ActiveStatementTextBox
            // 
            this.ActiveStatementTextBox.Location = new System.Drawing.Point(117, 6);
            this.ActiveStatementTextBox.Name = "ActiveStatementTextBox";
            this.ActiveStatementTextBox.Size = new System.Drawing.Size(220, 20);
            this.ActiveStatementTextBox.TabIndex = 10;
            // 
            // CommandRadio
            // 
            this.CommandRadio.AutoSize = true;
            this.CommandRadio.Location = new System.Drawing.Point(3, 29);
            this.CommandRadio.Name = "CommandRadio";
            this.CommandRadio.Size = new System.Drawing.Size(72, 17);
            this.CommandRadio.TabIndex = 9;
            this.CommandRadio.Text = "Command";
            this.CommandRadio.UseVisualStyleBackColor = true;
            // 
            // DataTableRadio
            // 
            this.DataTableRadio.AutoSize = true;
            this.DataTableRadio.Checked = true;
            this.DataTableRadio.Location = new System.Drawing.Point(3, 6);
            this.DataTableRadio.Name = "DataTableRadio";
            this.DataTableRadio.Size = new System.Drawing.Size(75, 17);
            this.DataTableRadio.TabIndex = 8;
            this.DataTableRadio.TabStop = true;
            this.DataTableRadio.Text = "DataTable";
            this.DataTableRadio.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(132, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Statement Name:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(15, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Statement Type:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(15, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Statement Text:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(361, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Database Language:";
            // 
            // StatementTextTextBox
            // 
            this.StatementTextTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StatementTextTextBox.Location = new System.Drawing.Point(18, 95);
            this.StatementTextTextBox.Multiline = true;
            this.StatementTextTextBox.Name = "StatementTextTextBox";
            this.StatementTextTextBox.Size = new System.Drawing.Size(473, 367);
            this.StatementTextTextBox.TabIndex = 1;
            this.StatementTextTextBox.TextChanged += new System.EventHandler(this.StatementTextTextBox_TextChanged);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(395, 74);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 15;
            this.button5.Text = "save";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // LanguageTextBox
            // 
            this.LanguageTextBox.Location = new System.Drawing.Point(370, 26);
            this.LanguageTextBox.Name = "LanguageTextBox";
            this.LanguageTextBox.Size = new System.Drawing.Size(95, 20);
            this.LanguageTextBox.TabIndex = 16;
            // 
            // LanguageMenu
            // 
            this.LanguageMenu.Name = "LanguageMenu";
            this.LanguageMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.Location = new System.Drawing.Point(466, 24);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(25, 23);
            this.button6.TabIndex = 17;
            this.button6.Text = "...";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 548);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADP Statement Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataTableStatementsGridView)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CommandStatementsGridView)).EndInit();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button SaveAsSchemaButon;
        private System.Windows.Forms.Button SaveSchemaButton;
        private System.Windows.Forms.Button LoadSchemaButton;
        private System.Windows.Forms.Button NewSchemaButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox StatementTextTextBox;
        private System.Windows.Forms.DataGridView DataTableStatementsGridView;
        private System.Windows.Forms.DataGridView CommandStatementsGridView;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.RadioButton CommandRadio;
        private System.Windows.Forms.RadioButton DataTableRadio;
        private System.Windows.Forms.TextBox ActiveStatementTextBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox LanguageTextBox;
        private System.Windows.Forms.ContextMenuStrip LanguageMenu;
        private System.Windows.Forms.Button button6;
    }
}

