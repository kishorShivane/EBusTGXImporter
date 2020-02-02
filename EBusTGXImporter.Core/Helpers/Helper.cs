using EBusTGXImporter.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EBusTGXImporter.Core.Helpers
{
    public class Helper
    {
        private static ILogService Log;
        EmailHelper emailHelper = null;
        public Helper(ILogService logger)
        {
            Log = logger;
            emailHelper = new EmailHelper(logger);
        }

        public bool IsFileLocked(string filepath)
        {
            FileInfo file = new FileInfo(filepath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                Log.Info("File is locked" + filepath);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public DateTime ConvertToInsertDateString(string date)
        {
            try
            {
                return Convert.ToDateTime(DateTime.ParseExact(date, "ddMMyy", null).ToString("yyyy-MM-dd"));
            }
            catch (Exception)
            {
                Log.Info("Error: ConvertToInsertDateString: Date-" + date);
                throw;
            }
        }

        public DateTime ConvertToInsertDateTimeString(string strDate, string strTime)
        {
            try
            {
                if (strTime.Length < 6) strTime = strTime.PadRight(6, '0');
                string time = DateTime.ParseExact(strTime, "HHmmss", null).ToString("HH:mm:ss");
                return Convert.ToDateTime(DateTime.ParseExact(strDate, "ddMMyy", null).ToString("yyyy-MM-dd") + " " + time);
            }
            catch (Exception)
            {
                Log.Info("Error: ConvertToInsertDateTimeString: Date-" + strDate + " Time-" + strTime);
                throw;
            }
        }

        public DateTime ConvertToInsertDateTimeStringWithOutSeconds(string strDate, string strTime)
        {
            try
            {
                if (strTime.Length < 6) strTime = strTime.PadRight(6, '0');
                string time = DateTime.ParseExact(strTime, "HHmmss", null).ToString("HH:mm");
                return Convert.ToDateTime(DateTime.ParseExact(strDate, "ddMMyy", null).ToString("yyyy-MM-dd") + " " + time);
            }
            catch (Exception)
            {
                Log.Info("Error: ConvertToInsertDateTimeString: Date-" + strDate + " Time-" + strTime);
                throw;
            }
        }

        public string ConvertToInsertTimeString(string strTime)
        {
            var time = "";
            try
            {
                if (strTime.Length < 6) strTime = strTime.PadRight(6, '0');
                time = DateTime.ParseExact(strTime, "HHmmss", null).ToString("HH:mm:ss");
                return time;
            }
            catch (Exception)
            {
                Log.Info("Error: ConvertToInsertTimeString: Time-" + strTime);
                throw;
            }
        }

        public void MoveErrorFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations
                string path = currentpath;
                string file = Path.GetFileName(currentpath);
                //Current Path
                string dirPath1 = Constants.DirectoryPath + @"\" + dbname + @"\" + @"Error\" + DateTime.Now.Year + @"\" + DateTime.Now.ToString("MMMM") + @"\" + DateTime.Now.Day;
                //New Path
                string path2 = dirPath1 + @"\" + file;

                //Checks if path exists and creates it if not
                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                //Checks if path exists and creates it if not
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Constants.DetailedLogging) Log.Info("Moved error file - " + file);
            }
            catch (Exception ex)
            {
                var exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Log.Info("File move to error folder failed:" + exception);
                    Log.Info("File name: " + currentpath + " DBName: " + dbname);
                }
                if (Constants.EnableEmailTrigger) emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
                return;
            }
        }

        public void MoveDateProblemFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations
                string path = currentpath;
                string file = Path.GetFileName(currentpath);
                //Current Path
                string dirPath1 = Constants.DirectoryPath + @"\" + dbname + @"\" + @"DateProblem\" + DateTime.Now.Year + @"\" + DateTime.Now.ToString("MMMM") + @"\" + DateTime.Now.Day;
                //New Path
                string path2 = dirPath1 + @"\" + file;

                //Checks if path exists and creates it if not
                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                //Checks if path exists and creates it if not
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Constants.DetailedLogging) Log.Info("Moved date problem file - " + file);
            }
            catch (Exception ex)
            {
                var exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Log.Info("File move to date problem folder failed:" + exception);
                    Log.Info("File name: " + currentpath + " DBName: " + dbname);
                }
                if (Constants.EnableEmailTrigger) emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
                return;
            }
        }

        public void MoveDuplicateFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations
                string path = currentpath;
                string file = Path.GetFileName(currentpath);
                string dirPath = Constants.DirectoryPath + @"\" + dbname + @"\" + @"Duplicate\" + DateTime.Now.Year + @"\" + DateTime.Now.ToString("MMMM") + @"\" + DateTime.Now.Day;
                string path2 = dirPath + @"\" + file;

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                //Checks if files exists and deletes if they do
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Constants.DetailedLogging) Log.Info("Moved duplicate file - " + file);
                if (Constants.DetailedLogging) Log.Info("Sending duplicate file mail started");
                //if (Constants.EnableEmailTrigger) emailHelper.SendMail(currentpath, dbname, "", EmailType.Duplicate);
                if (Constants.DetailedLogging) Log.Info("Sending duplicate file mail completed");
            }
            catch (Exception ex)
            {
                var exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Log.Info("File move to duplicate folder failed:" + exception);
                    Log.Info("File name: " + currentpath + " DBName: " + dbname);
                }
                if (Constants.EnableEmailTrigger) emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
                return;
            }
        }

        public void MoveSuccessFile(string currentpath, string dbname)
        {
            try
            {
                //Declarations 
                string result = Path.GetFileNameWithoutExtension(currentpath);
                string path = currentpath;
                string file = Path.GetFileName(currentpath);

                string dirPath1 = Constants.DirectoryPath + @"\" + dbname + @"\" + @"Out\" + DateTime.Now.Year + @"\" + DateTime.Now.ToString("MMMM") + @"\" + DateTime.Now.Day;
                string path2 = dirPath1 + @"\" + file;

                //Checks directory or create it
                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }
                //Checks if files exists and deletes if they do
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Constants.DetailedLogging) Log.Info("Moved successful file - " + file);
            }
            catch (Exception ex)
            {
                var exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Log.Info("File move to success folder failed:" + exception);
                    Log.Info("File name: " + currentpath + " DBName: " + dbname);
                }
                if (Constants.EnableEmailTrigger) emailHelper.SendMail(currentpath, dbname, exception, EmailType.Error);
                return;
            }
        }

        public int GetTripBalanceFromProductData(string productData)
        {
            try
            {
                var result = 0;
                if (productData.Length >= 12)
                {
                    var charArrayProdData = productData.ToCharArray();
                    var tripBalance = charArrayProdData[2].ToString() + charArrayProdData[3].ToString();
                    result = Convert.ToInt32(tripBalance, 16);
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetTripBalanceFromProductData: ProductData-" + productData);
                throw;
            }
        }

        public int GetTripRechargedFromProductData(string productData)
        {
            try
            {
                var result = 0;
                if (productData.Length >= 12)
                {
                    var charArrayProdData = productData.ToCharArray();
                    var tripBalance = charArrayProdData[0].ToString() + charArrayProdData[1].ToString();
                    result = Convert.ToInt32(tripBalance, 16);
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetTripRechargedFromProductData: ProductData-" + productData);
                throw;
            }
        }

        public bool IsTransferTransaction(string productData)
        {
            try
            {
                var result = false;
                if (productData.Length >= 12)
                {
                    var charArrayProdData = productData.ToCharArray();
                    var transfer = charArrayProdData[4].ToString() + charArrayProdData[5].ToString();
                    result = (Convert.ToInt32(transfer, 16) > 0) ? true : false;
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: IsTransferTransaction: ProductData-" + productData);
                throw;
            }
        }

        public int GetNonRevenueFromProductData(string productData)
        {
            try
            {
                var result = 0;
                if (productData.Length >= 12)
                {
                    var nonRevenue = productData.Substring(0, 6);
                    result = Convert.ToInt32(nonRevenue);
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetNonRevenueFromProductData: ProductData-" + productData);
                throw;
            }
        }

        public int GetRevenueBalanceFromProductData(string productData)
        {
            try
            {
                var result = 0;
                if (productData.Length >= 12)
                {
                    var revenueBalance = productData.Substring(6, 6);
                    result = Convert.ToInt32(revenueBalance);
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetRevenueBalanceFromProductData: ProductData-" + productData);
                throw;
            }
        }

        public int GetHalfProductData(string productData, bool isFirstHalf)
        {
            try
            {
                var result = 0;
                if (productData.Length >= 12)
                {
                    var revenueBalance = "";
                    if (isFirstHalf) revenueBalance = productData.Substring(0, 6); else revenueBalance = productData.Substring(6, 6);
                    result = Convert.ToInt32(revenueBalance);
                }
                return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetHalfProductData: ProductData-" + productData);
                throw;
            }
        }

        public DateTime? GetSmartCardExipryFromProductDate(string productData)
        {
            try
            {
                DateTime? result = null;
            if (productData.Length >= 12)
            {
                var hexValue = productData.Substring(8, 4);
                var binaryValue = Convert.ToString(Convert.ToInt32(hexValue, 16), 2).PadLeft(16, '0');
                var day = Convert.ToInt32(binaryValue.Substring(0, 5), 2);
                var month = Convert.ToInt32(binaryValue.Substring(5, 4), 2);
                var year = Convert.ToInt32(binaryValue.Substring(9, 7), 2) + 1991;
                try
                {
                    result = new DateTime(year, month, day);
                }
                catch (Exception)
                {
                    result = null;
                }

            }
            return result;
            }
            catch (Exception)
            {
                Log.Info("Error: GetSmartCardExipryFromProductDate: ProductData-" + productData);
                throw;
            }
        }

        public List<string> DirSearch(string sDir)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception)
            {
                if (Constants.DetailedLogging) Log.Error("Failed in DirSearch");
                throw;
            }

            return files;
        }
    }
}
