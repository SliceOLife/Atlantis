using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using Atlantis.Hub;

namespace Atlantis.Server
{
    class Program
    {
        static TcpChannel channel;
        static int port;
        static Logger logHandler = new Logger("exec.log");
        static void Main(string[] args)
        {
            Console.Title = "Atlantis.Server";

            // Check if our server.ini exists
            // TODO, wrap ini handling into a safer method, exceptions.
            logHandler.WriteLine(LogType.Debug, "Trying to load server.ini");
            var mainPath = @"server.ini";

            if (!File.Exists(mainPath))
            {
                logHandler.WriteLine(LogType.Error, "server.ini not found");
                logHandler.WriteLine(LogType.Info, "creating it now with default values");
                IniFile iniCreator = new IniFile(mainPath);
                iniCreator.Write("port", "4296", "server");
                iniCreator.Write("channels", "default", "server");
            }

            IniFile iniHandler = new IniFile(mainPath);
            port = Int32.Parse(iniHandler.Read("port", "server"));
            var channels = iniHandler.Read("channels", "server").Split(',');

            if (channel == null)
            {
                channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, false);
                AtlantisObject obj = new AtlantisObject();

                // Open up a new chatroom for each one defined in the configuration file.
                foreach (var channelObj in channels)
                {
                    obj.addRoom(channelObj.ToString());
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(AtlantisObject), channelObj, WellKnownObjectMode.Singleton);
                    logHandler.WriteLine(LogType.Info, String.Format("Opened up chatroom: {0}", channelObj));
                }

                //RemotingConfiguration.RegisterWellKnownServiceType(typeof(LimboObject), "limbo", WellKnownObjectMode.Singleton);

                logHandler.WriteLine(LogType.Info, String.Format("Running server on port {0}", port));

                Console.ReadLine();
            }
        }

        // Cleanly exit by unregistering all chatrooms before killing off the server process.
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (channel != null)
            {
                ChannelServices.UnregisterChannel(channel);
                channel = null;
                logHandler.WriteLine(LogType.Info, String.Format("Stopped server running on port {0}", port));
            }
        }
    }
}