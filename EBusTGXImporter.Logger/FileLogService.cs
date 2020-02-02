using System;
using System.IO;

namespace EBusTGXImporter.Logger
{
    public sealed class FileLogService : ILogService
    {
        public static string filePath;
        public FileLogService(string logFilePath)
        {
            filePath = logFilePath;
        }

        public void Fatal(string errorMessage)
        {
            WriteLog(errorMessage, "Fatal");
        }

        public void Error(string errorMessage)
        {
            WriteLog(errorMessage, "Error");
        }

        public void Warn(string message)
        {
            WriteLog(message, "Warn");
        }

        public void Info(string message)
        {
            WriteLog(message, "Info");
        }

        public void Debug(string message)
        {
            WriteLog(message, "Debug");
        }

        public static void WriteLog(string strLog, string messageType)
        {
            StreamWriter log = null;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo = null;
            try
            {
                string logFilePath = "";
                logFilePath = filePath + "Log_" + System.DateTime.Today.ToString("MM_dd_yyyy") + "." + "txt";
                logFileInfo = new FileInfo(logFilePath);
                logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                if (!logDirInfo.Exists) logDirInfo.Create();
                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                log = new StreamWriter(fileStream);
                log.WriteLine(messageType + ": " + strLog);
                log.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                log.Dispose();
                fileStream.Dispose();
            }
           
        }
    }
}
