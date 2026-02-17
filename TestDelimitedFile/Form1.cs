using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cati.ADP.Common;
using Cati.ADP.Objects;

namespace TestDelimitedFile {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            ADPConnectionInfo info = new ADPConnectionInfo();
            info.DatabaseName = "C:\\Temp1\\DataSet.xml";
            info.DatabaseDriver = "XmlDataSet";

            string test = "Delete";
            ADPSession session;

            switch (test) {
                case "Insert":
                    session = new ADPSession(info);
                    session.Load<Record>(new ADPLoadOptions());
                    Record r1 = new Record(session);
                    Record r2 = new Record(session);
                    Record r3 = new Record(session);
                    r1.Key = "Renato";
                    r1.Value = "Bacurau";

                    r2.Key = "Pepelino";
                    r2.Value = "do Araguaia";

                    r3.Key = "Renato";
                    r3.Value = "Tuiuiu";

                    session.BeginPersist();
                    r1.Persist();
                    r2.Persist();
                    r3.Persist();
                    session.EndPersist();
                    break;
                case "Update":
                    //Update records
                    session = new ADPSession(info, ADPProviderType.Remote);
                    //session.Load<Record>(new ADPLoadOptions());
                    Record r4 = (Record)session.Load<Record>("Renato");
                    r4.Value = "Amaral";
                    session.BeginPersist();
                    r4.Persist();
                    session.EndPersist();
                    break;
                case "Delete":
                    //Delete records
                    session = new ADPSession(info, ADPProviderType.Remote);
                    //session.Load<Record>(new ADPLoadOptions());
                    Record r5 = (Record)session.Load<Record>("Renato");
                    r5.Delete();
                    session.BeginPersist();
                    r5.Persist();
                    session.EndPersist();
                    break;
            }
        }
    }
    
    public class Record : ADPObject {
        public Record(ADPSession session, bool registerInSession)
            : base(session, registerInSession) {
            KeyType = typeof(String);
        }
        public Record(ADPSession session)
            : base(session) {
            KeyType = typeof(String);
        }
        private string valueProperty;
        public string Value {
            get { return valueProperty; }
            set {
                valueProperty = value;
                Notify("Value");
            }
        }
    }
}