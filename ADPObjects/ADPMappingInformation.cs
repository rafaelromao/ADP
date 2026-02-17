using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Store the information necessary to perform the object mapping to database
    /// </summary>
    public sealed class ADPMappingInformation {
        /// <summary>
        /// Creates a ADPMappingInformation
        /// </summary>
        /// <param name="objectSession">
        /// Session that must be used to create objects
        /// and reach the server
        /// </param>
        /// <param name="objectType">
        /// Type of object persisted by this persister
        /// </param>
        internal ADPMappingInformation(Type objectType) {
            SQLStatements = new Dictionary<string, ADPSQLStatement>();
            ObjectType = objectType;
            Initialize();
        }

        #region Property Fields
        private string commaFields = null;
        private string commaParams;
        private string setFieldsText;
        private PropertyDescriptorCollection properties = null;
        #endregion

        /// <summary>
        /// This method must be overriden to populate the PropertyMapping dictionary
        /// </summary>
        private void Initialize() {
            AttributeCollection typeAttributes = TypeDescriptor.GetAttributes(ObjectType);
            //If specified to not auto set mapping information
            if (typeAttributes.Contains(new NoAutoMappingAttribute())) {
                return;
            }
            //Get the table name
            MappingTableAttribute mta = (MappingTableAttribute)typeAttributes[typeof(MappingTableAttribute)];
            if (mta.TableName != null) {
                TableName = mta.TableName.ToUpper();
                KeyField = TablePrefix + "_CODE";
                KeyGenerator = KeyField + "_GEN";
            }
            //Get the mapping key field
            MappingKeyFieldAttribute mkfa = (MappingKeyFieldAttribute)typeAttributes[typeof(MappingKeyFieldAttribute)];
            if (mkfa != null) {
                KeyField = mkfa.KeyFieldName.ToUpper();
                KeyGenerator = KeyField + "_GEN";
            }
            //Set the property mapping
            foreach (PropertyDescriptor p in Properties) {
                string fieldName = DefinePropertyFieldName(p);
                if (fieldName != null) {
                    PropertyMapping[p.Name] = fieldName.ToUpper();
                }
            }
        }

        /// <summary>
        /// Define and return a field name to map a given property
        /// </summary>
        /// <param name="p">
        /// Property to be mapped
        /// </param>
        /// <returns>
        /// Name of the field that maps the property
        /// </returns>
        private string DefinePropertyFieldName(PropertyDescriptor prop) {
            string result = null;
            //If it is the Key property
            if (prop.Name == "Key") {
                return KeyField;
            }
            //If its another ADPObject property
            if (prop.ComponentType == typeof(ADPObject)) {
                return null;
            }
            //If specified to not auto set mapping information
            if (prop.Attributes.Contains(new NoAutoMappingAttribute())) {
                return null;
            }
            //If it is an ADPCollection property
            if (prop.PropertyType.IsSubclassOf(typeof(ADPCollection<>))) {
                return null;
            }
            //If it has a field name attribute or is a property of a type that extends ADPObject
            MappingFieldAttribute a = (MappingFieldAttribute)prop.Attributes[typeof(MappingFieldAttribute)];
            if (a != null) {
                result = a.FieldName;
            } else {
                if (prop.PropertyType.IsSubclassOf(typeof(ADPObject))) {
                    AttributeCollection typeAttributes = prop.Attributes;
                    MappingTableAttribute mta = (MappingTableAttribute)typeAttributes[typeof(MappingTableAttribute)];
                    if (mta != null) {
                        string propTableName = mta.TableName;
                        if (propTableName != null) {
                            int k = propTableName.IndexOf("_");
                            if (k > -1) {
                                string propTablePrefix = propTableName.Substring(0, k);
                                result = propTablePrefix + "_CODE";
                            }
                        }
                    }
                }
            }
            //If could not find a name to the field, build one
            if (result == null) {
                result = TablePrefix;
                string n = prop.Name;
                for (int i = 0; i < n.Length; i++) {
                    string s = n.Substring(i, 1);
                    if (s == s.ToUpper()) {
                        result += "_";
                    }
                    result += s;
                }
            }
            return result;
        }

        /// <summary>
        /// Build the properties CommaFields, CommaParams and SetFieldsText
        /// </summary>
        private void GenerateHelperFields() {
            commaFields = "";
            commaParams = "";
            setFieldsText = "";
            foreach (string p in PropertyMapping.Keys) {
                string f = PropertyMapping[p];
                commaFields += ", " + f;
                commaParams += ", :" + f;
                setFieldsText += String.Format(", {0} = :{0}", f);
            }
            commaFields = commaFields.Remove(0, 2);
            commaParams = commaParams.Remove(0, 2);
            setFieldsText = setFieldsText.Remove(0, 2);
        }

        /// <summary>
        /// Type of object persisted by this persister
        /// </summary>
        internal Type ObjectType;
        /// <summary>
        /// Identificator to be used by the IADPConnection to generate keys
        /// </summary>
        internal string KeyGenerator = "";
        /// <summary>
        /// Dictionay containing all the SQL statements already used
        /// </summary>
        internal Dictionary<string, ADPSQLStatement> SQLStatements;
        /// <summary>
        /// Name of the table of the database that store this object when persisted
        /// </summary>
        internal string TableName = "";
        /// <summary>
        /// The sub string found before the first occurrency of the char "_"
        /// in the table name, or empty if no occurrency be found
        /// </summary>
        internal string TablePrefix {
            get {
                int k = TableName.IndexOf("_");
                if (k == -1) {
                    return "";
                } else {
                    return TableName.Substring(0, k);
                }
            }
        }
        /// <summary>
        /// Name of the primary key field of the table that stores this object when persisted
        /// </summary>
        internal string KeyField = "";
        /// <summary>
        /// Sort expression to be used by default
        /// </summary>
        internal string SortExpression = "";
        /// <summary>
        /// Fields of the table that stores this object, separated by commas
        /// </summary>
        internal string CommaFields {
            get {
                if (commaFields == null) {
                    GenerateHelperFields();
                }
                return commaFields;
            }
        }
        /// <summary>
        /// Fields of the table that stores this object, 
        /// separated by commas and formated as parameter names
        /// </summary>
        internal string CommaParams {
            get {
                if (commaParams == null) {
                    GenerateHelperFields();
                }
                return commaParams;
            }
        }
        /// <summary>
        /// Fields of the table that stores this object, 
        /// separated by commas and formated to be used in an UPDATE statement
        /// </summary>
        internal string SetFieldsText {
            get {
                if (setFieldsText == null) {
                    GenerateHelperFields();
                }
                return setFieldsText;
            }
        }
        /// <summary>
        /// PropertyDescriptorCollection containing all the public properties of
        /// the persisted object type.
        /// </summary>
        internal PropertyDescriptorCollection Properties {
            get {
                if (properties == null) { 
                    properties = TypeDescriptor.GetProperties(ObjectType); 
                }
                return properties;
            }
        }
        /// <summary>
        /// Dictionary containing the mapping of fields to properties
        /// It is used to perform the automatic preparation of sql statements
        /// </summary>
        public Dictionary<string, string> PropertyMapping = new Dictionary<string, string>();
    }
}
