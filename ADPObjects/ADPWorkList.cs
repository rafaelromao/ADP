using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Provides local transaction support
    /// </summary>
    public class ADPWorkList {
        /// <summary>
        /// Creates a new worklist
        /// </summary>
        public ADPWorkList() {
            objectList = new List<ADPObject>();
            bufferObjectList = new List<ADPObject>();
        }
        /// <summary>
        /// Store the objects that compose the work list buffer
        /// </summary>
        private List<ADPObject> objectList;
        /// <summary>
        /// Store the original object of the list, before the start of the works
        /// </summary>
        private List<ADPObject> bufferObjectList;
        /// <summary>
        /// Add a new object to the worklist
        /// </summary>
        /// <param name="obj">
        /// Object to be added
        /// </param>
        public void Add(ADPObject obj) {
            if (Working) {
                throw new ADPException("Worklist active. Cannot insert!");
            }
            if (!objectList.Contains(obj)) {
                objectList.Add(obj);
            }
        }
        /// <summary>
        /// Add a list of objects to the worklist
        /// </summary>
        /// <param name="collection">
        /// Collection of objects to be added
        /// </param>
        public void Add(IEnumerable collection) {
            foreach (ADPObject obj in collection) {
                Add(obj);
            }
        }
        /// <summary>
        /// Remove an object from the worklist
        /// </summary>
        /// <param name="obj">
        /// Object to be removed
        /// </param>
        public void Remove(ADPObject obj) {
            if (Working) {
                throw new ADPException("Worklist active. Cannot remove!");
            }
            objectList.Remove(obj);
        }
        /// <summary>
        /// Remove a list of objects from the worklist
        /// </summary>
        /// <param name="collection">
        /// Collection of objects to be removed
        /// </param>
        public void Remove(IEnumerable collection) {
            foreach (ADPObject obj in collection) {
                Remove(obj);
            }            
        }
        /// <summary>
        /// Clear the worklist
        /// </summary>
        public void Clear() {
            if (Working) {
                throw new ADPException("Worklist active. Cannot clear!");
            }
            objectList.Clear();
        }

        private bool working = false;
        /// <summary>
        /// Indicates if the worklist has an active transaction
        /// </summary>
        public bool Working {
            get { return working; }
        }
        /// <summary>
        /// Begins a worklist transaction
        /// </summary>
        public void BeginWork() {
            if (working) {
                throw new ADPException("Worklist already active!");
            }
            bufferObjectList.Clear();
            foreach (ADPObject o in objectList) {
                ADPObject clone = (ADPObject)((ADPObject)o).Clone();
                bufferObjectList.Add(clone);
            }
            working = true;
        }
        /// <summary>
        /// Commits a worklist transaction
        /// </summary>
        public void EndWork() {
            bufferObjectList.Clear();
            working = false;
            GC.Collect();
        }
        /// <summary>
        /// Cancels a worklist transaction
        /// </summary>
        public void CancelWork() { 
            //Remove objects added after BeginWork()
            List<ADPObject> removeList = new List<ADPObject>();
            foreach (ADPObject o in objectList) {
                if (!bufferObjectList.Contains(o)) {
                    removeList.Add(o);
                }
            }
            foreach (ADPObject o in removeList) {
                objectList.Remove(o);
            }
            //Insert objects removed or update objects changed after BeginWork()
            foreach (ADPObject o in bufferObjectList) {
                if (!objectList.Contains(o)) {
                    objectList.Add(o);
                } else { 
                    int k = objectList.IndexOf(o);
                    ADPObject obj = objectList[k];
                    obj.Assign(o);
                }
            }
            working = false;
        }
    }
}
