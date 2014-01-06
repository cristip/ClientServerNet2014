using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerNet2014
{
    class ServerModelLocator
    {
        private static ServerModelLocator Me;
        private String serverFilePath;
        public const string APP_FOLDER = "Chat2014";
        private const string NQ_GET_USER_BY_USERNAME = "SELECT * FROM users WHERE ScreenName='{0}'";
        private const string NQ_GET_USER_BY_ID = "SELECT * FROM users WHERE Id='{0}'";
        private const string NQ_GET_FRIENS_FOR_USER = "SELECT * FROM friendships WHERE userId='{0}' OR friendId='{0}'";
        private const string NQ_GET_FRIENDSHIP_ID_BETWEEN = "SELECT * FROM friendships WHERE (userId='{0}' AND friendId='{1}') OR (userId='{1}' AND friendId='{0}') ";
        private const string NQ_GET_USERS_BY_IDS = "SELECT * FROM users WHERE Id in ({0})";
        private const string NQ_GET_INVITES_BY_USER_AND_FRIEND = "SELECT * FROM invites WHERE userId='{0}' AND friendId='{1}'";
        
        private String defaultPath;
        SQLiteDatabase db;
        
       
        private ServerModelLocator()
        {
            
        }



        public static ServerModelLocator Instance
        {
            get
            {
                if(null == Me)
                {
                    Me = new ServerModelLocator();
                }
                return Me;
            }
        }
        public String DefaultPath
        {
            get { return defaultPath; }
            set { defaultPath = value; }
        }


        public String ServerFilePath
        {
            get { return serverFilePath; }
            set { 
                serverFilePath = value;
                Properties.Settings.Default.dbfile = value;
                Properties.Settings.Default.Save();
                db = new SQLiteDatabase(serverFilePath);
            }
        }

        public int getUIDByUsername(string username)
        {
            string sql = string.Format(NQ_GET_USER_BY_USERNAME, username);
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            DataRow dr = dt.Rows[0];
            return int.Parse(dr["Id"].ToString());
        }

        public int createUser(string username)
        {
            Dictionary<string, string> userData = new Dictionary<string, string>();
            userData.Add("ScreenName", username);
            if(db.Insert("users", userData))
            {
                return getUIDByUsername(username);
            }
            return 0;
        }

        internal List<int> getFriedsIdsForUID(int uid)
        {
            List<int> uids = new List<int>();
            string sql = string.Format(NQ_GET_FRIENS_FOR_USER, uid);
            DataTable dt = db.GetDataTable(sql);
            foreach(DataRow dr in dt.Rows)
            {
                int userId = int.Parse(dr["userId"].ToString());
                int friendId = int.Parse(dr["friendId"].ToString());
                if(userId == uid)
                {
                    uids.Add(friendId);
                }
                else
                {
                    uids.Add(userId);
                }
            }
            return uids;
        }
        internal List<User> getFullUsersByIds(List<int> ids)
        {
            if(null == ids || ids.Count == 0)
            {
                return null;
            }
            List<User> users = new List<User>();
            string sql = string.Format(NQ_GET_USERS_BY_IDS, String.Join(",", ids.ToArray()));
            DataTable dt = db.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                User user = new User(int.Parse(dr["Id"].ToString()), dr["ScreenName"].ToString(), dr["Status"].ToString());
                users.Add(user);
            }
            return users;
        }

        internal User getUserById(int userId)
        {
            string sql = string.Format(NQ_GET_USER_BY_ID, userId);
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                return null;
            }
            DataRow dr = dt.Rows[0];
            User user = new User(userId, dr["ScreenName"].ToString(), dr["Status"].ToString());
            return user;
        }
        private DataTable queryInvitesByUserAndFriend(int userId, int friendId)
        {
            string sql = string.Format(NQ_GET_INVITES_BY_USER_AND_FRIEND, userId, friendId);
            DataTable dt = db.GetDataTable(sql);
            return dt;

        }
        internal bool isUserRejectedBy(int userId, int friendId)
        {
            DataTable dt = queryInvitesByUserAndFriend(userId, friendId);
            if (dt.Rows.Count == 0)
            {
                return false;
            }
            DataRow dr = dt.Rows[0];
            string rejected = dr["rejected"].ToString();
            if(rejected.ToLower() == "true")
            {
                return true;
            }
            return false;
        }

        internal void addFriendRequest(int fromUserId, int toUserId)
        {
            Dictionary<string, string> friendData = new Dictionary<string, string>();
            friendData.Add("userId", fromUserId.ToString());
            friendData.Add("friendId", toUserId.ToString());
            db.Insert("invites", friendData);
        }

        internal int getFriendRequestId(int fromUserId, int toUserId)
        {
            DataTable dt = queryInvitesByUserAndFriend(fromUserId, toUserId);
            if(dt.Rows.Count == 0)
            {
                return 0;
            }
            DataRow dr = dt.Rows[0];
            return int.Parse(dr["Id"].ToString());
        }

        internal int getFriendshipIdBetween(int fromUserId, int toUserId)
        {
            string sql = string.Format(NQ_GET_FRIENDSHIP_ID_BETWEEN, fromUserId, toUserId);
            DataTable dt = db.GetDataTable(sql);
            if(dt.Rows.Count == 0)
            {
                return 0;
            }
            DataRow dr = dt.Rows[0];
            return int.Parse(dr["Id"].ToString());
        }

        internal void removeInviteById(int indirectFriendRequestDBId)
        {
            db.Delete("invites", string.Format("Id = {0}", indirectFriendRequestDBId));
        }

        internal void addFriendship(int userId, int friendId)
        {
            Dictionary<string, string> friendshipData = new Dictionary<string, string>();
            friendshipData.Add("userId", userId.ToString());
            friendshipData.Add("friendId", friendId.ToString());
            db.Insert("friendships", friendshipData);
        }

        internal void UpdateInviteAsRejected(int inviteId)
        {
            Dictionary<string, string> inviteData = new Dictionary<string, string>();
            inviteData.Add("rejected", "true");
            db.Update("invites", inviteData, string.Format("Id = {0}", inviteId));
        }

        internal void UpdateInviteAsAccepted(int inviteId)
        {
            db.Delete("invites", string.Format("Id = {0}", inviteId));
        }
    }
}
