using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net;
using System.Collections;
using System.Linq;

namespace Atlantis.Hub
{
    /// <remarks>
    /// Sample object to demonstrate the use of .NET Remoting.
    /// </remarks>
    public class AtlantisObject : MarshalByRefObject
    {
        static List<Chatroom> chatStorage = new List<Chatroom>();
        Logger logHandler = new Logger("exec.log");

        public void addRoom(string chName)
        {
            Chatroom chatRoom = new Chatroom(chName);
            chatStorage.Add(chatRoom);
        }

        public Chatroom returnRoom(string roomName)
        {
            var chatExists = from chatroom in chatStorage
                             where chatroom.roomName == roomName
                             select chatroom;
            Chatroom roomObject = chatExists.FirstOrDefault();

            return roomObject;
        }
        public bool ConnectRoom(string roomName, string clientName, IPAddress publicIP)
        {
            Chatroom roomObject = returnRoom(roomName);
            if (roomObject != null)
            {
                var hasJoined = roomObject.addUser(clientName, publicIP);

                if (hasJoined != true)
                {
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
                return false;
            }
        }

        public void DisconnectRoom(string roomName, string clientName)
        {
                Chatroom roomObject = returnRoom(roomName);

                roomObject.LeaveChatRoom(clientName);
                SendInternalMessage(String.Format("{0} has left room {1}", clientName, roomName), roomName);
        }

        public ArrayList getRoomUsers(string roomName)
        {
            Chatroom roomObject = returnRoom(roomName);

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

        public void SetUserStatus(string nick, string status, string roomName)
        {
            // We have to make sure status is either: online/busy/away/offline
            // TODO -- if set to offline, make sure user is invisible.

            // Make sure status isn't a duplicate of the former.

            // Get the room
            Chatroom roomObject = returnRoom(roomName);
            
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
            Chatroom roomObject = returnRoom(roomName);

            return roomObject.CurrentKeyNo();
        }

        private void SendInternalMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Info, String.Format("{0}", message));

            Chatroom roomObject = returnRoom(roomName);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        public void SendServerMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Chat, String.Format("{0}", message));

            Chatroom roomObject = returnRoom(roomName);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        public string retrieveMessage(string roomName, int lastKey)
        {
            Chatroom roomObject = returnRoom(roomName);

            var room_c_key = roomObject.key;
            if (room_c_key > lastKey)
                return roomObject.messageStore[lastKey + 1].ToString();
            else
                return "";
        }
    }
}
