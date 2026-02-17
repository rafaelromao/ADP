using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace ADPStmtBuilder {
    public partial class MainForm : Form {
        private class ConfigHandler : ConfigurationSection {
            public ConfigHandler() {
            }
            [ConfigurationProperty("LastLoadedSchema")]
            public string LastLoadedSchema {
                get { return (string)this["LastLoadedSchema"]; }
                set { this["LastLoadedSchema"] = value; }
            }
        }

        #region Events
        public MainForm() {
            InitializeComponent();
            Initialize();
        }
        private void NewSchemaButton_Click(object sender, EventArgs e) {
            StatementBuilderServer.NewSchema();
        }
        private void LoadSchemaButton_Click(object sender, EventArgs e) {
            StatementBuilderServer.LoadSchema();
        }
        private void SaveSchemaButton_Click(object sender, EventArgs e) {
            StatementBuilderServer.SaveSchema();
        }
        private void SaveAsSchemaButon_Click(object sender, EventArgs e) {
            StatementBuilderServer.SaveSchemaAs();
        }
        private void DataTableStatementsGridView_Enter(object sender, EventArgs e) {
            ActiveDataGridView = (DataGridView)sender;
        }
        private void OnSelectionChanged(object sender, EventArgs e) {
            LoadActiveStatement();
        }
        private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e) {
            LoadActiveStatement();
            StatementBuilderServer.Modified = true;
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = !StatementBuilderServer.AllowCloseSchema();
        }
        private void OnSchemaLoaded(object sender, EventArgs e) {
            FileName = StatementBuilderServer.FileName;
            LoadActiveStatement();
            UpdateTitle();
        }
        #endregion

        private string FileName {
            get {
                //read value from the config file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigHandler section = (ConfigHandler)config.GetSection("Custom");
                if (section == null) {
                    return "";
                } else {
                    return section.LastLoadedSchema;
                }
            }
            set {
                //Write value to the config file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigHandler section = (ConfigHandler)config.GetSection("Custom");
                if (section == null) {
                    section = new ConfigHandler();
                    config.Sections.Add("Custom", section);
                }
                section.LastLoadedSchema = value;
                section.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Full);
                UpdateTitle();
            }
        }
        private DataGridView activeDataGridView = null;
        private DataGridView ActiveDataGridView {
            get { return activeDataGridView; }
            set {
                activeDataGridView = value;
                LoadActiveStatement();
            }
        }
        private void LoadActiveStatement() {
            if ((activeDataGridView != null) && (activeDataGridView.SelectedRows.Count > 0)) {
                if (activeDataGridView == DataTableStatementsGridView) {
                    DataTableRadio.Checked = true;
                    LoadStatementText("", false);
                } else {
                    CommandRadio.Checked = true;
                    LoadStatementText("", true);
                }
                DataGridViewColumn col = activeDataGridView.Columns[0];
                DataGridViewRow row = activeDataGridView.SelectedRows[0];
                if ((row.Cells[col.Index].Value == null) || (row.Cells[col.Index].Value is DBNull)) {
                    ActiveStatementTextBox.Text = "";
                } else {
                    ActiveStatementTextBox.Text = (string)row.Cells[col.Index].Value;
                }
            }
        }
        private int SelectedStatementCode {
            get {
                if (activeDataGridView == null) {
                    return 0;
                }
                DataGridViewColumn col;
                DataGridViewRow row = activeDataGridView.SelectedRows[0];
                if (activeDataGridView == CommandStatementsGridView) {
                    col = activeDataGridView.Columns["COST_CODE"];
                } else {
                    col = activeDataGridView.Columns["DTST_CODE"];
                }
                if (col == null) {
                    return 0;
                } else {
                    return ((int)(row.Cells[col.Index].Value));
                }
            }
        }
        private void LoadStatementText(string dbLanguage, bool isCommand) {
            int statementCode = SelectedStatementCode;
            //Clear current list
            LanguageMenu.Items.Clear();
            //Get a list with all languages already inserted for that statement
            List<DBLanguage> languages = StatementBuilderServer.GetStatementLanguages(statementCode, isCommand);
            DBLanguage selectedLanguage = null;
            //Verify if the selected language is found in the list
            if ((dbLanguage == "") && (languages.Count > 0)) {
                selectedLanguage = languages[0];
            } else {
                foreach (DBLanguage l in languages) {
                    if (l.Text == dbLanguage) {
                        selectedLanguage = l;
                        break;
                    }
                }
            }
            //If the language was not found, create as a new one
            if (selectedLanguage == null) {
                selectedLanguage = new DBLanguage(0, dbLanguage);
                languages.Add(selectedLanguage);
            }
            //LanguageMenu.Items.AddRange(languages.ToArray());
            StatementTextTextBox.Text = StatementBuilderServer.GetStatementText(statementCode, selectedLanguage.Code, isCommand);
        }
        private void UpdateTitle() {
            string modFlag = "";
            if (StatementBuilderServer.Modified) {
                modFlag = "*";
            }
            Text = String.Format("{0} - {1} [{2}]{3}",
                                      Application.ProductName,
                                      Application.ProductVersion,
                                      FileName,
                                      modFlag);
        }
        private void UpdateData() {
            DBLanguage dbLanguage = null;
            /*foreach (DBLanguage l in LanguageMenu.Items) {
                if (l.Text == LanguageComboBox.Text) {
                    dbLanguage = l;
                    break;
                }
            }*/
            if (dbLanguage == null) {
                dbLanguage = new DBLanguage(0, LanguageTextBox.Text);
            }
            StatementBuilderServer.UpdateStatement(SelectedStatementCode, dbLanguage, StatementTextTextBox.Text, false);
        }

        private void Initialize() {
            StatementBuilderServer.FileName = FileName;
            StatementBuilderServer.OnSchemaLoaded += OnSchemaLoaded;
            StatementBuilderServer.Initialize();
            //DataTable Grid View
            DataTableStatementsGridView.DataSource = StatementBuilderServer.DataTableStatementTable;
            DataTableStatementsGridView.SelectionChanged += new EventHandler(OnSelectionChanged);
            DataTableStatementsGridView.CellValueChanged += new DataGridViewCellEventHandler(OnCellValueChanged);
            foreach (DataGridViewColumn col1 in DataTableStatementsGridView.Columns) {
                if (col1.Name == "DTST_NAME") {
                    col1.Visible = true;
                    col1.Width = DataTableStatementsGridView.Width - 4;
                    col1.HeaderText = "DataTable Name";
                } else {
                    col1.Visible = false;
                }

            }
            //Command Grid View
            CommandStatementsGridView.DataSource = StatementBuilderServer.CommandStatementTable;
            CommandStatementsGridView.SelectionChanged += new EventHandler(OnSelectionChanged);
            CommandStatementsGridView.CellValueChanged += new DataGridViewCellEventHandler(OnCellValueChanged);
            foreach (DataGridViewColumn col2 in CommandStatementsGridView.Columns) {
                if (col2.Name == "COST_NAME") {
                    col2.Visible = true;
                    col2.Width = CommandStatementsGridView.Width - 4;
                    col2.HeaderText = "Command Name";
                } else {
                    col2.Visible = false;
                }

            }
            StatementBuilderServer.Modified = false;
        }


        private void StatementTextTextBox_TextChanged(object sender, EventArgs e) {
           
        }

        private void button5_Click(object sender, EventArgs e) {
            UpdateData(); 
        }
   }
}