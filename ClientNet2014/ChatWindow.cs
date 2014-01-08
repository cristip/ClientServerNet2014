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
    public partial class ChatWindow : Form
    {
        public ClientUser FriendCU{set; get;}
        
        public event EventHandler<SendMessageEvent> SendMessageContent;
        public event EventHandler<SendFileEvent> AskToSendFile;
        public event EventHandler<TransferFileEvent> TransferFile;

        private ClientModelLocator model = ClientModelLocator.Instance;

        private string pendingFileToSend;

        public ChatWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendMessage();
        }

        private void sendMessage()
        {
            EventHandler<SendMessageEvent> handler = SendMessageContent;
            if(null != handler)
            {
                handler(this, new SendMessageEvent() { content = this.textBox2.Text, toUID = this.FriendCU.Id });
            }
            this.textBox1.Text += string.Format("{0}:{1}\r\n", model.clientUser.ScreenName, this.textBox2.Text);
            this.textBox2.Text = "";
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || e.Control)
            {
                return;
            }
            this.textBox2.Text = this.textBox2.Text.TrimEnd(new char[] { '\r', '\n' });
            sendMessage();
        }

        public void addTextMessage(string text)
        {
            this.textBox1.Text += string.Format("{0}:{1}\r\n", FriendCU.ScreenName, text);
        }

        private void sendFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            if (null == fd.FileName)
            {
                return;
            }
            pendingFileToSend = fd.FileName;

            FileInfo fi = new FileInfo(fd.FileName);

            EventHandler<SendFileEvent> handler = AskToSendFile;
            if (null != handler)
            {
                handler(this, new SendFileEvent() { fileName = fd.SafeFileName, fileSize = fi.Length.ToString() });
            }
            
        }

       


        internal void initializeFileTransfer(bool hasAccepted, string fileName)
        {
            if(!hasAccepted)
            {
                this.textBox1.Text += string.Format("{0} has rejected your file {1}\r\n", FriendCU.ScreenName, fileName);
                this.pendingFileToSend = null;
                return;
            }
            this.textBox1.Text += string.Format("Accepted. Sending {0} to {1}\r\n", fileName, FriendCU.ScreenName);
            EventHandler<TransferFileEvent> handler = TransferFile;
            if(null != handler)
            {
                handler(this, new TransferFileEvent() { filePath = this.pendingFileToSend, toUID = FriendCU.Id });
            }
        }
    }
}
