using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Cati.ADP.Common {
    /// <summary>
    /// This surrogate helps in the binary serialization of the ADPMessage objects
    /// </summary>
    internal sealed class ADPMessageSurrogate : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info,
                                                StreamingContext context) {
            ADPMessage message = obj as ADPMessage;
            info.AddValue("Id", (int)message.Id);
            info.AddValue("GUID", Convert.ToString(message.GUID));
            info.AddValue("ParamCount", message.Params.Count);
            int i = 0;
            foreach (string s in message.Params.Keys) {
                info.AddValue(String.Format("ParamName_{0}", i), s);
                info.AddValue(String.Format("ParamValue_{0}", i), message.Params[s]);
                i++;
            }
        }

        public object SetObjectData(object obj, SerializationInfo info,
                   StreamingContext context, ISurrogateSelector selector) {
            ADPMessage message = obj as ADPMessage;
            int count = info.GetInt32("ParamCount");
            message.Id = info.GetInt32("Id");
            message.Params = new Dictionary<string, string>();
            message.GUID = new Guid(info.GetString("GUID"));
            for (int i = 0; i < count; i++) { 
                string key = info.GetString(String.Format("ParamName_{0}", i));
                string value = info.GetString(String.Format("ParamValue_{0}", i));
                message.Params[key] = value;
            }
            return null;
        }
    }

    /// <summary>
    /// Encapsulate a message sent from the ADPCommandClient to the ADPCommandServer
    /// or a response sent back from the ADPCommandServer to the ADPCommandClient
    /// </summary>
    public sealed class ADPMessage {
        public ADPMessage() {
            Params = new Dictionary<String, String>();
            GUID = Guid.NewGuid();
        }
        public int Id;
        public Guid GUID;
        public Dictionary<String, String> Params;
        
        #region Serialization Properties
        /// <summary>
        /// Assign this ADPMessage to another one
        /// </summary>
        /// <param name="destination">
        /// The ADPMessage that will receive the assignement
        /// </param>
        public void AssignTo(ADPMessage destination) {
            destination.Id = this.Id;
            destination.GUID = this.GUID;
            destination.Params.Clear();
            foreach (String p in this.Params.Keys) {
                destination.Params[p] = this.Params[p];
            }
        }
        /// <summary>
        /// Flag that indicates that the object is being serialized
        /// </summary>
        private bool serializing = false;
        /// <summary>
        /// Gets a string containing the current object serialized
        /// or assign an object created according to the given value
        /// to the current object
        /// </summary>
        public String Serialized {
            get {
                if (!serializing) {
                    serializing = true;
                    try {
                        return ADPSerializer.Serialize(this, new ADPMessageSurrogate());
                    } finally {
                        serializing = false;
                    }
                } else {
                    return null;
                }
            }
            set {
                ADPMessage clone = (ADPMessage)ADPSerializer.Deserialize(this.GetType(), value, new ADPMessageSurrogate());
                if (clone != null) {
                    clone.AssignTo(this);
                }
            }
        }
        #endregion
    }
}
