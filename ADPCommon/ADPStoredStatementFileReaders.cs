using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace Cati.ADP.Common {
    /// <summary>
    /// Specified if a ADPStoredStatement is a query statement or a command statement
    /// </summary>
    public enum ADPStoredStatementType { 
        Query,
        Command
    }

    /// <summary>
    /// Represents a statement stored in a disk file
    /// </summary>
    public class ADPStoredStatement {
        public string DatabaseName = "";
        public string DatabaseLanguage = "";
        public ADPStoredStatementType StatementType;
        public string StatementId = "";
        public string StatementText = "";
    }

    /// <summary>
    /// Specifies the type of the file that contains the database stored statements
    /// </summary>
    public enum ADPStoredStatementFileType {
        Delimited,
        Xml
    }

    /// <summary>
    /// Interface that provides a method to read a StoredStatements file
    /// </summary>
    internal interface IADPStoredStatementFileReader {
        List<ADPStoredStatement> ReadStoredStatementFile(string StoredStatementsFile);
    }

    /// <summary>
    /// Provides a method to return a StoredStatements reader according to the given StoredStatements type
    /// </summary>
    internal static class ADPStoredStatementFileReaderFactory {
        /// <summary>
        /// Return a StoredStatements reader according to the given StoredStatements type
        /// </summary>
        /// <param name="StoredStatementsType">
        /// StoredStatements type
        /// </param>
        /// <returns></returns>
        public static IADPStoredStatementFileReader GetStoredStatementFileReader(ADPStoredStatementFileType StoredStatementsType) {
            switch (StoredStatementsType) {
                case ADPStoredStatementFileType.Delimited:
                    return new ADPStoredStatementDelimitedFileReader();
                case ADPStoredStatementFileType.Xml:
                    return new ADPStoredStatementXmlFileReader();
            }
            return null;
        }
    }

    /// <summary>
    /// Read the stored statements from a dataset saved in a XML file
    /// </summary>
    internal sealed class ADPStoredStatementXmlFileReader : IADPStoredStatementFileReader {
        /// <summary>
        /// Read the stored statements from a dataset saved in a XML file
        /// </summary>
        /// <param name="storedStatementsFile">
        /// XML file to be read
        /// </param>
        /// <returns>
        /// A list containing the read ADPStoredStatements
        /// </returns>
        public List<ADPStoredStatement> ReadStoredStatementFile(string storedStatementsFile) {
            List<ADPStoredStatement> result = new List<ADPStoredStatement>();
            if (File.Exists(storedStatementsFile)) {
                DataTable dt = new DataTable();
                try {
                    dt.ReadXml(storedStatementsFile);
                    foreach (DataRow row in dt.Rows) {
                        ADPStoredStatement ss = new ADPStoredStatement();
                        ss.DatabaseName = Convert.ToString(row[0]);
                        ss.DatabaseLanguage = Convert.ToString(row[1]);
                        ss.StatementType = (ADPStoredStatementType)(Convert.ToInt32(row[2]));
                        ss.StatementId = Convert.ToString(row[3]);
                        ss.StatementText = Convert.ToString(row[4]);
                        result.Add(ss);
                    }
                } catch {
                    throw new ADPInvalidStoredStatementFileException();
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Read the StoredStatements from a delimited file
    /// </summary>
    internal sealed class ADPStoredStatementDelimitedFileReader : IADPStoredStatementFileReader {
        /// <summary>
        /// Extract the content of the source string from the beginning to the first ocurrency
        /// of the tab character
        /// </summary>
        /// <param name="source">
        /// Reference of a string containing the source text
        /// </param>
        /// <returns>
        /// Extracted field
        /// </returns>
        private string ExtractNextField(ref string source) {
            int delimiterPos = source.IndexOf("\t");
            string result = source.Substring(0, delimiterPos - 1);
            source = source.Remove(0, delimiterPos);
            return result;
        }
        /// <summary>
        /// Read the stored statements from a delimited file
        /// </summary>
        /// <param name="storedStatementsFile">
        /// File to be read
        /// </param>
        /// <returns>
        /// A list containing the read ADPStoredStatements
        /// </returns>
        public List<ADPStoredStatement> ReadStoredStatementFile(string storedStatementsFile) {
            List<ADPStoredStatement> result = new List<ADPStoredStatement>();
            if (File.Exists(storedStatementsFile)) {
                try {
                    FileStream stream = new FileStream(storedStatementsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader reader = new StreamReader(stream);
                    reader.BaseStream.Position = 0;
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        ADPStoredStatement ss = new ADPStoredStatement();
                        ss.DatabaseName = ExtractNextField(ref line);
                        ss.DatabaseLanguage = ExtractNextField(ref line);
                        ss.StatementType = (ADPStoredStatementType)Convert.ToInt32(ExtractNextField(ref line));
                        ss.StatementId = ExtractNextField(ref line);
                        ss.StatementText = ExtractNextField(ref line);
                        result.Add(ss);
                    }
                } catch {
                    throw new ADPInvalidStoredStatementFileException();
                }
            }
            return result;
        }
    }
}
