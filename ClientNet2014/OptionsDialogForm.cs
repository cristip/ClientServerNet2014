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
    public partial class OptionsDialogForm : Form
    {
        ClientModelLocator model = ClientModelLocator.Instance;

        private String initialScreenName;
        private String initialServerAddr;

        public OptionsDialogForm()
        {
            InitializeComponent();
            this.initialScreenName = model.ScreenName;
            this.initialServerAddr = model.ServerAddr;
            Binding ServerAddrBinding = new Binding("Text", model, "ServerAddr", true, DataSourceUpdateMode.OnPropertyChanged);
            Binding ServerPortBinding = new Binding("Text", model, "ServerPort", true, DataSourceUpdateMode.OnPropertyChanged);
            Binding ScreenNameBinding = new Binding("Text", model, "ScreenName", true, DataSourceUpdateMode.OnPropertyChanged);
            textBox1.DataBindings.Add(ServerAddrBinding);
            textBox3.DataBindings.Add(ServerPortBinding);
            textBox2.DataBindings.Add(ScreenNameBinding);
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            model.ServerAddr = initialServerAddr;
            model.ScreenName = initialScreenName;
            Close();
        }
    }
}
