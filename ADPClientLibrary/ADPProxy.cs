using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using Cati.ADP.Common;

namespace Cati.ADP.Client {
    /// <summary>
    /// Scope internal to the namespace;
    /// This object represent a request made to the proxy to execute
    /// a select statement in a separate thread.
    /// </summary>
    internal sealed class ADPRequest {
        public Guid RequestId;
        public Guid DatabaseSessionID;
        public Guid ConnectionId;
        public Guid TransactionId;
        public String StatementId;
        public ADPParam[] Parameters;
    }

    /// <summary>
    /// Provides access to the data handling functions;
    /// Through a ADPProxy you can access a local or remote IADPProvider in order 
    /// to handle your data.
    /// </summary>
    public sealed class ADPProxy {
        /// <summary>
        /// Initialize a new ADPProxy setting a provider of the given type.
        /// </summary>
        /// <param name="providerType">
        /// ADPProviderType.Local  : Initialize a ADPProvider object
        /// ADPProviderType.Remote : Initialize a ADPClient object
        /// </param>
        public ADPProxy(ADPProviderType providerType) {
            requestedDataTables = new Dictionary<Guid, DataTable>();
            provider = ADPProviderFactory.GetProvider(providerType);
        }
        /// <summary>
        /// Provider used to handle the data
        /// </summary>
        private IADPProvider provider;
        /// <summary>
        /// List of DataTables requested using the RequestDataTable and RequestTransactedDataTable methods
        /// </summary>
        private Dictionary<Guid,DataTable> requestedDataTables;
        /// <summary>
        /// Callback method executed by the thread created in the RequestDataTable method
        /// </summary>
        /// <param name="o">
        /// ADPRequest containing the necessary information to execute the request
        /// </param>
        private void RequestDataTableCallBack(Object o) {
            ADPRequest request = (ADPRequest)o;
            string statementText = GetSQLStatement(request.StatementId, request.DatabaseSessionID);
            DataTable dt = ExecuteSelectStatement(request.ConnectionId, statementText, request.Parameters);
            requestedDataTables[request.RequestId] = dt;
        }
        /// <summary>
        /// Callback method executed by the thread created in the RequestTransactedDataTable method
        /// </summary>
        /// <param name="o">
        /// ADPRequest containing the necessary information to execute the request
        /// </param>
        private void RequestTransactedDataTableCallBack(Object o) {
            ADPRequest request = (ADPRequest)o;
            string statementText = GetSQLStatement(request.StatementId, request.DatabaseSessionID);
            DataTable dt = ExecuteSelectStatementInTransaction(request.TransactionId, statementText, request.Parameters);
            requestedDataTables[request.RequestId] = dt;
        }
        /// <summary>
        /// Start a separate thread to get a dataset from the server
        /// </summary>
        /// <returns>
        /// A guid that identify your request and must be used to get the result using the method GetRequestedDataTable()
        /// </returns>
        public Guid RequestDataTable(Guid databaseSessionID, Guid connectionID, string dataTableID, params ADPParam[] parameters) {
            ADPRequest request = new ADPRequest();
            request.RequestId = Guid.NewGuid();
            request.ConnectionId = connectionID;
            request.DatabaseSessionID = databaseSessionID;
            request.StatementId = dataTableID;
            request.Parameters = parameters;
            ThreadPool.QueueUserWorkItem((WaitCallback)RequestDataTableCallBack, request);
            return request.RequestId;
        }
        /// <summary>
        /// Start a separate thread to get a dataset from the server inside a transaction
        /// </summary>
        /// <returns>
        /// A guid that identify your request and must be used to get the result using the method GetRequestedDataTable()
        /// </returns>
        public Guid RequestTransactedDataTable(Guid transactionID, string dataTableID, params ADPParam[] parameters) {
            ADPRequest request = new ADPRequest();
            request.RequestId = Guid.NewGuid();
            request.TransactionId = transactionID;
            request.StatementId = dataTableID;
            request.Parameters = parameters;
            ThreadPool.QueueUserWorkItem((WaitCallback)RequestTransactedDataTableCallBack, request);
            return request.RequestId;
        }
        /// <summary>
        /// Return the data table requested and assigned to the given requestId.
        /// Return null if the given timeOut exceed
        /// </summary>
        /// <param name="requestId">
        /// Identify the data table. This value will be returned by the method RequestDataTable()
        /// </param>
        /// <param name="timeOut">
        /// Count of seconds to wait for an answer
        /// </param>
        /// <returns>
        /// Requested DataTable
        /// </returns>
        public DataTable GetRequestedDataTable(Guid requestId, int timeOut) {
            ADPTimeOut t = new ADPTimeOut();
            t.Start(timeOut);
            while (true) {
                if (requestedDataTables.ContainsKey(requestId)) {
                    return requestedDataTables[requestId];
                }
                if (t.TimeOutExceeded()) {
                    return null;
                }
            }
        }

