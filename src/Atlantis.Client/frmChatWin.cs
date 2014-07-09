using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using Atlantis.Hub;

namespace AtlantisClient
{
    public partial class frmChatWin : Form
    {
        internal AtlantisObject remoteObj;
        internal int key = 0;
        internal string yourName;
        internal string currentRoom;
        ArrayList alOnlineUser = new ArrayList();

        public frmChatWin(string room)
        {
            currentRoom = room;
            InitializeComponent();
        }

        // Event handler for message sending
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Timer that retrieves new server messages every few seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (remoteObj != null)
            {
                string tempStr = remoteObj.retrieveMessage(currentRoom, key);
                if (tempStr.Trim().Length > 0)
                {
                    key++;
                    txtAllChat.Text = txtAllChat.Text + "\n" + tempStr;
                    LaunchNotification("Limbo", tempStr);
                    txtAllChat.SelectionStart = txtAllChat.Text.Length;
                    txtAllChat.ScrollToCaret();
                }

                {
                    ArrayList onlineUser = remoteObj.getRoomUsers(currentRoom);
                    lstOnlineUser.DataSource = onlineUser;

                    if (onlineUser.Count < 2)
                    {
                        txtChatHere.Text = "Please wait until atleast two users are in the room";
                        txtChatHere.Enabled = false;
                    }
                    else if (txtChatHere.Text == "Please wait until atleast two users are in the room" && txtChatHere.Enabled == false)
                    {
                        txtChatHere.Text = "";
                        txtChatHere.Enabled = true;
                    }
                }
            }
        }        

        /// <summary>
        /// Sends the users message over the remote object to the server.
        /// </summary>
        private void SendMessage()
        {

            if (remoteObj != null && txtChatHere.Text.Trim().Length>0)
            {
                remoteObj.SendServerMessage(String.Format("{0} > {1}", yourName, txtChatHere.Text), currentRoom);
                txtChatHere.Text = "";
            }
        }

        /// <summary>
        /// Shows a notification to the user in the lower right corner
        /// </summary>
        /// <param name="title">Title of given notification</param>
        /// <param name="message">Message of given notification</param>
        private void LaunchNotification(string title, string message)
        {
            if (!message.Contains("now") && !message.Contains("joined") && !message.Contains("left") && !message.Contains(yourName))
            {
                //NotificationWindow.PopupNotifier pn = new NotificationWindow.PopupNotifier();
                //pn.BodyColor = Color.Gray;
                //pn.BorderColor = Atlantis.Hub.aero.AeroColor();
                //pn.TitleText = title;
                //pn.ContentText = message;
                //pn.Popup();
            }
        }


        // Some simple event handlers below for catching link clicks, focusing textboxes, etc.
        private void txtAllChat_LinkClicked_1(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void txtAllChat_MouseHover(object sender, EventArgs e)
        {
            txtAllChat.Focus();
        }

        private void frmChatWin_KeyDown(object sender, KeyEventArgs e)
        {
            txtChatHere.Focus();
        }

        private void onlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "online", currentRoom);
        }

        private void awayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "away", currentRoom);
        }

        private void busyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "busy", currentRoom);
        }

        private void offlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "offline", currentRoom);
        }

        private void frmChatWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (remoteObj != null)
            {
                remoteObj.DisconnectRoom(currentRoom, yourName);
                txtChatHere.Text = "";
            }
            Application.Exit();
        }
    }
}

