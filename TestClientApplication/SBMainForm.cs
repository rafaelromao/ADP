using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Cati.ADP.Client;
using Cati.ADP.Common;
using Cati.ADP.Server;
using System.IO;

namespace TestClientApplication {
    public partial class SBMainForm : Form {
        public SBMainForm() {
            InitializeComponent();
        }
        ADPProxy dbProxy;
        ADPConnectionInfo connectionInfo;
        Queue<Guid> transactionList;

        private void Form1_Load(object sender, EventArgs e) {
            if (File.Exists("TestClientApplication.log")) {
                File.Delete("TestClientApplication.log");
            }
            ADPTracer.LogToFile("TestClientApplication.log");
            connectionInfo = new ADPConnectionInfo();
            transactionList = new Queue<Guid>();
            connectionInfo.DatabaseDriver = "IBProvider";
            connectionInfo.DatabaseName = "C:\\Sistemas\\BBMS\\Data\\BBMS.GDB";
            connectionInfo.ADPServerTimeOut = 180;
            connectionInfo.DatabaseTimeOut = 180;
            connectionInfo.DatabasePoolSize = 1;
        }

        private void button1_Click(object sender, EventArgs e) {
            connectionInfo.ADPServerHost = textBox1.Text;
            dbProxy = new ADPProxy(ADPProviderType.Remote);
            try {
                dbProxy.Login(connectionInfo);
            } catch (Exception ex) {
                throw ex;
            }
            Application.DoEvents();
        }

