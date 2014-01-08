using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNet2014
{
    public class SendMessageEvent : EventArgs
    {
        public string content { get; set; }
        public string toUID { get; set; }
    }
}
