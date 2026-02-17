using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Data;

namespace Cati.ADP.Common {
    /// <summary>
    /// Specifies if a ADPParam is a SQLParameter, passed to a SQL command just before its execution
    /// or a SQLToken, a work that must be replaced by a value and compose the statement text
    /// </summary>
    public enum ADPParamType {
        SQLParameter,
        SQLToken
    }

    /// <summary>
    /// This surrogate helps the binary serialization of the ADPParam object
    /// </summary>
    internal sealed class ADPParamSurrogate : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info,
                                            StreamingContext context) {
            ADPParam param = obj as ADPParam;
            info.AddValue("Name", param.Name);
            info.AddValue("DataType", param.DataType);
            info.AddValue("Value", param.Value);
            info.AddValue("ParamType", param.ParamType);
        }

        public object SetObjectData(object obj, SerializationInfo info,
                   StreamingContext context, ISurrogateSelector selector) {
            ADPParam param = obj as ADPParam;
            param.Name = info.GetString("Name");
            DbType dataType = (DbType)(info.GetInt32("DataType"));
            param.Value = info.GetValue("Value", ADPParam.GetObjectType(dataType));
            param.ParamType = (ADPParamType)info.GetInt32("ParamType");
            return null;
        }
    }

    /// <summary>
    /// A parameter of a SQL statement execution
    /// </summary>
    public sealed class ADPParam {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ADPParam() {
        }
        /// <summary>
        /// Initializer constructor
        /// </summary>
        /// <param name="paramName">
        /// Name of the parameter
        /// </param>
        /// <param name="paramType">
        /// Type of the parameter
        /// </param>
        /// <param name="paramValue">
        /// Value of the parameter
        /// </param>
        public ADPParam(string paramName, ADPParamType paramType, Object paramValue) {
            this.Name = paramName;
            this.Value = paramValue;
            this.ParamType = paramType;
        }
        /// <summary>
        /// Converts a DbType to a Type
        /// </summary>
        /// <param name="dbType">
        /// DbType to be converted
        /// </param>
        /// <returns>
        /// Equivalent Type
        /// </returns>
        public static Type GetObjectType(DbType dbType) {
            switch (dbType) {
                #region Integer Types
                case DbType.Byte:
                    return Type.GetType("System.Byte", true, true);
                case DbType.SByte:
                    return Type.GetType("System.SByte", true, true);
                case DbType.Int16:
                    return Type.GetType("System.Int16", true, true);
                case DbType.Int32:
                    return Type.GetType("System.Int32", true, true);
                case DbType.Int64:
                    return Type.GetType("System.Int64", true, true);
                case DbType.UInt16:
                    return Type.GetType("System.UInt16", true, true);
                case DbType.UInt32:
                    return Type.GetType("System.UInt32", true, true);
                case DbType.UInt64:
                    return Type.GetType("System.UInt64", true, true);
                #endregion

                #region Float Types
                case DbType.Decimal:
                    return Type.GetType("System.Decimal", true, true);
                case DbType.Single:
                    return Type.GetType("System.Single", true, true);
                case DbType.Double:
                    return Type.GetType("System.Double", true, true);
                case DbType.Currency:
                    return Type.GetType("System.Double", true, true);
                case DbType.VarNumeric:
                    return Type.GetType("System.Double", true, true);
                #endregion

                #region Date Types
                case DbType.Date:
                    return Type.GetType("System.DateTime", true, true);
                case DbType.Time:
                    return Type.GetType("System.DateTime", true, true);
                case DbType.DateTime:
                    return Type.GetType("System.DateTime", true, true);
                #endregion

                #region Boolean Types
                case DbType.Boolean:
                    return Type.GetType("System.Boolean", true, true);
                #endregion

                #region String Types
                case DbType.Guid:
                    return Type.GetType("System.Guid", true, true);
                case DbType.String:
                    return Type.GetType("System.String", true, true);
                case DbType.AnsiString:
                    return Type.GetType("System.String", true, true);
                case DbType.Binary:
                    return Type.GetType("System.IO.MemoryStream", true, true);
                #endregion

                default:
                    throw new ADPException(String.Format("Invalid parameter type: {0}", dbType));
            }
        }
        /// <summary>
        /// Converts a Type to a DbType
        /// </summary>
        /// <param name="objectType">
        /// Type to be converted
        /// </param>
        /// <returns>
        /// Equivalent DbType
        /// </returns>
        public static DbType GetDbType(Type objectType) {
            switch (objectType.Name) {
                #region Integer Types
                case "Int16":
                    return DbType.Int16;
                case "Int32":
                    return DbType.Int32;
                case "Int64":
                    return DbType.Int64;
                case "UInt16":
                    return DbType.UInt16;
                case "UInt32":
                    return DbType.UInt32;
                case "UInt64":
                    return DbType.UInt64;
                #endregion

                #region Float Types
                case "Decimal":
                    return DbType.Decimal;
                case "Single":
                    return DbType.Single;
                case "Double":
                    return DbType.Double;
                #endregion

                #region Date Types
                case "DateTime":
                    return DbType.DateTime;
                #endregion

                #region Boolean Types
                case "Boolean":
                    return DbType.Boolean;
                #endregion

                #region String Types
                case "Guid":
                    return DbType.Guid;
                case "String":
                    return DbType.String;
                case "MemoryStream":
                    return DbType.Binary;
                #endregion

                default: 
                    throw new ADPException(String.Format("Invalid parameter type: {0}!", objectType.Name));
            }
        }

        private DbType dataType;
        /// <summary>
        /// Type of the parameter value
        /// </summary>
        public DbType DataType {
            get { return dataType; }
        }
        private Object dataValue;
        /// <summary>
        /// Parameter value
        /// </summary>
        public Object Value { 
            get { return dataValue; }
            set {
                if (value == null) {
                    dataType = DbType.String;
                } else {
                    dataType = GetDbType(value.GetType());
                }
                dataValue = value;
                if (dataValue != null) {
                    if (dataValue.GetType() == typeof(String)) {
                        dataValue = ADPUtils.TrimQuotes((String)dataValue);
                    }
                }
            }
        }
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name;
        /// <summary>
        /// Inform if the param is a SQLParameter or a SQLToken
        /// </summary>
        public ADPParamType ParamType;

        #region Serialization Properties
        /// <summary>
        /// Assign this ADPParam to another one
        /// </summary>
        /// <param name="destination">
        /// The ADPParam that will receive the assignement
        /// </param>
        public void AssignTo(ADPParam destination) {
            destination.Name = this.Name;
            destination.Value = this.Value;
            destination.ParamType = this.ParamType;
        }
        /// <summary>
        /// A flag used in the serialization process to indicate if its being done
        /// </summary>
        private bool serializing = false;
        /// <summary>
        /// Gets a string containing the current object serialized
        /// or assign an object created according to the given value
        /// to the current object
        /// </summary>
        public string Serialized {
            get {
                if (!serializing) {
                    serializing = true;
                    try {
                        return ADPSerializer.Serialize(this, new ADPParamSurrogate());
                    } finally {
                        serializing = false;
                    }
                } else {
                    return null;
                }
            }
            set {
                ADPParam clone = (ADPParam)ADPSerializer.Deserialize(this.GetType(), value, new ADPParamSurrogate());
                if (clone != null) {
                    clone.AssignTo(this);
                }
            }
        }
        #endregion
    }
}
