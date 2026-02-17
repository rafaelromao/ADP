using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Indicate the name of the table that store the persistent object
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class MappingTableAttribute : Attribute {
        readonly string mappingTableName = null;
        public MappingTableAttribute(string tableName) {
            this.mappingTableName = tableName;
        }
        public string TableName {
            get {
                return this.mappingTableName;
            }
        }
    }

    /// <summary>
    /// Indicate the name of the primary key field of the table that store the object
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class MappingKeyFieldAttribute : Attribute {
        readonly string mappingKeyFieldName = null;
        public MappingKeyFieldAttribute(string fieldName) {
            this.mappingKeyFieldName = fieldName;
        }
        public string KeyFieldName {
            get {
                return this.mappingKeyFieldName;
            }
        }
    }

    /// <summary>
    /// Indicate that the property mapping must be performed manually
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class NoAutoMappingAttribute : Attribute {
        public NoAutoMappingAttribute() {
        }
    }

    /// <summary>
    /// Indicate the name of the field that store the value of the property
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class MappingFieldAttribute : Attribute {
        readonly string mappingFieldName = null;
        public MappingFieldAttribute(string fieldName) {
            this.mappingFieldName = fieldName;
        }
        public string FieldName {
            get {
                return this.mappingFieldName;
            }
        }
    }

    /// <summary>
    /// Indicate the name of the property that will give the value to be stored as the property value
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class MappingPropertyAttribute : Attribute {
        readonly string mappingPropertyName = null;
        public MappingPropertyAttribute(string propertyName) {
            this.mappingPropertyName = propertyName;
        }
        public string PropertyName {
            get {
                return this.mappingPropertyName;
            }
        }
    }

    /// <summary>
    /// Indicate the type of the value to be stored as the property value
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class MappingTypeAttribute : Attribute {
        readonly string mappingTypeName = null;
        public MappingTypeAttribute(string typeName) {
            this.mappingTypeName = typeName;
        }
        public string TypeName {
            get {
                return this.mappingTypeName;
            }
        }
    }

    /// <summary>
    /// Indicate the property must have its value loaded together to the parent object
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited=false, AllowMultiple=false)]
    public sealed class EarlyLoadingAttribute : Attribute {
        public EarlyLoadingAttribute() {
        }
    }
}
