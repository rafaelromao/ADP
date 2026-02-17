using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Cati.ADP.Common;

namespace Cati.ADP.Server {
    /// <summary>
    /// Delegate used to handle the received messages
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
    public delegate void MessageReceivedEventHandler(Object sender, TcpClient messageClient, string messageText);
    /// <summary>
    /// Listen to a IP port for a client message.
    /// When a message arrives, launch the OnMessageReceived event
    /// </summary>
    public sealed class ADPListener : TcpListener {
        /// <summary>
        /// Creates a new ADPListener
        /// </summary>
        /// <param name="localEP">
        /// IPEndPoint of the port that must be listened
        /// </param>
        public ADPListener(IPEndPoint localEP)
            : base(localEP) {
            Port = localEP.Port;
        }
        /// <summary>
        /// Thread used to get the received messages from the port
        /// </summary>
        Thread listenerThread;
        /// <summary>
        /// Return a IPEndPoint to a given port number
        /// </summary>
        /// <param name="port">
        /// Number of the port
        /// </param>
        /// <returns>
        /// IPEndPoint to the given port number
        /// </returns>
        public static IPEndPoint GetLocalEndPoint(int port) {
            IPAddress ipAddress = ADPUtils.GetIPAddress(Dns.GetHostName());
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            return localEndPoint;
        }
        /// <summary>
        /// Size of the network buffer
        /// </summary>
        public static int BufferSize = ADPUtils.BufferSize;
        /// <summary>
        /// Port to be listened
        /// </summary>
        public int Port;
        /// <summary>
        /// Indicate if the listener must throw exceptions occurred during the read process
        /// </summary>
        public bool ThrowExceptions = false;
        /// <summary>
        /// Amount of time to wait after each atempt to read the port
        /// </summary>
        public int Interval = ADPUtils.ThreadSleepInterval;
        /// <summary>
        /// Event fired when a message is received
        /// </summary>
        public event MessageReceivedEventHandler OnMessageReceived;
        /// <summary>
        ///  Start the listener thread and open the port
        /// </summary>
        public new void Start() {
            base.Stop();
            listenerThread = new Thread(new ThreadStart(ListenerThreadStart));
            listenerThread.IsBackground = true;
            listenerThread.Priority = ThreadPriority.AboveNormal;
            listenerThread.Start();
            base.Start();
        }
        /// <summary>
        /// Stop the listener thread and closes the port
        /// </summary>
        public new void Stop() {
            if (listenerThread != null) {
                listenerThread.Abort();
            }
            base.Stop();
        }
        /// <summary>
        /// Callback method used to listen the port in a separate thread
        /// </summary>
        private void ListenerThreadStart() {
            try {
                string buffer = "";
                TcpClient client = null;
                while (true) {
                    Thread.Sleep(Interval);
                    if (base.Active) {
                        bool pending = Pending();
                        if (pending) {
                            //Blocks the thread until a client has been connected
                            client = AcceptTcpClient();
                            //Set TcpClient properties
                            client.ReceiveBufferSize = BufferSize;
                            client.SendBufferSize = BufferSize;
                            client.LingerState = new LingerOption(true, 0);
                            client.NoDelay = true;
                            NetworkStream ns = client.GetStream();
                            if (ns != null) {
                                //Get the next message packet, if any
                                buffer += ADPSerializer.ReadStream(ns, BufferSize);
                                //Check if the packet has finished
                                if (buffer.Contains(ADPUtils.PacketEnd)) {
                                    int k1 = buffer.IndexOf(ADPUtils.PacketStart) + ADPUtils.PacketStart.Length;
                                    int k2 = buffer.IndexOf(ADPUtils.PacketEnd) - k1;
                                    string msg = buffer.Substring(k1, k2);
                                    k1 = buffer.IndexOf(ADPUtils.PacketStart);
                                    k2 = buffer.IndexOf(ADPUtils.PacketEnd);
                                    k2 = k2 + ADPUtils.PacketEnd.Length;
                                    if (buffer.Length > k2) {
                                        buffer = buffer.Substring(k2 + 1);
                                    } else {
                                        buffer = "";
                                    }
                                    ADPTracer.Print(this, "Packet received on port {0}", Port);
                                    OnMessageReceived(this, client, msg);
                                    client = null;
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                if (!(e is ThreadAbortException)) {
                    ADPTracer.Print(this, "Exception listening port {0}", Port);
                    ADPTracer.Print(this, "Exception: {0}", e.Message);
                    //If not throw exceptions, the message will be ignored
                    if (ThrowExceptions) {
                        throw e;
                    }
                }
            }   
        }
    }
}
