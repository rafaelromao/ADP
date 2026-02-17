using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Cati.ADP.Objects {
    /// <summary>
    /// A generic list that implements all the available binding capabilities
    /// to handle objects derived from the ADPObject
    /// </summary>
    /// <typeparam name="T">
    /// Type of object to be stored
    /// </typeparam>
    public class ADPCollection<T> : BindingList<T>, 
                                    IADPType,
                                    IBindingList, ICloneable,
                                    IRaiseItemChangedEvents,
                                    IBindingListView where T : ADPObject {

        #region Constructors
        /// <summary>
        /// Creates a new list populated with the same contant of the given list
        /// </summary>
        /// <param name="session">
        /// Session used to register the objects created in by this list
        /// </param>
        /// <param name="list">
        /// List containing the objects that will be used to populate the new list
        /// </param>
        public ADPCollection(ADPSession session, ADPCollection<T> list) 
            : base(new List<T>(list)) {
            Session = session;
        }
        /// <summary>
        /// Creates a new list
        /// </summary>
        /// <param name="session">
        /// Session used to register the objects created in by this list
        /// </param>
        public ADPCollection(ADPSession session) 
            : base() {
            Session = session;
        }
        /// <summary>
        /// Creates a new list populated with the same contant of the given list
        /// </summary>
        /// <param name="session">
        /// Session used to register the objects created in by this list
        /// </param>
        /// <param name="list">
        /// List containing the objects that will be used to populate the new list
        /// </param>
        public ADPCollection(ADPSession session, List<T> list) 
            : base(list) {
            Session = session;
        }
        /// <summary>
        /// Creates a new list populated with the same contant of the given list
        /// </summary>
        /// <param name="session">
        /// Session used to register the objects created in by this list
        /// </param>
        /// <param name="list">
        /// List containing the objects that will be used to populate the new list
        /// </param>
        public ADPCollection(ADPSession session, IList list)
            : base(GetListTFromIList(list)) {
            Session = session;
        }
        /// <summary>
        /// Creates a new List<T> populated with the contant of the given list
        /// </summary>
        /// <param name="list">
        /// List containing the objects that will be used to populate the new list
        /// </param>
        /// <returns>
        /// A new List<T> populated with the contant of the given list
        /// </returns>
        private static List<T> GetListTFromIList(IList list) {
            List<T> result = new List<T>();
            foreach (object o in list) {
                result.Add((o as T));
            }
            return result;
        }
        #endregion

        #region Session
        /// <summary>
        /// Session used to register the objects created in by this list
        /// </summary>
        public ADPSession Session;
        #endregion

        #region Search
        /// <summary>
        /// Indicate to the binding mechanism that the list supports searching
        /// </summary>
        protected override bool SupportsSearchingCore {
            get { return true; }
        }
        /// <summary>
        /// Used by the binding mechanisms to find a object in the list
        /// </summary>
        /// <param name="property">
        /// Property to be evaluated
        /// </param>
        /// <param name="key">
        /// Value to be evaluated
        /// </param>
        /// <returns>
        /// Position of the found object in the list or
        /// -1 if no object found
        /// </returns>
        protected override int FindCore(PropertyDescriptor property, object key) {
            //Create a delegate method to perform the comparison
            Predicate<T> pred = delegate(T item) {
                if (property.GetValue(item).Equals(key)) {
                    return true;
                } else {
                    return false;
                }
            };
            //Get the lost to search in
            List<T> list = Items as List<T>;
            if (list == null) {
                return -1;
            }
            //Perform search using index
            return list.FindIndex(pred);
            /* Implementation for searching without to use indexes
            for (int i = 0; i < Count; i++) {
                T item = this[i];
                if (property.GetValue(item).Equals(key)) {
                    return i;
                }
            }
            return -1; // Not found
            */
        }
        /// <summary>
        /// Find an object in the list
        /// </summary>
        /// <param name="property">
        /// Property to be evaluated
        /// </param>
        /// <param name="key">
        /// Value to be evaluated
        /// </param>
        /// <returns>
        /// Position of the found object in the list or
        /// -1 if no object found
        /// </returns>
        public int Find(PropertyDescriptor property, object key) {
            return FindCore(property, key);
        }

        /// <summary>
        /// Try to find in the list an object containing the given key
        /// </summary>
        /// <param name="key">
        /// Key to be matched
        /// </param>
        /// <returns>
        /// Object found or null if no object found
        /// </returns>
        public ADPObject Find(object key) {
            PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(T))["Key"];
            int k = Find(property, key);
            if (k == -1) {
                return null;
            } else {
                return Items[k];
            }
        }
        /// <summary>
        /// Try to find in the list an object equals to the given one
        /// </summary>
        /// <param name="item">
        /// Object to be compared
        /// </param>
        /// <returns>
        /// Object found or null if no object found
        /// </returns>
        public ADPObject Find(T item) {
            foreach (T o in this) { 
                if (o.Equals(item)) {
                    return o;
                }
            }
            return null;
        }
        /// <summary>
        /// Try to find objects that matches the given value in the given properties
        /// </summary>
        /// <param name="propertyName">
        /// Property to be evaluated
        /// </param>
        /// <param name="propertyValue">
        /// Value to be evaluated
        /// </param>
        /// <returns>
        /// A list containing all the abjects that matches the criteria
        /// or an empty list if no matches found
        /// </returns>
        public ADPCollection<T> Find(string propertyName, object propertyValue) {
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(T), String.Format("{0} = '{1}'", propertyName, propertyValue));
            return Find(criteria);
        }
        /// <summary>
        /// Try to find objects that matches the given value in the given properties
        /// </summary>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <returns>
        /// A list containing all the abjects that matches the criteria
        /// or an empty list if no matches found
        /// </returns>
        public ADPCollection<T> Find(ADPFilterCriteria filterCriteria) {
            ADPCollection<T> result;
            if (Count == 0) {
                result = new ADPCollection<T>(Session);
            } else {
                result = new ADPCollection<T>(Session, this);
                if (filterCriteria != null) {
                    result.Filter = filterCriteria.FilterExpression;
                    result.ConsolidateFilteredData();
                }
            }
            return result;
        }

        /// <summary>
        /// Add an object to the list if such list does not contains an object equals to this one
        /// </summary>
        /// <param name="item">
        /// Item to be added
        /// </param>
        /// <returns>
        /// True if the object was added
        /// False if another object equals to the given
        /// item was found in the list
        /// </returns>
        public bool AddIfNew(T item) {
            T obj = (T)Find(item);
            if (obj == null) {
                Add(item);
                return true;
            } else {
                return false;
            }
        }
        #endregion

        #region Sorting
        /// <summary>
        /// Flag used to know if the list is sorted
        /// </summary>
        private bool sorted = false;

        private ListSortDescriptionCollection sortDescriptions = new ListSortDescriptionCollection();
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        private PropertyDescriptor sortProperty = null;

        /// <summary>
        /// Indicate to the binding mechanism that the list supports sorting
        /// </summary>
        protected override bool SupportsSortingCore {
            get { return true; }
        }
        /// <summary>
        /// Used by the binding mechanisms to know if the list is sorted
        /// </summary>
        protected override bool IsSortedCore {
            get { return sorted; }
        }
        /// <summary>
        /// Store a ListSortDirection to be used by the binding mechanisms to perform a single sort
        /// </summary>
        protected override ListSortDirection SortDirectionCore {
            get { return sortDirection; }
        }
        /// <summary>
        /// Store a PropertyDescriptor to be used by the binding mechanisms to perform a single sort
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore {
            get { return sortProperty; }
        }
        /// <summary>
        /// Used by the binding mechanisms to apply a single sort on list
        /// </summary>
        /// <param name="property">
        /// Property to be sorted
        /// </param>
        /// <param name="direction">
        /// Sort direction: ASC or DESC
        /// </param>
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction) {
            /*if (sorted) {
                RemoveSortCore();
            }*/ //The commented code avoid the growing of the OriginalSortOrderList of its items, but it put the performance down
            sortDirection = direction;
            sortProperty = property;
            ADPSortComparer<T> comparer = new ADPSortComparer<T>(property, direction);
            ApplySortInternal(comparer);
        }
        /// <summary>
        /// Apply sort on the list
        /// </summary>
        /// <param name="comparer">
        /// Comparer to be used to determine which object is greater
        /// </param>
        private void ApplySortInternal(ADPSortComparer<T> comparer) {
            ADPObject obj;
            for (int i = 0; i < Count; i++) {
                obj = (ADPObject)(this[i]);
                obj.OriginalSortOrderList.Add(i);
            }
            List<T> listRef = this.Items as List<T>;
            if (listRef == null) {
                return;
            }
            // Let List<T> do the actual sorting based on your comparer
            listRef.Sort(comparer);
            sorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            //Apply sort in the non filtered list
            if (filtered) {
                originalList.ApplySortInternal(comparer);
            }
        }
        /// <summary>
        /// Used by the binding mechanisms to remove sort from the list
        /// </summary>
        protected override void RemoveSortCore() {
            if (!sorted) {
                return;
            }
            sorted = false;
            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(ADPObject))["LastSortOrder"];
            ApplySortCore(prop, ListSortDirection.Ascending);
            sortProperty = null;
            sortDescriptions = null;

            if (filtered) {
                originalList.RemoveSortCore();
            }
        }
        /// <summary>
        /// Apply a single sort on the list 
        /// </summary>
        /// <param name="property">
        /// Property to be sorted
        /// </param>
        /// <param name="direction">
        /// Sort direction: ASC or DESC
        /// </param>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction) {
            ApplySortCore(property, direction);
        }
        /// <summary>
        /// Apply a multiple sort on the list
        /// </summary>
        /// <param name="sorts">
        /// Collection of ListSortDescription to be used to sort
        /// </param>
        public void ApplySort(ListSortDescriptionCollection sorts) {
            sortProperty = null;
            sortDescriptions = sorts;
            ADPSortComparer<T> comparer = new ADPSortComparer<T>(sorts);
            ApplySortInternal(comparer);
        }
        /// <summary>
        /// Apply sort according to a given expression
        /// </summary>
        /// <param name="sortExpression">
        /// Expression indicatino the fields that must be considered,
        /// separated by commas and succeeded by ASC or DESC to indicate
        /// the sort direction
        /// </param>
        public void ApplySort(string sortExpression) {
            List<ListSortDescription> listSorts = new List<ListSortDescription>();
            sortExpression = sortExpression + ",";
            while (sortExpression.Contains(",")) {
                int k = sortExpression.IndexOf(",");
                string propertyName = sortExpression.Substring(0, k);
                ListSortDirection direction = ListSortDirection.Ascending;
                propertyName = propertyName.Replace(" ASC", "");
                if (propertyName.ToUpper().Contains(" DESC")) {
                    direction = ListSortDirection.Descending;
                    int k1 = propertyName.IndexOf(" ");
                    propertyName = propertyName.Substring(0, k1);
                }
                sortExpression = sortExpression.Remove(0, k + 1);
                sortExpression = sortExpression.Trim();
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                PropertyDescriptor property = properties[propertyName];
                ListSortDescription sort = new ListSortDescription(property, direction);
                listSorts.Add(sort);
            }
            ListSortDescriptionCollection sorts = new ListSortDescriptionCollection(listSorts.ToArray());
            ApplySort(sorts);
        }
        /// <summary>
        /// Store a collection of ListSortDescription to be used by the binding mechanisms to perform multiple sorts
        /// </summary>
        ListSortDescriptionCollection IBindingListView.SortDescriptions {
            get { return sortDescriptions; }
        }
        /// <summary>
        /// Indicate to the binding mechanism that the list supports multiple sortings
        /// </summary>
        bool IBindingListView.SupportsAdvancedSorting {
            get { return true; }
        }
        /// <summary>
        /// Indicate to the binding mechanism that the list allows add new objects
        /// </summary>
        bool IBindingList.AllowNew {
            get { return true; }
        }
        /// <summary>
        /// Indicate to the binding mechanism that the list allows remove objects
        /// </summary>
        bool IBindingList.AllowRemove {
            get { return true; }
        }
        /// <summary>
        /// Method executed when the ItemChanged event be fired
        /// </summary>
        /// <param name="sender">
        /// Event sender
        /// </param>
        /// <param name="args">
        /// Event args
        /// </param>
        public void OnItemChanged(object sender, EventArgs args) {
            if ((this as IRaiseItemChangedEvents).RaisesItemChangedEvents) {
                int index = Items.IndexOf((T)sender);
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
        }
        /// <summary>
        /// Indicate to the binding mechanism that the list does not allow raise item changed events
        /// </summary>
        bool IRaiseItemChangedEvents.RaisesItemChangedEvents {
            get { return false; }
        }
        /// <summary>
        /// Method executed when the ListChanged event is fired
        /// </summary>
        /// <param name="e">
        /// Event args
        /// </param>
        protected override void OnListChanged(ListChangedEventArgs e) {
            if (SupportsChangeNotificationCore) {
                base.OnListChanged(e);
            }
        }
        #endregion

        #region Filtering
        /// <summary>
        /// Flag used to know if the list is filtered
        /// </summary>
        private bool filtered = false;
        /// <summary>
        /// Expression to be evaluated on the filter process
        /// </summary>
        private string filterExpression = null;
        /// <summary>
        /// List containing the objects of the list before the filter to be applied
        /// </summary>
        private ADPCollection<T> originalList;

        /// <summary>
        /// Used by the binding mechanisms to insert items on the list
        /// </summary>
        /// <param name="index">
        /// Position on the list where the item will be inserted
        /// </param>
        /// <param name="item">
        /// Item to be inserted
        /// </param>
        protected override void InsertItem(int index, T item) {
            if (filtered) {
                originalList.Insert(index, item);
                foreach (PropertyDescriptor propDesc in TypeDescriptor.GetProperties(item)) {
                    if (propDesc.SupportsChangeEvents) {
                        propDesc.AddValueChanged(item, OnItemChanged);
                    }
                }
            }
            base.InsertItem(index, item);
        }

        /// <summary>
        /// If true, the objects removed from the list will be marked for deletion
        /// </summary>
        public bool DeleteOnRemove = false;
        /// <summary>
        /// Used by the binding mechanisms to remove items from the list
        /// </summary>
        /// <param name="index">
        /// Position of the item that must be removed
        /// </param>
        protected override void RemoveItem(int index) {
            if (filtered) {
                originalList.RemoveAt(index);
                T item = Items[index];
                PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(item);
                foreach (PropertyDescriptor propDesc in propDescs) {
                    if (propDesc.SupportsChangeEvents) {
                        propDesc.RemoveValueChanged(item, OnItemChanged);
                    }
                }
            }
            if (DeleteOnRemove) {
                if (index > -1) {
                    ADPObject o = this[index];
                    if (o != null) {
                        o.Delete();
                    }
                }
            }
            base.RemoveItem(index);
        }
        /// <summary>
        /// Used by the binding mechanisms to clear the list
        /// </summary>
        protected override void ClearItems() {
            if (filtered) {
                originalList.Clear();
            }
            base.ClearItems();
        }

        /// <summary>
        /// Exposes the list as a List<T> object
        /// </summary>
        public new List<T> Items {
            get { return (base.Items) as List<T>; }
        }

        /// <summary>
        /// Used by the binding mechanisms to create new objects and add it
        /// to the end of the list
        /// </summary>
        /// <returns>
        /// The new added object
        /// </returns>
        protected override object AddNewCore() {
            ADPObject o = ADPObject.CreateInstance(Session, typeof(T));
            o.BindingList = this;
            this.Add((T)o);
            if (filtered) {
                originalList.Add((T)o);
            }
            return o;
        }
        /// <summary>
        /// Cancel the insertion of a new object in the list
        /// </summary>
        /// <param name="itemIndex">
        /// Position where the object was inserted
        /// </param>
        public override void CancelNew(int itemIndex) {
            if (filtered) {
                ADPObject obj = (ADPObject)(originalList.Items[itemIndex]);
                if (obj.IsNewObject) {
                    originalList.Remove((T)obj);
                }
            }
            base.CancelNew(itemIndex);
        }
        /// <summary>
        /// Commits the insertion of a new object in the list
        /// </summary>
        /// <param name="itemIndex">
        /// Position where the object was inserted
        /// </param>
        public override void EndNew(int itemIndex) {
            base.EndNew(itemIndex);
        }

        /// <summary>
        /// Indicate to the binding mechanism that the list supports filtering
        /// </summary>
        public bool SupportsFiltering {
            get { return true; }
        }
        private bool supportsChangeNotification = true;
        /// <summary>
        /// Indicate to the binding mechanism that the list supports change notification
        /// </summary>
        protected override bool SupportsChangeNotificationCore {
            get {
                return supportsChangeNotification;
            }
        }

        /// <summary>
        /// Get or set the filter expression to be used
        /// and also apply the new filter
        /// </summary>
        public string Filter {
            get { return filterExpression; }
            set {
                filterExpression = value;
                if ((filterExpression == null) || (filterExpression == "")) {
                    (this as IBindingListView).RemoveFilter();
                } else {
                    ApplyFilter();
                    filtered = true;
                }
            }
        }
        /// <summary>
        /// Restore the list to its original format before the filter to be applied
        /// </summary>
        public void RemoveFilter() {
            if (!filtered) {
                return;
            }
            filtered = false;
            Clear();
            foreach (T item in originalList) {
                Add(item);
            }
            filterExpression = null;
            originalList = null;
        }
        /// <summary>
        /// Apply filter to the list
        /// </summary>
        protected virtual void ApplyFilter() {
            supportsChangeNotification = false;
            ADPFilterCriteria filterCriteria;
            try {
                //Remove filter, if any
                if (filtered) {
                    string expression = filterExpression;
                    (this as IBindingListView).RemoveFilter();
                    filterExpression = expression;
                }
                if ((filterExpression == null) || (filterExpression == "")) {
                    return;
                }
                //Fill original list
                originalList = new ADPCollection<T>(Session, this);
                //Parse filterExpression
                filterCriteria = new ADPFilterCriteria(typeof(T), filterExpression);
                //Perform filter
                Clear();
            } finally {
                supportsChangeNotification = true;
            }
            foreach (T item in originalList) {
                bool pass = true;
                foreach (ADPFilterCondition condition in filterCriteria.FilterConditions) {
                    if (!condition.Evaluate(item)) {
                        pass = false;
                        break;
                    }
                }
                if (pass) {
                    Add(item);
                }
            }
        }
        #endregion

        #region Other methods
        /// <summary>
        /// Give the filtered list the current status by discarding the original list.
        /// </summary>
        private void ConsolidateFilteredData() {
            filtered = false;
            filterExpression = null;
            originalList = null;
        }
        /// <summary>
        /// Iterate all the items of the list calling its persist method
        /// and thus saving its changes to the database
        /// </summary>
        public void Persist() {
            foreach (T o in this) {
                o.Persist();
            }
        }
        /// <summary>
        /// Creates a clone of the current list
        /// </summary>
        /// <remarks>
        /// Each object of the list will be also cloned so that the new list
        /// will point to no reference used by the original one
        /// </remarks>
        /// <returns>
        /// Cloned list
        /// </returns>
        public object Clone() {
            ADPCollection<T> clone = new ADPCollection<T>(Session);
            foreach (ADPObject o in this) {
                ADPObject newObj = ADPObject.CreateInstance(Session, typeof(T));
                newObj.Assign(o);
                clone.Add((T)newObj);
            }
            return clone;
        }
        public void DeleteAll() {
            while (this.Count > 0) {
                ADPObject o = this[0];
                o.Delete();
            }        
        }
        #endregion
    }
}
