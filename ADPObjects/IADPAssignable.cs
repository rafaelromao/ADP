using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Indicate that an object may be assigned from other one of the same type
    /// </summary>
    public interface IADPAssignable {
        /// <summary>
        /// Assign the source object value to the current object
        /// </summary>
        /// <param name="source">
        /// Source object
        /// </param>
        void Assign(ADPObject source);
    }
}
