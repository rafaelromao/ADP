using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using Cati.ADP.Client;
using Cati.ADP.Common;
using Cati.ADP.Server;
using System.Diagnostics;
using System.Windows.Forms;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Represents a asyncronous request of a list of objects
    /// </summary>
    internal sealed class ADPObjectRequest {
        /// <summary>
        /// Identifies the request
        /// </summary>
        internal Guid Id;
        /// <summary>
        /// Filter criteria to be used to perform the request
        /// </summary>
        internal ADPFilterCriteria Criteria;
        /// <summary>
        /// Load options to be used to perform the request
        /// </summary>
        internal ADPLoadOptions LoadOptions;
    }

    /// <summary>
    /// Specifies if the session must perform login automatically
    /// before each proxy call or if you must to call the Login()
    /// method explicitly even than necessary
    /// </summary>
    public enum ADPSessionLoginMode {
        Automatic,
        Manual
    }

    /// <summary>
    /// Options to be considered when performing load operations
    /// </summary>
    public sealed class ADPLoadOptions {
        /// <summary>
        /// Create a new ADPLoadOptions
        /// </summary>
        public ADPLoadOptions() {
        }
        /// <summary>
        /// Create a new ADPLoadOptions
        /// </summary>
        /// <param name="loadDeletedObjects">
        /// Indicates if the new load options will allow load objects marked for deletion
        /// </param>
        /// <param name="loadPersistedObjects">
        /// Indicates if the new load options will allows load persisted objects
        /// </param>
        /// <param name="loadOnlyFirstObject">
        /// Indicates if the new load options will request only the loading of the first object
        /// </param>
        /// <param name="automaticReload">
        /// Indicates if the new load options will allows automatic reloading of the cached objects
        /// </param>
        public ADPLoadOptions(bool loadDeletedObjects, bool loadPersistedObjects, bool loadOnlyFirstObject, bool automaticReload) {
            LoadDeletedObjects = loadDeletedObjects;
            LoadPersistedObjects = loadPersistedObjects;
            LoadOnlyFirstObject = loadOnlyFirstObject;
            AutomaticReload = automaticReload;
        }
        /// <summary>
        /// Indicates if the new load options will allows load objects that does not exists in the database yet
        /// </summary>
        public bool LoadNewObjects = true;
        /// <summary>
        /// Indicates if the new load options will allows load objects that are nor new neither deleted
        /// </summary>
        public bool LoadCurrentObjects = true;
        /// <summary>
        /// Indicates if the new load options will allows load objects marked for deletion
        /// </summary>
        public bool LoadDeletedObjects = false;
        /// <summary>
        /// Indicates if the new load options will allows load objects that already exist on the cache
        /// </summary>
        public bool LoadCachedObjects = true;
        /// <summary>
        /// Indicates if the new load options will allows load objects that already exist on the database
        /// </summary>
        public bool LoadPersistedObjects = true;
        /// <summary>
        /// Indicates if the new load options will request only the loading of the first object
        /// </summary>
        public bool LoadOnlyFirstObject = false;
        /// <summary>
        /// Indicates if the new load options will allows automatic reloading of the cached objects
        /// </summary>
        public bool AutomaticReload = true;
    }

    public sealed class ADPSession {
        /// <summary>
        /// Set the parameters passed to the constructors of the ADPSession
        /// </summary>
        /// <param name="persisterRegister">
        /// Helper object used to register the persisters of the business objects handled by the session
        /// </param>
        /// <param name="providerType">
        /// Indicate if the connection with the data will be local or remote
        /// </param>
        /// <param name="connectionInfoList">
        /// List of connection infos used to target the servers.
        /// More that one item is usefull only when working with
        /// load balance.
        /// </param>
        /// <param name="loginMode">
        /// Indicate if the session must performs login automatically
        /// </param>
        private void Initialize(ADPProviderType providerType, ADPConnectionInfo[] connectionInfoList, ADPSessionLoginMode loginMode) {
            if (connectionInfoList.Length == 0) {
                throw new ADPException("A connection info must be informed!");
            }
            proxy = new ADPProxy(providerType);
            Cache = new ADPCache(this);
            infoList = connectionInfoList;
        }

        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <param name="providerType">
        /// Indicate if the connection with the data will be local or remote
        /// </param>
        /// <param name="connectionInfo">
        /// Connection info to be used to target the server
        /// </param>
        /// <param name="loginMode">
        /// Indicate if the session must performs login automatically
        /// </param>
        public ADPSession(ADPProviderType providerType, ADPConnectionInfo connectionInfo, ADPSessionLoginMode loginMode) {
            Initialize(providerType, new ADPConnectionInfo[] { connectionInfo }, loginMode);
        }
        /// <summary>
        /// Creates a new session with login mode set to automatic
        /// </summary>
        /// <param name="providerType">
        /// Indicate if the connection with the data will be local or remote
        /// </param>
        /// <param name="connectionInfo">
        /// Connection info to be used to target the server
        /// </param>
        public ADPSession(ADPConnectionInfo connectionInfo, ADPProviderType providerType) {
            Initialize(providerType, new ADPConnectionInfo[] { connectionInfo }, ADPSessionLoginMode.Automatic);
        }
        /// <summary>
        /// Creates a new session with the provider type set to remote
        /// </summary>
        /// <param name="connectionInfo">
        /// Connection info to be used to target the server
        /// </param>
        /// <param name="loginMode">
        /// Indicate if the session must performs login automatically
        /// </param>
        public ADPSession(ADPConnectionInfo connectionInfo, ADPSessionLoginMode loginMode) {
            Initialize(ADPProviderType.Remote, new ADPConnectionInfo[] { connectionInfo }, loginMode);
        }
        /// <summary>
        /// Creates a new session with the login mode set to automatic and provider type set to remote
        /// </summary>
        /// <param name="connectionInfo">
        /// Connection info to be used to target the server
        /// </param>
        public ADPSession(ADPConnectionInfo connectionInfo) {
            Initialize(ADPProviderType.Remote, new ADPConnectionInfo[] { connectionInfo }, ADPSessionLoginMode.Automatic);
        }

        /// <summary>
        /// Creates a new session prepared for load balance
        /// </summary>
        /// <param name="providerType">
        /// Indicate if the connection with the data will be local or remote
        /// </param>
        /// <param name="connectionInfoList">
        /// List of connection infos used to target the servers
        /// </param>
        /// <param name="loginMode">
        /// Indicate if the session must performs login automatically
        /// </param>
        public ADPSession(ADPProviderType providerType, ADPConnectionInfo[] connectionInfoList, ADPSessionLoginMode loginMode) {
            Initialize(providerType, connectionInfoList, loginMode);
        }
        /// <summary>
        /// Creates a new session prepared for load balance with login mode set to automatic
        /// </summary>
        /// <param name="providerType">
        /// Indicate if the connection with the data will be local or remote
        /// </param>
        /// <param name="connectionInfoList">
        /// List of connection infos used to target the servers
        /// </param>
        public ADPSession(ADPConnectionInfo[] connectionInfoList, ADPProviderType providerType) {
            Initialize(providerType, connectionInfoList, ADPSessionLoginMode.Automatic);
        }
        /// <summary>
        /// Creates a new session prepared for load balance with provider type set to remote
        /// </summary>
        /// <param name="connectionInfoList">
        /// List of connection infos used to target the servers
        /// </param>
        /// <param name="loginMode">
        /// Indicate if the session must performs login automatically
        /// </param>
        public ADPSession(ADPConnectionInfo[] connectionInfoList, ADPSessionLoginMode loginMode) {
            Initialize(ADPProviderType.Remote, connectionInfoList, loginMode);
        }
        /// <summary>
        /// Creates a new session prepared for load balance with provider type set to remote and login mode set to automatic
        /// </summary>
        /// <param name="connectionInfoList">
        /// List of connection infos used to target the servers
        /// </param>
        public ADPSession(ADPConnectionInfo[] connectionInfoList) {
            Initialize(ADPProviderType.Remote, connectionInfoList, ADPSessionLoginMode.Automatic);
        }

        /// <summary>
        /// List of connection infos used to target the servers
        /// </summary>
        private ADPConnectionInfo[] infoList;
        private ADPProxy proxy;
        /// <summary>
        /// Proxy to be used to reach the data
        /// </summary>
        public ADPProxy Proxy {
            get { return proxy; }
        }
        /// <summary>
        /// Cache to be used to store the loaded objects
        /// </summary>
        internal ADPCache Cache;
        /// <summary>
        /// Indicates if the cache must be used
        /// </summary>
        public bool CacheEnabled {
            get { return Cache.Enabled; }
            set { Cache.Enabled = value; }
        }
        private Guid databaseSessionID;
        /// <summary>
        /// Identifies the session on the remote server
        /// </summary>
        public Guid DatabaseSessionID {
            get { return databaseSessionID; }
        }
        /// <summary>
        /// Indicates if the session must perform login automatically
        /// </summary>
        public ADPSessionLoginMode LoginMode;
        /// <summary>
        /// If loginMode = Automatic, try to login during the info.communicationTimeOut, othrewise, does nothing
        /// </summary>
        private void CheckLogin() {
            if (LoginMode == ADPSessionLoginMode.Automatic) {
                ADPTimeOut t = new ADPTimeOut();
                t.Start(3);
                while (true) {
                    Application.DoEvents();
                    Thread.Sleep(100);
                    try {
                        Login();
                        return;
                    } catch {
                        if (t.TimeOutExceeded()) {
                            throw;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Connects to the server and gets a DatabaseSessionId to be used in the future requests
        /// </summary>
        public void Login() {
            //Try to connect to each one of the registered servers
            databaseSessionID = Guid.Empty;
            Random randomizer = new Random();
            int randomInfoIndex = randomizer.Next(infoList.Length);
            /* 
             * If Length = 5 and randomInfoIndex = 3, i starts with 3 and ends with 7
             * thus, index will start with (3 mod 5) = 3 and ends with (7 mod 5) = 2
             * this way, enumerating all the available connection infos.
             */
            for (int i = randomInfoIndex; i < (infoList.Length + randomInfoIndex); i++) {
                int index = (i % infoList.Length);
                ADPConnectionInfo info = infoList[index];
                if (proxy.Ping(info.ADPServerHost, info.ADPServerPort, 1000)) {
                    databaseSessionID = proxy.Login(info);
                }
            }
            if (databaseSessionID == Guid.Empty) {
                throw new ADPServerNotFoundException();
            }
        }

        /// <summary>
        /// Store all the deleted objects to be removed from the database then the EndPerist() be called
        /// </summary>
        internal List<ADPObject> deletedObjects =  new List<ADPObject>();

        #region Load Methods
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="filterCriteria">
        /// Filter criteria to be matched by the loaded objects
        /// </param>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// if options.LoadOnlyFirstObject:
        ///   returns the first object that matches the given criteria
        ///   or null if no matches found
        /// otherwise:
        ///   returns the list of objects that matches the given criterea
        ///   or an empty list if no matches found
        /// </returns>
        private object LoadRange<T>(ADPFilterCriteria filterCriteria, ADPLoadOptions options) where T : ADPObject {
            Cursor originalCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try {
                ADPCollection<T> resultList = new ADPCollection<T>(this);
                //If the object was not found in the cache and can load the object from the database
                if (options.LoadPersistedObjects) {
                    CheckLogin();
                    ADPCollection<T> persistedList = ADPObject.LoadRange<T>(this, filterCriteria, options.LoadOnlyFirstObject);
                    //Add the objects loaded from the database to the result list
                    foreach (object o in persistedList) {
                        resultList.AddIfNew((T)o);
                    }
                    Cache.Add(persistedList);
                }
                    //If must load the object from the cache first
                else if (options.LoadCachedObjects) {
                    ADPCollection<T> cachedList = Cache.Find<T>(filterCriteria);
                    //Check load options
                    foreach (ADPObject obj in cachedList) {
                        if ((obj.Deleted) && (!options.LoadDeletedObjects)) {
                            continue;
                        }
                        if ((obj.IsNew) && (!options.LoadNewObjects)) {
                            continue;
                        }
                        if ((!obj.IsNew) && (!options.LoadCurrentObjects)) {
                            continue;
                        }
                        //If must reload objects automatically
                        if (options.AutomaticReload) {
                            obj.Reload();
                        }
                        resultList.Add((T)obj);
                    }
                }
                //Return
                if (options.LoadOnlyFirstObject) {
                    if (resultList.Count > 0) {
                        return resultList[0];
                    } else {
                        return null;
                    }
                } else {
                    return resultList;
                }
            } finally {
                Cursor.Current = originalCursor;
            }
        }

        /// <summary>
        /// Load a single object from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="key">
        /// Primary key of the object to be loaded
        /// </param>
        /// <returns>
        /// The loaded object if any object found, otherwise null
        /// </returns>
        public object Load<T>(object key) where T : ADPObject {
            return Load<T>(new ADPLoadOptions(), key);
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="filterCriteria">
        /// Filter criteria to be matched by the loaded objects
        /// </param>
        /// <returns>
        /// Returns the list of objects that matches the given criterea
        /// or an empty list if no matches found
        /// </returns>
        public object Load<T>(ADPFilterCriteria filterCriteria) where T : ADPObject {
            return Load<T>(new ADPLoadOptions(), filterCriteria);
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="propertyName">
        /// Name of the property to be evaluated to filter the loaded objects
        /// </param>
        /// <param name="propertyValue">
        /// Value of the property to be evaluated to filter the loaded objects
        /// </param>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// if options.LoadOnlyFirstObject:
        ///   returns the first object that matches the given criteria
        ///   or null if no matches found
        /// otherwise:
        ///   returns the list of objects that matches the given criterea
        ///   or an empty list if no matches found
        /// </returns>
        public object Load<T>(string propertyName, object propertyValue) where T : ADPObject {
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), String.Format("{0} = '{1}'", propertyName, propertyValue));
            return LoadRange<T>(criteria, new ADPLoadOptions());
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <returns>
        /// Returns the list of all objects of the given type
        /// or an empty list if no object of that type be found
        /// </returns>
        public object Load<T>() where T : ADPObject {
            return Load<T>(new ADPLoadOptions());
        }

        /// <summary>
        /// Load a single object from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="key">
        /// Primary key of the object to be loaded
        /// </param>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// The loaded object if any object found, otherwise null
        /// </returns>
        public object Load<T>(ADPLoadOptions options, object key) where T : ADPObject {
            Cursor originalCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try {
                object result = null;
                //If must load the object from the cache first
                if (options.LoadCachedObjects) {
                    result = Cache.Find<T>(key);
                    //Check load options
                    if (result != null) {
                        ADPObject obj = (ADPObject)result;
                        if ((obj.Deleted) && (!options.LoadDeletedObjects)) {
                            return null;
                        }
                        if ((obj.IsNew) && (!options.LoadNewObjects)) {
                            return null;
                        }
                        if ((!obj.IsNew) && (!options.LoadCurrentObjects)) {
                            return null;
                        }
                        //If must reload objects automatically
                        if (options.AutomaticReload) {
                            obj.Reload();
                        }
                    }
                }
                //If the object was not found in the cache and can load the object from the database
                if ((result == null) && (options.LoadPersistedObjects)) {
                    CheckLogin();
                    result = ADPObject.CreateInstance(this, typeof(T));
                    ((ADPObject)result).Load(key);
                }
                return result;
            } finally {
                Cursor.Current = originalCursor;
            }
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="filterCriteria">
        /// Filter criteria to be matched by the loaded objects
        /// </param>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// if options.LoadOnlyFirstObject:
        ///   returns the first object that matches the given criteria
        ///   or null if no matches found
        /// otherwise:
        ///   returns the list of objects that matches the given criterea
        ///   or an empty list if no matches found
        /// </returns>
        public object Load<T>(ADPLoadOptions options, ADPFilterCriteria filterCriteria) where T : ADPObject {
            return LoadRange<T>(filterCriteria, options);
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="propertyName">
        /// Name of the property to be evaluated to filter the loaded objects
        /// </param>
        /// <param name="propertyValue">
        /// Value of the property to be evaluated to filter the loaded objects
        /// </param>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// if options.LoadOnlyFirstObject:
        ///   returns the first object that matches the given criteria
        ///   or null if no matches found
        /// otherwise:
        ///   returns the list of objects that matches the given criterea
        ///   or an empty list if no matches found
        /// </returns>
        public object Load<T>(ADPLoadOptions options, string propertyName, object propertyValue) where T : ADPObject {
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), String.Format("{0} = '{1}'", propertyName, propertyValue));
            return LoadRange<T>(criteria, options);
        }
        /// <summary>
        /// Load an object or a list of objects from the cache or database, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be loaded
        /// </typeparam>
        /// <param name="options">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// if options.LoadOnlyFirstObject:
        ///   returns the first object of the given type
        ///   or null if object of that type be found
        /// otherwise:
        ///   returns the list of all objects of the given type
        ///   or an empty list if no object of that type be found
        /// </returns>
        public object Load<T>(ADPLoadOptions options) where T : ADPObject {
            return LoadRange<T>(null, options);
        }
        #endregion

        #region Asyncronous Load Methods
        /// <summary>
        /// List of requests that where not obtained yet
        /// </summary>
        private Dictionary<Guid, object> requestedObjects;
        /// <summary>
        /// Callback method of the thread that will perform the asyncronous request
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object requested
        /// </typeparam>
        /// <param name="o">
        /// ADPRequestOptions to be used to perform the request
        /// </param>
        private void RequestThreadStart<T>(object o) where T : ADPObject {
            ADPObjectRequest objectRequest = (ADPObjectRequest)o;
            object result = Load<T>(objectRequest.LoadOptions, objectRequest.Criteria);
            if (requestedObjects == null) {
                requestedObjects = new Dictionary<Guid, object>();
            }
            requestedObjects[objectRequest.Id] = result;
        }
        /// <summary>
        /// Performs an asyncronous request of an object or list of objects, according to the given options
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to be loaded
        /// </typeparam>
        /// <param name="filterCriteria">
        /// Criteria to be matched by the loaded objects
        /// </param>
        /// <param name="loadOptions">
        /// Load options to be considered in the load operations
        /// </param>
        /// <returns>
        /// A guid that identifies your request. You must use it latter to obtain your request result
        /// </returns>
        public Guid Request<T>(ADPFilterCriteria filterCriteria, ADPLoadOptions loadOptions) where T : ADPObject {
            ADPObjectRequest objectRequest = new ADPObjectRequest();
            objectRequest.Id = Guid.NewGuid();
            objectRequest.Criteria = filterCriteria;
            objectRequest.LoadOptions = loadOptions;
            Thread requestObjectThread = new Thread(new ParameterizedThreadStart(RequestThreadStart<T>));
            requestObjectThread.IsBackground = true;
            requestObjectThread.Start(objectRequest);
            return objectRequest.Id;
        }
        /// <summary>
        /// Obtain an object or list of objects requested using the Request() method
        /// </summary>
        /// <param name="requestId">
        /// Identifies the request
        /// </param>
        /// <param name="timeOut">
        /// Amount of time to wait for the request to be finished before to throws a timeout exception
        /// </param>
        /// <returns>
        /// Requested object or list of objects
        /// </returns>
        public object GetRequestedObject(Guid requestId, int timeOut) {
            if (requestedObjects == null) {
                return null;
            }
            ADPTimeOut t = new ADPTimeOut();
            t.Start(timeOut);
            while (!t.TimeOutExceeded()) {
                if (requestedObjects.ContainsKey(requestId)) {
                    return requestedObjects[requestId];
                }
                Thread.Sleep(1);
            }
            return null;
        }
        #endregion

        #region Transaction Handling
        /// <summary>
        /// Amount of calls to BeginPersist()
        /// The same amount of calls to EndPersist() or CancelPersist()
        /// must be done in order to finish the persist operation
        /// </summary>
        private int nestedTransactions = 0;
        /// <summary>
        /// Id of the currently active transaction of this session on the server
        /// </summary>
        internal Guid TransactionID = Guid.Empty;
        /// <summary>
        /// Indicates if a persist operation is in process
        /// </summary>
        /// <returns>
        /// True if there is a persist operation in process
        /// </returns>
        public bool Persisting() {
            return TransactionID != Guid.Empty;
        }
        /// <summary>
        /// If nestedTransactions = 0, begins a persist operation by starting a transaction on the server
        /// otherwise does nothing
        /// </summary>
        public void BeginPersist() {
            if (!Persisting()) {
                Login();
                TransactionID = proxy.StartTransaction(DatabaseSessionID);
            }
            nestedTransactions++;
        }
        /// <summary>
        /// If nestedTransactions = 1, ends a persist operation by commiting the active transaction 
        /// of this session on the server, otherwise does nothing
        /// </summary>
        public void EndPersist() {
            nestedTransactions--;
            if (nestedTransactions == 0) {
                //Remove the deleted objects from the database
                while (deletedObjects.Count > 0) {
                    ADPObject obj = deletedObjects[0];
                    obj.Persist();
                    deletedObjects.Remove(obj);
                } 
                proxy.Commit(TransactionID);
                TransactionID = Guid.Empty;
            }
        }
        /// <summary>
        /// If nestedTransactions = 1, cancells a persist operation by rolling the active transaction 
        /// of this session back on the server, otherwise does nothing
        /// </summary>
        public void CancelPersist() {
            nestedTransactions--;
            if (nestedTransactions == 0) {
                try {
                    proxy.Rollback(TransactionID);
                } catch {
                    //Does not throws, but the transaction will be active in the server till the long connection timeout exceed
                }
                TransactionID = Guid.Empty;
            }
        }
        #endregion
    }
}
