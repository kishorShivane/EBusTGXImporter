using System;
using System.Configuration;

namespace EBusTGXImporter.Core.Helpers
{
    public partial class Constants
    {
        public static string DirectoryPath = ConfigurationManager.AppSettings["DirectoryPath"];
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string EmailTemplate = ConfigurationManager.AppSettings["EmailTemplate"];
        public static bool EnableEmailTrigger = ConfigurationManager.AppSettings["EnableEmailTrigger"] == "true" ? true : false;
        public static string DuplicateEmailSubject = ConfigurationManager.AppSettings["DuplicateEmailSubject"];
        public static string ErrorEmailSubject = ConfigurationManager.AppSettings["ErrorEmailSubject"];
        public static string DateProblemEmailSubject = ConfigurationManager.AppSettings["DateProblemEmailSubject"];
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static int Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        public static string ToEmail = ConfigurationManager.AppSettings["ToEmail"];
        public static string FromEmail = ConfigurationManager.AppSettings["FromEmail"];
        public static string EmailUserName = ConfigurationManager.AppSettings["EmailUserName"];
        public static string EbusPassword = ConfigurationManager.AppSettings["EbusPassword"];
        public static string GmailHost = ConfigurationManager.AppSettings["GmailHost"];
        public static int GmailPort = Convert.ToInt32(ConfigurationManager.AppSettings["GmailPort"]);
        public static string GmailUserName = ConfigurationManager.AppSettings["GmailUserName"];
        public static string GmailPassword = ConfigurationManager.AppSettings["GmailPassword"];
        public static string GmailFromEmail = ConfigurationManager.AppSettings["GmailFromEmail"];
        public static bool UseGmailForEmail = ConfigurationManager.AppSettings["UseGmailForEmail"] == null ? false : ConfigurationManager.AppSettings["UseGmailForEmail"] == "true" ? true : false;
        public static bool DetailedLogging = ConfigurationManager.AppSettings["DetailedLogging"] == "true" ? true : false;
        public static bool IgnoreCheckList = ConfigurationManager.AppSettings["IgnoreCheckList"] == "true" ? true : false;
        public static int DefaultNonRevenueValue = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultNonRevenueValue"]);
    }
}
