using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;

namespace Cati.ADP.Common {
    /// <summary>
    /// A thread safe helper to serialization operations
    /// </summary>
    public static class ADPSerializer {
        private static object serializeXmlLock = new Object();
        /// <summary>
        /// Serialize and object to a XML format string
        /// </summary>
        /// <param name="obj">
        /// Object to be serialized
        /// </param>
        /// <returns>
        /// String containing the serialized object
        /// </returns>
        public static String SerializeXml(Object obj) {
            lock (serializeXmlLock) {
                try {
                    XmlSerializer serializer = new XmlSerializer(obj.GetType());
                    MemoryStream stream = new MemoryStream();
                    serializer.Serialize(stream, obj);
                    stream.Position = 0;
                    string s = ReadStream(stream);
                    return s;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }

        private static object serialize1Lock = new Object();
        /// <summary>
        /// Serialize an object to a binary format string
        /// </summary>
        /// <param name="obj">
        /// Object to be serialized
        /// </param>
        /// <returns>
        /// Serialized object
        /// </returns>
        public static String Serialize(Object obj) {
            lock (serialize1Lock) {
                try {
                    BinaryFormatter serializer = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    serializer.Serialize(stream, obj);
                    stream.Position = 0;
                    string s = ReadStream(stream);
                    return s;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        
        private static object serialize2Lock = new Object();
        /// <summary>
        /// Serialize an object to a binary format string 
        /// with the help of a surrogate
        /// </summary>
        /// <param name="obj">
        /// Object to be serialized
        /// </param>
        /// <param name="surrogate">
        /// Surrogate used to help in the serialization process
        /// </param>
        /// <returns>
        /// Serialized object
        /// </returns>
        public static String Serialize(Object obj, ISerializationSurrogate surrogate) {
            lock (serialize2Lock) {
                try {
                    BinaryFormatter serializer = new BinaryFormatter();
                    SurrogateSelector selector = new SurrogateSelector();
                    serializer.SurrogateSelector = selector;
                    selector.AddSurrogate(obj.GetType(), new StreamingContext(StreamingContextStates.All), surrogate);
                    MemoryStream stream = new MemoryStream();
                    serializer.Serialize(stream, obj);
                    stream.Position = 0;
                    string s = ReadStream(stream);
                    return s;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        
        private static object deserializeXmlLock = new Object();
        /// <summary>
        /// Create an object of the given type according to a given XML format string
        /// </summary>
        /// <param name="type">
        /// Type of the serialized object
        /// </param>
        /// <param name="value">
        /// String containing the serialized object
        /// </param>
        /// <returns>
        /// Object deserialized
        /// </returns>
        public static Object DeserializeXml(Type type, String value) {
            lock (deserializeXmlLock) {
                try {
                    if (value == null) {
                        return null;
                    }
                    XmlSerializer serializer = new XmlSerializer(type);
                    MemoryStream stream = new MemoryStream();
                    WriteStream(stream, value);
                    stream.Position = 0;
                    Object result = serializer.Deserialize(stream);
                    return result;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        
        private static object deserialize1Lock = new Object();
        /// <summary>
        /// Create an object of the given type according to a given binary format string
        /// </summary>
        /// <param name="type">
        /// Type of the serialized object
        /// </param>
        /// <param name="value">
        /// String containing the serialized object
        /// </param>
        /// <returns>
        /// Object deserialized
        /// </returns>
        public static Object Deserialize(Type type, String value) {
            lock (deserialize1Lock) {
                try {
                    if (value == null) {
                        return null;
                    }
                    BinaryFormatter serializer = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    WriteStream(stream, value);
                    stream.Position = 0;
                    Object result = serializer.Deserialize(stream);
                    return result;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        
        private static object deserialize2Lock = new Object();
        /// <summary>
        /// Create an object of the given type according to a given binary format string
        /// with the help of a surrogate
        /// </summary>
        /// <param name="type">
        /// Type of the serialized object
        /// </param>
        /// <param name="value">
        /// String containing the serialized object
        /// </param>
        /// <param name="surrogate">
        /// Surrogate used to help in the deserialization process
        /// </param>
        /// <returns>
        /// Object deserialized
        /// </returns>
        public static Object Deserialize(Type type, String value, ISerializationSurrogate surrogate) {
            lock (deserialize2Lock) {
                try {
                    if (value == null) {
                        return null;
                    }
                    BinaryFormatter serializer = new BinaryFormatter();
                    SurrogateSelector selector = new SurrogateSelector();
                    serializer.SurrogateSelector = selector;
                    selector.AddSurrogate(type, new StreamingContext(StreamingContextStates.All), surrogate);
                    MemoryStream stream = new MemoryStream();
                    WriteStream(stream, value);
                    stream.Position = 0;
                    Object result = serializer.Deserialize(stream);
                    return result;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        
        private static object readStreamLock = new Object();
        /// <summary>
        /// Return a string containing the content of a stream
        /// </summary>
        /// <param name="stream">
        /// Stream containing the data to be read
        /// </param>
        /// <param name="bufferSize">
        /// Amount of bytes to be read
        /// </param>
        /// <returns>
        /// Result string
        /// </returns>
        public static String ReadStream(Stream stream, long bufferSize) {
            lock (readStreamLock) {
                try {
                    string result = "";
                    /* 
                     * A implementação comentada usa o objeto nativo de conversão de arrays,
                     * porém este método não é viável, pois consome uma quantidade gigantesca de memória
                     * 
                     * byte[] bytes = new byte[bufferSize];
                     * stream.Read(bytes, 0, bytes.Length);
                     * result = Encoding.ASCII.GetString(bytes);
                     * return result;
                    */
                    byte[] bytes = new byte[bufferSize];
                    byte c = 0x01;
                    for (int i = 0; i < bytes.Length; i++) {
                        bytes[i] = c++;
                    }
                    int readBytes = stream.Read(bytes, 0, bytes.Length);
                    char[] chars = new char[readBytes];
                    for (int count = 0; count < readBytes; count++) {
                        char ch = (char)bytes[count];
                        chars[count] = ch;
                    }
                    result += new string(chars);
                    return result;
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }
        /// <summary>
        /// Return a string containing the content of a stream
        /// </summary>
        /// <param name="stream">
        /// Stream containing the data to be read
        /// </param>
        /// <returns>
        /// Result string
        /// </returns>
        public static String ReadStream(Stream stream) {
            return ReadStream(stream, stream.Length);
        }
        
        private static object writeStreamLock = new Object();
        /// <summary>
        /// Writes a string into a given stream
        /// </summary>
        /// <param name="stream">
        /// Stream to write in
        /// </param>
        /// <param name="text">
        /// String to be writed
        /// </param>
        public static void WriteStream(Stream stream, string text) {
            lock (writeStreamLock) {
                try {
                    byte[] bytes = new byte[text.Length];
                    for (int i = 0; i < bytes.Length; i++) {
                        bytes[i] = Convert.ToByte(text[i]);
                    }
                    stream.Write(bytes, 0, bytes.Length);
                } catch (Exception e) {
                    throw new ADPSerializationException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// Creates a checksum for given string
        /// </summary>
        /// <param name="message">
        /// String to be calculated
        /// </param>
        /// <returns>
        /// Calculated checksum
        /// </returns>
        public static string CheckSum(string message) {
            byte result = 0;
            int len = message.Length;
            for (int i = 0; i < len; i++) {
                byte b = Convert.ToByte(message[i]);
                result = result ^= b;
            }
            if (result == 0) {
                return "1";
            } else {
                return Convert.ToString(result);
            }
        }
    }
}
