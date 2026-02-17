using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cati.ADP.Common;
using System.Diagnostics;

namespace Cati.ADP.Server {
    /// <summary>
    /// Encapulate a received message
    /// </summary>
    public sealed class ADPReceivedMessage {
        /// <summary>
        /// Client that sent the message
        /// </summary>
        public TcpClient Client;
        /// <summary>
        /// Message text
        /// </summary>
        public string MessageText = "";
    }

    /// <summary>
    /// Delegate used to handle the received commands
    /// </summary>
    /// <param name="sender">
    /// ADPCommandServer that received the command
    /// </param>
    /// <param name="client">
    /// Client that sent the command
    /// </param>
    /// <param name="command">
    /// Received command
    /// </param>
    public delegate void CommandReceivedEventHandler(Object sender, TcpClient client, ADPMessage command);
    
    /// <summary>
    /// Receive, process and answer messages sent by the ADPCommandClient
    /// </summary>
    public sealed class ADPCommandServer {
        /// <summary>
        /// Creates a new ADPCommandServer
        /// </summary>
        /// <param name="port">
        /// Port to be listened
        /// </param>
        public ADPCommandServer(int port) {
            ADPTracer.Print(this, "Command Server created!");
            //Create ReceivedData buffer
            receiveBufferList = new Dictionary<string, string>();
            //Create RequestListener
            requestListener = new ADPListener(ADPListener.GetLocalEndPoint(port));
            requestListener.Interval = ADPUtils.ThreadSleepHighInterval;
            requestListener.OnMessageReceived += ReceiveMessageData;
            ThreadPool.SetMaxThreads(256, 256);
        }
        /// <summary>
        /// Listener connected to the port passed in the ADPCommandServer constructor
        /// </summary>
        ADPListener requestListener;
        /// <summary>
        /// Buffer of messages received from each client
        /// </summary>
        Dictionary<string, string> receiveBufferList; //IPEndPoint=MessageText
        /// <summary>
        /// Starts the command server
        /// </summary>
        public void Start() {
            requestListener.Start();
            ADPTracer.Print(this, "Request listener started!");
        }
        /// <summary>
        /// Stops the command server
        /// </summary>
        public void Stop() {
            requestListener.Stop();
            ADPTracer.Print(this, "Request listener stopped!");
        }
        /// <summary>
        /// Event fired when a new command is received
        /// </summary>
        public event CommandReceivedEventHandler OnCommandReceived;
        /// <summary>
        /// Amount of seconds that the server will wait to get an idle listener before to raise a TimeOutException
        /// </summary>
        public int TimeOut = ADPUtils.DefaultTimeOut;
        /// <summary>
        /// Callback method used to process a received message
        /// </summary>
        /// <param name="o">
        /// ADPMessage to be processed
        /// </param>
        private void ProcessMessageCallback(object o) {
            ADPReceivedMessage msg = (ADPReceivedMessage)o;
            string clientEndPoint;
            try {
                clientEndPoint = msg.Client.Client.RemoteEndPoint.ToString();
            } catch {
                clientEndPoint = "Unknown";
            }
            ADPMessage command = new ADPMessage();
            string commandId = "Unknown";
            try {
                ADPTracer.Print(this, "Command processing started! EndPoint: {0}", msg.Client.Client.LocalEndPoint);
                //Get the command block limits
                int commandStart;
                int commandEnd;
                commandStart = msg.MessageText.IndexOf(ADPUtils.MessageStart);
                commandStart += ADPUtils.MessageStart.Length;
                commandEnd = msg.MessageText.IndexOf(ADPUtils.MessageEnd);
                //Get the command to be processed
                string commandText = msg.MessageText.Substring(commandStart, commandEnd - commandStart);
                commandStart = commandStart - ADPUtils.MessageStart.Length;
                commandEnd = commandEnd + ADPUtils.MessageEnd.Length;
                msg.MessageText = msg.MessageText.Remove(commandStart, commandEnd - commandStart);
                //Validate message
                int checkSumStart = commandText.IndexOf(ADPUtils.CheckSumStart);
                checkSumStart = checkSumStart + ADPUtils.CheckSumStart.Length;
                int checkSumLength = commandText.Length - checkSumStart;
                string receivedCheckSum = commandText.Substring(checkSumStart, checkSumLength);
                checkSumStart = checkSumStart - ADPUtils.CheckSumStart.Length;
                commandText = commandText.Substring(0, checkSumStart);
                string calculatedCheckSum = ADPSerializer.CheckSum(commandText);
                //If checksum is invalid, raise exception
                if (receivedCheckSum != calculatedCheckSum) {
                    throw new ADPChecksumException(Convert.ToInt32(receivedCheckSum), Convert.ToInt32(calculatedCheckSum));
                }
                command.Serialized = commandText;
                commandId = Convert.ToString((ADPMessageTypes)command.Id);
                //Launch the event that will process the command
                ADPTracer.Print(this, "Command {0} - {1} received from client: \t{2}", commandId, command.GUID, clientEndPoint);
                OnCommandReceived(this, msg.Client, command);
                ADPTracer.Print(this, "Command {0} - {1} responded to client: \t{2}", commandId, command.GUID, clientEndPoint);
                ADPTracer.Print(this, "------------------------------------------------------------");
            } catch (Exception e) {
                //Send a exception message to the client
                ADPMessage exception = new ADPMessage();
                exception.Id = (int)ADPMessageTypes.Exception;
                exception.Params[ADPUtils.ExceptionName] = e.GetType().Name;
                exception.Params[ADPUtils.ExceptionSource] = e.Source;
                exception.Params[ADPUtils.ExceptionMessage] = e.Message;
                exception.Params[ADPUtils.ExceptionStack] = e.StackTrace;
                ADPTracer.Print(this, "Error on trying to execute Request!");
                ADPTracer.Print(this, "Exception: " + e.Message);
                Respond(msg.Client, exception);
            }
        }
        /// <summary>
        /// Object used to avoid access conflits to the receiveBufferList,
        /// once it may be accessed by several threads at the same time
        /// </summary>
        private Object receiveBufferListLock = new Object();
        /// <summary>
        /// Event fired by the listener to store the received data in the messages buffer
        /// </summary>
        /// <param name="sender">
        /// Listener that received the message
        /// </param>
        /// <param name="messageClient">
        /// Client that sent the message
        /// </param>
        /// <param name="messageText">
        /// Message text
        /// </param>
        public void ReceiveMessageData(object sender, TcpClient messageClient, string messageText) {
            string clientIPEndPoint = "Unknown";
            try {
                //Insert message on the port buffer
                string msg;
                clientIPEndPoint = messageClient.Client.RemoteEndPoint.ToString();
                if (receiveBufferList.ContainsKey(clientIPEndPoint)) {
                    msg = receiveBufferList[clientIPEndPoint];
                } else {
                    msg = "";
                }
                msg += messageText;
                //Avoid two threads to access the buffer at the same time
                lock (receiveBufferListLock) {
                    receiveBufferList[clientIPEndPoint] = msg;
                }
                //Process message if it is complete
                while (msg.Contains(ADPUtils.MessageEnd)) {
                    //Cut message text
                    int commandStart = msg.IndexOf(ADPUtils.MessageStart);
                    int commandEnd = msg.IndexOf(ADPUtils.MessageEnd) + ADPUtils.MessageEnd.Length;
                    string m = msg.Substring(commandStart, commandEnd - commandStart);
                    msg = msg.Remove(commandStart, commandEnd - commandStart);
                    lock (receiveBufferListLock) {
                        receiveBufferList[clientIPEndPoint] = msg;
                    }
                    //Build received message object
                    ADPReceivedMessage message = new ADPReceivedMessage();
                    message.Client = messageClient;
                    message.MessageText = m;
                    //Process the message
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessMessageCallback), message);
                    ADPTracer.Print(this, "Command identified! Thread pool thread recruited!");
                }
                ADPTracer.Print(this, "Message data received on endpoint: {0}", clientIPEndPoint);
            } catch (Exception ex) {
                //If an exception occurs, the command will be ignored and will timeout
                ADPTracer.Print(this, "Error receiving message data on endpoint: {0}", clientIPEndPoint);
                ADPTracer.Print(this, "Exception: " + ex.Message);
            } finally {
                GC.Collect();                
            }
        }
        /// <summary>
        /// Object used to avoid access conflits to the Respond() method,
        /// once it may be accessed by several threads at the same time
        /// </summary>
        private object respondLock = new Object();
        /// <summary>
        /// Sends a response to the given client
        /// </summary>
        /// <param name="client">
        /// Client that will receive the response
        /// </param>
        /// <param name="response">
        /// Response to be sent
        /// </param>
        public void Respond(TcpClient client, ADPMessage response) {
            lock (respondLock) {
                int port = 0;
                try {
                    //Build response message text
                    string responseText = response.Serialized;
                    string checksum = ADPSerializer.CheckSum(responseText);
                    responseText = ADPUtils.MessageStart + responseText + ADPUtils.CheckSumStart + checksum + ADPUtils.MessageEnd;
                    //Split response message in packets
                    List<string> packets = new List<string>();
                    while (responseText.Length > 0) {
                        int maxBytes = (int)ADPListener.BufferSize;
                        maxBytes = maxBytes - ADPUtils.PacketStart.Length;
                        maxBytes = maxBytes - ADPUtils.PacketEnd.Length;
                        maxBytes = Math.Min(maxBytes, responseText.Length);
                        string packet = responseText.Substring(0, maxBytes);
                        responseText = responseText.Remove(0, maxBytes);
                        packet = ADPUtils.PacketStart + packet + ADPUtils.PacketEnd;
                        packets.Add(packet);
                    }
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
                            return;
                        }
                    }
                    //Close the client connection
                    IPEndPoint ep = (IPEndPoint)(client.Client.LocalEndPoint);
                    port = ep.Port;
                    ADPTracer.Print(this, "Client socket closed on port {0}!", port);
                    //TODO Check if it realy works - the server is sundenly stoping to respond. The cause may be its closing
                    client.Close();
                    //Wait until all the data have been sent to the client 
                    while (client.Connected) {
                        Thread.Sleep(10);
                        ADPTracer.Print(this, "Waiting data to be sent on port {0}!", port);
                    }
                } catch (Exception e) {
                    ADPTracer.Print(this, "Exception on trying to respond command on port {0}!", port);
                    ADPTracer.Print(this, "Exception: " + e.Message);
                } finally {
                    //Forces garbage collector in order to reduce the amount of useless resources in the memory
                    GC.Collect();
                }
            }
        }

    }
}
