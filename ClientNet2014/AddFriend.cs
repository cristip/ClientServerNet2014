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
    public partial class AddFriendDialogForm : Form
    {
        private string friendName;

        
        public AddFriendDialogForm()
        {
            InitializeComponent();
            Binding friendBinding = new Binding("Text", this, "FriendName", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox1.DataBindings.Add(friendBinding);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (FriendName == null || FriendName == string.Empty)
            {
                MessageBox.Show("You must enter the friend name.", "Missing friend...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Close();
        }
        public string FriendName
        {
            get { return friendName; }
            set { friendName = value; }
        }
    }
}
