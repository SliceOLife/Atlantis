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
using System.IO;

namespace AtlantisClient
{
    public partial class frmLogin : Form
    {
        TcpChannel chan;
        ArrayList alOnlineUser = new ArrayList();
        frmChatWin objChatWin;
        Logger logHandler = new Logger("exec.log");
        public frmLogin()
        {
            InitializeComponent();
        }

        // Event handler for the join button
        private void btnJoin_Click(object sender, EventArgs e)
        {
            JoinToChatRoom();
        }

        /// <summary>
        /// Loads the users configuration file, and tries to setup a connection to the remote server and chatroom.
        /// it then initializes the main chat window.
        /// </summary>
        private void JoinToChatRoom()
        {
            if (chan == null && txtName.Text.Trim().Length != 0)
            {
                chan = new TcpChannel();
                ChannelServices.RegisterChannel(chan, false);

                // Create an instance of the remote object
                objChatWin = new frmChatWin(txtServerAdd.Text);

                // Built connection string
                logHandler.WriteLine(LogType.Debug, "Trying to load client.ini");
                var mainPath = @"client.ini";

                if (!File.Exists(mainPath))
                {
                    logHandler.WriteLine(LogType.Error, "client.ini not found");
                    Environment.Exit(0);
                }

                IniFile iniHandler = new IniFile(mainPath);
                var port = Int32.Parse(iniHandler.Read("port", "connection"));
                var ip = iniHandler.Read("ip", "connection");
                var connString = String.Format("tcp://{0}:{1}/{2}", ip, port, txtServerAdd.Text);
                objChatWin.remoteObj = (AtlantisObject)Activator.GetObject(typeof(AtlantisObject), connString);

                var pubIP = new System.Net.WebClient().DownloadString("http://bot.whatismyipaddress.com");
                try
                {
                    if (!objChatWin.remoteObj.ConnectRoom(txtServerAdd.Text, txtName.Text, System.Net.IPAddress.Parse(pubIP)))
                    {
                        //MessageBox.Show(String.Format("A user with the name {0} is in that chatroom, or the chatroom doesn't exist."), txtName.Text);
                        ChannelServices.UnregisterChannel(chan);
                        chan = null;
                        objChatWin.Dispose();
                        return;
                    }
                }
                catch(System.Runtime.Remoting.RemotingException ex)
                {
                    logHandler.WriteLine(LogType.Error, ex.Message);
                    MessageBox.Show("That chatroom doesn't exist.");
                    ChannelServices.UnregisterChannel(chan);
                    chan = null;
                    objChatWin.Dispose();
                }
                objChatWin.key = objChatWin.remoteObj.CurrentRoomKeyNo(txtServerAdd.Text);

                objChatWin.yourName = txtName.Text;

                this.Hide();
                objChatWin.Text = String.Format("Limbo -- Connected as: {0}", txtName.Text);
                objChatWin.Show();

            }
        }

    }
}