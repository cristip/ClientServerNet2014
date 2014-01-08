using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        delegate void ListChangedDelegate();
        delegate void FriendOnlineDelegate(ClientUser clientUser);
        delegate void ChatFromDelegate(string uid, string content);
        delegate void FileOfferFromDelegate(string uid, string fileName, string fileSize);
        delegate void FileAcceptDelegate(bool hasAccepted, string fromUID, string fileName);

        Dictionary<string, ChatWindow> Chats = new Dictionary<string, ChatWindow>();

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;

            

           
        }

        void Form1_Load(object sender, EventArgs e)
        {
            optionsToolStripMenuItem_Click(null, null);

            
            model.Friends.ListChanged += Friends_ListChanged;
            //Binding friendsBinding = new Binding("DataSource", model, "Friends", false, DataSourceUpdateMode.OnPropertyChanged);
            this.listBox1.ValueMember = "Id";
            this.listBox1.DisplayMember = "DisplayValue";

            this.listBox1.DoubleClick += listBox1_DoubleClick;
            
        }

        void listBox1_DoubleClick(object sender, EventArgs e)
        {
            displayChatWindow((ClientUser)listBox1.SelectedItem);
            
        }

        void cw_FormClosed(object sender, FormClosedEventArgs e)
        {
            ChatWindow cw = sender as ChatWindow;
            Chats.Remove(cw.FriendCU.Id);
        }

        void cw_SendMessageContent(object sender, SendMessageEvent e)
        {
            clientSocket.sentChatMessage(e.toUID, e.content);
        }

        void Friends_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(this.InvokeRequired)
            {
                ListChangedDelegate d = new ListChangedDelegate(RefreshListBox);
                this.Invoke(d);
            }else
            {
                RefreshListBox();
            }
            
        }

        private void RefreshListBox()
        {
            this.listBox1.DataSource = null;
            this.listBox1.DataSource = model.Friends;
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
            clientSocket.FriendshipReplied += clientSocket_FrinshipReplied;
            clientSocket.FriendOnline += clientSocket_FriendOnline;
            clientSocket.ChatReceived += clientSocket_ChatReceived;
            clientSocket.FileOfferReceived += clientSocket_FileOfferReceived;
            clientSocket.FileOfferAccepted += clientSocket_FileOfferAccepted;
            clientSocket.connect();
        }

        void clientSocket_FileOfferAccepted(object sender, AcceptedFileByFriend e)
        {
            if(this.InvokeRequired)
            {
                FileAcceptDelegate d = new FileAcceptDelegate(displayAcceptedFile);
                this.Invoke(d, new object[] { e.hasAccepted, e.uid, e.fileName });
            }
            else
            {
                displayAcceptedFile(e.hasAccepted, e.uid, e.fileName);
            }
        }

        private void displayAcceptedFile(bool hasAccepted, string fromUID, string fileName)
        {
            ChatWindow cw = null;
            if (!Chats.ContainsKey(fromUID))
            {
                if(hasAccepted)
                {
                    clientSocket.sendCanceledFileTransfer(fromUID, fileName);
                }
                return;
            }
            cw = Chats[fromUID];
            cw.initializeFileTransfer(hasAccepted, fileName);
        }

        void clientSocket_FileOfferReceived(object sender, ReceiveFileFromFriend e)
        {
            if(this.InvokeRequired)
            {
                FileOfferFromDelegate d = new FileOfferFromDelegate(displayFileOffer);
                this.Invoke(d, new object[] { e.fromUID, e.fileName, e.fileSize });
            }
            else
            {
                displayFileOffer(e.fromUID, e.fileName, e.fileSize);
            }
        }

        void displayFileOffer(string uid, string fileName, string fileSize)
        {
            DialogResult dialog = MessageBox.Show(string.Format("{0} is sending you the file: {1} [{2}MB]. Do you accept this file?", model.getFriendById(uid), fileName, fileSize), "File Transfer Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(dialog != DialogResult.OK)
            {
                //reject file
                clientSocket.sendRejectFile(uid, fileName);
                return;
            }
            //accept file
            clientSocket.sendAcceptFile(uid, fileName);
        }

        void clientSocket_ChatReceived(object sender, ChatFromFriend e)
        {
            if(this.InvokeRequired)
            {
                ChatFromDelegate d = new ChatFromDelegate(addChatFrom);
                this.Invoke(d, new object[] { e.uid, e.content });
            }else
            {
                this.addChatFrom(e.uid, e.content);
            }
        }

        private void addChatFrom(string fromUID, string content)
        {
            ChatWindow cw = null;
            if(Chats.ContainsKey(fromUID))
            {
                cw = Chats[fromUID];
            }
            else
            {
                cw = displayChatWindow(model.getFriendById(fromUID));
            }
            cw.addTextMessage(content);
        }

        private ChatWindow displayChatWindow(ClientUser clientUser)
        {
            ChatWindow cw = new ChatWindow();
            cw.FriendCU = clientUser;
            cw.Text = string.Format("Chat with {0}", clientUser.ScreenName);
            cw.FormClosed += cw_FormClosed;
            cw.AskToSendFile += cw_AskToSendFile;
            cw.TransferFile += cw_TransferFile;
            cw.SendMessageContent += cw_SendMessageContent;
            cw.Show();
            Chats.Add(clientUser.Id, cw);
            return cw;
        }

        void cw_TransferFile(object sender, TransferFileEvent e)
        {
            ProcessRead(e.filePath).Wait();

        }

        async Task ProcessRead(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                MessageBox.Show(string.Format("Error opening {0}: file not found", filePath), "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    string text = await ReadTextAsync(filePath);
                    Console.WriteLine(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                //StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    //clientSocket.sendDataChunk()
                   // string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                   // sb.Append(text);
                }

                return "";
            }
        }

        void cw_AskToSendFile(object sender, SendFileEvent e)
        {
            clientSocket.askToReceiveFile(((ChatWindow)sender).FriendCU.Id, e.fileName, e.fileSize);
        }

        void clientSocket_FriendOnline(object sender, FriendOnlineEvent e)
        {
            Friends_ListChanged(null, null); if (this.InvokeRequired)
            {
                FriendOnlineDelegate d = new FriendOnlineDelegate(displayOnlineDialog);
                this.Invoke(d, new object[]{e.clientUser});
            }
            else
            {
                displayOnlineDialog(e.clientUser);
            }
        }

        private void displayOnlineDialog(ClientUser friend)
        {
            this.listBox1.DataSource = null;
            this.listBox1.DataSource = model.Friends;
            MessageBox.Show(string.Format("{0} is now {1}!", friend.ScreenName, friend.IsOnline ? "online" : "offline"), friend.IsOnline ? "Online" : "Offline", MessageBoxButtons.OK , MessageBoxIcon.Information);
            
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

        private void aboutTema3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.model.Friends.RaiseListChangedEvents = true ;
        }

        
    }
}
