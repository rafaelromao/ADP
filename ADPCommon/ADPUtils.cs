using System;
using System.Collections.Generic;
using System.Text;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Net;
using System.Threading;

namespace Cati.ADP.Common {
    /// <summary>
    /// Specifies the messages used by the ADPClient and ADPServer to provide their services
    /// </summary>
    public enum ADPMessageTypes {
        Login,
        GetConnection,
        ReleaseConnection,
        StartTransaction,
        Commit,
        Rollback,
        GetKey,
        GetSQLStatement,
        ExecuteSelectStatement,
        ExecuteSelectStatementInTransaction,
        ExecuteCommandStatement,
        Exception
    }

    /// <summary>
    /// Provides constants and static methods to be used by all the ADP Framework
    /// </summary>
    public static class ADPUtils {

        #region Communication Flags and Constants
        public const int BufferSize = 4194304; /*(int)Math.Pow(2, 22)*/
        public const int DefaultTimeOut = 180;
        public const int ThreadSleepInterval = 1;
        public const int ThreadSleepHighInterval = 100;
        public const string MessageStart = "{02593D2C-5C75-4821-B7B7-AFF659B92E59}";
        public const string CheckSumStart = "{7D6380FE-995C-47F6-AE15-08D056B40C35}";
        public const string MessageEnd   = "{1F34483C-D556-42BE-90DA-6507679BB8B3}";
        public const string PacketStart  = "{8AF5C804-8C87-4441-98D3-0C7447286ED7}";
        public const string PacketEnd    = "{1BBE6E03-BF7C-4B97-A739-7094EB04B3BD}";
        #endregion

        #region Communication Parameter Names
        public const string Result = "@Result";
        public const string Ok = "@Ok";
        public const string ConnectionInfo = "@ConnectionInfo";
        public const string StackTrace = "StackTrace";
        public const string Port = "@Port";
        public const string TryAgain = "@TryAgain";
        public const string DatabaseSessionID = "@DatabaseSessionID";
        public const string TransactionID = "@TransactionID";
        public const string ConnectionID = "@ConnectionID";
        public const string DataTableID = "@DataTableID";
        public const string SqlCommandID = "@SqlCommandID";
        public const string ExceptionName = "@ExceptionName";
        public const string ExceptionMessage = "@ExceptionMessage";
        public const string ExceptionStack = "@ExceptionStack";
        public const string ExceptionSource = "@ExceptionSource";
        public const string KeyId = "@KeyID";
        public const string StatementID = "@StatementId";
        #endregion

        #region Statement Provider Constants
        public const string DataTables = "DataTables";
        public const string Commands = "Commands";
        public const string StatementText = "StatementText";
        public const string DBLanguage = "DBLanguage";
        public const string DefaultStatement = "DefaultStatement";
        #endregion

        #region Connection Drivers
        public const string IBProvider = "IBProvider";
        #endregion

        #region Windows Registry Access
        public static void SetLocalMachineValue(string key, string value, object content) {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, true);
            if (registryKey == null) {
                registryKey = Registry.LocalMachine.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            registryKey.SetValue(value, content);
        }
        public static object GetLocalMachineValue(string key, string value, object defaultResult) {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key);
            if (registryKey == null) {
                return defaultResult;
            }
            object result = registryKey.GetValue(value);
            if (result == null) {
                return defaultResult;
            } else {
                return result;
            }
        }
        public static object GetLocalMachineValue(string key, string value) {
            return GetLocalMachineValue(key, value, null);
        }
        public static string ADPRegistryKey = "Software\\CATI\\ADP";
        #endregion

        #region Other methods
        public static string TrimQuotes(string value) {
            if (value.Length == 0) {
                return "";
            }
            if (value.Substring(0, 1) == "'") {
                value = value.Substring(1, value.Length - 1);
            }
            if (value.Substring(value.Length - 1, 1) == "'") {
                value = value.Substring(0, value.Length - 1);
            }
            return value;
        }
        public static IPAddress GetIPAddress(string hostName) {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            if ((ipAddresses == null) || (ipAddresses.Length == 0)) {
                return null;
            }
            IPAddress ipAddress = null;
            foreach (IPAddress ipa in ipAddresses) {
                if (!ipa.IsIPv6LinkLocal) {
                    ipAddress = ipa;
                    break;
                }
            }
            if (ipAddress == null) {
                ipAddress = ipAddresses[0];
            }
            return ipAddress;
        }
        #endregion
    }
}