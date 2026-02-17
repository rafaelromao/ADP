using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cati.ADP.Objects;

namespace ADPStatementBuilder {
    public class ADPStoredStatementID : ADPObject {
        #region Constructors
        public ADPStoredStatementID(ADPSession session, bool registerInSession)
            : base(session, registerInSession) {
            KeyType = typeof(Guid);
            if (session != null) {
                statementTexts = new ADPCollection<ADPStoredStatementText>(session);
            }
        }
        public ADPStoredStatementID(ADPSession session)
            : base(session) {
            KeyType = typeof(Guid);
            if (session != null) {
                statementTexts = new ADPCollection<ADPStoredStatementText>(session);
            }
        }
        #endregion

        #region Property Fields
        private string databaseName = "";
        private string statementType = "Query";
        private string statementId = "<New Statement>";
        private ADPCollection<ADPStoredStatementText> statementTexts;
        #endregion

        #region Properties
        public string DatabaseName {
            get { return databaseName; }
            set { 
                databaseName = value;
                Notify("DatabaseName");
            }
        }
        public string StatementType {
            get { return statementType; }
            set {
                statementType = value;
                Notify("StatementType");
            }
        }
        public string StatementId {
            get { return statementId; }
            set {
                statementId = value;
                Notify("StatementId");
            }
        }
        public ADPCollection<ADPStoredStatementText> StatementTexts {
            get { return statementTexts; }
            set { statementTexts = value; }
        }
        #endregion

        #region Methods
        public override void Delete() {
            StatementTexts.DeleteAll();
            StatementTexts.Clear();
            base.Delete();
        }
        public override void Persist() {
            if (StatementTexts != null) {
                foreach (ADPStoredStatementText text in StatementTexts) {
                    text.Persist();
                }
            }
            base.Persist();
        }
        #endregion
    }

    public class ADPStoredStatementText : ADPObject {
        #region Constructors
        public ADPStoredStatementText()
            : base(null, false) {
            KeyType = typeof(Guid);
        }
        public ADPStoredStatementText(ADPSession session, bool registerInSession)
            : base(session, registerInSession) {
            KeyType = typeof(Guid);
        }
        public ADPStoredStatementText(ADPSession session)
            : base(session) {
            KeyType = typeof(Guid); ;
        }
        #endregion

        #region Property Fields
        private string statementId = "";
        private string databaseLanguage = "<New Language>";
        private string statementText = "";
        #endregion

        #region Properties
        public string StatementId {
            get { return statementId; }
            set { 
                statementId = value;
                Key = statementId + "|" + DatabaseLanguage;
                Notify("StatementId");
            }
        }
        public string DatabaseLanguage {
            get { return databaseLanguage; }
            set {
                databaseLanguage = value;
                Key = statementId + "|" + DatabaseLanguage;
                Notify("DatabaseLanguage");
            }
        }
        public string StatementText {
            get { return statementText; }
            set {
                statementText = value;
                Notify("StatementText");
            }
        }
        #endregion
    }

    public class ADPStoredStatementIDCollection : ADPCollection<ADPStoredStatementID> {
        public ADPStoredStatementIDCollection(ADPSession session, ADPCollection<ADPStoredStatementID> list)
            : base(session, list) {
        }
        public ADPStoredStatementIDCollection(ADPSession session)
            : base(session) {
        }
        public ADPStoredStatementIDCollection(ADPSession session, List<ADPStoredStatementID> list)
            : base(session, list) {
        }
        public ADPStoredStatementIDCollection(ADPSession session, IList list)
            : base(session, list) {
        }
    
    }
}
