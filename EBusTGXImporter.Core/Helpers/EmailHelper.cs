using EBusTGXImporter.Logger;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace EBusTGXImporter.Core.Helpers
{
    public class EmailHelper
    {
        private static ILogService Log;
        public EmailHelper(ILogService logger)
        {
            Log = logger;
        }

        public void SendMail(string fileName, string customer, string exception, EmailType type)
        {
            string body = "";
            try
            {
                MailAddress mailfrom = null;
                SmtpClient smtp = null;
                MailAddress mailto = null;
                MailMessage newmsg = null;
                using (StreamReader reader = new StreamReader(Constants.EmailTemplate + "EmailTemplate.html"))
                {
                    body = reader.ReadToEnd();
                }

                if (Constants.UseGmailForEmail)
                {
                    smtp = new SmtpClient(Constants.GmailHost, Convert.ToInt32(Constants.GmailPort));
                    smtp.Credentials = new NetworkCredential(Constants.GmailUserName, Constants.GmailPassword);
                    mailfrom = new MailAddress(Constants.GmailFromEmail);
                    mailto = new MailAddress(Constants.ToEmail);
                    newmsg = new MailMessage(mailfrom, mailto);
                    smtp.EnableSsl = true;
                }
                else
                {
                    smtp = new SmtpClient(Constants.Host, Convert.ToInt32(Constants.Port));
                    smtp.Credentials = new NetworkCredential(Constants.EmailUserName, Constants.EbusPassword);
                    mailfrom = new MailAddress(Constants.FromEmail);
                    mailto = new MailAddress(Constants.ToEmail);
                    newmsg = new MailMessage(mailfrom, mailto);
                    smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                }



                var message = GetMessageByEmailType(type);
                string file = Path.GetFileName(fileName);

                body = body.Replace("[*FileName*]", file);
                body = body.Replace("[*ClientName*]", customer);
                if (type == EmailType.Error)
                {
                    body = body.Replace("[*Error*]", "Error Message: <div style='color:red'>" + exception + "</div>");
                    newmsg.Subject = Constants.ErrorEmailSubject;
                }
                else if (type == EmailType.Duplicate)
                {
                    body = body.Replace("[*Error*]", "");
                    newmsg.Subject = Constants.DuplicateEmailSubject;
                }
                else if (type == EmailType.DateProblem)
                {
                    body = body.Replace("[*Error*]", "");
                    newmsg.Subject = Constants.DateProblemEmailSubject;
                }
                body = body.Replace("[*Message*]", message);

                newmsg.IsBodyHtml = true;
                newmsg.Body = body;

              //  System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
              //System.Security.Cryptography.X509Certificates.X509Certificate certificate,
              //System.Security.Cryptography.X509Certificates.X509Chain chain,
              //System.Net.Security.SslPolicyErrors sslPolicyErrors)
              //  {
              //      return true;
              //  };

                smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                Log.Error("Email Sending failed");
                Log.Info("Email Body: " + body);
                Log.Error("Exception - " + JsonConvert.SerializeObject(ex));
                return;
            }
        }

        public string GetMessageByEmailType(EmailType type)
        {
            var result = "";
            switch (type)
            {
                case EmailType.Error:
                    result = "The following error occurred while importing the audit file and moved to Error Folder.";
                    break;
                case EmailType.Duplicate:
                    result = "The following error occurred while importing the audit file and moved to Duplicate Folder.";
                    break;
                case EmailType.DateProblem:
                    result = "The following error occurred while importing the audit file and moved to date problem Folder.";
                    break;
                case EmailType.Default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }

    public enum EmailType
    {
        Error,
        Duplicate,
        Default,
        DateProblem
    }
}
