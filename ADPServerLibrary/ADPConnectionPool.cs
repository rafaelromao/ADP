using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Provides a pool mechanism to handle the database connections
    /// </summary>
    public sealed class ADPConnectionPool {
        /// <summary>
        /// Creates a new ADPConnectionPool
        /// </summary>
        public ADPConnectionPool(){
            //Keeps alive during all the life of the pool removing the expired connections from the pool
            Thread CleanupExpiredConnectionThread = new Thread(new ThreadStart(CleanupExpiredConnections));
            CleanupExpiredConnectionThread.IsBackground = true;
            CleanupExpiredConnectionThread.Priority = ThreadPriority.Lowest;
            CleanupExpiredConnectionThread.Start();
        }
        /// <summary>
        /// List of connection factories to be used by each DatabaseSessionIDs
        /// </summary>
        private Dictionary<Guid, ADPBaseConnectionFactory> connectionFactoryList = new Dictionary<Guid,ADPBaseConnectionFactory>();
        /// <summary>
        /// Get a connection factory to the given connection info
        /// </summary>
        /// <param name="info">
        /// Connection info to be used
        /// </param>
        /// <returns>
        /// Connection factory registered for the given connection info
        /// </returns>
        public ADPBaseConnectionFactory GetConnectionFactory(ADPConnectionInfo info) {
            ADPBaseConnectionFactory connectionFactory = null;
            if (connectionFactoryList.ContainsKey(info.DatabaseSessionID)) {
                connectionFactory = connectionFactoryList[info.DatabaseSessionID];
            } else {
                string assemblyName = info.ADPConnectionFactoryAssemblyName;
                string factoryName = info.ADPConnectionFactoryTypeName;
                connectionFactory = ADPBaseConnectionFactory.GetConnectionFactory(assemblyName, factoryName);
                connectionFactoryList[info.DatabaseSessionID] = connectionFactory;
            }
            return connectionFactory;
        }
        /// <summary>
        /// Provides thread safe access to the ConnectionList
        /// </summary> 
        private Object connectionListLock = new Object();
        /// <summary>
        /// Store the connections handled by the pool
        /// </summary>
        public List<IADPConnection> ConnectionList = new List<IADPConnection>();
        /// <summary>
        /// Store all the registered connection info
        /// </summary>
        public List<ADPConnectionInfo> ConnectionInfoList = new List<ADPConnectionInfo>();
        /// <summary>
        /// Close and dispose connections that keep idle for a long period of time
        /// </summary>
        private void CleanupExpiredConnections() {
            while (true) {
                System.Threading.Thread.Sleep(1000);
                int i = 0;
                while ((ConnectionList.Count > 0) && (i < ConnectionList.Count)) {
                    IADPConnection c = ConnectionList[i];
                    TimeSpan ts = DateTime.Now - c.LastAccessTime;
                    bool idleTimeOutExceeded = (ts.TotalSeconds >= c.Info.DatabaseTimeOut) && (c.Idle);
                    //Every transaction that exceed 6 minutes to complete will be considered broken
                    bool brokenTimeOutExceeded = ts.TotalSeconds >= 360;
                    if (idleTimeOutExceeded || brokenTimeOutExceeded) {
                        if (c.TransactionID != Guid.Empty) {
                            c.Rollback();
                        }
                        c.Close();
                        lock (connectionListLock) {
                            ConnectionList.Remove(c);
                        }
                        //Forces the Garbage Collector to release c;
                        GC.Collect();
                        ADPTracer.Print(this, "Connection Expired: {0}", c.ConnectionID);
                        break;
                    } else {
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Search the pool in order to find an available connection or
        /// create one if necessary.
        /// If the pool is full, loop until find a connection or until
        /// the timeout be exceeded
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// Identifies the database that must be connected
        /// </param>
        /// <returns>
        /// An available connection
        /// </returns>
        public IADPConnection GetIdleConnection(Guid databaseSessionID, bool openConnection) {
            IADPConnection result = null;
            ADPConnectionInfo info = GetConnectionInfo(databaseSessionID);
            ADPTimeOut t = new ADPTimeOut();
            t.Start(info.DatabaseTimeOut);
            int count = 0;
            //Loop until find a connection or timeout exceed
            do {
                //Try to find a connection
                lock (connectionListLock) {
                    foreach (IADPConnection c in ConnectionList) {
                        if (c.Info.DatabaseSessionID == databaseSessionID) {
                            count++;
                            if (c.Idle) {
                                result = c;
                                break;
                            }
                        }
                    }
                    if (result != null) {
                        break;
                    }
                    ADPTracer.Print(this, "Waiting for an available connection!");
                    Thread.Sleep(ADPUtils.ThreadSleepHighInterval);

                    //Create a new connection
                    if (count < info.DatabasePoolSize) {
                        try {
                            ADPBaseConnectionFactory connectionFactory = GetConnectionFactory(info);
                            IADPConnection c = connectionFactory.GetConnection(info.DatabaseDriver);
                            if (c == null) {
                                throw new Exception(String.Format("Invalid driver name: {0}!", info.DatabaseDriver));
                            }
                            c.Info = info;
                            ConnectionList.Add(c);
                            result = c;
                            break;
                        } catch (Exception e) {
                            //If enter here, the connection info may be wrong or
                            //the DatabasePoolSize may be greater than the available connection count
                            ADPTracer.Print(this, "Error on trying to connect to database!.");
                            ADPTracer.Print(this, "This error will be ignored until the timeout exceed.");
                            ADPTracer.Print(this, e.Message);
                        }
                    }
                }
                //Check if the timeout has been exceeded
                if (t.TimeOutExceeded()) {
                    ADPTracer.Print(this, "TimeOut exceeded on trying to get connection!");
                    throw new ADPTimeoutException("GetConnection", info.DatabaseTimeOut);
                }
            } while (true);

            if ((openConnection) && (result != null)) {
                result.Open();
            }
            return result;
        }
        /// <summary>
        /// Find and return a connection according to its transaction id
        /// </summary>
        /// <param name="transactionID">
        /// Id of the transaction holded by the desired connection
        /// </param>
        /// <returns>
        /// Connection that holds the given transaction id or
        /// null if no transaction with the given id be found
        /// </returns>
        public IADPConnection GetConnectionByTransactionID(Guid transactionID) {
            lock (connectionListLock) {
                foreach (IADPConnection c in ConnectionList) {
                    if (c.TransactionID == transactionID) {
                        return c;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Find and return a connection according to its connection id
        /// </summary>
        /// <param name="connectionID">
        /// Id of the desired connection
        /// </param>
        /// <returns>
        /// Connection that holds the given connection id or
        /// null if no connection with the given id be found
        /// </returns>
        public IADPConnection GetConnectionByConnectionID(Guid connectionID) {
            lock (connectionListLock) {
                foreach (IADPConnection c in ConnectionList) {
                    if (c.ConnectionID == connectionID) {
                        return c;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Get the connection info for the given DatabaseSessionID
        /// </summary>
        /// <param name="DatabaseSessionID">
        /// DatabaseSessionID to be searched
        /// </param>
        /// <returns>
        /// Connection info that registered for the given DatabaseSessionID
        /// or null if not connection info with the given DatabaseSessionID be found
        /// </returns>
        public ADPConnectionInfo GetConnectionInfo(Guid DatabaseSessionID) { 
            foreach (ADPConnectionInfo c in ConnectionInfoList) {
                if (c.DatabaseSessionID == DatabaseSessionID) {
                    return c;
                }
            }
            return null;
        }
    }
}
