using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Cati.ADP.Common {
    /// <summary>
    /// Provides methods to get statements stored in disk files
    /// </summary>
    public class ADPStoredStatementProvider {
        /// <summary>
        /// Store the statements loaded from a file
        /// </summary>
        private List<ADPStoredStatement> storedStatementList;
        /// <summary>
        /// Indicate what database language must be used
        /// </summary>
        private string dbLanguage;
        /// <summary>
        /// Get a previously loaded statement
        /// </summary>
        /// <param name="storedStatementType">
        /// Statement type: Query or Command.
        /// </param>
        /// <param name="statementID">
        /// A string that identifies the statement
        /// </param>
        /// <returns>
        /// The desired statement text or an empty string, if no match statement be found
        /// </returns>
        private string GetStatement(ADPStoredStatementType storedStatementType, string statementID) {
            foreach (ADPStoredStatement ss in storedStatementList) {
                bool c1 = (ss.DatabaseLanguage == dbLanguage);
                bool c2 = (ss.StatementType == storedStatementType);
                bool c3 = (ss.StatementId == statementID);
                if (c1 && c2 && c3) {
                    return ss.StatementText;
                }
            }
            return "";
        }
        /// <summary>
        /// Load the stored statements from a file
        /// </summary>
        /// <param name="type">
        /// Type of the file to be read
        /// </param>
        /// <param name="storedStatementsFile">
        /// Name of the file to be read
        /// </param>
        /// <param name="language">
        /// Database language that must be filtered
        /// </param>
        public void LoadStoredStatements(ADPStoredStatementFileType type, string storedStatementsFile, string language) {
            if (storedStatementList == null) {
                IADPStoredStatementFileReader storedStatementFileReader = ADPStoredStatementFileReaderFactory.GetStoredStatementFileReader(type);
                storedStatementList = storedStatementFileReader.ReadStoredStatementFile(storedStatementsFile);
                dbLanguage = language; 
            }
        }
        /// <summary>
        /// Get a previously loaded query statement 
        /// </summary>
        /// <param name="statementID">
        /// A string that identifies the statement
        /// </param>
        /// <returns>
        /// The desired statement text or an empty string, if no match statement be found
        /// </returns>
        public string GetDataTableStatement(string dataTableID) {
            return GetStatement(ADPStoredStatementType.Query, dataTableID);
        }
        /// <summary>
        /// Get a previously loaded command statement 
        /// </summary>
        /// <param name="statementID">
        /// A string that identifies the statement
        /// </param>
        /// <returns>
        /// The desired statement text or an empty string, if no match statement be found
        /// </returns>
        public string GetCommandStatement(string sqlCommandID) {
            return GetStatement(ADPStoredStatementType.Command, sqlCommandID);
        }
    }
}
