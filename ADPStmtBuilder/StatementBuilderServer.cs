using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Configuration;
using System.IO;

namespace ADPStmtBuilder {
    internal class DBLanguage {
        public DBLanguage(int code, string text) {
            Code = code;
            Text = text;
        }
        public string Text = "";
        public int Code = 0;
        public override string ToString() { 
            return Text;
        }
    }

    internal static class StatementBuilderServer {

        #region Private Fields
        private static bool modified = false;
        #endregion

        #region Public Fields
        public static bool Modified {
            get { return modified; }
            set {
                modified = value;
                FileName = FileName;
            }
        }
        /// <summary>
        /// Read and Write the currently open file name in the app.config file
        /// </summary>
        public static string fileName = "";
        public static string FileName {
            get { return fileName; }
            set {
                fileName = value;
                if (OnSchemaLoaded != null) {
                    OnSchemaLoaded(null, null);
                }
            }
        }
        #endregion

        #region Private Methods
        private static void LoadSchemaFile(string fileName) {
            if (File.Exists(fileName)) {
                StatementsDataSet.Clear();
                StatementsDataSet.ReadXml(fileName);
                Modified = false;
            } else {
                FileName = "";
            }
            if (OnSchemaLoaded != null) {
                OnSchemaLoaded(null, null);
            }
        }
        #endregion

        #region Public Methods and Events
        public static bool AllowCloseSchema() {
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
        public static void NewSchema() {
            if (!AllowCloseSchema()) {
                return;
            }
            StatementsDataSet.Clear();
            FileName = "";
            if (OnSchemaLoaded != null) {
                OnSchemaLoaded(null, null);
            }
            SaveSchema("Inform the name of your new schema file!");
            LoadSchemaFile(FileName);
        }
        public static void LoadSchema() {
            if (!AllowCloseSchema()) {
                return;
            }
            if (OpenFileDialog.ShowDialog() == DialogResult.OK) {
                LoadSchemaFile(OpenFileDialog.FileName);
                FileName = OpenFileDialog.FileName;
            }
        }
        public static void SaveSchema() {
            SaveSchema("Inform the new name to your schema file");
        }
        public static void SaveSchema(string dialogTitle) {
            if (FileName == "") {
                SaveSchemaAs(dialogTitle);
            } else {
                StatementsDataSet.WriteXml(FileName, XmlWriteMode.WriteSchema);
                Modified = false;
            }
        }
        public static void SaveSchemaAs() {
            SaveSchemaAs("Inform the new name to your schema file");
        }
        public static void SaveSchemaAs(string dialogTitle) {
            SaveFileDialog.Title = dialogTitle;
            if (SaveFileDialog.ShowDialog() == DialogResult.OK) {
                FileName = SaveFileDialog.FileName;
                SaveSchema(dialogTitle);
            }        
        }
        public static event EventHandler OnSchemaLoaded;
        public static string GetStatementText(int statementCode, int dbLanguageCode, bool isCommand) {
            DataRow[] rows;
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
            }
        }
        public static List<DBLanguage> GetStatementLanguages(int statementCode, bool isCommand) {
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
        }
        public static void UpdateStatement(int statementCode, DBLanguage dbLanguage, string statementText, bool isCommand) {
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
        }
        #endregion

        #region Designer Fields
        private static System.Data.DataSet StatementsDataSet;

        public static System.Data.DataTable DataTableStatementTable;
        private static System.Data.DataColumn DTST_NAME_Column;
        private static System.Data.DataColumn DTST_TEXT_Column;
        private static System.Data.DataColumn DTST_CODE_Column;

        public static System.Data.DataTable DataTableStatementTextTable;
        private static System.Data.DataColumn DSTT_CODE_Column;
        private static System.Data.DataColumn DTST_CODE_FK_Column;
        private static System.Data.DataColumn DSTT_LANGUAGE_Column;
        private static System.Data.DataColumn DSTT_TEXT_Column;

        public static System.Data.DataTable CommandStatementTable;
        private static System.Data.DataColumn COST_NAME_Column;
        private static System.Data.DataColumn COST_TEXT_Column;
        private static System.Data.DataColumn COST_CODE_Column;

