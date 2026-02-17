using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Cati.ADP.Common {
    /// <summary>
    /// Interface used to expose the available database access methods
    /// </summary>
    public interface IADPProvider {
        /// <summary>
        /// Try to ping the given host in the given port
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
        /// True if an answer be received
        /// </returns>
        bool Ping(string hostName, int port, int timeOut);
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
        Guid Login(ADPConnectionInfo connectionInfo);
        /// <summary>
        /// Start a new transaction
        /// </summary>
        /// <param name="databaseSessionID">
        /// Identify the database
        /// </param>
        /// <returns>
        /// Return an GUID that identifies the new transaction
        /// </returns>
        Guid StartTransaction(Guid DatabaseSessionID);
        /// <summary>
        /// Rolls a transaction back
        /// </summary>
        /// <param name="transactionID">
        /// Identify the transaction to be rolled back
        /// </param>
        void Rollback(Guid transactionID);
        /// <summary>
        /// Commit a transaction
        /// </summary>
        /// <param name="transactionID">
        /// Identify the transaction to be commited
        /// </param>
        void Commit(Guid transactionID);
        /// <summary>
        /// Get a connection to be to execute query statements
        /// </summary>
        /// <param name="databaseSessionID">
        /// Identify the database
        /// </param>
        /// <returns>
        /// Connection id for the acquired connection
        /// </returns>
        Guid GetConnection(Guid DatabaseSessionID);
        /// <summary>
        /// Release a previous acquired connection
        /// If this method is not called after a GetConnection, 
        /// the connection will never be released, causing the pool to overflow
        /// </summary>
        /// <param name="connectionID">
        /// Identify the connection to be released
        /// </param>
        void ReleaseConnection(Guid connectionID);

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
        DataTable ExecuteSelectStatement(Guid connectionID, string selectStatement, params ADPParam[] parameters);
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
        DataTable ExecuteSelectStatementInTransaction(Guid transactionID, string selectStatement, params ADPParam[] parameters);
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
        void ExecuteCommandStatement(Guid transactionID, string commandStatement, params ADPParam[] parameters);

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
        object GetKey(string keyId, Guid DatabaseSessionID);

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
        string GetSQLStatement(string statementId, Guid DatabaseSessionID);
    }
}
