using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Describes an object that may be used to connect to a database.
    /// </summary>
    public interface IADPConnection {
        /// <summary>
        /// Connection info to be used to perform the connection
        /// </summary>
        ADPConnectionInfo Info { get; set; }
        /// <summary>
        /// Indicates if the connection is available for use
        /// </summary>
        bool Idle { get; set; }
        /// <summary>
        /// Identifies the connection inside the connection pool
        /// </summary>
        Guid ConnectionID { get; set; }
        /// <summary>
        /// Identifies the currently active transaction in the connection, if any,
        /// otherwise, returns a empty guid
        /// </summary>
        Guid TransactionID { get; set; }
        /// <summary>
        /// Last time the connection was accessed. It will be used to
        /// know if a connection is idle during much time and may be withdraw
        /// </summary>
        DateTime LastAccessTime { get; set; }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns>
        /// True if the connection was successfully opened
        /// </returns>
        bool Open();
        /// <summary>
        /// Closes the connection
        /// </summary>
        void Close();

        /// <summary>
        /// Start a transaction in the connection
        /// </summary>
        /// <returns>
        /// A guid that identifies the transaction
        /// </returns>
        Guid StartTransaction();
        /// <summary>
        /// Rolls the currently active transaction back, if any
        /// </summary>
        void Rollback();
        /// <summary>
        /// Commits the currently active transaction, if any
        /// </summary>
        void Commit();

        /// <summary>
        /// Execute a query statement against the connection
        /// </summary>
        /// <param name="selectStatement">
        /// Statement to be executed
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used in the statement execution
        /// </param>
        /// <returns>
        /// DataTable containing the result of the query
        /// </returns>
        DataTable ExecuteSelectStatement(string selectStatement, params ADPParam[] parameters);
        /// <summary>
        /// Executes a command statement against the connection
        /// </summary>
        /// <param name="commandStatement">
        /// Statement to be executed
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used in the statement execution 
        /// </param>
        void ExecuteCommandStatement(string commandStatement, params ADPParam[] parameters);
        /// <summary>
        /// Get a new primary key to the given key id
        /// </summary>
        /// <param name="keyId">
        /// Identifies the primary key that must be generated
        /// </param>
        /// <returns>
        /// A new primary key to the given key id
        /// </returns>
        object GetKey(string keyId);

        /// <summary>
        /// Create a new DBParameter of the proper derived type
        /// </summary>
        /// <returns>
        /// The created DBParameter
        /// </returns>
        DbParameter CreateDbParameter();
    }

    /// <summary>
    /// A base object that implements some basic functionalities of
    /// an object that intends to implement the IADPConnection interface
    /// </summary>
    public abstract class ADPBaseConnection {
        /// <summary>
        /// Default contructor that generates the connection id
        /// </summary>
        public ADPBaseConnection() {
            ConnectionID = Guid.NewGuid();
        }
        private bool idle = true;
        /// <summary>
        /// The Idle property of the IADPConnection
        /// </summary>
        public bool Idle {
            get { return idle; }
            set {
                LastAccessTime = DateTime.Now;
                idle = value;
            }
        }
        private Guid connectionID;
        /// <summary>
        /// The ConnectionID property of the IADPConnection
        /// </summary>
        public Guid ConnectionID {
            get { return connectionID; }
            set { connectionID = value; }
        }

        private Guid transactionID;
        /// <summary>
        /// The TransactionID property of the IADPConnection
        /// </summary>
        public Guid TransactionID {
            get { return transactionID; }
            set { transactionID = value; }
        }

        private DateTime lastAccessTime = DateTime.Now;
        /// <summary>
        /// The LastAccessTime property of the IADPConnection
        /// </summary>
        public DateTime LastAccessTime {
            get { return lastAccessTime; }
            set { lastAccessTime = value; }
        }

        /// <summary>
        /// The method CreateDbParameter() to be overriden and 
        /// used in the FillDbCommandParameters() protected method
        /// </summary>
        /// <returns>
        /// Nothing. It throws a NotImplementedException()
        /// </returns>
        public virtual DbParameter CreateDbParameter() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fill the parameters of a DbCommand
        /// </summary>
        /// <param name="command">
        /// DbCommand to be prepared
        /// </param>
        /// <param name="parameters">
        /// Parameters to be used
        /// </param>
        protected void FillDbCommandParameters(DbCommand command, ADPParam[] parameters) {
            foreach (ADPParam p in parameters) {
                if (p.ParamType == ADPParamType.SQLParameter) {
                    DbParameter param = CreateDbParameter();
                    param.ParameterName = p.Name;
                    param.DbType = p.DataType;
                    param.Value = p.Value;
                    command.Parameters.Add(param);
                }
            }
        }
    }
}