        public static System.Data.DataTable CommandStatementTextTable;
        private static System.Data.DataColumn CSTT_CODE_Column;
        private static System.Data.DataColumn COST_CODE_FK_Column;
        private static System.Data.DataColumn CSTT_LANGUAGE_Column;
        private static System.Data.DataColumn CSTT_TEXT_Column;

        private static System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private static System.Windows.Forms.SaveFileDialog SaveFileDialog;
        #endregion

        #region Designer Methods
        public static void Initialize() {
            #region Create components
            StatementsDataSet = new System.Data.DataSet();

            DataTableStatementTable = new System.Data.DataTable();
            DTST_NAME_Column = new System.Data.DataColumn();
            DTST_TEXT_Column = new System.Data.DataColumn();
            DTST_CODE_Column = new System.Data.DataColumn();

            DataTableStatementTextTable = new System.Data.DataTable();
            DSTT_CODE_Column = new System.Data.DataColumn();
            DTST_CODE_FK_Column = new System.Data.DataColumn();
            DSTT_LANGUAGE_Column = new System.Data.DataColumn();
            DSTT_TEXT_Column = new System.Data.DataColumn();

            CommandStatementTable = new System.Data.DataTable();
            COST_NAME_Column = new System.Data.DataColumn();
            COST_TEXT_Column = new System.Data.DataColumn();
            COST_CODE_Column = new System.Data.DataColumn();

            CommandStatementTextTable = new System.Data.DataTable();
            CSTT_CODE_Column = new System.Data.DataColumn();
            COST_CODE_FK_Column = new System.Data.DataColumn();
            CSTT_LANGUAGE_Column = new System.Data.DataColumn();
            CSTT_TEXT_Column = new System.Data.DataColumn();

            OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            #endregion

            #region Initialize Components
            ((System.ComponentModel.ISupportInitialize)(StatementsDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(DataTableStatementTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(DataTableStatementTextTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(CommandStatementTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(CommandStatementTextTable)).BeginInit();

            // 
            // StatementsDataSet
            // 
            StatementsDataSet.DataSetName = "Statements";
            StatementsDataSet.Tables.AddRange(new System.Data.DataTable[] {
            DataTableStatementTable,
            DataTableStatementTextTable,
            CommandStatementTable,
            CommandStatementTextTable});
            // 
            // DataTableStatementTable
            // 
            DataTableStatementTable.Columns.AddRange(new System.Data.DataColumn[] {
            DTST_NAME_Column,
            DTST_TEXT_Column,
            DTST_CODE_Column});
            DataTableStatementTable.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "DTST_CODE"}, true),
            new System.Data.UniqueConstraint("DTST_IX_NAME_Constraint", new string[] {
                        "DTST_NAME"}, false)});
            DataTableStatementTable.PrimaryKey = new System.Data.DataColumn[] {DTST_CODE_Column};
            DataTableStatementTable.TableName = "DTST_DATATABLE_STATEMENT";
            // 
            // DTST_NAME_Column
            // 
            DTST_NAME_Column.AllowDBNull = false;
            DTST_NAME_Column.Caption = "Name";
            DTST_NAME_Column.ColumnName = "DTST_NAME";
            // 
            // DTST_TEXT_Column
            // 
            DTST_TEXT_Column.Caption = "Statement Text";
            DTST_TEXT_Column.ColumnName = "DTST_TEXT";
            // 
            // DTST_CODE_Column
            // 
            DTST_CODE_Column.AllowDBNull = false;
            DTST_CODE_Column.AutoIncrement = true;
            DTST_CODE_Column.ColumnName = "DTST_CODE";
            DTST_CODE_Column.DataType = typeof(int);
            // 
            // DataTableStatementTextTable
            // 
            DataTableStatementTextTable.Columns.AddRange(new System.Data.DataColumn[] {
            DSTT_CODE_Column,
            DTST_CODE_FK_Column,
            DSTT_LANGUAGE_Column,
            DSTT_TEXT_Column});
            DataTableStatementTextTable.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "DSTT_CODE"}, true)});
            DataTableStatementTextTable.PrimaryKey = new System.Data.DataColumn[] { DSTT_CODE_Column };
            DataTableStatementTextTable.TableName = "DSTT_STATEMENT_TEXT";
            // 
            // DSTT_CODE_Column
            // 
            DSTT_CODE_Column.AutoIncrement = true;
            DSTT_CODE_Column.AllowDBNull = false;
            DSTT_CODE_Column.ColumnName = "DSTT_CODE";
            DSTT_CODE_Column.DataType = typeof(int);
            // 
            // DTST_CODE_FK_Column
            // 
            DTST_CODE_FK_Column.AllowDBNull = false;
            DTST_CODE_FK_Column.ColumnName = "DTST_CODE";
            DTST_CODE_FK_Column.DataType = typeof(int);
            // 
            // DSTT_LANGUAGE_Column
            // 
            DSTT_LANGUAGE_Column.AllowDBNull = false;
            DSTT_LANGUAGE_Column.ColumnName = "DSTT_LANGUAGE";
            // 
            // DSTT_TEXT_Column
            // 
            DSTT_TEXT_Column.ColumnName = "DSTT_TEXT";
            // 
            // CommandStatementTable
            // 
            CommandStatementTable.Columns.AddRange(new System.Data.DataColumn[] {
            COST_NAME_Column,
            COST_TEXT_Column,
            COST_CODE_Column});
            CommandStatementTable.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "COST_CODE"}, true),
            new System.Data.UniqueConstraint("COST_IX_NAME_Constraint", new string[] {
                        "COST_NAME"}, false)});
            CommandStatementTable.PrimaryKey = new System.Data.DataColumn[] {
        COST_CODE_Column};
            CommandStatementTable.TableName = "COST_COMMAND_STATEMENT";
            // 
            // COST_NAME_Column
            // 
            COST_NAME_Column.AllowDBNull = false;
            COST_NAME_Column.Caption = "Name";
            COST_NAME_Column.ColumnName = "COST_NAME";
            // 
            // COST_TEXT_Column
            // 
            COST_TEXT_Column.Caption = "Statement Text";
            COST_TEXT_Column.ColumnName = "COST_TEXT";
            // 
            // COST_CODE_Column
            // 
            COST_CODE_Column.AllowDBNull = false;
            COST_CODE_Column.AutoIncrement = true;
            COST_CODE_Column.ColumnName = "COST_CODE";
            COST_CODE_Column.DataType = typeof(int);
            // 
            // CommandStatementTextTable
            // 
            CommandStatementTextTable.Columns.AddRange(new System.Data.DataColumn[] {
            CSTT_CODE_Column,
            COST_CODE_FK_Column,
            CSTT_LANGUAGE_Column,
            CSTT_TEXT_Column});
            CommandStatementTextTable.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "COST_CODE"}, true)});
            CommandStatementTextTable.PrimaryKey = new System.Data.DataColumn[] {CSTT_CODE_Column };
            CommandStatementTextTable.TableName = "CSTT_STATEMENT_TEXT";
            // 
            // CSTT_CODE_Column
            // 
            CSTT_CODE_Column.AutoIncrement = true;
            CSTT_CODE_Column.AllowDBNull = false;
            CSTT_CODE_Column.ColumnName = "CSTT_CODE";
            CSTT_CODE_Column.DataType = typeof(int);
            // 
            // COST_CODE_FK_Column
            // 
            COST_CODE_FK_Column.AllowDBNull = false;
            COST_CODE_FK_Column.ColumnName = "COST_CODE";
            COST_CODE_FK_Column.DataType = typeof(int);
            // 
            // CSTT_LANGUAGE_Column
            // 
            CSTT_LANGUAGE_Column.AllowDBNull = false;
            CSTT_LANGUAGE_Column.ColumnName = "CSTT_LANGUAGE";
            // 
            // CSTT_TEXT_Column
            // 
            CSTT_TEXT_Column.ColumnName = "CSTT_TEXT";
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.DefaultExt = "adpSchema";
            OpenFileDialog.FileName = "openFileDialog1";
            OpenFileDialog.Filter = "ADPSchema|*.adpSchema";
            // 
            // SaveFileDialog
            // 
            SaveFileDialog.DefaultExt = "adpSchema";
            SaveFileDialog.Filter = "ADPSchema|*.adpSchema";

            ((System.ComponentModel.ISupportInitialize)(StatementsDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(DataTableStatementTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(DataTableStatementTextTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(CommandStatementTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(CommandStatementTextTable)).EndInit();
            #endregion

            if (FileName != "") {
                LoadSchemaFile(FileName);
            }
        }
        #endregion


    }
}
