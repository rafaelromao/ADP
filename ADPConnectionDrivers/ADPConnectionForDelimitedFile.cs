using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Encapsulate a connection to a Database
    /// </summary>
    public sealed class ADPConnectionForDelimitedFile : ADPBaseConnection, IADPConnection {
        public ADPConnectionForDelimitedFile() {
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
        private static Dictionary<string, Stream> transactionBuffers = new Dictionary<string, Stream>();
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
                    Stream stream = transactionBuffers[fileName];
                    if (stream != null) {
                        FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        int len = (int)stream.Length;
                        byte[] bytes = new byte[len];
                        stream.Position = 0;
                        stream.Read(bytes, 0, len);
                        fileStream.Position = 0;
                        fileStream.Write(bytes, 0, len);
                        fileStream.Close();
                    } else {
                        File.Create(fileName);
                    }
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
                Stream fileStream;
                if ((inTransaction) && (transactionBuffers.ContainsKey(GetFullFileName(tableName)))) {
                    fileStream = transactionBuffers[GetFullFileName(tableName)];
                } else {
                    if (File.Exists(GetFullFileName(tableName))) {
                        fileStream = new FileStream(GetFullFileName(tableName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    } else {
                        fileStream = new MemoryStream();
                    }
                    transactionBuffers[GetFullFileName(tableName)] = CloneStream(fileStream);
                }
                int len = (int)fileStream.Length;
                byte[] bytes = new byte[len];
                fileStream.Position = 0;
                fileStream.Read(bytes, 0, len);
                MemoryStream stream = new MemoryStream(bytes, true);
                //Create a data table to store the data
                DataTable dt = new DataTable();
                //Add columns to the data table
                foreach (string f in fieldList) {
                    DataColumn c1 = dt.Columns.Add();
                    c1.ColumnName = f;
                }
                //Fill data table
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                while (!reader.EndOfStream) {
                    string record = reader.ReadLine();
                    string separator = Info.DelimitedFileSeparator;
                    ADPStringList stringValues = new ADPStringList(record, separator, true);
                    DataRow row = dt.Rows.Add(stringValues.ToArray());
                }
                //Replace parameters
                if (parameters != null) {
                    foreach (ADPParam param in parameters) {
                        condition = condition.Replace(":"+param.Name.ToUpper(), "'"+Convert.ToString(param.Value)+"'");
                        condition = condition.Replace("@"+param.Name.ToUpper(), Convert.ToString(param.Value));
                    }
                }
                //Filter and sort data table
                if ((condition != "") || (sort != "")) {
                    DataRow[] rows = dt.Select(condition, sort);
                    //Build result data table
                    dt = new DataTable();
                    foreach (string f in fieldList) {
                        DataColumn c2 = dt.Columns.Add();
                        c2.ColumnName = f;
                    }
                    foreach (DataRow r in rows) {
                        object[] values = new object[dt.Columns.Count];
                        int i = 0;
                        foreach (DataColumn c in dt.Columns) {
                            object o = r[c.ColumnName];
                            values[i] = o;
                            i++;
                        }
                        dt.Rows.Add(values);
                    }
                }
                dt.TableName = tableName;
                return dt;
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to ExecuteSelectStatement: " + e.Message);
                throw e;
            }
        }

        private static Stream CloneStream(Stream stream) {
            int len = (int)stream.Length;
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, len);
            MemoryStream result = new MemoryStream(bytes, 0, len);
            return result;
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
                    //If row exists to dt1 and not exists in dt2 then add it to dt3
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
                foreach (DataRow row in dt3.Rows) {
                    ADPStringList valueList = new ADPStringList("", ",", false);
                    for (int i = 0; i < dt3.Columns.Count; i++) {
                        string s = Convert.ToString(row[i]);
                        valueList.Add(s);
                    }
                    string values = valueList.DelimitedText;
                    string insertStatement = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, fields, values);

                    ExecuteInsertCommand(tableName, insertStatement, null, false);
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
                        string o1 = (string)row1.ItemArray[i];
                        string o2 = (string)row2.ItemArray[i];
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
            //Clear
            if (transactionBuffers.ContainsKey(GetFullFileName(tableName))) {
                transactionBuffers[GetFullFileName(tableName)] = null;
            }
            //Rewrite
            foreach (DataRow row in dt3.Rows) {
                ADPStringList valueList = new ADPStringList("", ",", false);
                for (int i = 0; i < dt3.Columns.Count; i++) {
                    string s = Convert.ToString(row[i]);
                    valueList.Add(s);
                }
                string values = valueList.DelimitedText;
                string insertStatement = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, fields, values);

                ExecuteInsertCommand(tableName, insertStatement, null, false);
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
            int k1 = commandStatement.IndexOf("VALUES") + 8;
            string values = commandStatement.Substring(k1);
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
            values = values.Replace(",", Info.DelimitedFileSeparator);
            //Write new record to the file
            Stream stream;
            bool streamNotFound = !(transactionBuffers.ContainsKey(GetFullFileName(tableName)));
            if (!streamNotFound) {
                streamNotFound = transactionBuffers[GetFullFileName(tableName)] == null;
            }
            if (streamNotFound) {
                stream = new MemoryStream();
                transactionBuffers[GetFullFileName(tableName)] = stream;
            } else {
                stream = transactionBuffers[GetFullFileName(tableName)];
            }
            StreamWriter writer = new StreamWriter(stream);
            int len = (int)stream.Length + (int)writer.NewLine.Length + (int)values.Length;
            byte[] bytes = new byte[len];
            stream.Position = 0;
            for (int i = 0; i < stream.Length; i++) {
                byte b1 = (byte)stream.ReadByte();
                bytes[i] = b1;
            }
            int index = (int)stream.Length;
            if (index > 0) {
                for (int j = 0; j < writer.NewLine.Length; j++) {
                    byte b2 = Convert.ToByte(writer.NewLine[j]);
                    bytes[index + j] = b2;
                }
                index = index + writer.NewLine.Length;
            }
            for (int k = 0; k < values.Length; k++) {
                byte b3 = Convert.ToByte(values[k]);
                bytes[index + k] = b3;
            }
            stream = new MemoryStream(bytes, 0, len);
            transactionBuffers[GetFullFileName(tableName)] = stream;
            if ((!inTransaction) && (commit)) {
                Commit();
            }
        }

        public object GetKey(string keyId) {
            try {
                //keyId must contain the name of the delimited file
                FileStream fileStream = new FileStream(GetFullFileName(keyId), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                object result = fileStream.Length + 1;
                fileStream.Close();
                return result;
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