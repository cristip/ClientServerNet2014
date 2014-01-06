using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerNet2014
{
    public partial class ConfigureFormDialog : Form
    {
        private ServerModelLocator model = ServerModelLocator.Instance;
        public ConfigureFormDialog()
        {
            InitializeComponent();
            Binding ServerPathBinding = new Binding("Text", model, "ServerFilePath", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox2.DataBindings.Add(ServerPathBinding);
            Binding DefaultCloseMessageBinding = new Binding("Text", Properties.Settings.Default, "closeMessage", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox3.DataBindings.Add(DefaultCloseMessageBinding);
            Binding PortBinding = new Binding("Text", Properties.Settings.Default, "serverPort", false, DataSourceUpdateMode.OnPropertyChanged);
            textBox1.DataBindings.Add(PortBinding);
            Binding MaxClientsBinding = new Binding("Text", Properties.Settings.Default, "maxClients", false, DataSourceUpdateMode.OnPropertyChanged);
            numericUpDown1.DataBindings.Add(MaxClientsBinding);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Chat SQLite Database";
            ofd.Filter = "Chat 2014 Database (*.db3)|*.db3";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ofd.RestoreDirectory = true;
            ofd.ShowDialog();
            model.ServerFilePath = ofd.FileName;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //Properties.Settings.Default.closeMessage = textBox3.Text;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
