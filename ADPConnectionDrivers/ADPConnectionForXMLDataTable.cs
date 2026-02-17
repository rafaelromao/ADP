using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    public class ADPConnectionForXmlDataTable : ADPBaseConnection, IADPConnection {
        public ADPConnectionForXmlDataTable() {
        }
        private ADPConnectionInfo info;
        public ADPConnectionInfo Info {
            get { return info; }
            set { info = value; }
        }
        private string folderName = null;
        private bool inTransaction = false;
        public bool Open() {
            try {
                folderName = Info.DatabaseName;
                if (!Directory.Exists(folderName)) {
                    Directory.CreateDirectory(folderName);
                }
                return true;
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to open file: " + e.Message);
                throw e;
            }
        }
        public void Close() {
        }
        private void CheckActiveTransaction() {
            if (!inTransaction) {
                throw new ADPNoActiveTransactionException();
            }
        }
        private void CheckNoActiveTransaction() {
            if (inTransaction) {
                throw new ADPActiveTransactionException();
            }
        }
        private string GetFullFileName(string fileName) {
            string fullFileName = folderName;
            if (fullFileName[fullFileName.Length-1] != '\\') {
                fullFileName = fullFileName + "\\";
            }
            fullFileName = fullFileName + fileName;
            return fullFileName;
        }
        public Guid StartTransaction() {
            try {
                CheckNoActiveTransaction();
                inTransaction = true;
                return Guid.NewGuid();
            } catch (Exception e) {
                throw e;
            }
        }
        private static Dictionary<string, DataTable> transactionBuffers = new Dictionary<string, DataTable>();
        private static Dictionary<string, string> tableFields = new Dictionary<string, string>();
        public void Rollback() {
            try {
                CheckActiveTransaction();
                inTransaction = false;
            } catch (Exception e) {
                throw e;
            }
        }
        public void Commit() {
            try {
                CheckActiveTransaction();
                foreach (string fileName in transactionBuffers.Keys) {
                    if (File.Exists(fileName)) {
                        File.Delete(fileName);
                    }
                    DataTable dt = transactionBuffers[fileName];
                    dt.WriteXml(fileName, XmlWriteMode.WriteSchema);
                }
                inTransaction = false;
            } catch (Exception e) {
                throw e;
            }
        }
        public DataTable ExecuteSelectStatement(string selectStatement, params ADPParam[] parameters) {
            try {
                //Decode statement
                //SELECT FIELD1, FIELD2, FIELD3 FROM TABLENAME WHERE FIELD1 = FIELD2
                selectStatement = selectStatement.ToUpper();                   
                int posSpace = selectStatement.IndexOf(" ");
                int posFrom = selectStatement.IndexOf("FROM");
                //FIELD1, FIELD2, FIELD3
                string fields = selectStatement.Substring(posSpace, posFrom - posSpace).Trim(); 
                int posWhere = selectStatement.IndexOf("WHERE") + 5;
                int posOrder = selectStatement.IndexOf("ORDER") + 5;
                //FIELD1 = FIELD2
                string condition = "";
                if (posWhere > 4) {
                    if (posOrder > 4) {
                        condition = selectStatement.Substring(posWhere, posOrder - posWhere).Trim();
                    } else {
                        condition = selectStatement.Substring(posWhere).Trim();
                    }
                }
                string sort = "";
                if (posOrder > 4) {
                    sort = selectStatement.Substring(posOrder).Trim();
                }
                ADPStringList fieldList = new ADPStringList(fields, ",", true);
                //Get file name
                if (posWhere == 4) {
                    if (posOrder > 4) {
                        posWhere = posOrder - 5;
                    } else {
                        posWhere = selectStatement.Length;
                    }
                } else {
                    posWhere = posWhere - 5;
                }
                posFrom = posFrom + 4;
                string tableName = selectStatement.Substring(posFrom, posWhere - posFrom).Trim();
                //Store fields to be used in the delete and update commands
                tableFields[GetFullFileName(tableName)] = fields;
                //Load the file
                DataTable dt;
                if ((inTransaction) && (transactionBuffers.ContainsKey(GetFullFileName(tableName)))) {
                    dt = transactionBuffers[GetFullFileName(tableName)];
                } else {
                    if (File.Exists(GetFullFileName(tableName))) {
                        dt = new DataTable();
                        dt.ReadXml(GetFullFileName(tableName));
                    } else {
                        dt = new DataTable();
                        foreach (string f in fieldList) {
                            DataColumn c = new DataColumn(f);
                            dt.Columns.Add(c);
                        }
                    }
                    transactionBuffers[GetFullFileName(tableName)] = dt;//Clone
                }
                dt.TableName = tableName;
                //Apply filter
                if (condition != "") {
                    //Replace parameters
                    if (parameters != null) {
                        foreach (ADPParam param in parameters) {
                            condition = condition.Replace(":"+param.Name.ToUpper(), "'"+Convert.ToString(param.Value)+"'");
                            condition = condition.Replace("@"+param.Name.ToUpper(), Convert.ToString(param.Value));
                        }
                    }
                    //select the desired rows
                    DataRow[] rows = dt.Select(condition, sort);
                    DataTable dt1 = new DataTable();
                    dt1.TableName = tableName;
                    foreach (string f in fieldList) {
                        DataColumn c = new DataColumn(f);
                        dt1.Columns.Add(c);
                    }
                    foreach (DataRow row in rows) {
                        object[] rowValues = new object[dt1.Columns.Count];
                        int i = 0;
                        foreach (DataColumn c in dt1.Columns) {
                            object o = row[c.ColumnName];
                            rowValues[i] = o;
                            i++;
                        }
                        dt1.Rows.Add(rowValues);
                    }
                    dt = dt1;
                }
                return dt;
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to ExecuteSelectStatement: " + e.Message);
                throw e;
            }
        }

        public void ExecuteCommandStatement(string commandStatement, params ADPParam[] parameters) {
            try {
                CheckActiveTransaction();
                //Decode statement
                //INSERT INTO TABLENAME (FIELD1, FIELD2, FIELD3) VALUES (VALUE1, VALUE2, VALUE3)
                //DELETE FROM TABLENAME WHERE FIELD1 = VALUE1 AND (FIELD2 = VALUE2)
                //UPDATE TABLENAME SET FIELD1 = VALUE1, FIELD2 = VALUE2 WHERE FIELD3 = VALUE3
                //Identify command
                commandStatement = commandStatement.ToUpper();
                int k1 = commandStatement.IndexOf(" ");
                string commandId = commandStatement.Substring(0, k1).Trim();
                string tableName;
                switch (commandId) { 
                    case "INSERT":
                        int k2 = commandStatement.IndexOf("INTO") + 5;
                        int k3 = commandStatement.IndexOf("(") - 1;
                        tableName = commandStatement.Substring(k2, k3 - k2).Trim();
                        ExecuteInsertCommand(tableName, commandStatement, parameters, true);
                        return;
                    case "UPDATE":
                        int k4 = 7;
                        int k5 = commandStatement.IndexOf("SET");
                        tableName = commandStatement.Substring(k4, k5 - k4).Trim();
                        ExecuteUpdateCommand(tableName, commandStatement, parameters);
                        return;
                    case "DELETE":
                        int k6 = commandStatement.IndexOf("FROM") + 5;
                        int k7 = commandStatement.IndexOf("WHERE") - 1;
                        tableName = commandStatement.Substring(k6, k7 - k6).Trim();
                        ExecuteDeleteCommand(tableName, commandStatement, parameters);
                        return;
                }
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to ExecuteCommandStatement: " + e.Message);
                throw e;
            }
        }

        private void ExecuteDeleteCommand(string tableName, string commandStatement, ADPParam[] parameters) {
            //DELETE FROM TABLENAME WHERE FIELD1 = VALUE1 AND (FIELD2 = VALUE2)
            if (!tableFields.ContainsKey(GetFullFileName(tableName))) {
                throw new Exception("You must to perform a select before to try to delete!");
            }
            int k1 = commandStatement.IndexOf("WHERE") + 5;
            string condition = "";
            if (k1 == 4) {
                //Clear
                if (transactionBuffers.ContainsKey(GetFullFileName(tableName))) {
                    transactionBuffers[GetFullFileName(tableName)] = null;
                }
            } else {
                condition = commandStatement.Substring(k1).Trim();
                string fields = tableFields[GetFullFileName(tableName)];
                DataTable dt1 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1}", fields, tableName));
                DataTable dt2 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1} WHERE {2}", fields, tableName, condition), parameters);
                DataTable dt3 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1} WHERE 1 = 2", fields, tableName));
                //Clear
                if (transactionBuffers.ContainsKey(GetFullFileName(tableName))) {
                    transactionBuffers[GetFullFileName(tableName)] = null;
                }
                foreach (DataRow row1 in dt1.Rows) {
                    bool row1Found = false;
                    foreach (DataRow row2 in dt2.Rows) {
                        row1Found = true;
                        for (int i = 0; i < row2.ItemArray.Length; i++) {
                            string o1 = (string)row1.ItemArray[i];
                            string o2 = (string)row2.ItemArray[i];
                            if (o1 != o2) {                                                                                            
                                row1Found = false;
                                break;
                            }
                        }
                        if (row1Found) {
                            break;
                        }
                    }
                    //If row exists in dt1 and not exists in dt2 then add it to dt3
                    if (!row1Found) {
                        object[] rowValues = new object[dt3.Columns.Count];
                        int i = 0;
                        foreach (DataColumn c in dt3.Columns) {
                            object o = row1[c.ColumnName];
                            rowValues[i] = o;
                            i++;
                        }
                        dt3.Rows.Add(rowValues);
                    }
                }
                if (transactionBuffers.ContainsKey(GetFullFileName(tableName))) {
                    transactionBuffers[GetFullFileName(tableName)] = dt3;//Clone
                }
            }
            if (!inTransaction) {
                Commit();
            }
        }
        private void ExecuteUpdateCommand(string tableName, string commandStatement, ADPParam[] parameters) {
            //UPDATE TABLENAME SET FIELD1 = VALUE1, FIELD2 = VALUE2 WHERE FIELD3 = VALUE3 
            if (!tableFields.ContainsKey(GetFullFileName(tableName))) {
                throw new Exception("You must to perform a select before to try to update!");
            }
            string fields = tableFields[GetFullFileName(tableName)];
            int k1 = commandStatement.IndexOf("WHERE") + 5;
            string condition = "";
            if (k1 > 4) {
                condition = commandStatement.Substring(k1).Trim();
            }
            DataTable dt1, dt2, dt3;
            if (condition != "") {
                dt1 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1} WHERE {2}", fields, tableName, condition), parameters);
            } else {
                dt1 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1}", fields, tableName));
            }
            dt2 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1}", fields, tableName));
            dt3 = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1} WHERE 1 = 2", fields, tableName));
            foreach (DataRow row2 in dt2.Rows) {
                bool row2Found = false;
                //Check if the row was changed
                foreach (DataRow row1 in dt1.Rows) {
                    row2Found = true;
                    for (int i = 0; i < row1.ItemArray.Length; i++) {
                        string o1 = Convert.ToString(row1.ItemArray[i]);
                        string o2 = Convert.ToString(row2.ItemArray[i]);
                        if (o1 != o2) {
                            row2Found = false;
                            break;
                        }
                    }
                    if (row2Found) {
                        break;
                    }
                }
                //Change data in memory
                if (row2Found) {
                    foreach (ADPParam param in parameters) {
                        row2[param.Name] = param.Value;
                    }
                }
                //Add rows to the dt3
                object[] rowValues = new object[dt3.Columns.Count];
                int j = 0;
                foreach (DataColumn c in dt3.Columns) {
                    object o = row2[c.ColumnName];
                    rowValues[j] = o;
                    j++;
                }
                dt3.Rows.Add(rowValues);
            }
            if (transactionBuffers.ContainsKey(GetFullFileName(tableName))) {
                transactionBuffers[GetFullFileName(tableName)] = dt3;//Clone
            }
            if (!inTransaction) {
                Commit();
            }
        }
        private void ExecuteInsertCommand(string tableName, string commandStatement, ADPParam[] parameters, bool commit) {
            //INSERT INTO TABLENAME (FIELD1, FIELD2, FIELD3) VALUES (VALUE1, VALUE2, VALUE3)
            if (!tableFields.ContainsKey(GetFullFileName(tableName))) {
                throw new Exception("You must to perform a select before to try to insert!");
            }
            int k1 = commandStatement.IndexOf("(") + 1;
            int k2 = commandStatement.IndexOf(")");
            string fields = commandStatement.Substring(k1, k2 - k1).Trim();
            int k3 = commandStatement.IndexOf("VALUES") + 8;
            string values = commandStatement.Substring(k3);
            values = values.Substring(0, values.Length-1);
            ADPStringList list = new ADPStringList(values, ",", true);
            values = list.DelimitedText;
            //Replace parameters
            if (parameters != null) {
                foreach (ADPParam param in parameters) {
                    values = values.Replace(":"+param.Name.ToUpper(), Convert.ToString(param.Value));
                    values = values.Replace("@"+param.Name.ToUpper(), Convert.ToString(param.Value));
                }
            }
            DataTable dt = ExecuteSelectStatement(String.Format("SELECT {0} FROM {1}", fields, tableName));
            ADPStringList valueList = new ADPStringList(values, ",", true);
            dt.Rows.Add(valueList.ToArray());
            if ((!inTransaction) && (commit)) {
                Commit();
            }
        }

        public object GetKey(string keyId) {
            try {
                return Guid.NewGuid();
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to GetKey: " + e.Message);
                throw e;
            }
        }
        public override DbParameter CreateDbParameter() {
            throw new NotSupportedException();
        }
    }
}
