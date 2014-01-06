using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNet2014
{
    class User
    {
        public bool IsOnline;
        public int Id;
        public string ScreenName;
        public string Status;

        public User()
        {

        }
        public User(int Id, string ScreenName, string Status)
        {
            this.Id = Id;
            this.ScreenName = ScreenName;
            this.Status = Status;
        }
    }
}
