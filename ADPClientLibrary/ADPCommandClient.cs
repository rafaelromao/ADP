using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Client {
    /// <summary>
    /// Send commands to a remote ADPCommandServer object through a Tcp channel;
    /// Used by the ADPClient to send and receive the available commands in the
    /// ADPServer object.
    /// </summary>
    public sealed class ADPCommandClient {
        TcpClient client;
        /// <summary>
        /// Size of each package sent
        /// </summary>
        public static int BufferSize = ADPUtils.BufferSize;
        /// <summary>
        /// Name of the computer where it is supposed to find
        /// a ADPCommandServer listening for client connections
        /// </summary>
        public string HostName = "";
        /// <summary>
        /// Port of the remote computer where it is supposed to have
        /// a ADPCommandServer listener connected
        /// </summary>
        public int Port;
        /// <summary>
        /// Amount of time to wait for an answer from the ADPCommandServer
        /// </summary>
        public int TimeOut = ADPUtils.DefaultTimeOut;

        /// <summary>
        /// Object used to provide thread safe access to the pingResult
        /// </summary>
        private static object pingResultLock = new object();
        /// <summary>
        /// Result of an asyncronous call to a ping operation
        /// </summary>
        private bool pingResult = false;
        /// <summary>
        /// TcpClient to be used to perform the ping operation
        /// </summary>
        private TcpClient pingClient;
        /// <summary>
        /// Callback method of the thread that performs the asyncronous ping operation
        /// </summary>
        /// <param name="o">
        /// IpEndPoint to ping
        /// </param>
        private void pingThreadStart(object o) {
            IPEndPoint ipEndPoint = (IPEndPoint)o;
            try {
                pingClient.Connect(ipEndPoint);
                lock (pingResultLock) {
                    pingResult = true;
                    Thread.CurrentThread.Abort();
                }
            } catch (Exception e) {
                if (!(e is ThreadAbortException)) {
                    ADPTracer.Print(this, "Error on trying to ping:");
                    ADPTracer.Print(this, e.Message);
                }
            }
        }
        /// <summary>
        /// Performs a ping operation in the same TcpClient object used in the communication
        /// This method returns keeping the TcpClient connected. 
        /// This variation is used in the Connect() method, due to its last feature mentioned.
        /// </summary>
        /// <param name="hostName">
        /// Name of the computer to ping
        /// </param>
        /// <param name="port">
        /// Port to ping
        /// </param>
        /// <param name="timeOut">
        /// Amount of time to wait for an answer
        /// </param>
        /// <returns>
        /// True if the ping was successful
        /// </returns>
        public bool Ping(string hostName, int port, int timeOut) {
            return Ping(false, hostName, port, timeOut);
        }
        /// <summary>
        /// Performs a ping operation in a given TcpClient object or in the 
        /// default TcpClient object, according to the value of the useDefaultClient
        /// parameter. This method disconnects the TcpClient before to return if the
        /// such param value is "false"
        /// </summary>
        /// <param name="hostName">
        /// Name of the computer to ping
        /// </param>
        /// <param name="port">
        /// Port to ping
        /// </param>
        /// <param name="timeOut">
        /// Amount of time to wait for an answer
        /// </param>
        /// <returns>
        /// True if the ping was successful
        /// </returns>
        public bool Ping(bool useDefaultClient, string hostName, int port, int timeOut) {
            if (!useDefaultClient) {
                pingClient = new TcpClient();
            } else {
                pingClient = this.client;
            }
            pingResult = false;
            Thread pingThread = new Thread(new ParameterizedThreadStart(pingThreadStart));
            pingThread.IsBackground = true;
            IPAddress ipAddress = ADPUtils.GetIPAddress(hostName);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            ADPTimeOut t = new ADPTimeOut();
            int seconds = Convert.ToInt32(timeOut/1000); //Timeout is passed as milliseconds but t.Start() expect seconds
            t.Start(seconds);
            pingThread.Start(ipEndPoint);
            while (!t.TimeOutExceeded()) {
                lock (pingResultLock) {
                    if (pingResult) {
                        break;
                    }
                }
                Thread.Sleep(1);
            }
            lock (pingResultLock) {
                if (!useDefaultClient) {
                    pingClient.Close();
                }
                return pingResult;
            }
        }

        /// <summary>
        /// Estabilish the connection with the remote ADPCommandServer
        /// </summary>
        public void Connect() {
            client = new TcpClient();
            client.LingerState = new LingerOption(true, 0);
            client.ReceiveBufferSize = BufferSize;
            client.SendBufferSize = BufferSize;
            client.NoDelay = true;
            try {
                Ping(true, HostName, Port, 1000);
            } catch {
                throw new ADPServerNotFoundException();
            }
        }
        /// <summary>
        /// Closes the connection to the remote ADPCommandServer
        /// </summary>
        public void Disconnect() {
            client.Close();
        }

        /// <summary>
        /// Perform a request to the remote ADPCommandServer
        /// </summary>
        /// <param name="message">
        /// ADPMessage containing the command to be requested
        /// </param>
        /// <param name="exception">
        /// null or a ADPMessage containing an exception returned from the ADPCommandServer
        /// </param>
        /// <returns>
        /// ADPMessage containing the response to the command request
        /// </returns>
        private ADPMessage InternalRequest(ADPMessage message, out ADPMessage exception) {
            #region Get an available port and connect
            //Get an available port and connect
            Connect();
            #endregion
            #region Build command text
            //Build command text
            string packet;
            string commandText = message.Serialized;
            string checkSum = ADPSerializer.CheckSum(commandText);
            commandText = ADPUtils.MessageStart + commandText + ADPUtils.CheckSumStart + checkSum + ADPUtils.MessageEnd;
            #endregion
            #region Split command in packets
            //Split command in packets
            List<string> packets = new List<string>();
            while (commandText.Length > 0) {
                int maxBytes = ADPUtils.BufferSize;
                maxBytes = maxBytes - ADPUtils.PacketStart.Length;
                maxBytes = maxBytes - ADPUtils.PacketEnd.Length;
                maxBytes = Math.Min(maxBytes, commandText.Length);
                packet = commandText.Substring(0, maxBytes);
                commandText = commandText.Remove(0, maxBytes);
                packet = ADPUtils.PacketStart + packet + ADPUtils.PacketEnd;
                packet = packet + ADPSerializer.CheckSum(packet);
                packets.Add(packet);
            }
            #endregion
            #region Send packets
            //Send packets
            foreach (string s in packets) {
                byte[] bytes = new byte[s.Length];
                for (int i = 0; i < s.Length; i++) {
                    char c = s[i];
                    byte b = Convert.ToByte(c);
                    bytes[i] = b;
                }
                try {
                    client.GetStream().Write(bytes, 0, bytes.Length);
                } catch (Exception e) {
                    ADPTracer.Print(this, "Error on trying to write message: {0}!", e.Message);
                    throw e;
                }
            }
            ADPTracer.Print(this, "Command \t {0} - {1} sent to server on port {2}", (ADPMessageTypes)(message.Id), message.GUID, Port);
            #endregion
            #region Wait response
            //Wait response
            bool responseComplete = false;
            string buffer = "";
            string responseText = "";
            ADPTimeOut t = new ADPTimeOut();
            t.Start(TimeOut);
            while (!responseComplete) {
                NetworkStream ns = client.GetStream();
                if ((ns != null) && (client.Available > 0)) {
                    //Get the next message packet, if any
                    buffer += ADPSerializer.ReadStream(ns, BufferSize);
                    //Check if the packet has finished
                    if (buffer.Contains(ADPUtils.PacketEnd)) {
                        int k1 = buffer.IndexOf(ADPUtils.PacketStart) + ADPUtils.PacketStart.Length;
                        int k2 = buffer.IndexOf(ADPUtils.PacketEnd) - k1;
                        string msg = buffer.Substring(k1, k2);
                        k2 = buffer.IndexOf(ADPUtils.PacketEnd);
                        k2 = k2 + ADPUtils.PacketEnd.Length;
                        if (buffer.Length > k2) {
                            buffer = buffer.Substring(k2 + 1);
                        } else {
                            buffer = "";
                        }
                        responseText += msg;
                        ADPTracer.Print(this, "Response packet received on port {0}", Port);
                    }
                    if (responseText.Contains(ADPUtils.MessageEnd)) {
                        int responseStart;
                        int responseEnd;
                        responseStart = responseText.IndexOf(ADPUtils.MessageStart);
                        responseStart += ADPUtils.MessageStart.Length;
                        responseEnd = responseText.IndexOf(ADPUtils.MessageEnd);
                        responseText = responseText.Substring(responseStart, responseEnd - responseStart);
                        responseComplete = true;
                    }
                }
                Thread.Sleep(ADPUtils.ThreadSleepInterval);
                if (t.TimeOutExceeded()) {
                    throw new ADPTimeoutException("GetResponse", t.Interval);
                }
            }
            #endregion
            #region Validate message
            //Validate message
            int checkSumStart = responseText.IndexOf(ADPUtils.CheckSumStart);
            checkSumStart = checkSumStart + ADPUtils.CheckSumStart.Length;
            int checkSumLength = responseText.Length - checkSumStart;
            string receivedCheckSum = responseText.Substring(checkSumStart, checkSumLength);
            checkSumStart = checkSumStart - ADPUtils.CheckSumStart.Length;
            responseText = responseText.Substring(0, checkSumStart);
            string calculatedCheckSum = ADPSerializer.CheckSum(responseText);
            //If checksum is invalid, raise exception
            if (receivedCheckSum != calculatedCheckSum) {
                throw new ADPChecksumException(Convert.ToInt32(receivedCheckSum), Convert.ToInt32(calculatedCheckSum));
            }
            ADPMessage m = new ADPMessage();
            m.Serialized = responseText;
            ADPTracer.Print(this, "Response to the command \t {0} - {1} received on port {2}", (ADPMessageTypes)(m.Id), m.GUID, Port);
            #endregion
            #region Handle server exception
            //Handle server exception
            if (m.Id == (int)ADPMessageTypes.Exception) {
                Disconnect();
                string exceptionName = m.Params[ADPUtils.ExceptionName];
                string exceptionMessage = m.Params[ADPUtils.ExceptionMessage];
                string exceptionSource = m.Params[ADPUtils.ExceptionSource];
                string exceptionStack = m.Params[ADPUtils.ExceptionStack];
                exception = new ADPMessage();
                exception.Id = (int)ADPMessageTypes.Exception;
                exception.Params[ADPUtils.ExceptionName] = exceptionName;
                exception.Params[ADPUtils.ExceptionSource] = exceptionMessage;
                exception.Params[ADPUtils.ExceptionMessage] = exceptionSource;
                exception.Params[ADPUtils.ExceptionStack] = exceptionStack;
            } else {
                exception = null;
            }
            #endregion
            #region Finalize and Return
            //Finalize and Return
            Disconnect();
            return m;
            #endregion
        }
        /// <summary>
        /// Try to perform a request to the ADPCommandServer 3 times and return an exception if fail
        /// </summary>
        private ADPMessage Request(ADPMessage message, out ADPMessage exception) {
            ADPMessage response = null;
            exception = null;
            try {
                //Try each command 3 times before to raise server exception
                for (int i = 0; i < 3; i++) {
                    response = InternalRequest(message, out exception);
                    if (exception == null) {
                        break;
                    } else {
                        ADPTracer.Print(this, "{0} on trying to execute Request. Trying again!", exception.Params[ADPUtils.ExceptionName]);
                    }
                }
                return response;
            } finally {
                GC.Collect();
            }
        }

        /// <summary>
        /// Try to perform a request to the server and throws an exception if fail
        /// </summary>
        /// <param name="message">
        /// ADPMessage containing the command to be requested
        /// </param>
        /// <returns>
        /// ADPMessage containing the response to the command
        /// </returns>
        public ADPMessage Request(ADPMessage message) {
            ADPMessage exception;
            ADPMessage response = Request(message, out exception);
            if (exception != null) {
                string exceptionName = exception.Params[ADPUtils.ExceptionName];
                string exceptionMessage = exception.Params[ADPUtils.ExceptionMessage];
                string exceptionSource = exception.Params[ADPUtils.ExceptionSource];
                string exceptionStack = exception.Params[ADPUtils.ExceptionStack];
                ADPTracer.Print(this, "Exception occurred on trying to Request: {0}", (ADPMessageTypes)message.Id);
                ADPTracer.Print(this, "Exception Name: {0}", exceptionName);
                ADPTracer.Print(this, "Exception Message: {0}", exceptionMessage);
                ADPTracer.Print(this, "Exception Source: {0}", exceptionSource);
                ADPTracer.Print(this, "Exception Stack: {0}", exceptionStack);
                throw new ADPServerException(exceptionName, exceptionMessage, exceptionSource, exceptionStack);
            }
            return response;
        }
    }
}
