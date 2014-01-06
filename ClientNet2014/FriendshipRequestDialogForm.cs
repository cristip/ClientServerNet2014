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
    public partial class FriendshipRequestDialogForm : Form
    {

        public const int ACCEPT = 1;
        public const int REJECT_AND_BLOCK = 2;
        public const int IGNORE = 0;

        private int status;
        private string fromFriendScreenName;

        public FriendshipRequestDialogForm()
        {
            InitializeComponent();
            Binding friendBinding = new Binding("Text", this, "FromFriendScreenName", true, DataSourceUpdateMode.OnPropertyChanged);
            this.label1.DataBindings.Add(friendBinding);
        }
        public int Status
        {
            get
            {
                return status;
            }
        }
        public string FromFriendScreenName 
        { 
            set
            {
                fromFriendScreenName = value;
            } 
            get
            {
                return fromFriendScreenName;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            status = ACCEPT;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            status = REJECT_AND_BLOCK;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            status = IGNORE;
            Close();
        }
        

    }
}
