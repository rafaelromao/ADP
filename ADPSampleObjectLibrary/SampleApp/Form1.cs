using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ADPSampleObjectLibrary;
using Cati.ADP.Objects;
using Cati.ADP.Common;

namespace SampleApp {
    public partial class Form1 : Form {
        ADPSampleObjectCollection collection;
        ADPSession session;
        ADPConnectionInfo info1, info2;
        public Form1() {
            InitializeComponent();
            info1 = new ADPConnectionInfo();
            info1.DatabaseServer = "hibrid1b";
            info1.DatabaseName = "C:\\Sistemas\\BBMS\\Data\\BBMS.GDB";
            info1.ADPServerPort = (int)ADPUtils.GetLocalMachineValue("Software\\CATI\\ADP", "ADPServerPort", 9850);
            info1.ADPServerHost = "ds04-d7-i75-c11";
            
            info2 = new ADPConnectionInfo();
            //info2.DatabaseServer = "hibrid1b";
            info2.DatabaseServer = "localhost";
            info2.DatabaseName = "C:\\Sistemas\\BBMS\\Data\\BBMS.GDB";
            info2.ADPServerPort = (int)ADPUtils.GetLocalMachineValue("Software\\CATI\\ADP", "ADPServerPort", 9850);
            info2.ADPServerHost = "DS06-D7-VS05-SA";
            //info2.ADPConnectionFactoryAssemblyName = "ADPConnectionDrivers.dll";
            //info2.ADPConnectionFactoryTypeName = "ADPDefaultConnectionFactory";

            session = new ADPSession(new ADPConnectionInfo[] { /*info1,*/ info2 });
        }

        int loadType = 0;
        //ADPWorkList workList = new ADPWorkList();
        private void button1_Click(object sender, EventArgs e) {
            DateTime StartTime = DateTime.Now;
            label1.Text = Convert.ToString(StartTime) + ":" + Convert.ToString(StartTime.Millisecond);

            ADPLoadOptions options = new ADPLoadOptions(false, true, false, true);
            ADPCollection<ADPSampleObject> list = null;
            ADPFilterCriteria filterCriteria;
            ADPSampleObject so;
            object o;
            switch (loadType) { 
                case 0://"LoadAll":
                    options.LoadOnlyFirstObject = false;
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    list = (ADPCollection<ADPSampleObject>)session.Load<ADPSampleObject>(options);
                    break;
                case 1://"LoadByKey":
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    list = new ADPCollection<ADPSampleObject>(session);
                    so = (ADPSampleObject)session.Load<ADPSampleObject>(options, 3);
                    list.Add(so);
                    break;
                case 2://"LoadById":
                    break;
                case 3://"LoadRangeByPropertyName":
                    options.LoadOnlyFirstObject = false;
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    o = session.Load<ADPSampleObject>(options, "DonorNumber", "DONOR_6");
                    list = (ADPCollection<ADPSampleObject>)o;
                    break;
                case 4://"LoadSingleByPropertyName":
                    options.LoadOnlyFirstObject = true;
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    list = new ADPCollection<ADPSampleObject>(session);
                    so = (ADPSampleObject)session.Load<ADPSampleObject>(options, "DonorNumber", "DONOR_6");
                    list.Add(so);
                    break;
                case 5://"LoadRangeByCriteria":
                    options.LoadOnlyFirstObject = false;
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    filterCriteria = new ADPFilterCriteria(typeof(ADPSampleObject), "(Name containing 'Ferr') and (DonorNumber != '')");
                    o = session.Load<ADPSampleObject>(options, filterCriteria);
                    list = (ADPCollection<ADPSampleObject>)o;
                    break;
                case 6://"LoadSingleByCriteria":
                    options.LoadOnlyFirstObject = true;
                    options.LoadPersistedObjects = true;
                    options.AutomaticReload = true;
                    filterCriteria = new ADPFilterCriteria(typeof(ADPSampleObject), "(Name containing 'Ferr') and (DonorNumber != '')");
                    list = new ADPCollection<ADPSampleObject>(session);
                    so = (ADPSampleObject)session.Load<ADPSampleObject>(options, filterCriteria);
                    list.Add(so);
                    break;
                case 7://"LoadCached":
                    options.LoadOnlyFirstObject = false;
                    options.LoadPersistedObjects = false;
                    options.AutomaticReload = false;
                    o = session.Load<ADPSampleObject>(options);
                    list = (ADPCollection<ADPSampleObject>)o;
                    break;            
            }

            collection = new ADPSampleObjectCollection(session, list);
            bindingSource1.DataSource = collection;
            collection.DeleteOnRemove = true;

            //workList.Clear();
            //workList.Add(collection);

            DateTime EndTime = DateTime.Now;
            label2.Text = Convert.ToString(EndTime) + ":" + Convert.ToString(EndTime.Millisecond);
            TimeSpan ts = EndTime - StartTime;
            label5.Text = Convert.ToString(ts.TotalMilliseconds);
        }

        private void button2_Click(object sender, EventArgs e) {
            DateTime StartTime = DateTime.Now;
            label3.Text = Convert.ToString(StartTime) + ":" + Convert.ToString(StartTime.Millisecond);
            
            Guid c = session.Proxy.GetConnection(session.DatabaseSessionID);
            try {
                string statementText = session.Proxy.GetSQLStatement("GET_ALL_PERSONS", session.DatabaseSessionID);
                DataTable dt = session.Proxy.ExecuteSelectStatement(c, statementText);
                dataGridView2.DataSource = dt;
            } finally {
                session.Proxy.ReleaseConnection(c);
            }

            DateTime EndTime = DateTime.Now;
            label4.Text = Convert.ToString(EndTime) + ":" + Convert.ToString(EndTime.Millisecond);
            TimeSpan ts = EndTime - StartTime;
            label6.Text = Convert.ToString(ts.TotalMilliseconds);
        }

        private void button4_Click(object sender, EventArgs e) {
            bindingSource1.DataSource = null;
            if (textBox1.Text == "") {
                collection.Filter = "";
            } else {
                collection.Filter = String.Format("(Name containing '{0}')", textBox1.Text);
            }
            bindingSource1.DataSource = collection;
        }

        private void button5_Click(object sender, EventArgs e) {
            string expression = String.Format("(Name = '{0}') and (Id = '')", textBox1.Text);
            ADPFilterCriteria criteria = new ADPFilterCriteria(typeof(ADPSampleObject), expression);
            ADPCollection<ADPSampleObject> objlist = collection.Find(criteria);
            if (objlist.Count > 0) {
                MessageBox.Show(((ADPSampleObject)objlist[0]).Name);
            } else {
                MessageBox.Show(String.Format("No person named '{0}' was found!", textBox1.Text));
            }
        }

        private void button6_Click(object sender, EventArgs e) {
            collection.ApplySort("Name, Id DESC");
        }

        private void button7_Click(object sender, EventArgs e) {
            //workList.BeginWork();
            session.BeginPersist();
            collection.Persist(); 
        }

        private void button8_Click(object sender, EventArgs e) {
            //workList.EndWork();
            session.EndPersist();
        }

        private void button9_Click(object sender, EventArgs e) {
            //workList.CancelWork();
            session.CancelPersist();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void radioButton1_Click(object sender, EventArgs e) {
            loadType = Convert.ToInt32(((RadioButton)sender).Tag);
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e) {
            
        }

        private void bindingSource1_ListChanged(object sender, ListChangedEventArgs e) {

        }

        private void dataGridView1_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            
        }
    }
}