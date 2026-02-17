using System;
using System.Collections.Generic;
using System.Text;
using Cati.ADP.Common;

namespace Cati.ADP.Objects {
    /// <summary>
    /// Define the structure of an object supposed to help the ADPPersister with some specific Type
    /// </summary>
    /// <remarks>
    /// Must be overriden
    /// </remarks>
    public class ADPBasePersisterHelper {
        /// <summary>
        /// Initialize the given mapping information
        /// </summary>
        /// <param name="info">
        /// Mapping information to be initialized
        /// </param>
        protected internal virtual void Initialize(ADPMappingInformation info) { 
        }
        /// <summary>
        /// Get and prepare a statement according to the given prameters
        /// </summary>
        /// <param name="session">
        /// Session to be used to reach the Statement Provider
        /// </param>
        /// <param name="type">
        /// Type of the persisted object
        /// </param>
        /// <param name="filterCriteria">
        /// Criteria to be matched
        /// </param>
        /// <param name="statementType">
        /// Type of statement to be returned
        /// </param>
        /// <param name="obj">
        /// Object that will provide the statement parameter values
        /// </param>
        /// <returns>
        /// The prepared sql statement
        /// </returns>
        protected internal virtual ADPSQLStatement GetPreparedStatement(ADPSession session, Type type, ADPFilterCriteria filterCriteria, ADPSQLStatementType statementType, ADPObject obj) {
            return null;
        }
    }
}
