using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using Atlantis.Common;
using System.Net;

namespace Atlantis.Common
{
    /// <remarks>
    /// Sample object to demonstrate the use of .NET Remoting.
    /// </remarks>
    public class AtlantisObject : MarshalByRefObject
    {
        Logger logHandler = new Logger("exec.log");
        Hashtable messageStore = new Hashtable ();

        Hashtable connectedClients = new Hashtable();
        Hashtable clientStatuses = new Hashtable();
        private int key = 0;
        
        public bool JoinToChatRoom(string name, IPAddress pubIP)
        {
            try
            {
                if (connectedClients.Contains(name))
                    return false;
                else
                {
                    var nxtClient = connectedClients.Count + 1;
                    connectedClients.Add(name, pubIP.ToString());
                    clientStatuses.Add(name, "online");

                    logHandler.WriteLine(LogType.Debug, String.Format("Client {0} connected with IP: {1}", name, pubIP.ToString()));
                    SendInternalMessage(String.Format("{0} has joined", name));
                    return true;
                }
            }
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                logHandler.WriteLine(LogType.Error, ex.Message);
                return false;
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
            foreach(DictionaryEntry user in connectedClients)
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

            if((string)oldStatus == status)
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
