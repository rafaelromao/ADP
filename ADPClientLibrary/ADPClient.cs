using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Client {
    /// <summary>
    /// Implements IADPProvider;
    /// Scope internal to the namespace;
    /// Used by the ADPProxy to provide access to the ADPServer application.
    /// </summary>
    internal sealed class ADPClient : IADPProvider {
        /// <summary>
        /// Creates a new ADPClient
        /// </summary>
        public ADPClient() {
            commandClient = new ADPCommandClient();
        }
        /// <summary>
        /// Command client to be used to send the commands to the ADPCommandServer
        /// </summary>
        private ADPCommandClient commandClient;
        /// <summary>
        /// Not supported
        /// </summary>
        public bool Ping(string hostName, int port, int timeOut) {
            return commandClient.Ping(hostName, port, timeOut);
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
            try {
                commandClient.TimeOut = connectionInfo.ADPServerTimeOut;
                commandClient.HostName = connectionInfo.ADPServerHost;
                commandClient.Port = connectionInfo.ADPServerPort;
                //Request Login to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.Login;
                command.Params[ADPUtils.ConnectionInfo] = connectionInfo.Serialized;
                //Get result
                ADPMessage response = commandClient.Request(command);
                String s = response.Params[ADPUtils.Result];
                Guid g = new Guid(s);
                //Log operation
                ADPTracer.Print(this, "Logged in - DatabaseSessionID: {0}", g);
                return g;
            } catch (Exception e) {
                throw e;
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
        public Guid StartTransaction(Guid DatabaseSessionID) {
            try {
                //Request StartTransaction to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.StartTransaction;
                command.Params[ADPUtils.DatabaseSessionID] = Convert.ToString(DatabaseSessionID);
                //Get result
                ADPMessage response = commandClient.Request(command);
                String s = response.Params[ADPUtils.Result];
                Guid g = new Guid(s);
                ADPTracer.Print(this, "Transaction Started - TransactionID: {0}", g);
                return g;
            } catch (Exception e) {
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
                //Request Rollback to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.Rollback;
                command.Params[ADPUtils.TransactionID] = Convert.ToString(transactionID);
                commandClient.Request(command);
                ADPTracer.Print(this, "Transaction Aborted - TransactionID: {0}", transactionID);
            } catch (Exception e) {
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
                //Request Commit to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.Commit;
                command.Params[ADPUtils.TransactionID] = Convert.ToString(transactionID);
                commandClient.Request(command);
                ADPTracer.Print(this, "Transaction Commited - TransactionID: {0}", transactionID);
            } catch (Exception e) {
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
        public Guid GetConnection(Guid DatabaseSessionID) {
            try {
                //Request Connection to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.GetConnection;
                command.Params[ADPUtils.DatabaseSessionID] = Convert.ToString(DatabaseSessionID);
                //Get result
                ADPMessage response = commandClient.Request(command);
                String s = response.Params[ADPUtils.Result];
                Guid g = new Guid(s);
                ADPTracer.Print(this, "Connection Acquired");
                return g;
            } catch (Exception e) {
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
                //Request Commit to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.ReleaseConnection;
                command.Params[ADPUtils.ConnectionID] = Convert.ToString(connectionID);
                commandClient.Request(command);
                ADPTracer.Print(this, "Connection Released");
            } catch (Exception e) {
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
                //Request DataTableContent to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.ExecuteSelectStatement;
                command.Params[ADPUtils.ConnectionID] = Convert.ToString(connectionID);
                command.Params[ADPUtils.StatementText] = selectStatement;
                //Write parameters
                foreach (ADPParam p in parameters) {
                    command.Params[p.Name] = p.Serialized;
                }
                //Get result
                ADPMessage response = commandClient.Request(command);
                String result = response.Params[ADPUtils.Result];
                DataTable dt = (DataTable)ADPSerializer.Deserialize((new DataTable()).GetType(), result);
                ADPTracer.Print(this, "Select statement executed: {0}, Connection ID: {1}", selectStatement, connectionID);
                return dt;
            } catch (Exception e) {
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
                //Request TransactedDataTableContent to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.ExecuteSelectStatementInTransaction;
                command.Params[ADPUtils.TransactionID] = Convert.ToString(transactionID);
                command.Params[ADPUtils.StatementText] = selectStatement;
                //Write parameters
                foreach (ADPParam p in parameters) {
                    command.Params[p.Name] = p.Serialized;
                }
                //Get result
                ADPMessage response = commandClient.Request(command);
                String result = response.Params[ADPUtils.Result];
                DataTable dt = (DataTable)ADPSerializer.Deserialize((new DataTable()).GetType(), result);
                ADPTracer.Print(this, "Select statement executed in transaction: {0}, Connection ID: {1}", selectStatement, transactionID);
                return dt;
            } catch (Exception e) {
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
                //Request ExecuteSQLCommand to the remote server
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.ExecuteCommandStatement;
                command.Params[ADPUtils.TransactionID] = Convert.ToString(transactionID);
                command.Params[ADPUtils.StatementText] = commandStatement;
                //Write parameters
                foreach (ADPParam p in parameters) {
                    command.Params[p.Name] = p.Serialized;
                }
                commandClient.Request(command);
                ADPTracer.Print(this, "Command statement executed: {0}, TransactionID ID: {1}", commandStatement, transactionID);
            } catch (Exception e) {
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
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.GetKey;
                command.Params[ADPUtils.DatabaseSessionID] = Convert.ToString(DatabaseSessionID);
                command.Params[ADPUtils.KeyId] = Convert.ToString(keyId);
                ADPMessage response = commandClient.Request(command);
                String result = response.Params[ADPUtils.Result];
                ADPTracer.Print(this, "New key acquired: {0}, New key: {1}", keyId, result);
                return result;
            } catch (Exception e) {
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
        public string GetSQLStatement(string statementId, Guid DatabaseSessionID) {
            try {
                ADPMessage command = new ADPMessage();
                command.Id = (int)ADPMessageTypes.GetSQLStatement;
                command.Params[ADPUtils.DatabaseSessionID] = Convert.ToString(DatabaseSessionID);
                command.Params[ADPUtils.StatementID] = statementId;
                ADPMessage response = commandClient.Request(command);
                string result = response.Params[ADPUtils.Result];
                ADPTracer.Print(this, "Statement text acquired: {0}", statementId);
                return result;
            } catch (Exception e) {
                throw e;
            }
        }
    }
}
