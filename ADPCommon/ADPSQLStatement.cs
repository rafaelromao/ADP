using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Common {
    /// <summary>
    /// Specifies the type of statement that must be built and/or executed
    /// </summary>
    public enum ADPSQLStatementType { 
        SelectByKey,
        SelectBySingleField,
        SelectAll,
        AutoBuiltCustomSelect,
        CustomSelect,
        CustomSelectRange,
        Insert,
        Update,
        Delete,
        GetModifiedDateTime
    }

    /// <summary>
    /// Specifies if an SQL statement must be built automaticaly
    /// </summary>
    public enum ADPStatementGeneration {
        Manual,
        Automatic
    }

    /// <summary>
    /// Encapsulate a SQL statement
    /// </summary>
    public class ADPSQLStatement {
        /// <summary>
        /// Creates a statement and sets its Id
        /// </summary>
        /// <param name="statementId">
        /// Id of the new statement
        /// </param>
        public ADPSQLStatement(string statementId) {
            Id = statementId;
            Parameters = new List<ADPParam>();
        }
        /// <summary>
        /// Creates a statement and sets its Id and StatementText
        /// </summary>
        /// <param name="statementId">
        /// Id of the new statement
        /// </param>
        /// <param name="statementText">
        /// StatementText of the new statement
        /// </param>
        public ADPSQLStatement(string statementId, string statementText) {
            Id = statementId;
            StatementText = statementText;
            Parameters = new List<ADPParam>();
        }
        /// <summary>
        /// Creates a statement and sets its Id and Parameters
        /// </summary>
        /// <param name="statementId">
        /// Id of the new statement
        /// </param>
        /// <param name="statementParameters">
        /// Parameters of the new statement
        /// </param>
        public ADPSQLStatement(string statementId, List<ADPParam> statementParameters) {
            Id = statementId;
            Parameters = statementParameters;
        }
        /// <summary>
        /// Creates a statement and sets its Id, StatementText and Parameters
        /// </summary>
        /// <param name="statementId">
        /// Id of the new statement
        /// </param>
        /// <param name="statementText">
        /// StatementText of the new statement
        /// </param>
        /// <param name="statementParameters">
        /// Parameters of the new statement
        /// </param>
        public ADPSQLStatement(string statementId, string statementText, List<ADPParam> statementParameters) {
            Id = statementId;
            StatementText = statementText;
            Parameters = statementParameters;
        }
        /// <summary>
        /// Id of the statement.
        /// It is most usefull when working with identified and custom statements
        /// </summary>
        public string Id = "";
        /// <summary>
        /// Type of the statement
        /// </summary>
        public ADPSQLStatementType Type;
        /// <summary>
        /// Text of the statement
        /// </summary>
        public string StatementText;
        /// <summary>
        /// Parameters of the statement
        /// </summary>
        public List<ADPParam> Parameters;
        /// <summary>
        /// Return a parameter according to a given param name.
        /// </summary>
        /// <param name="paramName">
        /// Param name
        /// </param>
        /// <returns>
        /// Parameter with the given name.
        /// null if no parameter with the given name be found.
        /// </returns>
        public ADPParam ParamByName(string paramName) {
            foreach (ADPParam p in Parameters) {
                if (p.Name == paramName) {
                    return p;
                }
            }
            return null;
        }
        /// <summary>
        /// Indicate that the parameters of the statement were already loaded. 
        /// The methods LoadSQLParameters() and LoadSQLTokens()
        /// load the parameters according to its StatementText
        /// </summary>
        public bool Prepared = false;
        /// <summary>
        /// Load all the SQLParameters found in the StatementText
        /// </summary>
        private void LoadSQLParameters() {
            /*
             * INSERT INTO PPP (A, B, C) VALUES (:A, :B, :C)
             * UPDATE PPP SET A = :A, B = :B WHERE C = :C       19-21   27-29
             * DELETE FROM PPP WHERE A = :A
             */
            string text = StatementText;
            while (text.Contains(":")) {
                int k1 = text.IndexOf(":");
                int k2 = text.IndexOf(",", k1);
                if (k2 < 0) {
                    k2 = text.IndexOf(" ", k1);
                }
                if (k2 < 0) {
                    k2 = text.IndexOf(")", k1);
                }
                if (k2 < 0) {
                    k2 = text.Length;
                }
                string paramName = text.Substring(k1 + 1, k2 - k1 - 1);
                ADPParam parameter = ParamByName(paramName);
                //Add each param only once
                if (parameter == null) {
                    parameter = new ADPParam(paramName, ADPParamType.SQLParameter, null);
                    Parameters.Add(parameter);
                }
                text = text.Remove(k1, k2 - k1);
            }
        }
        /// <summary>
        /// Load all the SQLTokens found in the StatementText
        /// </summary>
        private void LoadSQLTokens() {
            string text = StatementText;
            while (text.Contains("@")) {
                int k1 = text.IndexOf("@");
                int k2 = text.IndexOf(",", k1);
                if (k2 < 0) {
                    k2 = text.IndexOf(" ", k1);
                }
                if (k2 < 0) {
                    k2 = text.IndexOf(")", k1);
                }
                if (k2 < 0) {
                    k2 = text.Length;
                }
                string paramName = text.Substring(k1 + 1, k2 - k1 - 1);
                //Add each param only once
                ADPParam parameter = ParamByName(paramName);
                if (parameter == null) {
                    parameter = new ADPParam(paramName, ADPParamType.SQLToken, null);
                    Parameters.Add(parameter);
                }
                text = text.Remove(k1, k2 - k1);
            }
        }
        /// <summary>
        /// Prepares a statement
        /// </summary>
        public void Prepare() {
            Parameters.Clear();
            LoadSQLParameters();
            LoadSQLTokens();
            Prepared = true;
        }
    }
}
