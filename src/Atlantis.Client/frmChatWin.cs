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
using Atlantis.Common;

namespace AtlantisClient
{
    public partial class frmChatWin : Form
    {
        internal AtlantisObject remoteObj;
        internal int key = 0;
        internal string yourName;
        ArrayList alOnlineUser = new ArrayList();

        public frmChatWin()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        int skipCounter = 4;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (remoteObj != null)
            {
                string tempStr = remoteObj.retrieveMessage(key);
                if (tempStr.Trim().Length > 0)
                {
                    key++;
                    txtAllChat.Text = txtAllChat.Text + "\n" + tempStr;
                    LaunchNotification("Limbo", tempStr);
                    txtAllChat.SelectionStart = txtAllChat.Text.Length;
                    txtAllChat.ScrollToCaret();
                }

                //if (skipCounter > 3)
                {
                    ArrayList onlineUser = remoteObj.GetOnlineUser();
                    lstOnlineUser.DataSource = onlineUser;
                    skipCounter = 0;

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
                //else
                  //  skipCounter++;
            }
        }        
        private void SendMessage()
        {

            if (remoteObj != null && txtChatHere.Text.Trim().Length>0)
            {
                remoteObj.SendServerMessage(String.Format("{0} > {1}", yourName, txtChatHere.Text));
                txtChatHere.Text = "";
            }
        }

        private void LaunchNotification(string title, string message)
        {
            if (!message.Contains("now") && !message.Contains("joined") && !message.Contains("left") && !message.Contains(yourName))
            {
                NotificationWindow.PopupNotifier pn = new NotificationWindow.PopupNotifier();
                pn.BodyColor = Color.Gray;
                pn.BorderColor = Atlantis.Common.aero.AeroColor();
                pn.TitleText = title;
                pn.ContentText = message;
                pn.Popup();
            }
        }

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
            remoteObj.SetUserStatus(yourName, "online");
        }

        private void awayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "away");
        }

        private void busyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "busy");
        }

        private void offlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            remoteObj.SetUserStatus(yourName, "offline");
        }

        private void frmChatWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (remoteObj != null)
            {
                remoteObj.LeaveChatRoom(yourName);
                txtChatHere.Text = "";
            }
            Application.Exit();
        }
    }
}

