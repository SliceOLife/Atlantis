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
        static List<Chatroom> chatStorage = new List<Chatroom>(); // Stores instances of chatrooms
        Logger logHandler = new Logger("exec.log");


        /// <summary>
        /// Adds a new public chatroom to the server's chatroom list.
        /// </summary>
        /// <param name="chName">The chatroom's name</param>
        public void addRoom(string chName)
        {
            Chatroom chatRoom = new Chatroom(chName);
            chatStorage.Add(chatRoom);
        }

        /// <summary>
        /// Returns an instance of a chatroom if existant.
        /// </summary>
        /// <param name="roomName">The chatroom's name to retrieve.</param>
        /// <returns>If the chatroom exists, it returns a reference to it.</returns>
        public Chatroom returnRoom(string roomName)
        {
            var chatExists = from chatroom in chatStorage
                             where chatroom.roomName == roomName
                             select chatroom;
            Chatroom roomObject = chatExists.FirstOrDefault();

            return roomObject;
        }

        /// <summary>
        /// Connects a client to the appropriate chatroom
        /// </summary>
        /// <param name="roomName">The room name to connect the client to</param>
        /// <param name="clientName">The client's name</param>
        /// <param name="publicIP">The client's public-facing IP address</param>
        /// <returns>True if client was able to connect, otherwise false.</returns>
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

        /// <summary>
        /// Disconnects a client from a chatroom
        /// </summary>
        /// <param name="roomName">Chatroom to disconnect client from</param>
        /// <param name="clientName">Client to remove from the chatroom</param>
        public void DisconnectRoom(string roomName, string clientName)
        {
                Chatroom roomObject = returnRoom(roomName);

                roomObject.LeaveChatRoom(clientName);
                SendInternalMessage(String.Format("{0} has left room {1}", clientName, roomName), roomName);
        }

        /// <summary>
        /// Gets all the chatroom's users and their statuses.
        /// </summary>
        /// <param name="roomName">The room to collect the userlist from</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Sets a users status for the given chatroom.(online/busy/away/offline)
        /// </summary>
        /// <param name="nick">The user's nickname</param>
        /// <param name="status">The user's new status</param>
        /// <param name="roomName">The chatroom in which to change the user's status in.</param>
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

        /// <summary>
        /// Retrieves the last keynumber for the message from that room.
        /// </summary>
        /// <param name="roomName">Roomname from which to retrieve the keynumber.</param>
        /// <returns></returns>
        public int CurrentRoomKeyNo(string roomName)
        {
            Chatroom roomObject = returnRoom(roomName);

            return roomObject.CurrentKeyNo();
        }

        /// <summary>
        /// Sends an internal message to the given chatroom
        /// </summary>
        /// <param name="message">The internal message to send</param>
        /// <param name="roomName">The room which to send the message to</param>
        private void SendInternalMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Info, String.Format("{0}", message));
            Chatroom roomObject = returnRoom(roomName);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        /// <summary>
        /// Sends a server message to the given chatroom
        /// </summary>
        /// <param name="message">The server message to send</param>
        /// <param name="roomName">The room which to send the message to</param>
        public void SendServerMessage(string message, string roomName)
        {
            logHandler.WriteLine(LogType.Chat, String.Format("{0}", message));

            Chatroom roomObject = returnRoom(roomName);

            roomObject.messageStore.Add(++roomObject.key, message);
        }

        /// <summary>
        /// Retrieves the latest message from the given room using the last keynumber.
        /// </summary>
        /// <param name="roomName">The room to retrieve the message from</param>
        /// <param name="lastKey">The last keynumber we have.</param>
        /// <returns></returns>
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
