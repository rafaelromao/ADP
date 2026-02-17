using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.ComponentModel;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Provides persister functionalities to be used by the business objects
    /// Must exist one persister, inherited from this, to each business object type
    /// </summary>
    public static class ADPPersister {
        /// <summary>
        /// Store the mapping information of the persistent types
        /// </summary>
        private static Dictionary<Guid, ADPMappingInformation> mappingInformations = new Dictionary<Guid, ADPMappingInformation>();
        /// <summary>
        /// Create a select statement containing a single WHERE condition
        /// </summary>
        /// <param name="searchField">
        /// Field to be evaluated
        /// </param>
        /// <returns>
        /// Statement text
        /// </returns>
        private static string CreateStatementText(ADPMappingInformation info, string searchField) {
            return CreateStatementText(info, searchField, searchField, null);
        }
        /// <summary>
        /// Create a select statement containing a single WHERE condition
        /// </summary>
        /// <param name="searchFieldName">
        /// Field to be evaluated
        /// </param>
        /// <param name="searchFieldValue">
        /// Value to be evaluated
        /// </param>
        /// <param name="sortExpression">
        /// Expression to be used to sort select statements
        /// </param>  
        /// <returns>
        /// Statement text
        /// </returns>
        private static string CreateStatementText(ADPMappingInformation info, string searchFieldName, string searchFieldValue, string sortExpression) {
            string sql;
            if ((sortExpression != null) && (sortExpression != "")) {
                sql = "SELECT {0} FROM {1} WHERE {2} = :{3} ORDER BY {4}";
                sql = String.Format(sql, info.CommaFields, info.TableName, searchFieldName, searchFieldValue, sortExpression);
                return sql;
            } else {
                sql = "SELECT {0} FROM {1} WHERE {2} = :{3}";
                sql = String.Format(sql, info.CommaFields, info.TableName, searchFieldName, searchFieldValue);
                return sql;
            }
        }
        /// <summary>
        /// Create a statement of a given type
        /// </summary>
        /// <param name="statementId">
        /// Id of the statement
        /// </param>
        /// <param name="statementType">
        /// Type of statement
        /// </param>
        /// <param name="sortExpression">
        /// Expression to be used to sort select statements
        /// </param>  
        /// <returns>
        /// Statement text
        /// </returns>
        private static string CreateStatement(ADPMappingInformation info, string statementId, ADPSQLStatementType statementType, string sortExpression) {
            if ((info.TableName == "") || (info.PropertyMapping.Count == 0)) {
                throw new ADPException("TableName and Fields must be informed in order to create statements!");
            }
            switch (statementType) {
                case ADPSQLStatementType.SelectByKey:
                    if (info.KeyField == "") {
                        throw new ADPException("KeyField must be informed in order to create this statement!");
                    }
                    return CreateStatementText(info, info.KeyField);
                case ADPSQLStatementType.SelectBySingleField:
                    return CreateStatementText(info, "@FIELDNAME", "FIELDVALUE", sortExpression);
                case ADPSQLStatementType.SelectAll:
                    string sqlAll;
                    if ((sortExpression != null) && (sortExpression != "")) {
                        sqlAll = "SELECT {0} FROM {1} ORDER BY {2}";
                        sqlAll = String.Format(sqlAll, info.CommaFields, info.TableName, ReplacePropertyNamesByFieldNames(info, sortExpression));
                    } else {
                        sqlAll = "SELECT {0} FROM {1}";
                        sqlAll = String.Format(sqlAll, info.CommaFields, info.TableName);
                    }
                    return sqlAll;
                case ADPSQLStatementType.Insert:
                    string sqlInsert = "INSERT INTO {0} ({1}) VALUES ({2})";
                    sqlInsert = String.Format(sqlInsert, info.TableName, info.CommaFields, info.CommaParams);
                    return sqlInsert;
                case ADPSQLStatementType.Delete:
                    string sqlDelete = "DELETE FROM {0} WHERE {1} = :{1}";
                    sqlDelete = String.Format(sqlDelete, info.TableName, info.KeyField);
                    return sqlDelete;
                case ADPSQLStatementType.Update:
                    string sqlUpdate = "UPDATE {0} SET {1} WHERE {2} = :{2}";
                    sqlUpdate = String.Format(sqlUpdate, info.TableName, info.SetFieldsText, info.KeyField);
                    return sqlUpdate;
                case ADPSQLStatementType.GetModifiedDateTime:
                    string sqlModified;
                    sqlModified = "SELECT @FIELDNAME AS MODIFIED FROM @TABLENAME WHERE {0} = :FIELDVALUE";
                    sqlModified = String.Format(sqlModified, info.KeyField);
                    return sqlModified;
                default:
                    throw new NotSupportedException(String.Format("The statement type {0} cannot be generated automatically", statementType));
            }
        }
        /// <summary>
        /// Load or create an statement of a given type and id
        /// </summary>
        /// <param name="statementId">
        /// Id of the statement
        /// </param>
        /// <param name="statementType">
        /// Type of statement
        /// </param>
        /// <param name="generationType">
        /// Indicate if the statement must be generated automatically or must
        /// be loaded from a stored statement file
        /// </param>
        /// <param name="sortExpression">
        /// Expression to be used to sort select statements
        /// </param>        
        /// <returns>
        /// Statement text
        /// </returns>
        private static ADPSQLStatement LoadStatement(ADPMappingInformation info, ADPSession session, string statementId, ADPSQLStatementType statementType, ADPStatementGeneration generationType, string sortExpression) {
            ADPSQLStatement statement = null;
            if (!info.SQLStatements.ContainsKey(statementId)) {
                statement = new ADPSQLStatement(statementId);
                switch (generationType) {
                    case ADPStatementGeneration.Manual:
                        statement.StatementText = session.Proxy.GetSQLStatement(statement.Id, session.DatabaseSessionID);
                        break;
                    case ADPStatementGeneration.Automatic:
                        statement.StatementText = CreateStatement(info, statementId, statementType, sortExpression);
                        break;
                }
                statement.Type = statementType;
                //Does not cache SelectAll due to the auto generated statements
                if (statement.Type != ADPSQLStatementType.SelectAll) {
                    info.SQLStatements[statementId] = statement;
                }
            } else {
                statement = info.SQLStatements[statementId];
            }
            return statement;
        }

        /// <summary>
        /// Get the name of the sub property that must be used to map a parent property
        /// </summary>
        /// <param name="prop">
        /// Parent property
        /// </param>
        /// <returns>
        /// Name of the sub property
        /// </returns>
        private static string GetPropertyName(PropertyDescriptor prop) {
            MappingPropertyAttribute attrib = (MappingPropertyAttribute)prop.Attributes[typeof(MappingPropertyAttribute)];
            string result = prop.Name;
            if (attrib != null) {
                result = attrib.PropertyName;
            } else if (prop.PropertyType.IsSubclassOf(typeof(ADPObject))) {
                result = prop.Name + ".Key";
            }
            return result;
        }
        
        /// <summary>
        /// Replace property names by its mapped fields
        /// </summary>
        /// <param name="statementText">
        /// Statement containing the text to be processed
        /// </param>
        /// <returns>
        /// Processed statement
        /// </returns>
        private static string ReplacePropertyNamesByFieldNames(ADPMappingInformation info, string statementText) {
            foreach (string p in info.PropertyMapping.Keys) {
                string f = info.PropertyMapping[p];
                PropertyDescriptor prop = info.Properties[p];
                statementText = statementText.Replace(GetPropertyName(prop), f);
            }
            return statementText;
        }
        /// <summary>
        /// Automatically fills the parameters of a given statement
        /// </summary>
        /// <param name="statement">
        /// Statement to have the parameters filled
        /// </param>
        /// <param name="o">
        /// Object to be used as source of the data
        /// </param>
        private static void FillStatementParameters(ADPMappingInformation info, ADPSQLStatement statement, object o) {
            foreach (string p in info.PropertyMapping.Keys) {
                string f = info.PropertyMapping[p];
                PropertyDescriptor prop = info.Properties[p];
                //Define the property that holds the value
                string propName = GetPropertyName(prop);
                //Set the value to the parameter
                statement.ParamByName(f).Value = ADPObject.GetExpressionValue(propName, o);
            }
        }
        /// <summary>
        /// Store a list of objects to be used to help the ADPPersister with some specific types
        /// </summary>
        private static Dictionary<Guid, ADPBasePersisterHelper> helpers = null;

        /// <summary>
        /// Get the mapping information for the given type
        /// </summary>
        /// <param name="type">
        /// Type
        /// </param>
        /// <returns>
        /// Mapping information
        /// </returns>
        /// <remarks>
        /// If a ADPPersisterHelper is registered to the given type and it's the 
        /// first time the mapping information is accessed, the helper
        /// Initialize() method will be called to initilize the mapping information
        /// </remarks>
        internal static ADPMappingInformation GetMappingInformation(Type type) {
            if (!mappingInformations.ContainsKey(type.GUID)) {
                mappingInformations[type.GUID] = new ADPMappingInformation(type);
                if ((helpers != null) && (helpers.ContainsKey(type.GUID))) {
                    helpers[type.GUID].Initialize(mappingInformations[type.GUID]);
                }
            }
            return mappingInformations[type.GUID];
        }
        /// <summary>
        /// Return a prepared statement
        /// </summary>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <param name="statementType">
        /// Type of statement to be loaded
        /// </param>
        /// <param name="obj">
        /// Object to be used as source of data
        /// </param>
        /// <returns>
        /// Desired statement
        /// </returns>
        internal static ADPSQLStatement GetPreparedStatement(ADPSession session, Type type, ADPFilterCriteria filterCriteria, ADPSQLStatementType statementType, ADPObject obj) {
            ADPMappingInformation info = ADPPersister.GetMappingInformation(type);
            ADPSQLStatement statement = null;
            //Access a helper that may provide the statement
            if ((helpers != null) && (helpers.ContainsKey(type.GUID))) {
                statement = helpers[type.GUID].GetPreparedStatement(session, type, filterCriteria, statementType, obj);
                if (statement != null) {
                    return statement;
                }
            }
            //Insert, delete and update
            switch (statementType) {
                case ADPSQLStatementType.Insert:
                    statement = LoadStatement(info, session, "Insert", ADPSQLStatementType.Insert, ADPStatementGeneration.Automatic, null);
                    statement.Prepare();
                    FillStatementParameters(info, statement, obj);
                    return statement;
                case ADPSQLStatementType.Update:
                    statement = LoadStatement(info, session, "Update", ADPSQLStatementType.Update, ADPStatementGeneration.Automatic, null);
                    statement.Prepare();
                    FillStatementParameters(info, statement, obj);
                    return statement;
                case ADPSQLStatementType.Delete:
                    statement = LoadStatement(info, session, "Delete", ADPSQLStatementType.Delete, ADPStatementGeneration.Automatic, null);
                    statement.Prepare();
                    statement.ParamByName(info.KeyField).Value = obj.Key;
                    return statement;
                case ADPSQLStatementType.GetModifiedDateTime:
                    statement = LoadStatement(info, session, "GetModifiedDateTime", ADPSQLStatementType.GetModifiedDateTime, ADPStatementGeneration.Automatic, null);
                    statement.Prepare();
                    statement.ParamByName("TABLENAME").Value = info.TableName;
                    statement.ParamByName("FIELDNAME").Value = String.Format("{0}_MODIFIED_DATE", info.TablePrefix);
                    statement.ParamByName("FIELDVALUE").Value = obj.Key;
                    return statement;
            }
            string statementId = "";
            //Select all
            if (filterCriteria == null) {
                statementId = "SelectAll";
                statement = LoadStatement(info, session, statementId, ADPSQLStatementType.SelectAll, ADPStatementGeneration.Automatic, info.SortExpression);
                statement.Prepare();
                return statement;
            }
            //Select according to criteria
            if (filterCriteria != null) {
                //Auto built select statemens
                if (filterCriteria.StatementType == ADPSQLStatementType.AutoBuiltCustomSelect) {
                    statement = LoadStatement(info, session, statementId, ADPSQLStatementType.SelectAll, ADPStatementGeneration.Automatic, filterCriteria.SortExpression);
                    if (!statement.StatementText.Contains("WHERE")) {
                        statement.Type = ADPSQLStatementType.AutoBuiltCustomSelect;
                        string sql = statement.StatementText;
                        sql += " WHERE ";
                        sql += filterCriteria.SqlExpression;
                        sql = ReplacePropertyNamesByFieldNames(info, sql);
                        statement.StatementText = sql;
                        statement.Prepare();
                    }
                    return statement;
                }
                //Standard select statements
                statementId = filterCriteria.Id;
                switch (filterCriteria.StatementType) {
                    case ADPSQLStatementType.SelectByKey:
                        statement = LoadStatement(info, session, statementId, ADPSQLStatementType.SelectByKey, ADPStatementGeneration.Automatic, filterCriteria.SortExpression);
                        statement.Prepare();
                        statement.ParamByName(info.KeyField).Value = filterCriteria.FilterConditions[0].Value;
                        return statement;
                    case ADPSQLStatementType.SelectBySingleField:
                        statement = LoadStatement(info,session,  statementId, ADPSQLStatementType.SelectBySingleField, ADPStatementGeneration.Automatic, filterCriteria.SortExpression);
                        statement.Prepare();
                        string propertyName = filterCriteria.FilterConditions[0].PropertyName;
                        string fieldName = info.PropertyMapping[propertyName];
                        statement.ParamByName("FIELDNAME").Value = fieldName;
                        statement.ParamByName("FIELDVALUE").Value = filterCriteria.FilterConditions[0].Value;
                        return statement;
                }
            }
            //If no method above has returned a statement, returns null
            return null;
        }
        /// <summary>
        /// Return a prepared statement
        /// </summary>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <returns>
        /// Desired statement
        /// </returns>
        internal static ADPSQLStatement GetPreparedStatement(ADPSession session, Type type, ADPFilterCriteria filterCriteria) {
            return GetPreparedStatement(session, type, filterCriteria, ADPSQLStatementType.CustomSelect, null);
        }
        /// <summary>
        /// Return a prepared statement
        /// </summary>
        /// <param name="statementType">
        /// Type of statement to be loaded
        /// </param>
        /// <param name="obj">
        /// Object to be used as source of data
        /// </param>
        /// <returns>
        /// Desired statement
        /// </returns>
        internal static ADPSQLStatement GetPreparedStatement(ADPSession session, Type type, ADPSQLStatementType statementType, ADPObject obj) {
            return GetPreparedStatement(session, type, null, statementType, obj);
        }
        /// <summary>
        /// Load an object from a given DataRow
        /// </summary>
        /// <param name="row">
        /// DataRow to be loaded
        /// </param>
        /// <param name="obj">
        /// Object to be loaded
        /// </param>
        internal static void LoadObjectFromDataRow(ADPSession session, Type type, DataRow row, ADPObject obj) {
            ADPMappingInformation info = ADPPersister.GetMappingInformation(type);
            foreach (string p in info.PropertyMapping.Keys) {
                string f = info.PropertyMapping[p];
                PropertyDescriptor prop = info.Properties[p];
                if (prop == null) {
                    throw new ADPException(String.Format("Property {0} not found", p));
                }
                object o = row[f];
                //Null values
                if (o is DBNull) {
                    prop.SetValue(obj, null);
                    continue;
                }
                //ADPObject types
                if (prop.PropertyType.IsSubclassOf(typeof(ADPObject))) {
                    bool earlyLoading = prop.Attributes.Contains(new EarlyLoadingAttribute());
                    if (o is int) {
                        if (earlyLoading) {
                            ADPObject adpObj = ADPObject.CreateInstance(session, prop.PropertyType);
                            adpObj.Load(Convert.ToInt32(o));
                            o = adpObj;
                        } else {
                            if (obj.LazyLoadingPropertyKeys == null) {
                                obj.LazyLoadingPropertyKeys = new Dictionary<string, int>();
                            }
                            obj.LazyLoadingPropertyKeys[prop.Name] = (int)o;
                            o = null;
                        }
                    } else {
                        o = null;
                    }
                }
                //Convert o to the correct type
                if (o != null) {
                    if (o.GetType() != prop.PropertyType) {
                        o = Convert.ChangeType(o, prop.PropertyType);
                    }
                }
                //Set the property value
                obj.SetPropertyValue(prop.Name, o);
            }
        }
        /// <summary>
        /// Load an object from the current row of a DataTable
        /// </summary>
        /// <param name="dataTable">
        /// DataTable containing the record to be loaded
        /// </param>
        /// <param name="obj">
        /// Object to be loaded
        /// </param>
        internal static void LoadObjectFromDataTable(ADPSession session, Type type, DataTable dataTable, ADPObject obj) {
            if (dataTable.Rows.Count > 0) {
                LoadObjectFromDataRow(session, type, dataTable.Rows[0], obj);
            }
        }
        /// <summary>
        /// Load a list of objects from a DataTable
        /// </summary>
        /// <param name="dataTable">
        /// DataTable containing the objects to be loaded
        /// </param>
        /// <param name="list">
        /// List that will hold the loaded objects
        /// </param>
        internal static void LoadRangeFromDataTable(ADPSession session, Type type, DataTable dataTable, IList list) {
            foreach (DataRow row in dataTable.Rows) {
                ADPObject obj = ADPObject.CreateInstance(type);
                LoadObjectFromDataRow(session, type, row, obj);
                list.Add(obj);
            }
        }
        
        /// <summary>
        /// Register a ADPPersisterHelper for a given type
        /// </summary>
        /// <param name="type">
        /// Type
        /// </param>
        /// <param name="helper">
        /// Helper
        /// </param>
        public static void RegisterHelper(Type type, ADPBasePersisterHelper helper) {
            if (helpers == null) {
                helpers = new Dictionary<Guid, ADPBasePersisterHelper>();
            }
            helpers[type.GUID] = helper;
        }
    }
}
