using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Implements IADPProvider.
    /// Provide database handling routines
    /// </summary>
    public sealed class ADPProvider : IADPProvider {
        /// <summary>
        /// Connection pool to be used to get connections to the database
        /// </summary>
        public ADPConnectionPool Pool = new ADPConnectionPool();
        /// <summary>
        /// Provides thread sage access to the login method
        /// </summary>
        private Object loginLock = new Object();
        /// <summary>
        /// Not supported
        /// </summary>
        public bool Ping(string hostName, int port, int timeOut) {
            return true;
        }
        /// <summary>
        /// Validate the connection info, log in the database server,
        /// register the connection info and return an identification for the Database
        /// </summary>
        /// <param name="connectionInfo">
        /// Give all the information necessary to stabilish the connection
        /// </param>
        /// <returns>
        /// DatabaseSessionID that identifies the database
        /// </returns>
        public Guid Login(ADPConnectionInfo connectionInfo) {
            lock (loginLock) {
                try {
                    if (connectionInfo == null) {
                        throw new ADPParameterMissingException(String.Format("{0}.Login",this.ToString()), "connectionInfo");
                    }
                    //Try to find a connection info with the same Connectionstring
                    foreach (ADPConnectionInfo ci in Pool.ConnectionInfoList) {
                        if (ci.DatabaseID == connectionInfo.DatabaseID) {
                            return ci.DatabaseSessionID;
                        }
                    }
                    //Create a new connection info
                    Guid g = Guid.NewGuid();
                    connectionInfo.DatabaseSessionID = g;
                    Pool.ConnectionInfoList.Add(connectionInfo);
                    //Try to connect to validate the login
                    IADPConnection c = Pool.GetIdleConnection(g, false);
                    c.Info = connectionInfo;
                    if (!c.Open()) {
                        throw new ADPInvalidDatabaseException();
                    }
                    ADPTracer.Print(this, "Logged in - DatabaseSessionID: {0} - ConnectionID: {1}",
                                                             connectionInfo.DatabaseSessionID, c.ConnectionID);
                    return g;
                } catch (Exception e) {
                    ADPTracer.Print(this, "Exception occured on trying to Login: {0}", e.Message);
                    throw e;
                }
            }
        }
        /// <summary>
        /// Start a new transaction
        /// </summary>
        /// <param name="databaseSessionID">
        /// Identify the database
        /// </param>
        /// <returns>
        /// Return an GUID that identifies the new transaction
        /// </returns>
        public Guid StartTransaction(Guid databaseSessionID) {
            try {
                IADPConnection c = Pool.GetIdleConnection(databaseSessionID, true);
                c.TransactionID = c.StartTransaction();
                c.Idle = false;
                ADPTracer.Print(this, "Transaction Started - ConnectionID: {0} - TransactionID: {1}", 
                                                          c.ConnectionID, c.TransactionID);
                return c.TransactionID;
            } catch (Exception e) {
                ADPTracer.Print(this, String.Format("Exception occured on trying to StartTransaction: {0}", e.Message));
                throw e;
            }
        }
        /// <summary>
        /// Rolls a transaction back
        /// </summary>
        /// <param name="transactionID">
        /// Identify the transaction to be rolled back
        /// </param>
        public void Rollback(Guid transactionID) {
            try {
                IADPConnection c = Pool.GetConnectionByTransactionID(transactionID);
                if (c == null) {
                    throw new ADPParameterMissingException(String.Format("{0}.Rollback", this.ToString()), "ADPConnection");
                } else {
                    c.Rollback();
                    c.Idle = true;
                    c.TransactionID = Guid.Empty;
                }
                ADPTracer.Print(this, "Transaction Aborted - ConnectionID: {0} - TransactionID: {1}",
                                                          c.ConnectionID, transactionID);
            } catch (Exception e) {
                ADPTracer.Print(this, String.Format("Exception occured on trying to RollBack: {0}", e.Message));
                throw e;
            } 
        }
        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="transactionID">
        /// Identify the transaction to be commited
        /// </param>
        public void Commit(Guid transactionID) {
            try {
                IADPConnection c = Pool.GetConnectionByTransactionID(transactionID);
                if (c == null) {
                    throw new ADPParameterMissingException(String.Format("{0}.Commit", this.ToString()), "ADPConnection");
                } else {
                    c.Commit();
                    c.Idle = true;
                    c.TransactionID = Guid.Empty;
                }
                ADPTracer.Print(this, "Transaction Commited - ConnectionID: {0} - TransactionID: {1}",
                                                          c.ConnectionID, transactionID);
            } catch (Exception e) {
                ADPTracer.Print(this, String.Format("Exception occured on trying to Commit: {0}", e.Message));
                throw e;
            }
        }
        /// <summary>
        /// Get a connection to be to execute query statements
        /// </summary>
        /// <param name="databaseSessionID">
        /// Identify the database
        /// </param>
        /// <returns>
        /// Connection id for the acquired connection
        /// </returns>
        public Guid GetConnection(Guid databaseSessionID) {
            try {
                IADPConnection c = Pool.GetIdleConnection(databaseSessionID, true);
                c.Idle = false;
                ADPTracer.Print(this, "Connection Acquired - ConnectionID: {0}",
                                                          c.ConnectionID);
                return c.ConnectionID;
            } catch (Exception e) {
                ADPTracer.Print(this, String.Format("Exception occured on trying to GetConnection: {0}", e.Message));
                throw e;
            }
        }
        /// <summary>
        /// Release a previous acquired connection
        /// If this method is not called after a GetConnection, 
        /// the connection will never be released, causing the pool to overflow
        /// </summary>
        /// <param name="connectionID">
        /// Identify the connection to be released
        /// </param>
        public void ReleaseConnection(Guid connectionID) {
            try {
                IADPConnection c = Pool.GetConnectionByConnectionID(connectionID);
                if (c.TransactionID == Guid.Empty) {
                    c.Idle = true;
                } else {
                    throw new ADPException("Cannot release a connection with an active transaction.\n" +
                                        "You must to commit or rollback its transaction to release it.");
                }
                ADPTracer.Print(this, "Connection Released - ConnectionID: {0}",
                                                          c.ConnectionID);
            } catch (Exception e) {
                ADPTracer.Print(this, String.Format("Exception occured on trying to ReleaseConnection: {0}", e.Message));
                throw e;
            }
        }

        /// <summary>
        /// Execute a query statement
        /// </summary>
        /// <param name="connectionID">
        /// Connection to be used
        /// </param>
        /// <param name="selectStatement">
        /// Statement to be executed
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used
        /// </param>
        /// <returns>
        /// DataTable containing the result of the execution
        /// </returns>
        public DataTable ExecuteSelectStatement(Guid connectionID, string selectStatement, params ADPParam[] parameters) {
            try {
                selectStatement = ParseSQLTokens(selectStatement, parameters);
                IADPConnection c = Pool.GetConnectionByConnectionID(connectionID);
                DataTable dt = c.ExecuteSelectStatement(selectStatement, parameters);
                ADPTracer.Print(this, "Select statement executed: {0}, Connection ID: {1}", selectStatement, connectionID);
                return dt;
            } catch (Exception e) {
                ADPTracer.Print(this, "Exception occured on trying to ExecuteSelectStatement: {0}", e.Message);
                throw e;
            }
        }
        /// <summary>
        /// Execute a query statement
        /// </summary>
        /// <param name="transactionID">
        /// Transaction to be used
        /// </param>
        /// <param name="selectStatement">
        /// Statement to be executed
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used
        /// </param>
        /// <returns>
        /// DataTable containing the result of the execution
        /// </returns>
        public DataTable ExecuteSelectStatementInTransaction(Guid transactionID, string selectStatement, params ADPParam[] parameters) {
            try {
                selectStatement = ParseSQLTokens(selectStatement, parameters);
                IADPConnection c = Pool.GetConnectionByTransactionID(transactionID);
                DataTable dt = c.ExecuteSelectStatement(selectStatement, parameters);
                ADPTracer.Print(this, "Select statement executed in transaction: {0}, TransactionID ID: {1}", selectStatement, transactionID);
                return dt;
            } catch (Exception e) {
                ADPTracer.Print(this, "Exception occured on trying to ExecuteSelectStatementInTransaction: {0}", e.Message);
                throw e;
            }
        }
        /// <summary>
        /// Executes a command statement
        /// </summary>
        /// <param name="transactionID">
        /// Transaction to be used
        /// </param>
        /// <param name="selectStatement">
        /// Statement to be executed
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used
        /// </param>
        public void ExecuteCommandStatement(Guid transactionID, string commandStatement, params ADPParam[] parameters) {
            try {
                commandStatement = ParseSQLTokens(commandStatement, parameters);
                IADPConnection c = Pool.GetConnectionByTransactionID(transactionID);
                c.ExecuteCommandStatement(commandStatement, parameters);
                ADPTracer.Print(this, "Command statement executed: {0}, TransactionID ID: {1}", commandStatement, transactionID);
            } catch (Exception e) {
                ADPTracer.Print(this, "Exception occured on trying to ExecuteCommandStatement: {0}", e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Get a new primary key to the given key id
        /// </summary>
        /// <param name="keyId">
        /// Identifies the primary key that must be generated
        /// </param>
        /// <param name="DatabaseSessionID">
        /// Identify the database
        /// </param>
        /// <returns>
        /// A new primary key to the given key id
        /// </returns>
        public object GetKey(string keyId, Guid DatabaseSessionID) {
            try {
                IADPConnection c = Pool.GetIdleConnection(DatabaseSessionID, true);
                object newKey = c.GetKey(keyId);
                ADPTracer.Print(this, "New key acquired: {0}, New key: {1}", keyId, newKey);
                return newKey;
            } catch (Exception e) {
                ADPTracer.Print(this, "Exception occured on trying to GetKey: {0}", e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Get an stored statement
        /// </summary>
        /// <param name="statementId">
        /// Id of the statement to be loaded
        /// </param>
        /// <param name="databaseSessionID">
        /// Identifies the database
        /// </param>
        /// <returns>
        /// Statement text of the loaded statement
        /// or null if no statement with the given id be found
        /// </returns>
        public string GetSQLStatement(string statementId, Guid databaseSessionID) {
            try {
                IADPConnection c = Pool.GetIdleConnection(databaseSessionID, true);
                ADPStoredStatementProvider statementProvider = GetStatementProvider(c.Info);
                string statementText = statementProvider.GetDataTableStatement(statementId);
                if (statementText == null) {
                    statementText = statementProvider.GetCommandStatement(statementId);
                }
                ADPTracer.Print(this, "Statement text acquired: {0}", statementId);
                return statementText;
            } catch (Exception e) {
                ADPTracer.Print(this, "Exception occured on trying to GetSQLStatement: {0}", e.Message);
                throw e;
            }

        }

        /// <summary>
        /// Statement providers of each registered DatabaseSessionID
        /// </summary>
        private Dictionary<Guid, ADPStoredStatementProvider> statementProviderList = new Dictionary<Guid, ADPStoredStatementProvider>();
        /// <summary>
        /// Get a statement provider according to the DatabaseSessionID of the given ADPConnectionInfo
        /// </summary>
        /// <param name="info">
        /// ADPConnectionInfo that identifies the database
        /// </param>
        /// <returns>
        /// ADPStoredStatementProvider for the given database
        /// </returns>
        private ADPStoredStatementProvider GetStatementProvider(ADPConnectionInfo info) {
            ADPStoredStatementProvider result = null;
            if (statementProviderList.ContainsKey(info.DatabaseSessionID)) {
                result = statementProviderList[info.DatabaseSessionID];
            } else {
                result = new ADPStoredStatementProvider();
                result.LoadStoredStatements(info.DatabaseStoredStatementFileType, info.DatabaseStoredStatementFileName, info.DatabaseLanguage);
                statementProviderList[info.DatabaseSessionID] = result;
            }
            return result;
        }

        /// <summary>
        /// Replace the token of an sql statement by the given parameter values
        /// </summary>
        /// <param name="statementText">
        /// Statement text
        /// </param>
        /// <param name="parameters">
        /// Parameters containing the token values
        /// </param>
        /// <returns>
        /// Statement text with the tokens replaced
        /// </returns>
        private string ParseSQLTokens(string statementText, ADPParam[] parameters) {
            while (statementText.Contains("@")) {
                int k1 = statementText.IndexOf("@");
                int k2 = statementText.IndexOf(",", k1);
                if (k2 == -1) {
                    k2 = statementText.IndexOf(" ", k1);
                }
                if (k2 == -1) {
                    k2 = statementText.IndexOf(")", k1);
                }
                if (k2 == -1) {
                    k2 = statementText.Length;
                }
                string paramName = statementText.Substring(k1 + 1, k2 - k1 - 1);
                ADPParam parameter = null;
                foreach (ADPParam p in parameters) {
                    if ((p.Name == paramName) || (p.ParamType == ADPParamType.SQLToken)) {
                        parameter = p;
                        break;
                    }
                }
                if (parameter != null) {
                    statementText = statementText.Remove(k1, k2 - k1);
                    statementText = statementText.Insert(k1, Convert.ToString(parameter.Value));
                }
            }
            return statementText;
        }
    }
}
