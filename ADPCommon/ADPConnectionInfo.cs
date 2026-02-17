using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Net;

namespace Cati.ADP.Common {
    /// <summary>
    /// Specifies if the IADPProvider to be used is Local (ADPProvider) or remote (ADPClient)
    /// </summary>
    public enum ADPProviderType {
        Local,
        Remote
    }
    
    /// <summary>
    /// This surrogate is used to help on the XML serialization of the ADPConnectionInfo
    /// </summary>
    [Obsolete("Not used anymore!")]
    internal sealed class ADPConnectionInfoSurrogate : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info,
                                                StreamingContext context) {
            ADPConnectionInfo ci = obj as ADPConnectionInfo;
            info.AddValue("DatabaseDriver", ci.DatabaseDriver);
            info.AddValue("DatabaseSessionID", Convert.ToString(ci.DatabaseSessionID));
            info.AddValue("DatabaseName", ci.DatabaseName);
            info.AddValue("DatabaseServer", ci.DatabaseServer);
            info.AddValue("DatabaseStoredStatementFileType", (int)ci.DatabaseStoredStatementFileType);
            info.AddValue("DatabaseStoredStatementFileName", ci.DatabaseStoredStatementFileName);
            info.AddValue("DatabaseLanguage", ci.DatabaseLanguage);
            info.AddValue("DatabaseUserName", ci.DatabaseUserName);
            info.AddValue("DatabasePassword", ci.DatabasePassword);
            info.AddValue("DatabasePoolSize", ci.DatabasePoolSize);
            info.AddValue("DatabaseTimeOut", ci.DatabaseTimeOut);
            
            info.AddValue("ADPServerTimeOut", ci.ADPServerTimeOut);
            info.AddValue("ADPServerHost", ci.ADPServerHost);
            info.AddValue("ADPServerPort", ci.ADPServerPort);

            info.AddValue("ADPConnectionFactoryAssemblyName", ci.ADPConnectionFactoryAssemblyName);
            info.AddValue("ADPConnectionFactoryTypeName", ci.ADPConnectionFactoryTypeName);

            info.AddValue("DelimitedFileSeparator", ci.DelimitedFileSeparator);
        }

        public object SetObjectData(object obj, SerializationInfo info,
                   StreamingContext context, ISurrogateSelector selector) {
            ADPConnectionInfo ci = obj as ADPConnectionInfo;
            ci.DatabaseDriver = info.GetString("DatabaseDriver");
            ci.DatabaseSessionID = new Guid(info.GetString("DatabaseSessionID"));
            ci.DatabaseName = info.GetString("DatabaseName");
            ci.DatabaseServer = info.GetString("DatabaseServer");
            ci.DatabaseStoredStatementFileType = (ADPStoredStatementFileType)info.GetInt32("DatabaseStoredStatementFileType");
            ci.DatabaseStoredStatementFileName = info.GetString("DatabaseStoredStatementFileName");
            ci.DatabaseLanguage = info.GetString("DatabaseLanguage");
            ci.DatabaseUserName = info.GetString("DatabaseUserName");
            ci.DatabasePassword = info.GetString("DatabasePassword");
            ci.DatabasePoolSize = info.GetInt32("DatabasePoolSize");
            ci.DatabaseTimeOut = info.GetInt32("DatabaseTimeOut");

            ci.ADPServerHost = info.GetString("ADPServerHost");
            ci.ADPServerPort = info.GetInt32("ADPServerPort");
            ci.ADPServerTimeOut = info.GetInt32("ADPServerTimeOut");

            ci.ADPConnectionFactoryAssemblyName = info.GetString("ADPConnectionFactoryAssemblyName");
            ci.ADPConnectionFactoryTypeName = info.GetString("ADPConnectionFactoryTypeName");

            ci.DelimitedFileSeparator = info.GetString("DelimitedFileSeparator");
            return null;
        }
    }

    /// <summary>
    /// Store all the information necessary to estabilish a connection with the 
    /// ADPServer or ADPProvider and with the system database
    /// </summary>
    public sealed class ADPConnectionInfo {
        /// <summary>
        /// Guid that identifies your connection with the database.
        /// It is setted at the end of the ADPProxy.ConnectionInfo.
        /// </summary>
        public Guid DatabaseSessionID;
        /// <summary>
        /// Driver to be used to connect to the database
        /// </summary>
        public string DatabaseDriver = "IBProvider";
        /// <summary>
        /// Name of the computer that host the database server
        /// </summary>
        public string DatabaseServer = "localhost";
        /// <summary>
        /// File address or alias of the database
        /// </summary>
        public string DatabaseName = "";
        /// <summary>
        /// User name to be used to connect to the database
        /// </summary>
        public string DatabaseUserName = "SYSDBA";
        /// <summary>
        /// Password to be used to connect to the database
        /// </summary>
        public string DatabasePassword = "masterkey";
        /// <summary>
        /// Identifies the SQL Statements language to be used
        /// </summary>
        /// <remarks>
        /// When you use identified statements, the language indicates which
        /// version of the statement text you want to use, according to the
        /// database server used
        /// </remarks>
        public string DatabaseLanguage = "";
        /// <summary>
        /// Amount of time to wait for a response after to try to estabilish
        /// a connection to the database
        /// </summary>
        public int DatabaseTimeOut = 10;
        /// <summary>
        /// Maximum amount of connection to be estabilished with the database
        /// </summary>
        /// <remarks>
        /// If the DatabasePoolSize is greater than the current amount of licenses
        /// available in the database, no error will be displayed and your request
        /// will be wait for an available connection
        /// </remarks>
        public int DatabasePoolSize = 4;

        /// <summary>
        /// Name of the file that contains the available identified statements
        /// </summary>
        public string DatabaseStoredStatementFileName;
        /// <summary>
        /// Type of the file specified in the DatabaseSchema field
        /// </summary>
        public ADPStoredStatementFileType DatabaseStoredStatementFileType;
        
        /// <summary>
        /// Name of the computer that hosts the ADPServer to be used
        /// </summary>
        public string ADPServerHost = Dns.GetHostName();
        /// <summary>
        /// Port listened by the ADPServer on the ADPServerHost computer
        /// </summary>
        public int ADPServerPort = 9850;
        /// <summary>
        /// Amount of time to wait after try to communicate with the ADPServer
        /// </summary>
        public int ADPServerTimeOut = 30;
        /// <summary>
        /// Name of the assembly that hosts the ADPConnectionFactory you want to use
        /// to create your connection with the database
        /// </summary>
        /// <remarks>
        /// If a null value is passed, the ADPDefaultConnectionFactory will be used
        /// </remarks>
        public string ADPConnectionFactoryAssemblyName = null;
        /// <summary>
        /// Name of your ADPConnectionFactory type
        /// </summary>
        /// <remarks>
        /// If a null value is passed, the ADPDefaultConnectionFactory will be used
        /// </remarks>
        public string ADPConnectionFactoryTypeName = null;

        /// <summary>
        /// A string that identifies your database connection settings.
        /// It is used to provide the same connections of the pool to the client
        /// applications that try to connect to the same database.
        /// </summary>
        public string DatabaseID {
            get {
                string result = "{0};{1};{2};{3};{4}";
                result = String.Format(result, DatabaseStoredStatementFileName, DatabaseDriver, DatabaseServer, 
                                               DatabaseName, DatabaseUserName);

                return result;
            }
        }

        /// <summary>
        /// A separator string to use with delimeted files
        /// </summary>
        public string DelimitedFileSeparator = "|";

        #region Serialization Properties
        /// <summary>
        /// Assign this ADPConnectionInfo to another one
        /// </summary>
        /// <param name="destination">
        /// The ADPConnectionInfo that will receive the assignement
        /// </param>
        public void AssignTo(ADPConnectionInfo destination) {
            destination.DatabaseSessionID = this.DatabaseSessionID;
            destination.DatabaseDriver = this.DatabaseDriver;
            destination.DatabaseServer = this.DatabaseServer;
            destination.DatabaseName = this.DatabaseName;
            destination.DatabaseUserName = this.DatabaseUserName;
            destination.DatabasePassword = this.DatabasePassword;
            destination.DatabaseStoredStatementFileName = this.DatabaseStoredStatementFileName;
            destination.DatabaseLanguage = this.DatabaseLanguage;
            destination.DatabasePoolSize = this.DatabasePoolSize;
            destination.DatabaseTimeOut = this.DatabaseTimeOut;

            destination.ADPServerHost = this.ADPServerHost;
            destination.ADPServerPort = this.ADPServerPort;
            destination.ADPServerTimeOut = this.ADPServerTimeOut;
            destination.ADPConnectionFactoryAssemblyName = this.ADPConnectionFactoryAssemblyName;
            destination.ADPConnectionFactoryTypeName = this.ADPConnectionFactoryTypeName;

            destination.DelimitedFileSeparator = this.DelimitedFileSeparator;
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
                        return ADPSerializer.SerializeXml(this);
                    } finally {
                        serializing = false;
                    }
                } else {
                    return null;
                }
            }
            set {
                Object o = ADPSerializer.DeserializeXml(this.GetType(), value);
                ADPConnectionInfo clone = (ADPConnectionInfo)o;
                if (clone != null) {
                    clone.AssignTo(this);
                }
            }
        }
        #endregion
    }
}
