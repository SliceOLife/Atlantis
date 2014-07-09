using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using Atlantis.Hub;
using System.Net;

namespace Atlantis.Hub
{
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

        /// <summary>
        /// Adds a new user to this chatroom's active user list.
        /// </summary>
        /// <param name="clientName">Name of the new user</param>
        /// <param name="pubIP">The new user's public-facing IP address</param>
        /// <returns>false if a user with that name is already in here, otherwise true.</returns>
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

        /// <summary>
        /// Removes given user from this chatroom's user list(disconnect)
        /// </summary>
        /// <param name="name">The given user's name</param>
        public void LeaveChatRoom(string name)
        {
            connectedClients.Remove(name);
            clientStatuses.Remove(name);
        }

        /// <summary>
        /// Builds an ArrayList of current chatroom users and their accompanying status.
        /// </summary>
        /// <returns>Userlist of users.</returns>
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

        // Returns the last given key for keeping up which messages we sent/got last.
        public int CurrentKeyNo()
        {
            return key;
        }

        /// <summary>
        /// Method for retrieving the latest message from this chatroom
        /// </summary>
        /// <param name="lastKey">The key for the last message that was received. So we don't get older messages.</param>
        /// <returns></returns>
        public string retrieveMessage(int lastKey)
        {
            if (key > lastKey)
                return messageStore[lastKey + 1].ToString();
            else
                return "";
        }
    }
}
