using System;
using System.Collections.Generic;
using System.Text;

namespace Cati.ADP.Common {
    public class ADPException : Exception {
        public ADPException() {
        }
        public ADPException(string message)
            : base(message) {
        }
        public ADPException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "An unknown exception has ocurred inside the ADP Framework!"; }
        }
    }
    public class ADPServerNotFoundException : ADPException {
        public ADPServerNotFoundException() {
        }
        public ADPServerNotFoundException(string message)
            : base(message) {
        }
        public ADPServerNotFoundException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Could not find the ADPServer running on the specified host!"; }
        }
    }
    public class ADPNoResponseException : ADPException {
        public ADPNoResponseException() {
        }
        public ADPNoResponseException(int timeOut) {
            TimeOut = timeOut;
        }
        public ADPNoResponseException(string message)
            : base(message) {
        }
        public ADPNoResponseException(string message, Exception inner)
            : base(message, inner) {
        }
        public int TimeOut = 0;
        public override String Message {
            get {
                string message = "No response from server after the specified timeout interval!";
                if (TimeOut > 0) {
                    message = String.Format("No response from server after {0} seconds!", TimeOut);
                }
                return message;
            }
        }
    }
    public class ADPTimeoutException : ADPException {
        public ADPTimeoutException() {
        }
        public ADPTimeoutException(string operation, int timeOut) {
            Operation = operation;
            TimeOut = timeOut;
        }
        public ADPTimeoutException(string message)
            : base(message) {
        }
        public ADPTimeoutException(string message, Exception inner)
            : base(message, inner) {
        }
        public int TimeOut = 0;
        public string Operation = "";
        public override String Message {
            get {
                string message = "An operation has timed out inside the ADP Framework!";
                if (TimeOut > 0) {
                    message = String.Format("Operation {0} has timed out after {1} seconds!", Operation, TimeOut);
                }
                return message;
            }
        }
    }
    public class ADPParameterMissingException : ADPException {
        public ADPParameterMissingException() {
        }
        public ADPParameterMissingException(string parameterOwner, string parameterName) {
            ParameterOwner = parameterOwner;
            ParameterName = parameterName;
        }
        public ADPParameterMissingException(string message)
            : base(message) {
        }
        public ADPParameterMissingException(string message, Exception inner)
            : base(message, inner) {
        }
        public string ParameterName = "";
        public string ParameterOwner = "";
        public override String Message {
            get {
                string message = "A mandatory parameter is missing. Cannot proceed with this operation!";
                if (ParameterName != "") {
                    message = String.Format("The parameter {0}.{1} is missing. Cannot proceed with this operation!", ParameterOwner, ParameterName);
                }
                return message;
            }
        }
    }
    public class ADPNoActiveTransactionException : ADPException {
        public ADPNoActiveTransactionException() {
        }
        public ADPNoActiveTransactionException(string message)
            : base(message) {
        }
        public ADPNoActiveTransactionException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "No active transaction. Cannot proceed!"; }
        }
    }
    public class ADPActiveTransactionException : ADPException {
        public ADPActiveTransactionException() {
        }
        public ADPActiveTransactionException(string message)
            : base(message) {
        }
        public ADPActiveTransactionException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "There is already an active transaction!"; }
        }
    }
    public class ADPServerException : ADPException {
        public ADPServerException() {
        }
        public ADPServerException(string message)
            : base(message) {
        }
        public ADPServerException(string message, Exception inner)
            : base(message, inner) {
        }
        public string ExceptionName;
        public string ExceptionMessage;
        public string ExceptionSource;
        public string ExceptionStack;
        public ADPServerException(string name, string message, string source, string stack) {
            ExceptionName = name;
            ExceptionMessage = message;
            ExceptionSource = source;
            ExceptionStack = stack;
        }        
        public override String Message {
            get {
                return "Exception occurred on the ADPServer:\r\n" +
                       String.Format("Name: {0}\r\n", ExceptionName) +
                       String.Format("Message: {0}\r\n", ExceptionMessage) +
                       String.Format("Source: {0}\r\n", ExceptionSource) +
                       String.Format("Stack: {0}", ExceptionStack);
            }
        }
    }
    public class ADPSerializationException : ADPException {
        public ADPSerializationException() {
        }
        public ADPSerializationException(string message)
            : base(message) {
        }
        public ADPSerializationException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Error on trying to perform serialization!"; }
        }
    }
    public class ADPInvalidStoredStatementFileException : ADPException {
        public ADPInvalidStoredStatementFileException() {
        }
        public ADPInvalidStoredStatementFileException(string message)
            : base(message) {
        }
        public ADPInvalidStoredStatementFileException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Could not load the stored statement file!"; }
        }
    }
    public class ADPInvalidDatabaseException : ADPException {
        public ADPInvalidDatabaseException() {
        }
        public ADPInvalidDatabaseException(string message)
            : base(message) {
        }
        public ADPInvalidDatabaseException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Could not connect to Database!"; }
        }
    }
    public class ADPLogException : ADPException {
        public ADPLogException() {
        }
        public ADPLogException(string message)
            : base(message) {
        }
        public ADPLogException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Error on trying to perform log operation!"; }
        }
    }
    public class ADPChecksumException : ADPException {
        public ADPChecksumException(int wrongValue, int expectedValue) {
            wrongChecksum = (int)wrongValue;
            expectedChecksum = (int)expectedValue;
        }
        private int? wrongChecksum;
        private int? expectedChecksum;
        public override String Message {
            get {
                if ((wrongChecksum != null) && (expectedChecksum != null)) {
                    return String.Format("Invalid checksum! Excepted {0} but found {1}.", expectedChecksum, wrongChecksum);
                } else {
                    return "Invalid checksum!";
                }
            }
        }
    }
    public class ADPCacheUpdateNotTerminatedException : ADPException {
        public ADPCacheUpdateNotTerminatedException() {
        }
        public ADPCacheUpdateNotTerminatedException(string message)
            : base(message) {
        }
        public ADPCacheUpdateNotTerminatedException(string message, Exception inner)
            : base(message, inner) {
        }
        public override String Message {
            get { return "Cannot commit a transaction before to terminate all the started updates!"; }
        }
    }
}
