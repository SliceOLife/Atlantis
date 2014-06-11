using System;
using System.IO;

namespace Atlantis.Hub
{
    public enum LogType
    {
        Chat = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Debug = 5
    }

    public class Logger
    {

        // Privates
        private bool isReady = false;
        private StreamWriter swLog;
        private string strLogFile;

        // Constructors
        public Logger(string LogFileName)
        {
            this.strLogFile = LogFileName;
            openFile();
            _writelog("");
            closeFile();
        }


        private void openFile()
        {
            try
            {
                swLog = File.AppendText(strLogFile);
                isReady = true;
            }
            catch
            {
                isReady = false;
            }
        }

        private void closeFile()
        {

            if (isReady)
            {
                try
                {
                    swLog.Close();
                }
                catch
                {

                }
            }
        }

        public static string GetNewLogFilename()
        {
            AppDomain Ad = AppDomain.CurrentDomain;
            return Ad.BaseDirectory + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
        }

        public void WriteLine(LogType logtype, string message)
        {

            string stub = DateTime.Now.ToString("dd-MM-yyyy @ HH:mm:ss");
            switch (logtype)
            {
                case LogType.Chat:
                    stub += " - CHAT: ";
                    break;
                case LogType.Info:
                    stub += " - INFO: ";
                    break;
                case LogType.Warning:
                    stub += " - WARN: ";
                    break;
                case LogType.Error:
                    stub += " - ERROR: ";
                    break;
                case LogType.Debug:
                    stub += " - DEBUG: ";
                    break;
            }
            stub += message;
            openFile();
            _writelog(stub);
            closeFile();
            Console.WriteLine(stub);
        }

        private void _writelog(string msg)
        {
            if (isReady)
            {
                swLog.WriteLine(msg);
            }
            else
            {
                Console.WriteLine("Error cannot write to log file.");
            }
        }
    }
}
