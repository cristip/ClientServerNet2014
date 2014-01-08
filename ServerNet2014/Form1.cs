using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Security;

namespace ServerNet2014
{

    public partial class Form1 : Form
    {
        private ServerModelLocator model = ServerModelLocator.Instance;
        private BackgroundWorker bw;
        private ServerSocket serverSocket;

        public Form1()
        {
            InitializeComponent();

            model.DefaultPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ServerModelLocator.APP_FOLDER);

            if (!Directory.Exists(model.DefaultPath))
            {
                Directory.CreateDirectory(model.DefaultPath);
                model.ServerFilePath = Path.Combine(model.DefaultPath, Properties.Settings.Default.dbfile);
                string filePath = "DefaultDB.db3";
                File.Copy(filePath, model.ServerFilePath);
            }
            else
            {
                model.ServerFilePath = Properties.Settings.Default.dbfile;
            }
            
            
        }


        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureFormDialog confDialog = new ConfigureFormDialog();
            confDialog.Show();
        }
        
        private void toggleStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Enabled = false;
            bw = new BackgroundWorker();
            bw.DoWork += RunServer;
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += ServerProgressChanged;
            bw.RunWorkerCompleted += ServerWorkerCompleted;
            if (!bw.IsBusy)
            {
                bw.RunWorkerAsync();
            }

            //if(((ToolStripMenuItem)sender).Text == "Start Server")
            //{
            //    ((ToolStripMenuItem)sender).Text = "Stop Server";
            //    bw = new BackgroundWorker();
            //    bw.DoWork += RunServer;
            //    bw.WorkerSupportsCancellation = true;
            //    bw.WorkerReportsProgress = true;
            //    bw.ProgressChanged += ServerProgressChanged;
            //    bw.RunWorkerCompleted += ServerWorkerCompleted;
            //    if(!bw.IsBusy)
            //    {
            //        bw.RunWorkerAsync();
            //    }
                
            //}
            //else
            //{
            //    ((ToolStripMenuItem)sender).Text = "Start Server";
            //    if(bw.IsBusy)
            //    {
            //        serverSocket.stopServer();
            //        bw.CancelAsync();

            //    }
            //    bw.Dispose();
            //}
            

        }

        private void ServerWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SetText("ServerWorkerCompleted " + e.ToString());
        }

        private void ServerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.SetText("ServerProgressChanged " + e.ToString());
        }

        private void RunServer(object sender, DoWorkEventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            serverSocket = new ServerSocket();
            serverSocket.Changed += serverSocket_Changed;
            serverSocket.connect();
        }


        private void serverSocket_Changed(object sender, OutputEvent e)
        {
            this.SetText(e.Info);
        }
        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text += text + "\r\n";
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //int uid = model.createUser("Cristi");
            int uid = model.getUIDByUsername("Cristi");
            this.textBox1.Text += "query executed...";
        }
    }
}
