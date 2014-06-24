using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using Atlantis.Hub;
using System.Net;

namespace Atlantis.Hub
{
    /// <remarks>
    /// Sample object to demonstrate the use of .NET Remoting.
    /// </remarks>
    public class Chatroom
    {
        Logger logHandler = new Logger("exec.log");
        internal Hashtable messageStore = new Hashtable();
        public string roomName;

        internal Hashtable connectedClients = new Hashtable();
        internal Hashtable clientStatuses = new Hashtable();
        internal int key = 0;

        public Chatroom(string chanName)
        {
            roomName = chanName;
        }

        internal bool addUser(string clientName, IPAddress pubIP)
        {
            if(connectedClients.Contains(clientName))
            {
                return false;
            }
            else
            {
                var nxtClient = connectedClients.Count + 1;
                connectedClients.Add(clientName, pubIP.ToString());
                clientStatuses.Add(clientName, "online");

                return true;
            }
        }

        public void LeaveChatRoom(string name)
        {
            connectedClients.Remove(name);
            clientStatuses.Remove(name);
        }

        public ArrayList GetOnlineUser()
        {
            // Build a temporary ArrayList to store users(including status)
            ArrayList currentUsers = new ArrayList();
            foreach (DictionaryEntry user in connectedClients)
            {
                var name = user.Key;
                var status = clientStatuses[name];
                var nStatus = String.Format("{0}({1})", name, status);

                currentUsers.Add(nStatus);
            }
            return currentUsers;
        }

        public int CurrentKeyNo()
        {
            return key;
        }


        public string retrieveMessage(int lastKey)
        {
            if (key > lastKey)
                return messageStore[lastKey + 1].ToString();
            else
                return "";
        }
    }
}
