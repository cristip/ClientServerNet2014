using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientNet2014
{
    public class ClientUser
    {
        private String screenName;
        private String id;
        private bool isOnline;

        public ClientUser(String id, String screenName)
        {
            this.Id = id;
            this.ScreenName = screenName;
        }

        public String Id
        {
            get { return id; }
            set { id = value; }
        }

        public String ScreenName
        {
            get { return screenName; }
            set { screenName = value; }
        }
        public bool IsOnline
        {
            get { return isOnline; }
            set { isOnline = value; }
        }

        public string DisplayValue
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", Id, (IsOnline?"Online":"Offline") ,ScreenName);
        }
    }
     
}
