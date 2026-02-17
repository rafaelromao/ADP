using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Receive, process and answer commands sent by the ADPClient
    /// </summary>
    public sealed class ADPServer {
        /// <summary>
        /// Creates a new ADPServer
        /// </summary>
        /// <param name="start">
        /// if true, starts the server
        /// </param>
        public ADPServer(bool start) {
            provider = new ADPProvider();
            int port = ADPServer.GetServerPort();
            commandServer = new ADPCommandServer(port);
            commandServer.OnCommandReceived += commandServer_CommandReceived;
            if (start) {
                Start();
            }
            if (ADPServer.GetDebugModeEnabled()) {
                ADPTracer.LogToFile(ADPServer.GetServerAddress() + ADPServer.GetLogFileName());
            }
        }
        /// <summary>
        /// Command server to be used to receive and answer the commands
        /// </summary>
        ADPCommandServer commandServer;
        /// <summary>
        /// Provider to be used to process the commands
        /// </summary>
        ADPProvider provider;
        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start() {
            commandServer.Start();
            ADPTracer.Print(this, "------------------------------------------------------------");
        }
        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop() {
            commandServer.Stop();
            ADPTracer.Print(this, "------------------------------------------------------------");
        }

        #region Command Interpretation Methods
        /// <summary>
        /// Interpret the login command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_Login(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                ADPConnectionInfo info = new ADPConnectionInfo();
                string connectionInfoSerialized = command.Params[ADPUtils.ConnectionInfo];
                if (connectionInfoSerialized == "") {
                    throw new ADPParameterMissingException(String.Format("{0}.InterpretCommand_Login", this.ToString()), "connectionInfo!");
                }
                info.Serialized = connectionInfoSerialized;
                commandServer.TimeOut = info.ADPServerTimeOut;
                String result = Convert.ToString(provider.Login(info));
                response.Id = (int)ADPMessageTypes.Login;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = result;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }

        }
        /// <summary>
        /// Interpret the StartTransaction command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_StartTransaction(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid DatabaseSessionID = new Guid(command.Params[ADPUtils.DatabaseSessionID]);
                String result = Convert.ToString(provider.StartTransaction(DatabaseSessionID));
                response.Id = (int)ADPMessageTypes.StartTransaction;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = result;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the Commit command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_Commit(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid transactionID = new Guid(command.Params[ADPUtils.TransactionID]);
                provider.Commit(transactionID);
                response.Id = (int)ADPMessageTypes.Commit;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = ADPUtils.Ok;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the Rollback command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_Rollback(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid transactionID = new Guid(command.Params[ADPUtils.TransactionID]);
                provider.Rollback(transactionID);
                response.Id = (int)ADPMessageTypes.Rollback;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = ADPUtils.Ok;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the GetConnection command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_GetConnection(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid DatabaseSessionID = new Guid(command.Params[ADPUtils.DatabaseSessionID]);
                String result = Convert.ToString(provider.GetConnection(DatabaseSessionID));
                response.Id = (int)ADPMessageTypes.GetConnection;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = result;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the ReleaseConnection command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_ReleaseConnection(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid connectionID = new Guid(command.Params[ADPUtils.ConnectionID]);
                provider.ReleaseConnection(connectionID);
                response.Id = (int)ADPMessageTypes.ReleaseConnection;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = ADPUtils.Ok;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the ExecuteSelectStatemen command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_ExecuteSelectStatement(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid connectionID = new Guid(command.Params[ADPUtils.ConnectionID]);
                String statementText = command.Params[ADPUtils.StatementText];
                List<String> ignoreList = new List<String>();
                ignoreList.Add(ADPUtils.ConnectionID);
                ignoreList.Add(ADPUtils.StatementText);
                ADPParam[] parameters = new ADPParam[command.Params.Count - ignoreList.Count];
                int i = 0;
                foreach (string k in command.Params.Keys) {
                    if (ignoreList.IndexOf(k) > -1) {
                        continue;
                    }
                    string v = command.Params[k];
                    parameters[i] = new ADPParam();
                    parameters[i].Serialized = v;
                    i++;
                }
                DataTable dt = provider.ExecuteSelectStatement(connectionID, statementText, parameters);
                String result = ADPSerializer.Serialize(dt);
                response.Id = (int)ADPMessageTypes.ExecuteSelectStatement;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = result;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the ExecuteSelectStatementInTransaction command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_ExecuteSelectStatementInTransaction(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid transactionID = new Guid(command.Params[ADPUtils.TransactionID]);
                String statementText = command.Params[ADPUtils.StatementText];
                List<String> ignoreList = new List<String>();
                ignoreList.Add(ADPUtils.TransactionID);
                ignoreList.Add(ADPUtils.StatementText);
                ADPParam[] parameters = new ADPParam[command.Params.Count - ignoreList.Count];
                int i = 0;
                foreach (string k in command.Params.Keys) {
                    if (ignoreList.IndexOf(k) > -1) {
                        continue;
                    }
                    string v = command.Params[k];
                    parameters[i] = new ADPParam();
                    parameters[i].Serialized = v;
                    i++;
                }
                DataTable dt = provider.ExecuteSelectStatementInTransaction(transactionID, statementText, parameters);
                String result = ADPSerializer.Serialize(dt);
                response.Id = (int)ADPMessageTypes.ExecuteSelectStatementInTransaction;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = result;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the ExecuteCommandStatement command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_ExecuteCommandStatement(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                Guid transactionID = new Guid(command.Params[ADPUtils.TransactionID]);
                String statementText = command.Params[ADPUtils.StatementText];
                List<String> ignoreList = new List<String>();
                ignoreList.Add(ADPUtils.TransactionID);
                ignoreList.Add(ADPUtils.StatementText);
                ADPParam[] parameters = new ADPParam[command.Params.Count - ignoreList.Count];
                int i = 0;
                foreach (string k in command.Params.Keys) {
                    if (ignoreList.IndexOf(k) > -1) {
                        continue;
                    }
                    string v = command.Params[k];
                    parameters[i] = new ADPParam();
                    parameters[i].Serialized = v;
                    i++;
                }
                provider.ExecuteCommandStatement(transactionID, statementText, parameters);
                response.Id = (int)ADPMessageTypes.ExecuteCommandStatement;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = ADPUtils.Ok;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the GetKey command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_GetKey(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                string keyId = command.Params[ADPUtils.KeyId];
                Guid DatabaseSessionID = new Guid(command.Params[ADPUtils.DatabaseSessionID]);
                object newKey = provider.GetKey(keyId, DatabaseSessionID);
                response.Id = (int)ADPMessageTypes.GetKey;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = Convert.ToString(newKey);
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the GetSQLStatement command
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_GetSQLStatement(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                string statementId = command.Params[ADPUtils.StatementID];
                Guid DatabaseSessionID = new Guid(command.Params[ADPUtils.DatabaseSessionID]);
                string statementText = provider.GetSQLStatement(statementId, DatabaseSessionID);
                response.Id = (int)ADPMessageTypes.GetSQLStatement;
                response.Params.Clear();
                response.Params[ADPUtils.Result] = statementText;
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        /// <summary>
        /// Interpret the unknown commands
        /// </summary>
        /// <param name="client">
        /// Client that sent the command
        /// </param>
        /// <param name="command">
        /// Command to be interpreted
        /// </param>
        public void InterpretCommand_UnknownCommand(TcpClient client, ADPMessage command) {
            try {
                ADPMessage response = new ADPMessage();
                response.Id = (int)ADPMessageTypes.Exception;
                response.Params.Clear();
                response.Params[ADPUtils.ExceptionMessage] = "Unknown command received!";
                response.Params[ADPUtils.ExceptionStack] = "";
                commandServer.Respond(client, response);
            } catch (Exception e) {
                throw e;
            }
        }
        #endregion

        /// <summary>
        /// Event fired when a command is received
        /// </summary>
        public void commandServer_CommandReceived(Object sender, TcpClient client, ADPMessage command) {
            switch (command.Id) {
                //Login
                case (int)ADPMessageTypes.Login:
                    InterpretCommand_Login(client, command);
                    break;
                //StartTransaction
                case (int)ADPMessageTypes.StartTransaction:
                    InterpretCommand_StartTransaction(client, command);
                    break;
                //Commit
                case (int)ADPMessageTypes.Commit:
                    InterpretCommand_Commit(client, command);
                    break;
                //Rollback
                case (int)ADPMessageTypes.Rollback:
                    InterpretCommand_Rollback(client, command);
                    break;
                //GetConnection
                case (int)ADPMessageTypes.GetConnection:
                    InterpretCommand_GetConnection(client, command);
                    break;
                //ReleaseConnection
                case (int)ADPMessageTypes.ReleaseConnection:
                    InterpretCommand_ReleaseConnection(client, command);
                    break;
                //ExecuteSelectStatement
                case (int)ADPMessageTypes.ExecuteSelectStatement:
                    InterpretCommand_ExecuteSelectStatement(client, command);
                    break;
                //ExecuteSelectStatementInTransaction
                case (int)ADPMessageTypes.ExecuteSelectStatementInTransaction:
                    InterpretCommand_ExecuteSelectStatementInTransaction(client, command);
                    break;
                //ExecuteSQLCommand
                case (int)ADPMessageTypes.ExecuteCommandStatement:
                    InterpretCommand_ExecuteCommandStatement(client, command);
                    break;
                //GetKey
                case (int)ADPMessageTypes.GetKey:
                    InterpretCommand_GetKey(client, command);
                    break;
                //GetSQLStatement
                case (int)ADPMessageTypes.GetSQLStatement:
                    InterpretCommand_GetSQLStatement(client, command);
                    break;
                //Unknown Command
                default:
                    InterpretCommand_UnknownCommand(client, command);
                    break;
            }
        }

        /// <summary>
        /// Return the registry key that store the ADPServer information
        /// </summary>
        /// <returns>
        /// Registry key that store the ADPServer information
        /// </returns>
        private static string GetServerRegistryKey() {
            return ADPUtils.ADPRegistryKey;
        }
        /// <summary>
        /// Access the windows registry to get the address where the ADPServer application was installed
        /// </summary>
        /// <returns>
        /// Address where the ADPServer application was installed
        /// </returns>
        public static string GetServerAddress() {
            return Convert.ToString(ADPUtils.GetLocalMachineValue(GetServerRegistryKey(), "ADPServerFolder", ""));
        }
        /// <summary>
        /// Access the windows registry to know if the Debug Mode is enabled
        /// </summary>
        /// <returns>
        /// True if the Debug Mode is enabled
        /// </returns>
        public static bool GetDebugModeEnabled() {
            return Convert.ToBoolean(ADPUtils.GetLocalMachineValue(GetServerRegistryKey(), "ADPServerDebugMode"));
        }
        /// <summary>
        /// Access the windows registry to get the port that must be used by the ADPServer
        /// </summary>
        /// <returns>
        /// The port that must be used by the ADPServer
        /// </returns>
        private static int GetServerPort() {
            return Convert.ToInt32(ADPUtils.GetLocalMachineValue(GetServerRegistryKey(), "ADPServerPort", 9850));
        }

        /// <summary>
        /// Get the name of the ADPServer process
        /// </summary>
        /// <returns>
        /// The name of the ADPServer process
        /// </returns>
        public static string GetProcessName() {
            return "ADPServer";
        }
        /// <summary>
        /// Get the name of the ADPServer application
        /// </summary>
        /// <returns>
        /// The name of the ADPServer application
        /// </returns>
        public static string GetServerName() {
            return GetProcessName() + ".exe";
        }
        /// <summary>
        /// Get the name of the log file to report the trace data
        /// </summary>
        /// <returns>
        /// Name of the log file to report the trace data
        /// </returns>
        public static string GetLogFileName() {
            string baseFileName = GetProcessName();
            baseFileName += Convert.ToString(DateTime.Now.DayOfWeek);
            return baseFileName + ".log";
        }
    }
}
