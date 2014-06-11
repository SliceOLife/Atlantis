using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net;
using System.Collections;

namespace Atlantis.Hub
{
    /// <remarks>
    /// Sample object to demonstrate the use of .NET Remoting.
    /// </remarks>
    public class AtlantisObject : MarshalByRefObject
    {
        Dictionary<string, Chatroom> chatroomStorage = new Dictionary<string, Chatroom>();
        Logger logHandler = new Logger("exec.log");

        public bool addRoom(string chName)
        {
            Chatroom chatRoom = new Chatroom();
            chatroomStorage.Add(chName, chatRoom);

            return true;
        }
        
        public bool ConnectRoom(string roomName, string clientName, IPAddress publicIP)
        {
            if(chatroomStorage.ContainsKey(roomName))
            {
                Chatroom roomObject = new Chatroom();
                chatroomStorage.TryGetValue(roomName, out roomObject);
                var hasJoined = roomObject.addUser(clientName, publicIP);

                if(hasJoined != true) {
                    // User is in this room already.
                    return false;
                }
                else
                {
                    logHandler.WriteLine(LogType.Debug, String.Format("Client {0} connected with IP: {1}", clientName, publicIP.ToString()));
                    SendInternalMessage(String.Format("{0} has joined {1}", clientName, roomName), roomName);
                    return true;
                }
            }
            else
            {
                // This room doesn't exist.
                return false;
            }
        }

        public void DisconnectRoom(string roomName, string clientName)
        {
            if (chatroomStorage.ContainsKey(roomName))
            {
                Chatroom roomObject = new Chatroom();
                chatroomStorage.TryGetValue(roomName, out roomObject);

                roomObject.LeaveChatRoom(clientName);
                SendInternalMessage(String.Format("{0} has left room {1}", clientName, roomName), roomName);
            }
        }

        public ArrayList getRoomUsers(string roomName)
        {
            if (chatroomStorage.ContainsKey(roomName))
            {
                Chatroom roomObject = new Chatroom();
                chatroomStorage.TryGetValue(roomName, out roomObject);

                ArrayList currentUsers = new ArrayList();
                foreach(DictionaryEntry user in roomObject.connectedClients)
                {
                    var name = user.Key;
                    var status = roomObject.clientStatuses[name];
                    var nStatus = String.Format("{0}({1})", name, status);

                    currentUsers.Add(nStatus);
                }

                return currentUsers;
            }

            return null;
        }

        public void SetUserStatus(string nick, string status, string roomName)
        {
            // We have to make sure status is either: online/busy/away/offline
            // TODO -- if set to offline, make sure user is invisible.

            // Make sure status isn't a duplicate of the former.

            // Get the room
            Chatroom roomObject = new Chatroom();
            chatroomStorage.TryGetValue(roomName, out roomObject);
            
            var oldStatus = roomObject.clientStatuses[nick];

            if((string)oldStatus == status)
            {
                return;
            }

            switch (status)
            {
                case "online":
                    roomObject.clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status), roomName);
                    break;
                case "busy":
                    roomObject.clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status), roomName);
                    break;
                case "away":
                    roomObject.clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status), roomName);
                    break;
                case "offline":
                    roomObject.clientStatuses[nick] = status;
                    SendInternalMessage(String.Format("{0} is now {1}", nick, status), roomName);
                    break;
                default:
                    roomObject.clientStatuses[nick] = "online";
                    SendInternalMessage(String.Format("{0} is now online", nick), roomName);
                    break;
            }
        }

        public int CurrentRoomKeyNo(string roomName)
        {
            Chatroom roomObject = new Chatroom();
            chatroomStorage.TryGetValue(roomName, out roomObject);

            return roomObject.CurrentKeyNo();
        }

        private void SendInternalMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Info, String.Format("{0): {1}", roomName, message));

            Chatroom roomObject = new Chatroom();
            chatroomStorage.TryGetValue(roomName, out roomObject);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        public void SendServerMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Chat, String.Format("{0): {1}", roomName, message));

            Chatroom roomObject = new Chatroom();
            chatroomStorage.TryGetValue(roomName, out roomObject);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        public string retrieveMessage(string roomName, int lastKey)
        {
            Chatroom roomObject = new Chatroom();
            chatroomStorage.TryGetValue(roomName, out roomObject);

            var room_c_key = roomObject.key;
            if (room_c_key > lastKey)
                return roomObject.messageStore[lastKey + 1].ToString();
            else
                return "";
        }
    }
}
