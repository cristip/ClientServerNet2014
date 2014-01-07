using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientNet2014
{
    public class OutputEvent : EventArgs
    {
        public string Info { get; set; }
    }
    public class AuthEvent : EventArgs
    {
        public string UID { get; set; }
    }
    public class FriendRequestEvent : EventArgs
    {
        public string FriendName { get; set; }
    }
    public class FriendResponseEvent : EventArgs
    {
        public string FriendName { get; set; }
        public int Status { get; set; }
    }
    public class FriendOnlineEvent : EventArgs
    {
        public ClientUser clientUser { get; set; }
    }

    public class Messages
    {
        public const string S_AUTH = "Auth:{0}<EOF>";
        public const string R_UID = "UID";
        public const string R_FRIENDLIST = "Friends";
        public const string R_ONLINE = "Online";
        public const string R_OFFLINE = "Offline";

        public const string S_BEFRIENDTO = "BefriendTo:{0}<EOF>";
        public const string R_FRIENDREQUESTFROM = "FriendRequestFrom";
        public const string S_FRIENDRESPONSETO = "FriendResponseTo:{0}{1}<EOF>";
        public const string R_BEFRIENDTORESPONSE = "BefriendToResponse";
        public const string R_NEWFRIEND = "NewFriend";
    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
    public class ClientSocket
    {
            // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = 
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone = 
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = 
            new ManualResetEvent(false);

        // The response from the remote device.
        private String response = String.Empty;

        public event EventHandler<OutputEvent> Changed;
        public event EventHandler<AuthEvent> Authenticated;
        public event EventHandler<FriendRequestEvent> FriendshipRequested;
        public event EventHandler<FriendResponseEvent> FriendshipReplied;
        public event EventHandler<FriendOnlineEvent> FriendOnline;

        private ClientModelLocator model = ClientModelLocator.Instance;
        private string output;
        private Socket client;

        //private Socket ConnectSocket(string server, int port)
        //{
        //    Socket s = null;
        //    IPHostEntry hostEntry = null;

        //    // Get host related information.
        //    hostEntry = Dns.GetHostEntry(server);

        //    // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid 
        //    // an exception that occurs when the host IP Address is not compatible with the address family 
        //    // (typical in the IPv6 case). 
        //    foreach (IPAddress address in hostEntry.AddressList)
        //    {
        //        IPEndPoint ipe = new IPEndPoint(address, port);
        //        Socket tempSocket =
        //            new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        //        tempSocket.Connect(ipe);

        //        if (tempSocket.Connected)
        //        {
        //            s = tempSocket;
        //            break;
        //        }
        //        else
        //        {
        //            continue;
        //        }
        //    }
        //    return s;
        //}
        public void connect()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(model.ServerAddr);
                //IPAddress ipAddress = ipHostInfo.AddressList[0];

                IPEndPoint remoteEP = new IPEndPoint(serverAddr, int.Parse(model.ServerPort));

                // Create a TCP/IP socket.
               client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                // Send test data to the remote device.
                //Send(client, "This is a test<EOF>");
                //sendDone.WaitOne();
                sendMessage(string.Format(Messages.S_AUTH, model.ScreenName));

                
                    

                while(true)
                {
                    receiveDone.Reset();
                    // Receive the response from the remote device.
                    Receive(client);
                    receiveDone.WaitOne();
                    // Write the response to the console.
                    //Output = string.Format("Response received : {0}.", response);
                }


                
                // Release the socket.
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();

            }
            catch (Exception e)
            {
                Output = e.ToString();
            }

        }
       
        public String Output
        {
            get { return output; }
            set
            {
                EventHandler<OutputEvent> handler = Changed;
                if (null != handler)
                {
                    var args = new OutputEvent() { Info = value };
                    handler(this, args);
                }
                output = value;
            }
        }
        public IPAddress serverAddr
        {
            get
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(model.ServerAddr);
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)// && ip.ToString() == model.ServerAddr)
                    {
                        return ip;
                    }
                }
                return null;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Output = string.Format("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Output = string.Format(e.ToString());
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Output = (e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    String readStr = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    

                    if(readStr.IndexOf("<EOF>") > -1)
                    {
                        string message = readStr.Remove(readStr.Length - 5);

                        GotResponse(message);
                    }else{
                        // There might be more data, so store the data received so far.
                        state.sb.Append(readStr);
                        // Get the rest of the data.
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }

                    
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        GotResponse(response);
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Reset();
                }
                //continue to read from this...
                StateObject nstate = new StateObject();
                nstate.workSocket = client;
                client.BeginReceive(nstate.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), nstate);
            }
            catch (Exception e)
            {
                Output = (e.ToString());
            }
        }

        private void GotResponse(string content)
        {
            Output = string.Format("Received message {0}.", content);
            string[] parts = content.Split(':');
            string message = parts[0];
            switch (message)
            {
                case Messages.R_UID:
                    model.clientUser = new ClientUser(parts[1], model.ScreenName);
                    EventHandler<AuthEvent> authhandler = Authenticated;
                    if (null != authhandler)
                    {
                        authhandler(this, new AuthEvent() { UID = parts[1] });
                    }
                    break;
                case Messages.R_FRIENDLIST:
                    string[] friends = parts[1].Split(',');
                    foreach(string friend in friends)
                    {
                        string[] friendData = friend.Split('|');
                        ClientUser friendObj = new ClientUser(friendData[2], friendData[1]);
                        friendObj.IsOnline = friendData[0] == "1";
                        //model.Friends.RaiseListChangedEvents = true;
                        model.Friends.Add(friendObj);
                    }
                    break;
                case Messages.R_ONLINE:
                    ClientUser onlineFriend = model.getFriendById(parts[1]);
                    if(null == onlineFriend)
                    {
                        Output = string.Format("Could not find the friend with Id: {0}", parts[1]);
                        return;
                    }
                    onlineFriend.IsOnline = true;
                    dispatchOnlineFriend(onlineFriend);
                    break;
                case Messages.R_OFFLINE:
                    ClientUser offlineFriend = model.getFriendById(parts[1]);
                    if (null == offlineFriend)
                    {
                        Output = string.Format("Could not find the friend with Id: {0}", parts[1]);
                        return;
                    }
                    offlineFriend.IsOnline = false;
                    dispatchOnlineFriend(offlineFriend);
                    break;
                case Messages.R_FRIENDREQUESTFROM:
                    EventHandler<FriendRequestEvent> friendshiphandler = FriendshipRequested;
                    if (null != friendshiphandler)
                    {
                        friendshiphandler(this, new FriendRequestEvent() { FriendName = parts[1] });
                    }
                    break;
                case Messages.R_BEFRIENDTORESPONSE:
                    EventHandler<FriendResponseEvent> frienshipreplyhandler = FriendshipReplied;
                    int friendResponseResult = int.Parse(parts[1].Substring(0, 1));
                    string friendScreenName = parts[1].Substring(1);
                    string friendId = parts[2];
                    if (null != frienshipreplyhandler)
                    {
                        frienshipreplyhandler(this, new FriendResponseEvent() { FriendName = friendScreenName, Status = friendResponseResult });
                    }
                    if(friendResponseResult == 1)
                    {
                        ClientUser clientUser = new ClientUser(friendId, friendScreenName);
                        clientUser.IsOnline = true;
                        model.Friends.Add(clientUser);
                        
                    }
                    break;
                case Messages.R_NEWFRIEND:
                    string[] newFriendData = parts[1].Split('|');
                    ClientUser newFriend = new ClientUser(newFriendData[0], newFriendData[1]);
                    newFriend.IsOnline = true;
                    model.Friends.Add(newFriend);
                    break;
            }
        }

        private void dispatchOnlineFriend(ClientUser onlineFriend)
        {
            EventHandler<FriendOnlineEvent> handler = FriendOnline;
            if(null != handler)
            {
                handler(this, new FriendOnlineEvent() { clientUser = onlineFriend});
            }
        }

        public void sendMessage(String message)
        {
            Send(this.client, message);
            sendDone.WaitOne();
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Output = string.Format("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
                receiveDone.Set();
            }
            catch (Exception e)
            {
                Output = (e.ToString());
            }
        }


        internal void sendAddFriendMessage(string friendName)
        {
            Output = string.Format("Sending Befriend to {0}", friendName);
            sendMessage(string.Format(Messages.S_BEFRIENDTO, friendName));
        }

        internal void AcceptFriendship(string friendName)
        {
            Output = string.Format("Sending Accept friend to {0}", friendName);
            sendMessage(string.Format(Messages.S_FRIENDRESPONSETO, 1, friendName));
        }

        internal void RejectAndBlock(string friendName)
        {
            Output = string.Format("Sending Reject friend to {0}", friendName);
            sendMessage(string.Format(Messages.S_FRIENDRESPONSETO, 0, friendName));
        }
    }
}