        private bool logged = false;
        /// <summary>
        /// Informs if the ADPProxy has already performed login
        /// </summary>
        public bool Logged {
            get { return logged; }
        }
        /// <summary>
        /// Check if the given ADPProxy has already performed login
        /// </summary>
        /// <param name="dbProxy">
        /// ADPProxy to verify if logged
        /// </param>
        /// <param name="showMessage">
        /// Informs if the method must display an error message before to return
        /// </param>
        /// <param name="throwException">
        /// Informs if the method must throw an exception before to return
        /// </param>
        /// <returns>
        /// True if the given ADPProxy has already logged in
        /// </returns>
        public static bool CheckLogin(ADPProxy dbProxy, bool showMessage, bool throwException) {
            if ((dbProxy == null) || (!dbProxy.Logged)) {
                string message = "You must to perform login before to try this operation!";
                if (showMessage) {
                    MessageBox.Show(message);
                }
                if (throwException) {
                    throw new ADPException(message);
                }
                return false;
            } else {
                return dbProxy.Logged;
            }
        }
        /// <summary>
        /// Performs a ping operation agains the given host name and port
        /// </summary>
        /// <param name="hostName">
        /// Name of the computer to ping
        /// </param>
        /// <param name="port">
        /// Port to ping
        /// </param>
        /// <param name="timeOut">
        /// Amount of time to wait for an answer
        /// </param>
        /// <returns>
        /// True if the host has answered before the timeout exceed
        /// </returns>
        public bool Ping(string hostName, int port, int timeOut) {
            return provider.Ping(hostName, port, timeOut);
        }
        /// <summary>
        /// Performs login
        /// </summary>
        /// <param name="connectionInfo">
        /// Contains all the information necessary to estabilish the connection 
        /// with the IADPProvider and with the database
        /// </param>
        /// <returns>
        /// A GUID that identifies your database inside the ADPConnectionPool
        /// </returns>
        public Guid Login(ADPConnectionInfo connectionInfo) {
            try {
                connectionInfo.DatabaseSessionID = provider.Login(connectionInfo);
                logged = true;
                return connectionInfo.DatabaseSessionID;
            } catch (Exception e) {
                logged = false;
                throw e;
            }
        }
        /// <summary>
        /// Recruit a connection from the pool and starts a transaction on it
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// Identifies the database to connect to
        /// </param>
        /// <returns>
        /// A GUID that identifies your transaction
        /// </returns>
        public Guid StartTransaction(Guid DatabaseSessionID) {
            try {
                Guid g = provider.StartTransaction(DatabaseSessionID);
                return g;
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// Identifies the transaction to be rolled back
        /// </param>
        public void Rollback(Guid transactionID) {
            try {
                provider.Rollback(transactionID);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Commits a transaction
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// Identifies the transaction to be commited
        /// </param>
        public void Commit(Guid transactionID) {
            try {
                provider.Commit(transactionID);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Recruit a connection to be used to perform select statements
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// Identifies the database to connect to
        /// </param>
        /// <returns>
        /// A GUID that identifies your connection
        /// </returns>
        public Guid GetConnection(Guid DatabaseSessionID) {
            try {
                Guid g = provider.GetConnection(DatabaseSessionID);
                return g;
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Releases a connection recruited with the GetConnection()
        /// </summary>
        /// <param name="connectionID">
        /// Identifies the connection to be released
        /// </param>
        public void ReleaseConnection(Guid connectionID) {
            try {
                provider.ReleaseConnection(connectionID);
            } catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Executes a select statement
        /// </summary>
        /// <param name="connectionID">
        /// Identifies the connection to be used to execute the statement
        /// </param>
        /// <param name="selectStatement">
        /// Statement text to be executed
        /// </param>
        /// <param name="parameters">
        /// ADPParam[] containing the params to be passed to the statement
        /// </param>
        /// <returns>
        /// A DataTable containing the result of the execution
        /// </returns>
        public DataTable ExecuteSelectStatement(Guid connectionID, string selectStatement, params ADPParam[] parameters) {
            try {
                DataTable dt = provider.ExecuteSelectStatement(connectionID, selectStatement, parameters);
                return dt;
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Executes a select statement inside a transaction scope
        /// </summary>
        /// <param name="transactionID">
        /// Identifies the transaction to be used
        /// </param>
        /// <param name="selectStatement">
        /// Statement text to be executed
        /// </param>
        /// <param name="parameters">
        /// ADPParam[] containing the params to be passed to the statement
        /// </param>
        /// <returns>
        /// A DataTable containing the result of the execution
        /// </returns>
        public DataTable ExecuteSelectStatementInTransaction(Guid transactionID, string selectStatement, params ADPParam[] parameters) {
            try {
                DataTable dt = provider.ExecuteSelectStatementInTransaction(transactionID, selectStatement, parameters);
                return dt;
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Execute a command (Insert, Delete, Update or DDL) statement
        /// </summary>
        /// <param name="transactionID">
        /// Identifies the transaction to be used
        /// </param>
        /// <param name="selectStatement">
        /// Statement text to be executed
        /// </param>
        /// <param name="parameters">
        /// ADPParam[] containing the params to be passed to the statement
        /// </param>
        public void ExecuteCommandStatement(Guid transactionID, string commandStatement, params ADPParam[] parameters) {
            try {
                provider.ExecuteCommandStatement(transactionID, commandStatement, parameters);
            } catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Get a new primary key from the database
        /// </summary>
        /// <param name="keyId">
        /// The identification of the key generator (Ex: A interbase generator name)
        /// </param>
        /// <param name="DatabaseSessionID">
        /// The database to be accessed
        /// </param>
        /// <returns>
        /// The new key generated
        /// </returns>
        public object GetKey(string keyId, Guid DatabaseSessionID) {
            try {
                object newKey = provider.GetKey(keyId, DatabaseSessionID);
                return newKey;
            } catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Get a statement text from the statement provider
        /// </summary>
        /// <param name="statementId">
        /// Identifies the statement
        /// </param>
        /// <param name="DatabaseSessionID">
        /// Identifies the database
        /// </param>
        /// <returns>
        /// Statement text
        /// </returns>
        public string GetSQLStatement(string statementId, Guid DatabaseSessionID) {
            try {
                return provider.GetSQLStatement(statementId, DatabaseSessionID);
            } catch (Exception e) {
                throw e;
            }
        }
    }
}