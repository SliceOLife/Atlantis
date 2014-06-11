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

        internal Hashtable connectedClients = new Hashtable();
        internal Hashtable clientStatuses = new Hashtable();
        internal int key = 0;

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
            SendInternalMessage(String.Format("{0} has left", name));
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

        public void SetUserStatus(string nick, string status)
        {
            // We have to make sure status is either: online/busy/away/offline
            // TODO -- if set to offline, make sure user is invisible.

            // Make sure status isn't a duplicate of the former.
            var oldStatus = clientStatuses[nick];

            if ((string)oldStatus == status)
            {
                return;
            }

            switch (status)
            {
                case "online":
                    clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status));
                    break;
                case "busy":
                    clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status));
                    break;
                case "away":
                    clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status));
                    break;
                case "offline":
                    clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status));
                    break;
                default:
                    clientStatuses[nick] = "online";
                    SendInternalMessage(String.Format("{0} is now online", nick));
                    break;
            }
        }

        public int CurrentKeyNo()
        {
            return key;
        }

        private void SendInternalMessage(string message)
        {
            logHandler.WriteLine(LogType.Info, message);

            messageStore.Add(++key, message);
        }

        public void SendServerMessage(string message)
        {
            logHandler.WriteLine(LogType.Chat, message);

            messageStore.Add(++key, message);
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
