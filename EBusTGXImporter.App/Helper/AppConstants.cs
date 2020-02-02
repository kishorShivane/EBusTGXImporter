using System;
using System.Configuration;

namespace EBusTGXImporter.App.Helpers
{
    public class AppConstants
    {
        public static string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string EmailTemplate = ConfigurationManager.AppSettings["EmailTemplate"];
        public static bool EnableEmailTrigger = ConfigurationManager.AppSettings["EnableEmailTrigger"] == "true" ? true : false;
        public static string ToEmail = ConfigurationManager.AppSettings["ToEmail"];
        public static string FromEmail = ConfigurationManager.AppSettings["FromEmail"];
        public static string DuplicateEmailSubject = ConfigurationManager.AppSettings["DuplicateEmailSubject"];
        public static string ErrorEmailSubject = ConfigurationManager.AppSettings["ErrorEmailSubject"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static int Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        public static string EmailUserName = ConfigurationManager.AppSettings["EmailUserName"];
        public static string EbusPassword = ConfigurationManager.AppSettings["EbusPassword"];
        public static string DetailedLogging = ConfigurationManager.AppSettings["DetailedLogging"];
        public static bool IgnoreCheckList = ConfigurationManager.AppSettings["IgnoreCheckList"] == "true" ? true : false;
    }
}
