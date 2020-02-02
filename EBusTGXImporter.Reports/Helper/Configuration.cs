using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EBusTGXImporter.Reports.Helper
{
    public class Configuration
    {
        public static string ADMIN_USER_NAME = ConfigurationManager.AppSettings["AdminUserName"];
        public static string ADMIN_PASSWORD = ConfigurationManager.AppSettings["AdminPassword"];
        public static string CONNECTION_KEY = ConfigurationManager.AppSettings["ConnectionKey"];
    }
}