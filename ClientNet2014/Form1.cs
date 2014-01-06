using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNet2014
{
    public partial class Form1 : Form
    {
        private BackgroundWorker bw;
        private ClientModelLocator model = ClientModelLocator.Instance;
        private ClientSocket clientSocket;

        delegate void SetTextCallback(string text);
        delegate void ShowAddFriendDialogDelegate(string friendName);
        delegate void ShowAddFriendDialogResponseDelegate(string friendName, int status);

        BindingSource bs;

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;

            optionsToolStripMenuItem_Click(null, null);

           
        }

        void Form1_Load(object sender, EventArgs e)
        {
            bs = new BindingSource();
            bs.DataSource = model.Friends;

            //Binding friendsBinding = new Binding("DataSource", model, "Friends", false, DataSourceUpdateMode.OnPropertyChanged);
            this.listBox1.ValueMember = "Id";
            this.listBox1.DataSource = bs;
            //this.listBox1.DataBindings.CollectionChanged += DataBindings_CollectionChanged;
            //this.listBox1.DataBindings.Add(friendsBinding);
        }

        

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form optionsForm = new OptionsDialogForm();
            optionsForm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ClientModelLocator.Instance.IsConnected)
            {
                //disconnect...
            }
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing && model.IsConnected)
            {
                e.Cancel = true;
            }
            

        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Enabled = false;
            bw = new BackgroundWorker();
            bw.DoWork += StartClient;
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += ClientProgressChanged;
            bw.RunWorkerCompleted += ClientWorkerCompleted;
            if (!bw.IsBusy)
            {
                bw.RunWorkerAsync();
            }
        }

        private void ClientWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox1.Text += "ClientWorkerCompleted.. conn is out";
        }

        private void ClientProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            textBox1.Text += "ClientWorkerProgressChanged";
        }

        private void StartClient(object sender, DoWorkEventArgs e)
        {
            clientSocket = new ClientSocket();
            clientSocket.Changed += ClientSocket_Changed;
            clientSocket.FriendshipRequested += ClientSocket_FriendRequested;
            clientSocket.Authenticated += ClientSocket_Authenticated;
            clientSocket.FrinshipReplied += clientSocket_FrinshipReplied;
            clientSocket.connect();
        }

        void clientSocket_FrinshipReplied(object sender, FriendResponseEvent e)
        {
            if(this.InvokeRequired)
            {
                ShowAddFriendDialogResponseDelegate d = new ShowAddFriendDialogResponseDelegate(ShowAddFriendResponseDialog);
                this.Invoke(d, new object[] { e.FriendName, e.Status });
            }
            else
            {
                ShowAddFriendResponseDialog(e.FriendName, e.Status);
            }
            
        }
        private void ShowAddFriendResponseDialog(string friendName, int status)
        {
            MessageBox.Show(string.Format("{0} has {1} your friend request!", friendName, (status == 0 ? "rejected" : "accepted")), "Friendship Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClientSocket_FriendRequested(object sender, FriendRequestEvent e)
        {
            if(this.InvokeRequired)
            {
                ShowAddFriendDialogDelegate d = new ShowAddFriendDialogDelegate(ShowAddFriendDialog);
                this.Invoke(d, new object[] { e.FriendName });
            }
            else
            {
                ShowAddFriendDialog(e.FriendName);
            }
        }

        private void ShowAddFriendDialog(string friendName)
        {
            FriendshipRequestDialogForm fDialog = new FriendshipRequestDialogForm();
            fDialog.FromFriendScreenName = friendName;
            fDialog.Text = string.Format("Friend request from {0}", friendName);
            fDialog.FormClosed += fDialog_FormClosed;
            fDialog.ShowDialog();
        }

        void fDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            FriendshipRequestDialogForm fDialog = (FriendshipRequestDialogForm)sender;
            string friendScreenName = fDialog.FromFriendScreenName;
            switch(fDialog.Status)
            {
                case FriendshipRequestDialogForm.ACCEPT:
                    clientSocket.AcceptFriendship(friendScreenName);
                    break;
                case FriendshipRequestDialogForm.REJECT_AND_BLOCK:
                    clientSocket.RejectAndBlock(friendScreenName);
                    break;
            }
        }

        private void ClientSocket_Authenticated(object sender, AuthEvent e)
        {
            //do stuff here
            SetText("Authenticated ok... ");
            //do more stuff here
        }

        private void ClientSocket_Changed(object sender, OutputEvent e)
        {
            this.SetText(e.Info);
        }
        
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

        private void addFriendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddFriendDialogForm addfriendDialog = new AddFriendDialogForm();
            addfriendDialog.FormClosed += onAddFriendDialog;
            addfriendDialog.ShowDialog();
        }

        private void onAddFriendDialog(object sender, FormClosedEventArgs e)
        {
            if(e.CloseReason == CloseReason.None)
            {
                return;
            }
            AddFriendDialogForm addFriendDialog = (AddFriendDialogForm)sender;
            clientSocket.sendAddFriendMessage(addFriendDialog.FriendName);
        }
        
    }
}
