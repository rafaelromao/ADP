using System;
using System.Collections.Generic;
using System.Text;
using Cati.ADP.Common;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace Cati.ADP.Server {
    /// <summary>
    /// Provides a method to create connections according to a driver id
    /// This object must be overriden as well as its GetConnection method
    /// to return a proper IADPConnection object
    /// </summary>
    public class ADPBaseConnectionFactory {
        /// <summary>
        /// Creates a connection according to the given driver id
        /// </summary>
        /// <param name="driverID">
        /// Driver id that identifies the IADPConnection that must be created
        /// </param>
        /// <returns>
        /// A new IADPConnection
        /// </returns>
        public virtual IADPConnection GetConnection(string driverID) {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get a connection factory from the given assembly
        /// </summary>
        /// <param name="assemblyName">
        /// Name of assembly that contains the factory.
        /// It must reside in the same folder than the ADPServerLibrary assembly
        /// </param>
        /// <param name="factoryName">
        /// Name of the factory
        /// </param>
        /// <returns>
        /// Your connection factory
        /// </returns>
        public static ADPBaseConnectionFactory GetConnectionFactory(string assemblyName, string factoryName) {
            if ((factoryName == null) || (assemblyName == null)) {
                assemblyName = "ADPConnectionDrivers.dll";
                factoryName = "Cati.ADP.Server.ADPDefaultConnectionFactory";
            }
            string path = Application.ExecutablePath;
            int k = path.LastIndexOf("\\");
            path = path.Substring(0, k + 1);
            assemblyName = path + assemblyName;
            //Load the assembly where the factory is located
            Assembly assembly = Assembly.LoadFile(assemblyName);
            if (assembly == null) {
                throw new ADPException(String.Format("Could not find assembly {0}!", assemblyName));
            }
            Type[] types = assembly.GetExportedTypes();
            bool typeFound = false;
            //Try to find the type by its FullName
            foreach (Type type in types) {
                if (type.FullName == factoryName) {
                    typeFound = true;
                    break;
                }
            }
            //Try to find the type by its Name, if a fullname was not supplied
            foreach (Type type in types) {
                if (type.Name == factoryName) {
                    typeFound = true;
                    factoryName = type.FullName;
                    break;
                }
            }
            //Create and return a new instance of the type
            if (!typeFound) {
                throw new ADPException(String.Format("Could not find assembly {0}!", assemblyName));
            }
            ADPBaseConnectionFactory result = (ADPBaseConnectionFactory)assembly.CreateInstance(factoryName);
            return result;
        }
    }
}
