using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Cati.ADP.Common {
    /// <summary>
    /// Static class that can be used to manage trace data
    /// </summary>
    public static class ADPTracer { 
        /// <summary>
        /// Stream where the trace data will be stored
        /// </summary>
        private static MemoryStream stream;
        /// <summary>
        /// Listener that will be used
        /// </summary>
        private static TextWriterTraceListener listener;
        
        /// <summary>
        /// Name of the log file where the trace data must be saved
        /// </summary>
        private static string logFileName = null;
        /// <summary>
        /// Informs the tracer that the trace data must be persisted to a file
        /// </summary>
        /// <param name="fileName">
        /// File to be used to store the trace data
        /// </param>
        public static void LogToFile(string fileName) {
            logFileName = fileName;
            if (File.Exists(fileName) && (File.GetCreationTime(fileName).Date != DateTime.Now.Date)) {
                File.Delete(fileName);
            }
            saveLogFileThread = new Thread(new ThreadStart(saveLogFileThreadStart));
            saveLogFileThread.Priority = ThreadPriority.BelowNormal;
            saveLogFileThread.IsBackground = true;
            saveLogFileThread.Start();
        }

        /// <summary>
        /// Thread that keeps saving the trace data to a log file at every second
        /// </summary>
        private static Thread saveLogFileThread;
        /// <summary>
        /// Callback method that is executed to save the trace data to a log file at every second
        /// </summary>
        private static void saveLogFileThreadStart() {
            while (true) {
                Thread.Sleep(1000);
                lock (logLock) {
                    string text = GetThreadUnsafeOutPut();
                    if ((text != null) && (text != "")) {
                        StreamWriter writer = new StreamWriter(logFileName, true);
                        writer.WriteLine(text);
                        writer.Close();
                        writer = null;
                        GC.Collect();
                    }
                }
            }
        }

        private static object logLock = new object();
        /// <summary>
        /// Add a trace text to the trace stream
        /// </summary>
        /// <param name="owner">
        /// Object responsible by the trace entry
        /// </param>
        /// <param name="text">
        /// Text to be logged
        /// </param>
        /// <param name="parameters">
        /// Parameter that must be used to format the given text
        /// </param>
        public static void Print(object owner, string text, params object[] parameters) {
            string ownerName;
            if (owner != null) {
                ownerName = owner.GetType().Name;
            } else {
                ownerName = "Unknown!";
            }
            while (ownerName.Length < 40) {
                ownerName += " ";
            }
            ownerName = ownerName.Substring(ownerName.Length - 40, 40);
            string s = "{0}:{1} - Thread Id: {2} - Owner: {3} -> {4}";
            string threadId = Convert.ToString(Thread.CurrentThread.ManagedThreadId);
            while (threadId.Length < 4) {
                threadId = "0" + threadId;
            }
            string millisecond = Convert.ToString(DateTime.Now.Millisecond);
            while (millisecond.Length < 3) {
                millisecond = "0" + millisecond;
            }
            s = String.Format(s, DateTime.Now, millisecond, 
                              threadId,
                              ownerName, text);
            RegisterListener();
            lock (logLock) {
                Trace.WriteLine(String.Format(s, parameters));
            }
        }
        /// <summary>
        /// Returns a string containing the trace data and also remove such data from the trace stream
        /// </summary>
        public static string OutPut {
            get {
                lock (logLock) {
                    return GetThreadUnsafeOutPut();
                }
            }
        }
        /// <summary>
        /// Returns a string containing the trace data and also remove such data from the trace stream
        /// </summary>
        private static string GetThreadUnsafeOutPut() {
            if (stream == null) {
                return "";
            }
            StreamReader reader = new StreamReader(stream);
            listener.Flush();
            long oldPosition = stream.Position;
            stream.Position = 0;
            char[] buffer = new char[stream.Length];
            reader.Read(buffer, (int)stream.Position, (int)(stream.Length - stream.Position));
            stream.Position = oldPosition;
            UnRegisterListener();
            RegisterListener();
            return new String(buffer);
        }

        /// <summary>
        /// Create and register a new listener to catch the trace data
        /// </summary>
        private static void RegisterListener() {
            if (!Trace.Listeners.Contains(listener)) {
                stream = new MemoryStream();
                listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);
            }
        }
        /// <summary>
        /// Unregister the current trace listener
        /// </summary>
        private static void UnRegisterListener() {
            Trace.Listeners.Remove(listener);   
            stream = null;
            listener = null;
        }
    }
}
