using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerNet2014
{
    public class OutputEvent : EventArgs
    {
        public string Info { get; set; }
    }

    public class Messages
    {
        public const string R_AUTH = "Auth";
        public const string S_AUTH = "UID:{0}<EOF>";
        public const string S_FRIENDS = "Friends:{0}<EOF>";
        public const string S_ONLINE = "Online:{0}<EOF>";
        public const string S_OFFLINE = "Offline:{0}<EOF>";


        public const string R_BEFRIENDTO = "BefriendTo";
        public const string S_BEFRIENDTOERROR = "BefriendToError:{0}<EOF>";
        public const string S_BEFRIENDTORESPONSE = "BefriendToResponse:{0}{1}:{2}<EOF>";
        public const string S_FRIENDREQUESTFROM = "FriendRequestFrom:{0}<EOF>";
        public const string R_FRIENDRESPONSETO = "FriendResponseTo";


    }


    class ServerSocket
    {
        private bool isAllowedToRun;
        private string port;
        private String output;
        private Socket socketListener;
        private ServerModelLocator model = ServerModelLocator.Instance;
        //private List<ConnectionPair> Connections = new List<ConnectionPair>();
        private Dictionary<Socket, int> Connections = new Dictionary<Socket, int>();
        
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public event EventHandler<OutputEvent> Changed;

        public ServerSocket()
        {
            Port = Properties.Settings.Default.serverPort;
        }

        
        public String Output
        {
            get { return output; }
            set {
                EventHandler<OutputEvent> handler = Changed;
                if(null != handler)
                {
                    var args = new OutputEvent() { Info = value };
                    handler(this, args);
                }
                output = value; 
            }
        }
        //public void stopServer()
        //{
        //    if(null == socketListener)
        //    {
        //        return;
        //    }
        //    while(Connections.Count > 0)
        //    {
        //        Socket handler = Connections.ElementAt(0).Key;
        //        Connections.Remove(handler);
        //        sendCloseTo(handler);
        //    }
        //    //try
        //    //{
        //    //    socketListener.Shutdown(SocketShutdown.Both);
        //    //    socketListener.Close();
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    Output = e.ToString();
        //    //}
        //    isAllowedToRun = false;
        //    allDone.Close();
            
        //    isAllowedToRun = false;
        //    allDone.Close();
            
        //}

        //private void sendCloseTo(Socket sock)
        //{
        //    if (sock.Connected)
        //    {
        //        byte[] byteData = Encoding.ASCII.GetBytes(Properties.Settings.Default.closeMessage);

        //        // Begin sending the data to the remote device.
        //        sock.BeginSend(byteData, 0, byteData.Length, 0,
        //            new AsyncCallback(SendShutDownCallback), socketListener);
        //    }
        //}

        public void connect()
        {
            IPAddress address = localIP;
            socketListener = new Socket(address.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
            isAllowedToRun = true;
            try
            {
                IPEndPoint localEp = new IPEndPoint(address, int.Parse(this.Port));
                socketListener.Bind(localEp);
                Output = "Binding " + localEp.Address.ToString() + ":" + this.Port;
                socketListener.Listen(Backlog);
                
                while (isAllowedToRun)
                {
                    allDone.Reset();
                    Output = "Waiting for a connection...";
                    socketListener.BeginAccept(
                       new AsyncCallback(AcceptCallback),
                       socketListener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }catch(Exception e)
            {
                Output = "connect Exception " + e.ToString();
            }
            
        }
        private void AcceptCallback(IAsyncResult ar)
        {

            Output = "New Connection from...";

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            
            try
            {
                Socket handler = listener.EndAccept(ar);
                
                //ConnectionPair connection = new ConnectionPair(listener, handler);
                //Connections.Add(connection);
                Connections.Add(handler, 0);
                
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch ( ObjectDisposedException e )
            {
                Output = "ObjectDisposedException: " + e.ToString();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead;
            try
            {
                // Read data from the client socket. 
                bytesRead = handler.EndReceive(ar);
            }catch(SocketException e)
            {
                //TODO: notify all his friends that this user is now offline...
                Output = "Client id {} disconected...";
                OnClientDisconnected(handler);
                return;
            }
           

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.

                    Output = string.Format("Read {0} bytes from socket. Data : {1}", content.Length, content);
                        
                    // Echo the data back to the client.
                    //Send(handler, content);
                    string message = content.Remove(content.Length-5);
                    ParseMessage(handler, message);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            //continue to read from this...
            StateObject nstate = new StateObject();
            nstate.workSocket = handler;
            handler.BeginReceive(nstate.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), nstate);
        }

        private void OnClientDisconnected(Socket handler)
        {
            List<int> uids = model.getFriedsIdsForUID(Connections[handler]);
            foreach(int uid in uids)
            {
                SendToUserId(uid, string.Format(Messages.S_OFFLINE, uid));
            }
            Connections.Remove(handler);
        }
        /// <summary>
        /// Gaseste socketul pe care este conectat user Id
        /// </summary>
        /// <param name="userId">Id-ul userului</param>
        /// <param name="data">String, mesajul de trimis</param>
        private void SendToUserId(int userId, String data)
        {
            foreach (KeyValuePair<Socket, int> connection in Connections)
            {
                if (connection.Value == userId)
                {
                    SendToSocket(connection.Key, data);
                    return;
                }
            }
            Output = string.Format("User with id {0} was not found when trying to send {1}", userId, data);
        }
        private void SendToSocket(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            Output = string.Format("Sending {0} to {1}...", data, Connections[handler]);
            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        private void ParseMessage(Socket handler, string content)
        {
            if (null == content || null == handler)
            {
                return;
            }
            string[] parts = content.Split(':');
            string message = parts[0];
            switch(message)
            {
                case Messages.R_AUTH:
                    this.Authenticate(handler, parts[1]);
                    break;
                case Messages.R_BEFRIENDTO:
                    this.InitializeFriendship(handler, parts[1]);
                    break;
                case Messages.R_FRIENDRESPONSETO:
                    this.FriendshipResponse(handler, parts[1]);
                    break;
            }

        }

        private void Authenticate(Socket handler, string username)
        {
            int uid = model.getUIDByUsername(username);
            
            if(0 == uid)
            {
                uid = model.createUser(username);
            }
            Connections[handler] = uid;

            SendToSocket(handler, string.Format(Messages.S_AUTH, uid));

            List<int> friendIds = model.getFriedsIdsForUID(uid);
            List<User> friends = model.getFullUsersByIds(friendIds);
            if(null == friends)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach(User friend in friends)
            {
                if(Connections.ContainsValue(friend.Id))
                {
                    friend.IsOnline = true;
                    SendToUserId(friend.Id, string.Format(Messages.S_ONLINE, uid));
                }
                sb.Append(friend.IsOnline ? 1 : 0);
                sb.Append("|");
                sb.Append(friend.ScreenName);
                sb.Append("|");
                sb.Append(friend.Id);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            //send to those uids the message that this user is now online...
            SendToSocket(handler, string.Format(Messages.S_FRIENDS, sb.ToString()));
            //add the users
        }

        public void InitializeFriendship(Socket handler, string destinationFriendName)
        {
            int fromUserId = Connections[handler];
            User fromUser = model.getUserById(fromUserId);
            int toUserId = model.getUIDByUsername(destinationFriendName);
            if(0 == toUserId)
            {
                //there's no user registered with this name
                SendToSocket(handler, string.Format(Messages.S_BEFRIENDTOERROR, destinationFriendName));
                return;
            }
            if (model.getFriendshipIdBetween(fromUserId, toUserId) > 0)
            {
                //they are already friends... nothing to do here...
                return;
            }
            if(model.isUserRejectedBy(fromUserId, toUserId))
            {
                SendToSocket(handler, string.Format(Messages.S_BEFRIENDTORESPONSE,"0", destinationFriendName, 0));
                return;
            }
            
            int directFriendRequestDBId = model.getFriendRequestId(fromUserId, toUserId);
            int indirectFriendRequestDBId = model.getFriendRequestId(toUserId, fromUserId);
            if (directFriendRequestDBId == 0 && indirectFriendRequestDBId == 0)
            {
                model.addFriendRequest(fromUserId, toUserId);
                SendToUserId(toUserId, string.Format(Messages.S_FRIENDREQUESTFROM, fromUser.ScreenName));
            }
            else
            if (indirectFriendRequestDBId > 0)
            {
                model.removeInviteById(indirectFriendRequestDBId);
                model.addFriendship(toUserId, fromUserId);
                SendToSocket(handler, string.Format(Messages.S_BEFRIENDTORESPONSE, 1, destinationFriendName, toUserId));
                SendToUserId(toUserId, string.Format(Messages.S_BEFRIENDTORESPONSE, 1, fromUser.ScreenName, fromUserId));
                return;
            }
        }

        private void FriendshipResponse(Socket handler, string message)
        {
            int result = message.IndexOf('0') == 0?0:1;
            int fromUserId = Connections[handler];
            User fromUser = model.getUserById(fromUserId);
            string toUserScreenName = message.Substring(1);
            int toUserId = model.getUIDByUsername(toUserScreenName);
            int inviteId = model.getFriendRequestId(toUserId, fromUserId);
            if(inviteId == 0)
            {
                //there are no invites waiting for a response...
                return;
            }
            if(result == 0)
            {
                //rejected...
                model.UpdateInviteAsRejected(inviteId);
                SendToUserId(toUserId, string.Format(Messages.S_BEFRIENDTORESPONSE, 0, fromUser.ScreenName, 0));
                return;
            }
            //accepted
            model.UpdateInviteAsAccepted(inviteId);
            model.addFriendship(fromUserId, toUserId);
            SendToUserId(toUserId, string.Format(Messages.S_BEFRIENDTORESPONSE, 1, fromUser.ScreenName, fromUserId));
            
        }
        
        //private void SendShutDownCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the socket from the state object.
        //        Socket handler = (Socket)ar.AsyncState;

        //        // Complete sending the data to the remote device.
        //        int bytesSent = handler.EndSend(ar);
        //        Console.WriteLine("Sent {0} bytes to client {1}.", bytesSent, Connections[handler]);

        //        //SocketAsyncEventArgs args = new  SocketAsyncEventArgs();


        //        handler.Shutdown(SocketShutdown.Both);
        //        handler.Close();
                

        //        //socketListener.Shutdown(SocketShutdown.Both);
        //        //socketListener.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        Output = "SendShutDownCallback "  + e.ToString();
        //    }
        //    allDone.Close();
        //}
        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                //Output  = String.Format("Sent {0} bytes to client.", bytesSent);
                Output = string.Format("Sent {0} bytes to client {1}.", bytesSent, Connections[handler]);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Output = string.Format("Error at SendCallback to {0}: {1}", Connections[handler], e.ToString());
            }
        }

        public IPAddress localIP
        {
            get
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip;
                    }
                }
                return null;
            }
        }

        public int Backlog
        {
            get
            {
                int confMaxConnection = int.Parse(Properties.Settings.Default.maxClients);
                if(confMaxConnection < 1)
                {
                    confMaxConnection = 100;
                }
                return confMaxConnection;
            }
        }

        public string Port { 
            get
            {
                return port;
            }
            set{
                port = value;
            } 
        }
    }
}
