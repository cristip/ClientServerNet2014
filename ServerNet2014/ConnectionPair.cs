using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerNet2014
{
    class ConnectionPair
    {
        public ConnectionPair(Socket listener, Socket handler)
        {
            this.listener = listener;
            this.handler = handler;
        }
        public Socket listener;
        public Socket handler;
    }
}
