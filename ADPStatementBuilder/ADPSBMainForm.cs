using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Configuration;
using Cati.ADP.Objects;
using Cati.ADP.Common;

namespace ADPStatementBuilder {
    public partial class ADPSBMainForm : Form {
        public ADPSBMainForm() {
            InitializeComponent();
            Initialize();
        }

        private class ConfigHandler : ConfigurationSection {
            public ConfigHandler() {
            }
            [ConfigurationProperty("LastLoadedSchema")]
            public string LastLoadedSchema {
                get { return (string)this["LastLoadedSchema"]; }
                set { this["LastLoadedSchema"] = value; }
            }
        }

        #region User Interface Events
        private void commandStatementRadio_CheckedChanged(object sender, EventArgs e) {
            if (SelectedStatement != null) {
                SelectedStatement.StatementType = "Command";
            }
        }
        private void queryStatementRadio_CheckedChanged(object sender, EventArgs e) {
            if (SelectedStatement != null) {
                SelectedStatement.StatementType = "Query";
            }
        }
        private void statementList_CellContentClick(object sender, DataGridViewCellEventArgs e) {
        }
        private void newButton_Click(object sender, EventArgs e) {
            NewSchema();
        }
        private void loadButton_Click(object sender, EventArgs e) {
            LoadSchema();
        }
        private void saveButton_Click(object sender, EventArgs e) {
            SaveSchema();
        }
        private void saveAsButton_Click(object sender, EventArgs e) {
            SaveSchemaAs();
        }
        private void statementsGuid_SelectionChanged(object sender, EventArgs e) {
            LoadActiveStatement();
        }
        private void statementsGuid_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
            if (FileName != "") {
                LoadActiveStatement();
                Modified = true;
            }
        }
        private void ADPSBMainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = !AllowCloseSchema();
        }
        #endregion

        #region Property Fields
        private bool creatingFile = false;
        private bool modified = false;
        private ADPConnectionInfo info;
        private ADPSession session;
        private ADPStoredStatementIDCollection statementList;
        private ADPLoadOptions loadOptions;
        #endregion

        #region Properties
        private bool Modified {
            get { return modified; }
            set { modified = value; }
        }
        private string FileName {
            get {
                //read value from the config file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigHandler configSection = (ConfigHandler)config.GetSection("Custom");
                if (configSection != null) {
                    string fileName = configSection.LastLoadedSchema;
                    if ((File.Exists(fileName)) || (creatingFile)) {
                        return fileName;
                    }
                }
                return "";
            }
            set {
                //Write value to the config file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigHandler configSection = (ConfigHandler)config.GetSection("Custom");
                if (configSection == null) {
                    configSection = new ConfigHandler();
                    config.Sections.Add("Custom", configSection);
                }
                configSection.LastLoadedSchema = value;
                configSection.SectionInformation.ForceSave = true;
                config.Save(ConfigurationSaveMode.Full);
                UpdateControls();
            }
        }
        private ADPStoredStatementID SelectedStatement {
            get {
                DataGridViewRow row = statementsGuid.CurrentRow;
                if (row != null) {
                    if (row.DataBoundItem != null) {
                        return (ADPStoredStatementID)row.DataBoundItem;
                    }
                }
                return null;
            }
        }
        #endregion

        #region Actions
        private void UpdateControls() {
            if (FileName == "") {
                statementsPanel.Enabled = false;
                saveButton.Enabled = false;
                saveAsButton.Enabled = false;
            } else {
                statementsPanel.Enabled = true;
                saveButton.Enabled = true;
                saveAsButton.Enabled = true;
            }
            if (info != null) {
                info.DatabaseName = FileName;
            }
            UpdateTitle();
        }
        private void Initialize() {
            info = new ADPConnectionInfo();
            info.DatabaseDriver = "XmlDataSet";
            loadOptions = new ADPLoadOptions();
            loadOptions.AutomaticReload = false;
            session = new ADPSession(info, ADPProviderType.Local);
            UpdateControls();
            if (info.DatabaseName != "") {
                LoadSchemaFile();
            }
        }
        private void LoadSchemaFile() {
            object o = session.Load<ADPStoredStatementID>(loadOptions);
            statementList = new ADPStoredStatementIDCollection(session, (o as IList));
            statementList.DeleteOnRemove = true;
            statementsSource.DataSource = statementList;
            Modified = false;
            UpdateControls();
        }
        private bool AllowCloseSchema() {
            if (Modified) {
                DialogResult result = MessageBox.Show("Do you want to save changes before to close the current project?",
                                                      Application.ProductName,
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Question);
                switch (result) {
                    case DialogResult.Yes:
                        SaveSchema("Inform the name of your new schema file!");
                        return true;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }
        private void NewSchema() {
            if (!AllowCloseSchema()) {
                return;
            }
            FileName = "";
            if (statementList != null) {
                statementList.Clear();
            }
            SaveSchema("Inform the name of your new schema file!");
            UpdateControls();
        }
        private void LoadSchema() {
            if (!AllowCloseSchema()) {
                return;
            }
            openFileDialog.FileName = FileName;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                FileName = openFileDialog.FileName;
                LoadSchemaFile();
            }
        }
        private bool SaveSchema() {
            return SaveSchema("Inform the new name to your schema file");
        }
        private bool SaveSchemaAs() {
            return SaveSchemaAs("Inform the new name to your schema file");
        }

        private bool SaveSchema(string dialogTitle) {
            if (FileName == "") {
                return SaveSchemaAs(dialogTitle);
            } else {
                session.BeginPersist();
                if (statementList == null) {
                    LoadSchemaFile();
                }
                statementList.Persist();
                session.EndPersist();
                Modified = false;
                UpdateControls();
                return true;
            }
        }
        private bool SaveSchemaAs(string dialogTitle) {
            saveFileDialog.Title = dialogTitle;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                creatingFile = true;
                try {
                    FileName = saveFileDialog.FileName;
                    return SaveSchema(dialogTitle);
                } finally {
                    creatingFile = false;
                }
            }
            return false;
        }

        private string GetStatementText(int statementCode, int dbLanguageCode, bool isCommand) {
            /*DataRow[] rows;
            if (isCommand) {
                rows = CommandStatementTextTable.Select(String.Format("(COST_CODE = {0}) and (CSTT_CODE = '{1}')", statementCode, dbLanguageCode));
                if (rows.Length == 0) {
                    return "";
                } else {
                    DataRow row = rows[0];
                    return (string)(row["CSTT_TEXT"]);
                }
            } else {
                rows = DataTableStatementTextTable.Select(String.Format("(DTST_CODE = {0}) and (DSTT_CODE = '{1}')", statementCode, dbLanguageCode));
                if (rows.Length == 0) {
                    return "";
                } else {
                    DataRow row = rows[0];
                    return (string)(row["DSTT_TEXT"]);
                }
            }*/
            return null;
        }
        private void LoadActiveStatement() {
            /*if ((activeDataGridView != null) && (activeDataGridView.SelectedRows.Count > 0)) {
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
            }*/
        }
        private int SelectedStatementCode {
            get {
                /*if (activeDataGridView == null) {
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
                }*/
                return 0;
            }
        }
        private void LoadStatementText(string dbLanguage, bool isCommand) {
            /*int statementCode = SelectedStatementCode;
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
            StatementTextTextBox.Text = StatementBuilderServer.GetStatementText(statementCode, selectedLanguage.Code, isCommand);*/
        }
        private void UpdateTitle() {
            string modFlag = "";
            if (Modified) {
                modFlag = "*";
            }
            Text = String.Format("ADP Statement Builder [{0}]{1}",
                                      FileName,
                                      modFlag);
        }
        private void UpdateData() {
            /*DBLanguage dbLanguage = null;
            if (dbLanguage == null) {
                dbLanguage = new DBLanguage(0, LanguageTextBox.Text);
            }
            StatementBuilderServer.UpdateStatement(SelectedStatementCode, dbLanguage, StatementTextTextBox.Text, false);*/
        }
        #endregion

        private void languageCombo_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void statementsSource_CurrentChanged(object sender, EventArgs e) {

        }

        private void statementsGuid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e) {
        }

        private void statementsSource_ListChanged(object sender, ListChangedEventArgs e) {
        }

        private void closeButton_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void statementsPanel_Paint(object sender, PaintEventArgs e) {
            
        }

        private void panel1_Click(object sender, EventArgs e) {
            
        }

        private void statementsGuid_SelectionChanged_1(object sender, EventArgs e) {
            if (SelectedStatement != null) {
                switch (SelectedStatement.StatementType) {
                    case "Query":
                        queryStatementRadio.Checked = true;
                        break;
                    case "Command":
                        commandStatementRadio.Checked = true;
                        break;
                    default:
                        SelectedStatement.StatementType = "Query";
                        queryStatementRadio.Checked = true;
                        break;
                }
            }
        }

        /* private static List<DBLanguage> GetStatementLanguages(int statementCode, bool isCommand) {
            DataRow[] rows;
            List<DBLanguage> result;
            if (isCommand) {
                rows = CommandStatementTextTable.Select(String.Format("(COST_CODE = {0})", statementCode));
                result = new List<DBLanguage>();
                foreach (DataRow row in rows) {
                    DBLanguage l = new DBLanguage((int)(row["CSTT_CODE"]), (string)(row["CSTT_LANGUAGE"]));
                    result.Add(l);
                }
            } else {
                rows = DataTableStatementTextTable.Select(String.Format("(DTST_CODE = {0})", statementCode));
                result = new List<DBLanguage>();
                foreach (DataRow row in rows) {
                    DBLanguage l = new DBLanguage((int)(row["DSTT_CODE"]), (string)(row["DSTT_LANGUAGE"]));
                    result.Add(l);
                }
            }
            return result;
        }*/
        /*private static void UpdateStatement(int statementCode, DBLanguage dbLanguage, string statementText, bool isCommand) {
            if (isCommand) {
                DataRow[] rows = CommandStatementTextTable.Select(String.Format("(COST_CODE = {0}) and (CSTT_CODE = '{1}')", statementCode, dbLanguage.Code));
                if (rows.Length == 0) {
                    DataRow row = CommandStatementTextTable.NewRow();
                    row["COST_CODE"] = statementCode;
                    row["CSTT_LANGUAGE"] = dbLanguage.Text;
                    row["CSTT_TEXT"] = statementText;
                    DataTableStatementTextTable.Rows.Add(row);
                } else {
                    DataRow row = rows[0];
                    row["COST_CODE"] = statementCode;
                    row["CSTT_LANGUAGE"] = dbLanguage.Text;
                    row["CSTT_TEXT"] = statementText;
                }
            } else {
                DataRow[] rows = DataTableStatementTextTable.Select(String.Format("(DTST_CODE = {0}) and (DSTT_CODE = '{1}')", statementCode, dbLanguage.Code));
                if (rows.Length == 0) {
                    DataRow row = DataTableStatementTextTable.NewRow();
                    row["DTST_CODE"] = statementCode;
                    row["DSTT_LANGUAGE"] = dbLanguage.Text;
                    row["DSTT_TEXT"] = statementText;
                    DataTableStatementTextTable.Rows.Add(row);
                } else {
                    DataRow row = rows[0];
                    row["DTST_CODE"] = statementCode;
                    row["DSTT_LANGUAGE"] = dbLanguage.Text;
                    row["DSTT_TEXT"] = statementText;
                }
            }
            DataTableStatementTextTable.AcceptChanges();
            Modified = true;
        }*/
    }
}