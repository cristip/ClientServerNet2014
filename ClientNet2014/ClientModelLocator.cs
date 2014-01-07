using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNet2014
{
    class ClientModelLocator
    {
        private static ClientModelLocator Me;
        private ClientModelLocator()
        {
            
        }

        private String serverAddr;
        private String screenName;
        private BindingList<ClientUser> friends = new BindingList<ClientUser>();
        private bool isConnected;
        private string serverPort;
        public ClientUser clientUser;

        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }
        public String ScreenName
        {
            get { return screenName; }
            set { screenName = value; }
        }
        public String ServerPort
        {
            get
            {
                if (null == serverPort)
                {
                    serverPort = Properties.Settings.Default.port;
                }
                return serverPort;
            }
            set
            {
                serverPort = value;
                Properties.Settings.Default.port = value;
                Properties.Settings.Default.Save();
            }
        }
        public String ServerAddr
        {
            get 
            { 
                if(null == serverAddr)
                {
                    serverAddr = Properties.Settings.Default.server;
                }
                return serverAddr; 
            }
            set 
            { 
                serverAddr = value;
                Properties.Settings.Default.server = value;
                Properties.Settings.Default.Save();
            }
        }
        
        public BindingList<ClientUser> Friends
        {
            get
            {
                return this.friends;
            }
        }

        public static ClientModelLocator Instance{
            get
            {
                if(null == Me)
                {
                    Me = new ClientModelLocator();
                }
                return Me;
            }
        }

        internal ClientUser getFriendById(string p)
        {
            foreach(ClientUser clientUser in friends)
            {
                if(clientUser.Id == p)
                {
                    return clientUser;
                }
            }
            return null;
        }
    }
}
