namespace ADPStatementBuilder {
    partial class ADPSBMainForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ADPSBMainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.newButton = new System.Windows.Forms.ToolStripButton();
            this.loadButton = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.saveAsButton = new System.Windows.Forms.ToolStripButton();
            this.closeButton = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.statementsPanel = new System.Windows.Forms.Panel();
            this.commandStatementRadio = new System.Windows.Forms.RadioButton();
            this.queryStatementRadio = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.statementTextsSource = new System.Windows.Forms.BindingSource(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.statementTextBox = new System.Windows.Forms.TextBox();
            this.statementsGuid = new System.Windows.Forms.DataGridView();
            this.statementsNavigator = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorAddNewItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorDeleteItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.statementTextsNavigator = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorMoveFirstItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem1 = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorCountItem1 = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorAddNewItem1 = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorDeleteItem1 = new System.Windows.Forms.ToolStripButton();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.StatementId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statementsSource = new System.Windows.Forms.BindingSource(this.components);
            this.statementTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.keyGenerationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.keyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isNewDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.statementIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.modifiedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.databaseNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deletedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.panel1.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.statementsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementTextsSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statementsGuid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statementsNavigator)).BeginInit();
            this.statementsNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementTextsNavigator)).BeginInit();
            this.statementTextsNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementsSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mainToolStrip);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(809, 44);
            this.panel1.TabIndex = 10;
            this.panel1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newButton,
            this.loadButton,
            this.saveButton,
            this.saveAsButton,
            this.closeButton});
            this.mainToolStrip.Location = new System.Drawing.Point(10, 9);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(211, 25);
            this.mainToolStrip.TabIndex = 10;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // newButton
            // 
            this.newButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.newButton.Image = ((System.Drawing.Image)(resources.GetObject("newButton.Image")));
            this.newButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(32, 22);
            this.newButton.Text = "New";
            this.newButton.ToolTipText = "New";
            this.newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.loadButton.Image = ((System.Drawing.Image)(resources.GetObject("loadButton.Image")));
            this.loadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(34, 22);
            this.loadButton.Text = "Load";
            this.loadButton.TextImageRelation = System.Windows.Forms.TextImageRelation.Overlay;
            this.loadButton.ToolTipText = "Load";
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(35, 22);
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // saveAsButton
            // 
            this.saveAsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveAsButton.Image = ((System.Drawing.Image)(resources.GetObject("saveAsButton.Image")));
            this.saveAsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(61, 22);
            this.saveAsButton.Text = "Save as...";
            this.saveAsButton.Click += new System.EventHandler(this.saveAsButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.closeButton.Image = ((System.Drawing.Image)(resources.GetObject("closeButton.Image")));
            this.closeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(37, 22);
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "ssf";
            this.openFileDialog.Filter = "Stored Statement Files (*.ssf)|*.ssf";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "ssf";
            this.saveFileDialog.Filter = "Stored Statement Files (*.ssf)|*.ssf";
            // 
            // statementsPanel
            // 
            this.statementsPanel.Controls.Add(this.textBox1);
            this.statementsPanel.Controls.Add(this.label3);
            this.statementsPanel.Controls.Add(this.statementTextsNavigator);
            this.statementsPanel.Controls.Add(this.commandStatementRadio);
            this.statementsPanel.Controls.Add(this.queryStatementRadio);
            this.statementsPanel.Controls.Add(this.label2);
            this.statementsPanel.Controls.Add(this.label1);
            this.statementsPanel.Controls.Add(this.statementTextBox);
            this.statementsPanel.Controls.Add(this.statementsGuid);
            this.statementsPanel.Controls.Add(this.statementsNavigator);
            this.statementsPanel.Location = new System.Drawing.Point(3, 50);
            this.statementsPanel.Name = "statementsPanel";
            this.statementsPanel.Size = new System.Drawing.Size(809, 433);
            this.statementsPanel.TabIndex = 11;
            this.statementsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.statementsPanel_Paint);
            // 
            // commandStatementRadio
            // 
            this.commandStatementRadio.AutoSize = true;
            this.commandStatementRadio.Location = new System.Drawing.Point(110, 52);
            this.commandStatementRadio.Name = "commandStatementRadio";
            this.commandStatementRadio.Size = new System.Drawing.Size(72, 17);
            this.commandStatementRadio.TabIndex = 16;
            this.commandStatementRadio.Text = "Command";
            this.commandStatementRadio.UseVisualStyleBackColor = true;
            this.commandStatementRadio.CheckedChanged += new System.EventHandler(this.commandStatementRadio_CheckedChanged);
            // 
            // queryStatementRadio
            // 
            this.queryStatementRadio.AutoSize = true;
            this.queryStatementRadio.Checked = true;
            this.queryStatementRadio.Location = new System.Drawing.Point(54, 52);
            this.queryStatementRadio.Name = "queryStatementRadio";
            this.queryStatementRadio.Size = new System.Drawing.Size(53, 17);
            this.queryStatementRadio.TabIndex = 15;
            this.queryStatementRadio.TabStop = true;
            this.queryStatementRadio.Text = "Query";
            this.queryStatementRadio.UseVisualStyleBackColor = true;
            this.queryStatementRadio.CheckedChanged += new System.EventHandler(this.queryStatementRadio_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Type:";
            // 
            // statementTextsSource
            // 
            this.statementTextsSource.AllowNew = true;
            this.statementTextsSource.DataMember = "StatementTexts";
            this.statementTextsSource.DataSource = this.statementsSource;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(550, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Language Name:";
            // 
            // statementTextBox
            // 
            this.statementTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.statementTextsSource, "StatementText", true));
            this.statementTextBox.Location = new System.Drawing.Point(274, 52);
            this.statementTextBox.Multiline = true;
            this.statementTextBox.Name = "statementTextBox";
            this.statementTextBox.Size = new System.Drawing.Size(523, 376);
            this.statementTextBox.TabIndex = 11;
            // 
            // statementsGuid
            // 
            this.statementsGuid.AutoGenerateColumns = false;
            this.statementsGuid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.statementsGuid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.statementsGuid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StatementId,
            this.statementTypeDataGridViewTextBoxColumn,
            this.keyGenerationDataGridViewTextBoxColumn,
            this.keyDataGridViewTextBoxColumn,
            this.isNewDataGridViewCheckBoxColumn,
            this.statementIdDataGridViewTextBoxColumn,
            this.modifiedDataGridViewCheckBoxColumn,
            this.databaseNameDataGridViewTextBoxColumn,
            this.deletedDataGridViewCheckBoxColumn});
            this.statementsGuid.DataSource = this.statementsSource;
            this.statementsGuid.Location = new System.Drawing.Point(11, 75);
            this.statementsGuid.Name = "statementsGuid";
            this.statementsGuid.Size = new System.Drawing.Size(257, 353);
            this.statementsGuid.TabIndex = 10;
            this.statementsGuid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.statementsGuid_UserDeletingRow);
            this.statementsGuid.SelectionChanged += new System.EventHandler(this.statementsGuid_SelectionChanged_1);
            // 
            // statementsNavigator
            // 
            this.statementsNavigator.AddNewItem = this.bindingNavigatorAddNewItem;
            this.statementsNavigator.BindingSource = this.statementsSource;
            this.statementsNavigator.CountItem = this.bindingNavigatorCountItem;
            this.statementsNavigator.DeleteItem = this.bindingNavigatorDeleteItem;
            this.statementsNavigator.Dock = System.Windows.Forms.DockStyle.None;
            this.statementsNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem,
            this.bindingNavigatorMovePreviousItem,
            this.bindingNavigatorSeparator,
            this.bindingNavigatorPositionItem,
            this.bindingNavigatorCountItem,
            this.bindingNavigatorSeparator1,
            this.bindingNavigatorMoveNextItem,
            this.bindingNavigatorMoveLastItem,
            this.bindingNavigatorSeparator2,
            this.bindingNavigatorAddNewItem,
            this.bindingNavigatorDeleteItem});
            this.statementsNavigator.Location = new System.Drawing.Point(11, 18);
            this.statementsNavigator.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.statementsNavigator.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.statementsNavigator.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.statementsNavigator.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.statementsNavigator.Name = "statementsNavigator";
            this.statementsNavigator.PositionItem = this.bindingNavigatorPositionItem;
            this.statementsNavigator.Size = new System.Drawing.Size(256, 25);
            this.statementsNavigator.TabIndex = 9;
            this.statementsNavigator.Text = "bindingNavigator1";
            // 
            // bindingNavigatorAddNewItem
            // 
            this.bindingNavigatorAddNewItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorAddNewItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorAddNewItem.Image")));
            this.bindingNavigatorAddNewItem.Name = "bindingNavigatorAddNewItem";
            this.bindingNavigatorAddNewItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorAddNewItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorAddNewItem.Text = "Add new";
            // 
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem.Size = new System.Drawing.Size(36, 22);
            this.bindingNavigatorCountItem.Text = "of {0}";
            this.bindingNavigatorCountItem.ToolTipText = "Total number of items";
            // 
            // bindingNavigatorDeleteItem
            // 
            this.bindingNavigatorDeleteItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorDeleteItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorDeleteItem.Image")));
            this.bindingNavigatorDeleteItem.Name = "bindingNavigatorDeleteItem";
            this.bindingNavigatorDeleteItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorDeleteItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorDeleteItem.Text = "Delete";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem.Image")));
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem.Text = "Move first";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            this.bindingNavigatorMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem.Image")));
            this.bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            this.bindingNavigatorMovePreviousItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem.Text = "Move previous";
            // 
            // bindingNavigatorSeparator
            // 
            this.bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem
            // 
            this.bindingNavigatorPositionItem.AccessibleName = "Position";
            this.bindingNavigatorPositionItem.AutoSize = false;
            this.bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            this.bindingNavigatorPositionItem.Size = new System.Drawing.Size(50, 21);
            this.bindingNavigatorPositionItem.Text = "0";
            this.bindingNavigatorPositionItem.ToolTipText = "Current position";
            // 
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            this.bindingNavigatorSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorMoveNextItem
            // 
            this.bindingNavigatorMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem.Image")));
            this.bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            this.bindingNavigatorMoveNextItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem.Text = "Move next";
            // 
            // bindingNavigatorMoveLastItem
            // 
            this.bindingNavigatorMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem.Image")));
            this.bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            this.bindingNavigatorMoveLastItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem.Text = "Move last";
            // 
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            this.bindingNavigatorSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // statementTextsNavigator
            // 
            this.statementTextsNavigator.AddNewItem = this.bindingNavigatorAddNewItem1;
            this.statementTextsNavigator.BindingSource = this.statementTextsSource;
            this.statementTextsNavigator.CountItem = this.bindingNavigatorCountItem1;
            this.statementTextsNavigator.DeleteItem = this.bindingNavigatorDeleteItem1;
            this.statementTextsNavigator.Dock = System.Windows.Forms.DockStyle.None;
            this.statementTextsNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem1,
            this.bindingNavigatorMovePreviousItem1,
            this.bindingNavigatorSeparator3,
            this.bindingNavigatorPositionItem1,
            this.bindingNavigatorCountItem1,
            this.bindingNavigatorSeparator4,
            this.bindingNavigatorMoveNextItem1,
            this.bindingNavigatorMoveLastItem1,
            this.bindingNavigatorSeparator5,
            this.bindingNavigatorAddNewItem1,
            this.bindingNavigatorDeleteItem1});
            this.statementTextsNavigator.Location = new System.Drawing.Point(274, 18);
            this.statementTextsNavigator.MoveFirstItem = this.bindingNavigatorMoveFirstItem1;
            this.statementTextsNavigator.MoveLastItem = this.bindingNavigatorMoveLastItem1;
            this.statementTextsNavigator.MoveNextItem = this.bindingNavigatorMoveNextItem1;
            this.statementTextsNavigator.MovePreviousItem = this.bindingNavigatorMovePreviousItem1;
            this.statementTextsNavigator.Name = "statementTextsNavigator";
            this.statementTextsNavigator.PositionItem = this.bindingNavigatorPositionItem1;
            this.statementTextsNavigator.Size = new System.Drawing.Size(256, 25);
            this.statementTextsNavigator.TabIndex = 17;
            this.statementTextsNavigator.Text = "bindingNavigator1";
            // 
            // bindingNavigatorMoveFirstItem1
            // 
            this.bindingNavigatorMoveFirstItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem1.Image")));
            this.bindingNavigatorMoveFirstItem1.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem1.Text = "Move first";
            // 
            // bindingNavigatorMovePreviousItem1
            // 
            this.bindingNavigatorMovePreviousItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem1.Image")));
            this.bindingNavigatorMovePreviousItem1.Name = "bindingNavigatorMovePreviousItem";
            this.bindingNavigatorMovePreviousItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem1.Text = "Move previous";
            // 
            // bindingNavigatorSeparator3
            // 
            this.bindingNavigatorSeparator3.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem1
            // 
            this.bindingNavigatorPositionItem1.AccessibleName = "Position";
            this.bindingNavigatorPositionItem1.AutoSize = false;
            this.bindingNavigatorPositionItem1.Name = "bindingNavigatorPositionItem";
            this.bindingNavigatorPositionItem1.Size = new System.Drawing.Size(50, 21);
            this.bindingNavigatorPositionItem1.Text = "0";
            this.bindingNavigatorPositionItem1.ToolTipText = "Current position";
            // 
            // bindingNavigatorCountItem1
            // 
            this.bindingNavigatorCountItem1.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem1.Size = new System.Drawing.Size(36, 22);
            this.bindingNavigatorCountItem1.Text = "of {0}";
            this.bindingNavigatorCountItem1.ToolTipText = "Total number of items";
            // 
            // bindingNavigatorSeparator4
            // 
            this.bindingNavigatorSeparator4.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorMoveNextItem1
            // 
            this.bindingNavigatorMoveNextItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem1.Image")));
            this.bindingNavigatorMoveNextItem1.Name = "bindingNavigatorMoveNextItem";
            this.bindingNavigatorMoveNextItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem1.Text = "Move next";
            // 
            // bindingNavigatorMoveLastItem1
            // 
            this.bindingNavigatorMoveLastItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem1.Image")));
            this.bindingNavigatorMoveLastItem1.Name = "bindingNavigatorMoveLastItem";
            this.bindingNavigatorMoveLastItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem1.Text = "Move last";
            // 
            // bindingNavigatorSeparator5
            // 
            this.bindingNavigatorSeparator5.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorAddNewItem1
            // 
            this.bindingNavigatorAddNewItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorAddNewItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorAddNewItem1.Image")));
            this.bindingNavigatorAddNewItem1.Name = "bindingNavigatorAddNewItem";
            this.bindingNavigatorAddNewItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorAddNewItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorAddNewItem1.Text = "Add new";
            // 
            // bindingNavigatorDeleteItem1
            // 
            this.bindingNavigatorDeleteItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorDeleteItem1.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorDeleteItem1.Image")));
            this.bindingNavigatorDeleteItem1.Name = "bindingNavigatorDeleteItem";
            this.bindingNavigatorDeleteItem1.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorDeleteItem1.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorDeleteItem1.Text = "Delete";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Statements:";
            // 
            // textBox1
            // 
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.statementTextsSource, "DatabaseLanguage", true));
            this.textBox1.Location = new System.Drawing.Point(659, 19);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(138, 20);
            this.textBox1.TabIndex = 19;
            // 
            // StatementId
            // 
            this.StatementId.DataPropertyName = "StatementId";
            this.StatementId.HeaderText = "Statement Id";
            this.StatementId.Name = "StatementId";
            // 
            // statementsSource
            // 
            this.statementsSource.DataSource = typeof(ADPStatementBuilder.ADPStoredStatementIDCollection);
            this.statementsSource.ListChanged += new System.ComponentModel.ListChangedEventHandler(this.statementsSource_ListChanged);
            // 
            // statementTypeDataGridViewTextBoxColumn
            // 
            this.statementTypeDataGridViewTextBoxColumn.DataPropertyName = "StatementType";
            this.statementTypeDataGridViewTextBoxColumn.HeaderText = "Statement Type";
            this.statementTypeDataGridViewTextBoxColumn.Name = "statementTypeDataGridViewTextBoxColumn";
            this.statementTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // keyGenerationDataGridViewTextBoxColumn
            // 
            this.keyGenerationDataGridViewTextBoxColumn.DataPropertyName = "KeyGeneration";
            this.keyGenerationDataGridViewTextBoxColumn.HeaderText = "KeyGeneration";
            this.keyGenerationDataGridViewTextBoxColumn.Name = "keyGenerationDataGridViewTextBoxColumn";
            this.keyGenerationDataGridViewTextBoxColumn.Visible = false;
            // 
            // keyDataGridViewTextBoxColumn
            // 
            this.keyDataGridViewTextBoxColumn.DataPropertyName = "Key";
            this.keyDataGridViewTextBoxColumn.HeaderText = "Key";
            this.keyDataGridViewTextBoxColumn.Name = "keyDataGridViewTextBoxColumn";
            this.keyDataGridViewTextBoxColumn.Visible = false;
            // 
            // isNewDataGridViewCheckBoxColumn
            // 
            this.isNewDataGridViewCheckBoxColumn.DataPropertyName = "IsNew";
            this.isNewDataGridViewCheckBoxColumn.HeaderText = "IsNew";
            this.isNewDataGridViewCheckBoxColumn.Name = "isNewDataGridViewCheckBoxColumn";
            this.isNewDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isNewDataGridViewCheckBoxColumn.Visible = false;
            // 
            // statementIdDataGridViewTextBoxColumn
            // 
            this.statementIdDataGridViewTextBoxColumn.DataPropertyName = "StatementId";
            this.statementIdDataGridViewTextBoxColumn.HeaderText = "StatementId";
            this.statementIdDataGridViewTextBoxColumn.Name = "statementIdDataGridViewTextBoxColumn";
            this.statementIdDataGridViewTextBoxColumn.Visible = false;
            // 
            // modifiedDataGridViewCheckBoxColumn
            // 
            this.modifiedDataGridViewCheckBoxColumn.DataPropertyName = "Modified";
            this.modifiedDataGridViewCheckBoxColumn.HeaderText = "Modified";
            this.modifiedDataGridViewCheckBoxColumn.Name = "modifiedDataGridViewCheckBoxColumn";
            this.modifiedDataGridViewCheckBoxColumn.ReadOnly = true;
            this.modifiedDataGridViewCheckBoxColumn.Visible = false;
            // 
            // databaseNameDataGridViewTextBoxColumn
            // 
            this.databaseNameDataGridViewTextBoxColumn.DataPropertyName = "DatabaseName";
            this.databaseNameDataGridViewTextBoxColumn.HeaderText = "DatabaseName";
            this.databaseNameDataGridViewTextBoxColumn.Name = "databaseNameDataGridViewTextBoxColumn";
            this.databaseNameDataGridViewTextBoxColumn.Visible = false;
            // 
            // deletedDataGridViewCheckBoxColumn
            // 
            this.deletedDataGridViewCheckBoxColumn.DataPropertyName = "Deleted";
            this.deletedDataGridViewCheckBoxColumn.HeaderText = "Deleted";
            this.deletedDataGridViewCheckBoxColumn.Name = "deletedDataGridViewCheckBoxColumn";
            this.deletedDataGridViewCheckBoxColumn.ReadOnly = true;
            this.deletedDataGridViewCheckBoxColumn.Visible = false;
            // 
            // ADPSBMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 486);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statementsPanel);
            this.Name = "ADPSBMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ADP Statement Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ADPSBMainForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.statementsPanel.ResumeLayout(false);
            this.statementsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementTextsSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statementsGuid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statementsNavigator)).EndInit();
            this.statementsNavigator.ResumeLayout(false);
            this.statementsNavigator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementTextsNavigator)).EndInit();
            this.statementTextsNavigator.ResumeLayout(false);
            this.statementTextsNavigator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statementsSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton newButton;
        private System.Windows.Forms.ToolStripButton loadButton;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripButton closeButton;
        private System.Windows.Forms.ToolStripButton saveAsButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Panel statementsPanel;
        private System.Windows.Forms.RadioButton commandStatementRadio;
        private System.Windows.Forms.RadioButton queryStatementRadio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox statementTextBox;
        private System.Windows.Forms.DataGridView statementsGuid;
        private System.Windows.Forms.BindingNavigator statementsNavigator;
        private System.Windows.Forms.ToolStripButton bindingNavigatorAddNewItem;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorDeleteItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.BindingSource statementTextsSource;
        private System.Windows.Forms.BindingSource statementsSource;
        private System.Windows.Forms.BindingNavigator statementTextsNavigator;
        private System.Windows.Forms.ToolStripButton bindingNavigatorAddNewItem1;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorDeleteItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator3;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator4;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem1;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatementId;
        private System.Windows.Forms.DataGridViewTextBoxColumn statementTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn keyGenerationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn keyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isNewDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statementIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn modifiedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn databaseNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn deletedDataGridViewCheckBoxColumn;
    }
}

