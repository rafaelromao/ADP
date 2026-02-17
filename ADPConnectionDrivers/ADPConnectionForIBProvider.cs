using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Encapsulate a connection to a Database
    /// </summary>
    public sealed class ADPConnectionForIBProvider : ADPBaseConnection, IADPConnection {
        OleDbConnection connection;
        OleDbTransaction transaction;
        public ADPConnectionForIBProvider() {
        }
        private ADPConnectionInfo info;
        public ADPConnectionInfo Info {
            get { return info; }
            set { 
                info = value;
                connection = new OleDbConnection();
                connection.ConnectionString =
                    "provider=LCPI.IBProvider;" +
                    String.Format("data source={0}:{1};", info.DatabaseServer, info.DatabaseName) +
                    String.Format("user id={0};", info.DatabaseUserName) +
                    String.Format("password={0};", info.DatabasePassword) +
                    "auto_commit=true;" +
                    "free_threading=false";//disable the default connection pooling
            }
        }
        public bool Open() {
            try {
                if ((connection.State == ConnectionState.Closed) || (connection.State == ConnectionState.Broken)) {
                    connection.Open();
                    while (connection.State != ConnectionState.Open) {
                        Thread.Sleep(1);
                    }
                }
                return true;
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to connect to Database: " + e.Message);
                throw e;
            }
        }
        public void Close() {
            if (connection != null) {
                connection.Close();
            }            
        }
        private void CheckActiveTransaction() {
            if (transaction == null) {
                throw new ADPNoActiveTransactionException();
            }
        }
        private void CheckNoActiveTransaction() {
            if (transaction != null) {
                throw new ADPActiveTransactionException();
            }
        }
        public Guid StartTransaction() {
            try {
                CheckNoActiveTransaction();
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                return Guid.NewGuid();
            } catch (Exception e) {
                throw e;
            }
        }
        public void Rollback() {
            try {
                CheckActiveTransaction();
                transaction.Rollback();
                transaction = null;
            } catch (Exception e) {
                throw e;
            }
        }
        public void Commit() {
            try {
                CheckActiveTransaction();
                transaction.Commit();
                transaction = null;
            } catch (Exception e) {
                throw e;
            }
        }
        public DataTable ExecuteSelectStatement(string selectStatement, params ADPParam[] parameters) {
            try {
                OleDbCommand command = new OleDbCommand(selectStatement, connection);
                FillDbCommandParameters(command, parameters);
                command.Transaction = transaction;
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                dt.TableName = Convert.ToString(Guid.NewGuid());
                return dt;
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to ExecuteSelectStatement: " + e.Message);
                throw e;
            }
        }
        public void ExecuteCommandStatement(string commandStatement, params ADPParam[] parameters) {
            try {
                CheckActiveTransaction();
                OleDbCommand command = new OleDbCommand(commandStatement, connection);
                FillDbCommandParameters(command, parameters);
                command.Transaction = transaction;
                command.ExecuteNonQuery();
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to ExecuteCommandStatement: " + e.Message);
                throw e;
            }
        }
        public object GetKey(string keyId) {
            try {
                string statement = String.Format("SELECT GEN_ID({0},1) AS NEW_KEY FROM RDB$Database", keyId);
                OleDbCommand command = new OleDbCommand(statement, connection);
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                dt.TableName = Convert.ToString(Guid.NewGuid());
                return dt.Rows[0][0];
            } catch (Exception e) {
                ADPTracer.Print(this, "Error on trying to GetKey: " + e.Message);
                throw e;
            }
        }
        public override DbParameter CreateDbParameter() {
            return new OleDbParameter();
        }
    }
}
