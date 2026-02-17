using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Encapsulate a property value
    /// </summary>
    public class ADPPropertyValue {
        /// <summary>
        /// Property descriptor of the property
        /// </summary>
        public PropertyDescriptor Property;
        /// <summary>
        /// Value of the property
        /// </summary>
        public object Value;
    }
}
