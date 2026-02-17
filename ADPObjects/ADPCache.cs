using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Store lists of references of objects for future use
    /// </summary>
    public sealed class ADPCache {
        /// <summary>
        /// Creates a new cache and enable it
        /// </summary>
        /// <param name="cacheSession">
        /// Session to be used by the cache to create new objects and access the database
        /// </param>
        public ADPCache(ADPSession cacheSession) {
            StartCacheTimeOutThread();
            session = cacheSession;
            objectList = new ADPCollection<ADPObject>(session);
        }
        /// <summary>
        /// Creates a new cache
        /// </summary>
        /// <param name="cacheSession">
        /// Session to be used by the cache to create new objects and access the database
        /// </param>
        /// <param name="enable">
        /// Indicate if the cache must be enabled after created
        /// </param>
        public ADPCache(ADPSession cacheSession, bool enable)
            : this(cacheSession) {
            Enabled = enable;
        }
        /// <summary>
        /// List of objects stored. One list for each object type
        /// </summary>
        private ADPCollection<ADPObject> objectList = null;

        /// <summary>
        /// Session to be used by the cache to create new objects and access the database
        /// </summary>
        private ADPSession session;
        /// <summary>
        /// Starts the CacheTimeOutThread
        /// </summary>
        private void StartCacheTimeOutThread() {
            CacheTimeOutThread = new Thread(new ThreadStart(CacheTimeOutThreadStarter));
            CacheTimeOutThread.IsBackground = true;
            CacheTimeOutThread.Priority = ThreadPriority.BelowNormal;
            CacheTimeOutThread.Start();
        }
        private object cacheLock = new object();
        private bool enabled = true;
        /// <summary>
        /// Get and set if the cache is enabled.
        /// If false, the cache will not store any object
        /// </summary>
        public bool Enabled {
            get { return enabled; }
            set { 
                enabled = value;
                Clear();
            }
        }
        /// <summary>
        /// Indicate the amount of time an object can rely on the cache 
        /// before to be considered old and be removed
        /// </summary>
        public TimeSpan CacheTimeOut = new TimeSpan(1, 0, 0);
        /// <summary>
        /// Callback method executed by the CacheTimeOutThread
        /// </summary>
        private void CacheTimeOutThreadStarter() {
            while (true) {
                Thread.Sleep(1000);
                if (!Enabled) {
                    continue;
                }
                lock (cacheLock) {
                    int k = 0;
                    while (k < objectList.Count) {
                        ADPObject o = objectList[k];
                        TimeSpan ts = DateTime.Now - o.LastSessionLoadingTime;
                        if (ts.TotalHours > CacheTimeOut.TotalHours) {
                            objectList.Remove(o);
                        } else {
                            k++;
                        }
                    }
                    GC.Collect();
                }
            }
        }
        /// <summary>
        /// Thread responsible to remove old objects from the cache at every second
        /// </summary>
        private Thread CacheTimeOutThread;

        /// <summary>
        /// Add an object to the cache
        /// </summary>
        /// <param name="obj">
        /// Object to be added
        /// </param>
        public void Add(ADPObject obj) {
            if (!Enabled) {
                return;
            }
            lock (cacheLock) {
                foreach (ADPObject o in objectList) { 
                    if (o.Equals(obj)) {
                        return;
                    }
                }
                objectList.Add(obj);
            }
        }
        /// <summary>
        /// Add a list of objects to the cache
        /// </summary>
        /// <param name="objectList">
        /// List of objects to be added
        /// </param>
        /// <remarks>
        /// If the objects contained in the objectList does not 
        /// derive from ADPObject an invalid cast operation
        /// exception will be raised
        /// </remarks>
        public void Add(IList objectList) {
            if (!Enabled) {
                return;
            }
            lock (cacheLock) {
                foreach (object o in objectList) {
                    Add((ADPObject)o);
                }
            }
        }
        /// <summary>
        /// Remove an object from the cache
        /// </summary>
        /// <param name="obj">
        /// Object to be removed
        /// </param>
        public void Remove(ADPObject obj) {
            objectList.Remove(obj);
        }

        /// <summary>
        /// Try to find an object on the cache
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be found
        /// </typeparam>
        /// <param name="key">
        /// Key to be matched
        /// </param>
        /// <returns>
        /// Object found or null if no object found
        /// </returns>
        public ADPObject Find<T>(object key) where T : ADPObject {
            string s = "(TypeName = '{0}') and (Key = {1}))";
            s = String.Format(s, typeof(T).Name, key);
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), s);
            ADPCollection<ADPObject> list = objectList.Find(criteria);
            if (list.Count > 0) {
                return list[0];
            } else {
                return null;
            }
        }
        /// <summary>
        /// Try to find objects on the cache that matchs a given criteria
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be found
        /// </typeparam>
        /// <param name="propertyName">
        /// Property to be matched
        /// </param>
        /// <param name="propertyValue">
        /// Value to be matched
        /// </param>
        /// <returns>
        /// A list containing the objects found or an 
        /// empty list if no objects found
        /// </returns>
        public ADPCollection<T> Find<T>(string propertyName, object propertyValue) where T : ADPObject {
            string s = "(TypeName = '{0}') and ({1} = '{2}'))";
            s = String.Format(s, typeof(T).Name, propertyName, propertyValue);
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), s);
            ADPCollection<ADPObject> list = objectList.Find(criteria);
            return new ADPCollection<T>(session, list);
        }
        /// <summary>
        /// Try to find objects on the cache that matchs a given criteria
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be found
        /// </typeparam>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <returns>
        /// A list containing the objects found or an 
        /// empty list if no objects found
        /// </returns>
        public ADPCollection<T> Find<T>(ADPFilterCriteria filterCriteria) where T : ADPObject {
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T));
            string s;
            if (filterCriteria == null) {
                s = "(TypeName = '{0}')";
                s = String.Format(s, typeof(T).Name);
            } else {
                s = filterCriteria.FilterExpression;
                if (s == "()") {
                    s = "(TypeName = '{0}')";
                    s = String.Format(s, typeof(T).Name);
                } else {
                    s = "(TypeName = '{0}') and {1}";
                    s = String.Format(s, typeof(T).Name, filterCriteria.FilterExpression);
                }
            }
            criteria.FilterExpression = s;
            ADPCollection<ADPObject> list = objectList.Find(criteria);
            return new ADPCollection<T>(session, list);
        }
        /// <summary>
        /// Try to find objects of the given type in the cache
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to be found
        /// </typeparam>
        /// <returns>
        /// A list containing the objects found or an 
        /// empty list if no objects found
        /// </returns>
        public ADPCollection<T> Find<T>() where T : ADPObject {
            string s = "(TypeName = '{0}')";
            s = String.Format(s, typeof(T).Name);
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), s);
            ADPCollection<ADPObject> list = objectList.Find(criteria);
            return new ADPCollection<T>(session, list);

        }
        /// <summary>
        /// Removes all references from the cache and call GC.Collect
        /// </summary>
        public void Clear() {
            lock (cacheLock) {
                objectList = new ADPCollection<ADPObject>(session);
            }
            GC.Collect();
        }
    }
}