        private void button2_Click(object sender, EventArgs e) {
            if (!ADPProxy.CheckLogin(dbProxy, true, false)) {
                return;
            }
            try {
                Guid transactionID = dbProxy.StartTransaction(connectionInfo.DatabaseSessionID);
                transactionList.Enqueue(transactionID);
            } catch (Exception e1) {
                Console.WriteLine(e1.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            if (!ADPProxy.CheckLogin(dbProxy, true, false)) {
                return;
            }
            if (transactionList.Count > 0) {
                Guid transactionID = transactionList.Dequeue();
                dbProxy.Commit(transactionID);
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            if (!ADPProxy.CheckLogin(dbProxy, true, false)) {
                return;
            }
            if (transactionList.Count > 0) {
                Guid transactionID = transactionList.Dequeue();
                dbProxy.Rollback(transactionID);
            }
        }

        private void button5_Click(object sender, EventArgs e) {
            if (!ADPProxy.CheckLogin(dbProxy, true, false)) {
                return;
            }
            Guid g = dbProxy.GetConnection(connectionInfo.DatabaseSessionID);
            try {
                ADPParam param = new ADPParam("FINAL_PERS_CODE", ADPParamType.SQLParameter, Convert.ToInt32(textBox2.Text));
                string statementText = dbProxy.GetSQLStatement("dataTableId", connectionInfo.DatabaseSessionID);
                DataTable dt = dbProxy.ExecuteSelectStatement(g, statementText, param);
                dataGridView1.DataSource = dt; 
            } catch (Exception ex) {
                throw ex;
            } finally {
                dbProxy.ReleaseConnection(g);
            }
        }

        private void button6_Click(object sender, EventArgs e) {
            if (!ADPProxy.CheckLogin(dbProxy, true, false)) {
                return;
            }
            Guid g = dbProxy.StartTransaction(connectionInfo.DatabaseSessionID);
            try {
                string statementText = dbProxy.GetSQLStatement("sqlCommandID", connectionInfo.DatabaseSessionID);
                dbProxy.ExecuteCommandStatement(g, statementText);
                dbProxy.Commit(g);
            } catch {
                dbProxy.Rollback(g);
            }
        }
        private void StressThreadCallBack(object o) {
            ADPTracer.Print(this, "\t\t\t ------> Thread {0}, Id: {1} started!", (int)o, Thread.CurrentThread.ManagedThreadId);
            try {
                for (int i = 0; i < 1; i++) {
                    try {
                        Application.DoEvents();
                        ADPProxy p = new ADPProxy(ADPProviderType.Remote);
                        ADPConnectionInfo c = new ADPConnectionInfo();
                        c.DatabaseDriver = "IBProvider";
                        c.DatabaseName = "c:\\sistemas\\bbms\\data\\bbms.gdb";
                        c.DatabaseServer = "localhost";
                        c.ADPServerHost = textBox1.Text;
                        c.ADPServerTimeOut = 180;
                        c.DatabaseTimeOut = 180;
                        c.DatabasePoolSize = 4;
                        p.Login(c);
                        Guid g = p.GetConnection(c.DatabaseSessionID);
                        try {
                            ADPParam param = new ADPParam("FINAL_PERS_CODE", ADPParamType.SQLParameter, Convert.ToInt32(textBox2.Text));
                            string statementText = "SELECT * FROM PERS_PERSON WHERE PERS_CODE < :FINAL_PERS_CODE";
                            DataTable dt = p.ExecuteSelectStatement(g, statementText, param);
                            //dataGridView1.DataSource = dt;
                        } finally {
                            p.ReleaseConnection(g);
                        } 
                    } catch (Exception e) {
                        ADPTracer.Print(this, "EXCEPTION:" + e.Message);
                    }
                }
            } finally {
                GC.Collect();
                ADPTracer.Print(this, "\t\t\t ------> Thread {0}, Id: {1} finished!", (int)o, Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void button7_Click(object sender, EventArgs e) {
            ThreadPool.SetMinThreads(20, 20);
            ThreadPool.SetMaxThreads(256, 256);
            for (int i = 0; i < 10; i++) {
                ThreadPool.QueueUserWorkItem((WaitCallback)StressThreadCallBack, i);
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (AutoRefreshCheckBox.Checked) {
                RefreshButton_Click(null, null);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e) {
            string fileName = "TestClientApplication.log";
            if (File.Exists(fileName)) {
                FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(stream);
                reader.BaseStream.Position = 0;
                LogTextBox.Text = reader.ReadToEnd();
                LogTextBox.SelectionStart = LogTextBox.Text.Length;
                LogTextBox.ScrollToCaret();
            }
        }

        private void button8_Click(object sender, EventArgs e) {
            for (int i = 0; i < 50; i++) {
                StressThreadCallBack(0);
            }
        }

        private void button9_Click(object sender, EventArgs e) {
            Guid g = dbProxy.GetConnection(connectionInfo.DatabaseSessionID);
            try {
                ADPTracer.Print(this, "Just before to perform the request!");
                Application.DoEvents();
                ADPParam param = new ADPParam("FINAL_PERS_CODE", ADPParamType.SQLParameter, Convert.ToInt32(textBox2.Text));
                Guid requestID = dbProxy.RequestDataTable(connectionInfo.DatabaseSessionID, g, "dataTableID", param);
                ADPTracer.Print(this, "Request performed in a separate thread!");
                ADPTracer.Print(this, "Just before to check the answer!");
                Application.DoEvents();
                DataTable dt = dbProxy.GetRequestedDataTable(requestID, 5);
                dataGridView1.DataSource = dt;
                ADPTracer.Print(this, "DataTable loaded!");
            } finally {
                dbProxy.ReleaseConnection(g);
            }
        }

        private void button10_Click(object sender, EventArgs e) {
            connectionInfo.ADPServerHost = textBox1.Text;
            dbProxy = new ADPProxy(ADPProviderType.Local);
            dbProxy.Login(connectionInfo);
            Application.DoEvents();
        }

        private void button11_Click(object sender, EventArgs e) {
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e) {
            if ((DateTime.Now.Second == 0) || (DateTime.Now.Second == 30)) {
                StressThreadCallBack(0);
                timer2.Stop();
            }
        }

        private void button12_Click(object sender, EventArgs e) {
            Form1 f = new Form1();
            f.Show();
        }
    }
}