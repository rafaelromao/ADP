using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cati.ADP.Client;
using Cati.ADP.Common;
using System.ComponentModel;
using System.Reflection;
using System.Collections;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Specifies how will be performed the generation of the primary key on the associated object
    /// </summary>
    public enum ADPKeyGeneration { 
        Disable,
        Early,      //The key is generated with its true value at the first time the property Key is accessed
        Lazy        //The key is generated with a negative value and inside the Persist() method it gets the true value
    }

    public interface IADPType { 
    }

    /// <summary>
    /// Base class fot the ADP Persistent Objects
    /// </summary>
    /// <remarks>
    /// Each property must call the event Notify on its setter, 
    /// in order to have the property Modified working fine
    /// </remarks>
    public class ADPObject : IADPType, ICloneable, IADPAssignable, IEditableObject, INotifyPropertyChanged {
        #region Static members
        /// <summary>
        /// An internal generator to provide keys to the objects 
        /// created with the ADPKeyGeneration.Lazy parameter
        /// </summary>
        private static int nonPersistentKeyGen = 0;
        #endregion

        #region Constructor
        /// <summary>
        /// Initilizes a new ADPObject
        /// </summary>
        /// <param name="session">
        /// Session which the object may belongs to
        /// </param>
        /// <param name="registerInSession">
        /// Indicates that the new object must be registered on the given session
        /// </param>
        private void Initialize(ADPSession session, bool registerInSession) {
            PropertyChanged += new PropertyChangedEventHandler(UpdateModifiedFlag);
            if (registerInSession) {
                Session = session;
                Session.Cache.Add(this);
            }
        }

        /// <summary>
        /// Creates a new ADPObject
        /// </summary>
        /// <param name="session">
        /// Session which the object may belongs to
        /// </param>
        /// <param name="registerInSession">
        /// Indicates that the new object must be registered on the given session
        /// </param>
        public ADPObject(ADPSession session, bool registerInSession) {
            Initialize(session, registerInSession);
        }
        /// <summary>
        /// Creates a new ADPObject and register it on the given session
        /// </summary>
        /// <param name="session">
        /// Session which the object may belongs to
        /// </param>
        public ADPObject(ADPSession session) {
            Initialize(session, true);
        }
        #endregion

        #region Fields
        /// <summary>
        /// Session which the object belongs to
        /// </summary>
        public ADPSession Session;
        /// <summary>
        /// If true, the method Reload() will try to check if the object is
        /// up to date before to reload it and will reload only if necessary
        /// </summary>
        protected bool SupportsCheckModifiedSinceLastLoading = false;
        /// <summary>
        /// Last time the object was loaded from a session
        /// </summary>
        internal DateTime LastSessionLoadingTime = DateTime.Now;
        /// <summary>
        /// Last time the object was loaded from the database
        /// </summary>
        private DateTime LastDatabaseLoadingTime = DateTime.MinValue;
        #endregion

        #region Properties
        /// <summary>
        /// Store the keys of the properties that where marked to be lazy loaded
        /// </summary>
        internal Dictionary<string, int> LazyLoadingPropertyKeys = null;

        private ADPKeyGeneration keyGeneration = ADPKeyGeneration.Lazy;
        /// <summary>
        /// Indicate how will be performed the generation of the primary key on the associated object
        /// </summary>
        public ADPKeyGeneration KeyGeneration {
            get { return keyGeneration; }
            set { keyGeneration = value; }
        }
        
        /// <summary>
        /// The value of the primary key of the database registry the contains
        /// this object when it is persisted
        /// </summary>
        public object Key {
            get {
                object key = GetPropertyValue("Key", null);
                if (Session == null) {
                    return 0;
                }
                if (key == null) {
                    switch (KeyGeneration) {
                        case ADPKeyGeneration.Lazy:
                            key = --nonPersistentKeyGen;
                            break;
                        case ADPKeyGeneration.Early:
                            GenerateKey();
                            break;
                        default:
                            break;
                    }
                }
                return key;
            }
            set {
                SetPropertyValue("Key", value);
            }
        }

        /// <summary>
        /// Define the Type of object used in the Key property
        /// </summary>
        public Type KeyType = typeof(Int64);

        /// <summary>
        /// Generate a new Key to the object
        /// </summary>
        private void GenerateKey() {
            string keyGenerator = ADPPersister.GetMappingInformation(ObjectType).KeyGenerator;
            object k = Session.Proxy.GetKey(keyGenerator, Session.DatabaseSessionID);
            k = Convert.ChangeType(k, KeyType);
            Key = k;
        }

        private bool isNew = true;
        /// <summary>
        /// True if the object does not exists in the database yet
        /// </summary>
        public bool IsNew {
            get { return isNew; }
        }

        private bool modified = true;
        /// <summary>
        /// True if the object is new or was modified since the 
        /// last time it was loaded from the database
        /// </summary>
        public bool Modified {
            get { return modified; }
            internal set { 
                modified = value;
                Notify("Modified");
            }
        }

        private bool deleted = false;
        /// <summary>
        /// True if the object was marked for deletion
        /// </summary>
        public bool Deleted {
            get { return deleted; }
        }

        /// <summary>
        /// A PropertyDescriptorCollection for reflation purposes
        /// </summary>
        private PropertyDescriptorCollection properties = null;

        /// <summary>
        /// A PropertyDescriptorCollection for reflation purposes
        /// </summary>
        public PropertyDescriptorCollection Properties {
            get {
                if (properties == null) {
                    properties = ADPPersister.GetMappingInformation(this.GetType()).Properties;
                }
                return properties;
            }
        }

        /// <summary>
        /// Store the values of the properties
        /// </summary>
        private Dictionary<string, object> propertyValues = new Dictionary<string, object>();

        /// <summary>
        /// Get the value of a property field
        /// </summary>
        /// <typeparam name="T">
        /// Type of the property field
        /// </typeparam>
        /// <param name="propertyName">
        /// Name of the property
        /// </param>
        /// <param name="constructorParamTypes">
        /// Array of types passed to the object constructor
        /// </param>
        /// <param name="constructorParamValues">
        /// Array of values passed to the object constructor
        /// </param>
        /// <returns>
        /// Value of the property field
        /// </returns>
        /// <remarks>
        /// If the value is not set, create a new object of the type T and set as property value.
        /// </remarks>
        protected object GetPropertyValue<T>(string propertyName, Type[] constructorParamTypes, object[] constructorParamValues) {
            Type type = typeof(T);
            try {
                if (!propertyValues.ContainsKey(propertyName)) {
                    //Create an object of the property type
                    object value = TypeDescriptor.CreateInstance(null, type, constructorParamTypes, constructorParamValues);
                    //If it is a ADPObject type property and is not marked as Early Loading, then load it
                    PropertyDescriptor prop = Properties[propertyName];
                    if (prop.PropertyType.IsSubclassOf(typeof(ADPObject))) {
                        //Check if is marked as Early Loading
                        bool earlyLoading = prop.Attributes.Contains(new EarlyLoadingAttribute());
                        //If not marked as Early Loading, load the object
                        if (!earlyLoading) {
                            //Get the key of the object that must be loaded on that property
                            if (LazyLoadingPropertyKeys != null) {
                                int k = LazyLoadingPropertyKeys[prop.Name];
                                (value as ADPObject).Load(k);
                            }
                        }
                    }
                    //Set the property value
                    propertyValues[propertyName] = value;
                }
                return GetPropertyValue(propertyName);
            } catch (Exception e) {
                throw new NotSupportedException(String.Format("Could not create an instance of the type {0}!", type.Name), e);
            }
        }
        /// <summary>
        /// Get the value of a property field
        /// </summary>
        /// <typeparam name="T">
        /// Type of the property field
        /// </typeparam>
        /// <param name="propertyName">
        /// Name of the property
        /// </param>
        /// <param name="session">
        /// Session to be used to register the property value
        /// </param>
        /// <returns>
        /// Value of the property field
        /// </returns>
        /// <remarks>
        /// Supports only types that derives from ADPObject or ADPCollection
        /// </remarks>
        protected object GetPropertyValue<T>(string propertyName, ADPSession session) where T : IADPType {
            Type type = typeof(T);
            try {
                if (propertyValues.ContainsKey(propertyName)) {
                    return GetPropertyValue(propertyName);
                } else {
                    Type[] constructorParamTypes = new Type[] { typeof(ADPSession) };
                    object[] constructorParamValues = new object[] { session };
                    return GetPropertyValue<T>(propertyName, constructorParamTypes, constructorParamValues);
                }
            } catch (Exception e) {
                throw new NotSupportedException(String.Format("Could not create an instance of the type {0}!", type.Name), e);
            }
        }
        /// <summary>
        /// Get the value of a property field
        /// </summary>
        /// <typeparam name="T">
        /// Type of the property field
        /// </typeparam>
        /// <param name="propertyName">
        /// Name of the property
        /// </param>
        /// <returns>
        /// Value of the property field
        /// </returns>
        /// <remarks>
        /// Supports only the types that have a default constructor
        /// </remarks>
        protected object GetPropertyValue<T>(string propertyName) where T : new() {
            Type type = typeof(T);
            try {
                if (propertyValues.ContainsKey(propertyName)) {
                    return GetPropertyValue(propertyName);
                } else {
                    return GetPropertyValue<T>(propertyName, null, null);
                }
            } catch (Exception e) {
                throw new NotSupportedException(String.Format("Could not create an instance of the type {0}!", type.Name), e);
            }
        }
        /// <summary>
        /// Get the value of a property field
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property
        /// </param>
        /// <returns>
        /// Value of the property field
        /// </returns>
        /// <exception cref="System.Exception">
        /// The property field was not instanciated!
        /// </exception>
        protected object GetPropertyValue(string propertyName) {
            if (propertyValues.ContainsKey(propertyName)) {
                return propertyValues[propertyName];
            } else {
                throw new Exception("The property field was not instanciated!");
            }
        }
        /// <summary>
        /// Get the value of a property field
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property
        /// </param>
        /// <param name="initialValue">
        /// Value to be used to initialize the property
        /// </param>
        /// <returns>
        /// Value of the property field
        /// </returns>
        protected object GetPropertyValue(string propertyName, object initialValue) {
            if (!propertyValues.ContainsKey(propertyName)) {
                propertyValues[propertyName] = initialValue;
            }
            return propertyValues[propertyName];
        }
        /// <summary>
        /// Sets the value of the property field identified by the given property name
        /// </summary>
        /// <param name="propertyName">
        /// Property to be set
        /// </param>
        /// <param name="propertyValue">
        /// Value to be set
        /// </param>
        protected internal void SetPropertyValue(string propertyName, object propertyValue) {
            object oldPropertyValue = null;
            if (propertyValues.ContainsKey(propertyName)) {
                oldPropertyValue = propertyValues[propertyName];
            }
            if (oldPropertyValue != propertyValue) {
                propertyValues[propertyName] = propertyValue;
                Notify(propertyName);
            }
        }
        /// <summary>
        /// Get the value of an expression such as Patient.Key
        /// </summary>
        /// <param name="expression">
        /// Expression to be used
        /// </param>
        /// <param name="o">
        /// Object where to find the expression
        /// </param>
        /// <returns>
        /// Value of the expression or null if not found
        /// </returns>
        public static object GetExpressionValue(string expression, object o) {
             if (o == null) {
                return null;
            }
            //Get the value from the property
            PropertyDescriptor prop = null;
            if (expression != null) {
                Type type = o.GetType();
                //[Patient.Key]
                ADPStringList propSteps = new ADPStringList(expression, ".", true);  
                foreach (string step in propSteps) {
                    //[prop = Patient , prop = Key]
                    prop = TypeDescriptor.GetProperties(type)[step];                 
                    if (prop == null) {
                        return null;
                    }
                    //[o = Patient , o = Patient.Key]
                    o = prop.GetValue(o);        
                    if (o == null) {
                        return null;
                    }
                    //[type = typeof(Patient) , type = Patient.KeyType]
                    type = prop.PropertyType;                                        
                }
            }
            //Convert the value to the correct type
            MappingTypeAttribute mta = (MappingTypeAttribute)prop.Attributes[typeof(MappingTypeAttribute)];
            Type mappingType = null;
            //If it has its type identified in a MappingTypeAttribute
            if (mta != null) {
                mappingType = Type.GetType(mta.TypeName);
                o = Convert.ChangeType(o, mappingType);
            } else {
                //If it is a enum type
                if (prop.PropertyType.IsEnum) {
                    mappingType = prop.PropertyType;
                    mappingType = Enum.GetUnderlyingType(mappingType);
                    o = Convert.ChangeType(o, mappingType);
                }
            }
            return o;
        }

        private Type objectType = null;
        /// <summary>
        /// Gets the type of the object
        /// </summary>
        public Type ObjectType {
            get {
                if (objectType == null) {
                    objectType = this.GetType();
                }
                return objectType; 
            }
        }

        /// <summary>
        /// Gets the name of the Type of the object
        /// </summary>
        public string TypeName {
            get { return ObjectType.Name; }
        }

        #endregion

        #region Binding Control Members
        /// <summary>
        /// When an object is automaticaly created by a binding list control
        /// such list is stored in this field. It is necessary for a correct
        /// behaviour of the binding mechanisms
        /// </summary>
        internal IList BindingList = null;
        private List<int> originalSortOrderList;
        /// <summary>
        /// List of integers that represents the original position of an object
        /// after each sort operation
        /// </summary>
        internal List<int> OriginalSortOrderList {
            get {
                if (originalSortOrderList == null) {
                    originalSortOrderList = new List<int>();
                }
                return originalSortOrderList;
            }
            set {
                if (originalSortOrderList == null) {
                    originalSortOrderList = new List<int>();
                }
                originalSortOrderList = value;
            }
        }
        /// <summary>
        /// Position of the object before the last sort operation
        /// </summary>
        internal int LastSortOrder {
            get {
                if ((originalSortOrderList == null) || (originalSortOrderList.Count == 0)) {
                    return Int32.MaxValue;
                } else {
                    return originalSortOrderList[originalSortOrderList.Count-1];
                }
            }
        }
        /// <summary>
        /// True if the object was created by a binding list control
        /// but its creation was not finished yet
        /// </summary>
        internal bool IsNewObject = true;
        #endregion

        #region Load Methods

        /// <summary>
        /// Load a DataTable containing persisted objects
        /// </summary>
        /// <param name="session">
        /// Session that hold the ADPProxy to be used to reach the database
        /// </param>
        /// <param name="connectionID">
        /// Id of the connection to be used
        /// </param>
        /// <param name="transactionID">
        /// Id of the transaction to be used
        /// </param>
        /// <param name="statement">
        /// Statement to be executed
        /// </param>
        /// <returns>
        /// DataTable containing the result of the statement execution
        /// </returns>
        private static DataTable InternalGetDataTable(ADPSession session, Guid? connectionID, Guid? transactionID, ADPSQLStatement statement) {
            if (transactionID != null) {
                return session.Proxy.ExecuteSelectStatementInTransaction((Guid)transactionID, statement.StatementText, statement.Parameters.ToArray());
            } else {
                return session.Proxy.ExecuteSelectStatement((Guid)connectionID, statement.StatementText, statement.Parameters.ToArray());
            }
        }
        
        /// <summary>
        /// Load the object from the data returned from the execution of the given statement
        /// </summary>
        /// <param name="statement">
        /// Statement that must be executed
        /// </param>
        private void InternalLoad(ADPSQLStatement statement) {
            Guid? transactionID = null;
            Guid? connectionID = null;
            try {
                if (Session.Persisting()) {
                    transactionID = Session.TransactionID;
                } else {
                    connectionID = Session.Proxy.GetConnection(Session.DatabaseSessionID);
                }
                if (!statement.Prepared) {
                    throw new ADPException(String.Format("Statement {0} must be prepared before to be executed!", statement.Id));
                }
                DataTable  dt = InternalGetDataTable(Session, connectionID, transactionID, statement);
                //Launch method to perform the loading of the object
                ADPPersister.LoadObjectFromDataTable(Session, ObjectType, dt, this);
            } finally {
                modified = false;
                isNew = false;
                LastDatabaseLoadingTime = DateTime.Now;
                if (!Session.Persisting()) {
                    Session.Proxy.ReleaseConnection((Guid)connectionID);
                }
            }
        }
        /// <summary>
        /// Load an object according to the given primary key
        /// </summary>
        /// <param name="key">
        /// Primary key of the object to be loaded
        /// </param>
        internal void Load(object key) {
            ADPFilterCriteria filterCriteria = new ADPFilterCriteria(ObjectType);
            ADPFilterCondition filterCondition = new ADPFilterCondition(ObjectType);
            filterCondition.PropertyName = "Key";
            filterCondition.Value = key;
            filterCondition.Operator = ADPOperator.Equals;
            filterCriteria.Id = "FilterByPrimaryKey";
            filterCriteria.StatementType = ADPSQLStatementType.SelectByKey;
            filterCriteria.FilterConditions.Add(filterCondition);
            Load(filterCriteria);
        }
        /// <summary>
        /// Load an object according to the value of a given property
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property to be evaluated
        /// </param>
        /// <param name="propertyValue">
        /// Value to be matched
        /// </param>
        internal void Load(string propertyName, object propertyValue) {
            ADPFilterCriteria filterCriteria = new ADPFilterCriteria(ObjectType);
            ADPFilterCondition filterCondition = new ADPFilterCondition(ObjectType);
            filterCondition.PropertyName = propertyName;
            filterCondition.Value =  "'" + propertyValue + "'";
            filterCondition.Operator = ADPOperator.Equals;
            filterCriteria.Id = "FilterBySingleProperty";
            filterCriteria.StatementType = ADPSQLStatementType.SelectBySingleField;
            filterCriteria.FilterConditions.Add(filterCondition);
            Load(filterCriteria);
        }
        /// <summary>
        /// Load the object from the current record of a given data table
        /// </summary>
        /// <param name="dataTable">
        /// DataTable to be used
        /// </param>
        internal void Load(DataTable dataTable) {
            ADPPersister.LoadObjectFromDataTable(Session, ObjectType, dataTable, this);
        }
        /// <summary>
        /// Load an object according to the given filter criteria
        /// </summary>
        /// <param name="filterCriteria">
        /// Filter criteria to be matched
        /// </param>
        internal void Load(ADPFilterCriteria filterCriteria) {
            ADPSQLStatement statement = ADPPersister.GetPreparedStatement(Session, ObjectType, filterCriteria);
            InternalLoad(statement);
        }
        
        /// <summary>
        /// If SupportsCheckModifiedSinceLastLoading is true, 
        /// check on the database if the object is out of date
        /// </summary>
        /// <returns>
        /// True if the object is out of date
        /// </returns>
        private bool ModifiedSinceLastLoading() {
            if (!SupportsCheckModifiedSinceLastLoading) {
                return true;
            }
            ADPSQLStatement statement = ADPPersister.GetPreparedStatement(Session, ObjectType, ADPSQLStatementType.GetModifiedDateTime, this);
            if (statement != null) {
                Guid connectionID = Session.Proxy.GetConnection(Session.DatabaseSessionID);
                try {
                    //The SQLTokens @TABLENAME and @FIELDNAME must be filled by the method PrepareStatement
                    //This statement must return only one row and one field containing the result value
                    DataTable dt = Session.Proxy.ExecuteSelectStatement(connectionID, statement.StatementText, statement.Parameters.ToArray());
                    if (dt.Rows.Count > 0) {
                        DateTime modifiedDateTime = Convert.ToDateTime(dt.Rows[0][0]);
                        return modifiedDateTime >= LastDatabaseLoadingTime;
                    } else {
                        return true;
                    }
                } catch {
                    throw new ADPException("Error executing ModifiedSinceLastLoading().\nIf you don't want this feature enabled,\nset SupportsCheckModifiedSinceLastLoading to false in your object constructor.");
                } finally {
                    Session.Proxy.ReleaseConnection(connectionID);
                }
            } else {
                return true;
            }
        }
        /// <summary>
        /// Reload the object from the database
        /// </summary>
        public void Reload() {
            //If modified, discard the changes reloading the data, if modified since the last loading, refresh the data, otherwise, does nothing
            if ((Modified) ||(ModifiedSinceLastLoading())) {
                //Important: Use the field key instead of the property Key in order to avoid the call to the lazy loading setter
                if (Key == null) {
                    throw new ADPException("Cannot reload an object that was not loaded yet!");
                }
                Load(Key);
            }
        }

        /// <summary>
        /// Load a list of objects from the data returned from the execution of the given statement
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to be loaded
        /// </typeparam>
        /// <param name="session">
        /// Session to be used to register the loaded objects
        /// </param>
        /// <param name="statement">
        /// Statement to be executed
        /// </param>
        /// <param name="loadOnlyFirstObject">
        /// If true, load only the first object found and ignore the others
        /// </param>
        /// <returns>
        /// A list containing the loaded objects
        /// </returns>
        private static ADPCollection<T> InternalLoadRange<T>(ADPSession session, ADPSQLStatement statement, bool loadOnlyFirstObject) where T : ADPObject {
            Guid? transactionID = null;
            Guid? connectionID = null;
            try {
                if (session.Persisting()) {
                    transactionID = session.TransactionID;
                } else {
                    connectionID = session.Proxy.GetConnection(session.DatabaseSessionID);
                }
                if (!statement.Prepared) {
                    throw new ADPException(String.Format("Statement {0} must be prepared before to be executed!", statement.Id));
                }
                DataTable dt = InternalGetDataTable(session, connectionID, transactionID, statement);
                //Launch method to perform the loading of the object
                ADPCollection<T> list = new ADPCollection<T>(session);
                if (loadOnlyFirstObject) {
                    T obj = (T)ADPObject.CreateInstance(typeof(T));
                    list.Add(obj);
                    ADPPersister.LoadObjectFromDataTable(session, typeof(T), dt, obj);
                } else {
                    ADPPersister.LoadRangeFromDataTable(session, typeof(T), dt, list);
                }
                //Set item properties
                foreach (ADPObject o in list) {
                    o.Session = session;
                    o.isNew = false;
                    o.modified = false;
                }
                return list;
            } finally {
                if (!session.Persisting()) {
                    session.Proxy.ReleaseConnection((Guid)connectionID);
                }
            }
        }
        /// <summary>
        /// Load a list containing all the objects returned from the execution of the given statement
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to be loaded
        /// </typeparam>
        /// <param name="session">
        /// Session to be used to register the loaded objects
        /// </param>
        /// <param name="statement">
        /// Statement to be executed
        /// </param>
        /// <returns>
        /// A list containing the loaded objects
        /// </returns>
        private static ADPCollection<T> InternalLoadRange<T>(ADPSession session, ADPSQLStatement statement) where T : ADPObject {
            return InternalLoadRange<T>(session, statement, false);
        }
        /// <summary>
        /// Load a list of objects of a given type
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to be loaded
        /// </typeparam>
        /// <param name="session">
        /// Session to be used to register the loaded objects
        /// </param>
        /// <returns>
        /// A list containing the loaded objects
        /// </returns>
        internal static ADPCollection<T> LoadRange<T>(ADPSession session) where T : ADPObject {
            return LoadRange<T>(session, null, false);
        }
        /// <summary>
        /// Load a list of objects of a given type accoring to a filter criteria
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to be loaded
        /// </typeparam>
        /// <param name="session">
        /// Session to be used to register the loaded objects
        /// </param>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <param name="loadOnlyFirstObject">
        /// If true, load only the first object found and ignore the others
        /// </param>
        /// <returns>
        /// A list containing the loaded objects
        /// </returns>
        internal static ADPCollection<T> LoadRange<T>(ADPSession session, ADPFilterCriteria filterCriteria, bool loadOnlyFirstObject) where T : ADPObject {
            if (filterCriteria != null) {
                filterCriteria.StatementType = ADPSQLStatementType.AutoBuiltCustomSelect;
            }
            ADPSQLStatement statement = ADPPersister.GetPreparedStatement(session, typeof(T), filterCriteria);
            return InternalLoadRange<T>(session, statement, loadOnlyFirstObject);
        }
        /// <summary>
        /// Load a list of objects from a given DataTable
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to be loaded
        /// </typeparam>
        /// <param name="session">
        /// Session to be used to register the loaded objects
        /// </param>
        /// <param name="dataTable">
        /// DataTable containing the objects to be loaded
        /// </param>
        /// <returns>
        /// List containing the loaded objects
        /// </returns>
        internal static ADPCollection<T> LoadRangeFromDataTable<T>(ADPSession session, DataTable dataTable) where T : ADPObject {
            ADPCollection<T> result = new ADPCollection<T>(session);
            ADPPersister.LoadRangeFromDataTable(session, typeof(T), dataTable, result);
            return result;
        }
        #endregion

        #region Persist
        /// <summary>
        /// Mark the object for deletion
        /// </summary>
        public virtual void Delete() {
            deleted = true;
            Session.deletedObjects.Add(this);
            Notify("Deleted");
        }
        /// <summary>
        /// Save the object changes to the database
        /// </summary>
        public virtual void Persist() {
            if (!Session.Persisting()) {
                throw new ADPNoActiveTransactionException();
            }
            if (!Modified) {
                return;
            }
            ADPSQLStatementType statementType;
            //Remove object from the database
            if (Deleted) {
                if (!IsNew) {
                    statementType = ADPSQLStatementType.Delete;
                } else {
                    return;
                }
            } else if (IsNew) {
                //Insert object in the database
                if ((KeyGeneration == ADPKeyGeneration.Lazy) && ((Key == null) ||((Key is int) && (int)Key <= 0) )) {
                    GenerateKey();
                }
                statementType = ADPSQLStatementType.Insert;
            } else {
                //Update object in the database
                statementType = ADPSQLStatementType.Update;
            }
            ADPSQLStatement statement = ADPPersister.GetPreparedStatement(Session, ObjectType, statementType, this);
            if (!statement.Prepared) {
                throw new ADPException(String.Format("Statement {0} must be prepared before to be executed!", statement.Id));
            }
            ADPParam[] parameters = statement.Parameters.ToArray();
            Session.Proxy.ExecuteCommandStatement(Session.TransactionID, statement.StatementText, parameters);
            if (statementType != ADPSQLStatementType.Delete) {
                isNew = false;
            }
            Modified = false;
        }
        #endregion

        #region Clone, Assign and Equals
        /// <summary>
        /// Create a new instance of a ADPObject descendent and register such instance in the given session
        /// </summary>
        /// <param name="session">
        /// Session to be used to register the new instance
        /// </param>
        /// </summary>
        /// <param name="type">
        /// Type of the object to be created
        /// </param>
        /// <returns>
        /// The object created
        /// </returns>
        public static ADPObject CreateInstance(ADPSession session, Type type) {
            bool register = true;
            if (session == null) {
                register = false;
            }
            Type[] constructorParamTypes = new Type[] { typeof(ADPSession), typeof(bool) };
            object[] constructorParamValues = new object[] { session, register };
            ADPObject result = (ADPObject)TypeDescriptor.CreateInstance(null, type, constructorParamTypes, constructorParamValues);
            return result;
        }
        /// <summary>
        /// Create a new instance of a ADPObject descendent without register such instance in a session
        /// </summary>
        /// <param name="type">
        /// Type of the object to be created
        /// </param>
        /// <returns>
        /// The object created
        /// </returns>
        public static ADPObject CreateInstance(Type type) {
            return CreateInstance(null, type);
        }

        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <returns></returns>
        public virtual object Clone() {
            ADPObject clone = ADPObject.CreateInstance(ObjectType);
            clone.Assign(this);
            return clone;
        }
        /// <summary>
        /// Assign a given object to the current object
        /// </summary>
        /// <param name="source">
        /// Source object
        /// </param>
        public virtual void Assign(ADPObject source) {
            #region Set custom type properties
            foreach (PropertyDescriptor prop in Properties) {
                //Skip ADPObject properties
                if (prop.ComponentType == typeof(ADPObject)) {
                    continue;
                }
                object sourceValue = null;
                if (prop.PropertyType.IsValueType) {
                    //If value type
                    sourceValue = prop.GetValue(source);
                    prop.SetValue(this, sourceValue);
                } else {
                    //If reference type
                    sourceValue = prop.GetValue(source);
                    object newValue = null;
                    if (sourceValue is ICloneable) {
                        newValue = (sourceValue as ICloneable).Clone();
                    } else {
                        sourceValue = prop.GetValue(source);
                        prop.SetValue(this, sourceValue);
                    }
                    prop.SetValue(this, newValue);
                }
            }
            #endregion

            #region Set ADPObject properties
            Key = source.Key;
            Session = source.Session;
            isNew = source.isNew;
            IsNewObject = source.IsNewObject;
            modified = source.modified;
            LastDatabaseLoadingTime = source.LastDatabaseLoadingTime;
            LastSessionLoadingTime = source.LastSessionLoadingTime;
            originalSortOrderList = source.originalSortOrderList;
            BindingList = source.BindingList;
            #endregion
        }
        /// <summary>
        /// Compare to objects to check its equality
        /// </summary>
        /// <param name="obj">
        /// Object to be compared to the current object
        /// </param>
        /// <returns>
        /// True if the objects are equals
        /// </returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            ADPObject bo = (ADPObject)obj;
            bool b1 = bo == this;
            bool b2 = bo.Key == this.Key;
            bool b3 = bo.Key.Equals(this.Key);
            return b1 || b2 || b3;
        }
        /// <summary>
        /// Return a hash code that identifies the object
        /// </summary>
        /// <returns>
        /// Hash code that identifies the object
        /// </returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        #endregion

        #region IEditableObject
        /// <summary>
        /// Store a copy of the object to be used when cancelling an edit operation
        /// </summary>
        private ADPObject valueBeforeEdit;
        /// <summary>
        /// Used by the binding mechanisms to register the start of an edit operation
        /// </summary>
        public void BeginEdit() {
            if (valueBeforeEdit == null) {
                valueBeforeEdit = (ADPObject)Clone();
            }
        }
        /// <summary>
        /// Used by the binding mechanisms to cancel an edit operation
        /// </summary>
        public void CancelEdit() {
            if (valueBeforeEdit != null) {
                Assign(valueBeforeEdit);
            }
            if ((IsNewObject) && (BindingList != null)) {
                BindingList.Remove(this);
            }
            valueBeforeEdit = null;
        }
        /// <summary>
        /// Used by the binding mechanisms to commit an edit operation
        /// </summary>
        public void EndEdit() {
            valueBeforeEdit = null;
            IsNewObject = false;
        }
        #endregion

        #region INotifyPropertyChange
        /// <summary>
        /// Fire the property changed event
        /// </summary>
        /// <param name="propName">
        /// Name of the changed property
        /// </param>
        /// <remarks>
        /// This method must be called by the inherited types 
        /// on all its property setters in order to ensure a
        /// correct behaviour of the binding mechanisms
        /// </remarks>
        protected void Notify(string propName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        /// <summary>
        /// Update the value of the modified property when a property changed event is fired
        /// </summary>
        /// <param name="sender">
        /// Object that fired the event
        /// </param>
        /// <param name="e">
        /// Arguments of the event
        /// </param>
        private void UpdateModifiedFlag(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != "Modified") {
                Modified = true;
            }
        }
        /// <summary>
        /// Event that must be fired when a property value is changed
        /// Must be fired using the Notify method
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
