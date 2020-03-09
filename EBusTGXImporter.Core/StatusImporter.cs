using EBusTGXImporter.Core.Helpers;
using EBusTGXImporter.Core.Interfaces;
using EBusTGXImporter.DataProvider;
using EBusTGXImporter.DataProvider.Models;
using EBusTGXImporter.Logger;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EBusTGXImporter.Core
{
    public class StatusImporter : IImporter
    {
        public static ILogService Logger { get; set; }

        private Helper helper = null;
        private EmailHelper emailHelper = null;
        private DBService dbService = null;
        public static object thisLock = new object();
        public StatusImporter(ILogService logger)
        {
            Logger = logger;
            helper = new Helper(logger);
            emailHelper = new EmailHelper(logger);
            dbService = new DBService(logger);
        }

        public bool PostImportProcessing(string filePath)
        {
            bool result = false;

            return result;
        }

        public bool PreImportProcessing(string filePath)
        {
            bool result = false;
            if (ValidateFile(filePath))
            {
                result = true;
            }

            return result;
        }

        public bool ProcessFile(string filePath)
        {
            //MessageBox.Show("ProcessFile");
            bool result = false;
            string[] splitFilepath = filePath.Split('\\');
            string dbName = splitFilepath[splitFilepath.Length - 3];
            try
            {
                if (Constants.DetailedLogging)
                {
                    Logger.Info("***********************************************************");
                    Logger.Info("Started Import");
                    Logger.Info("***********************************************************");
                }

                DateTime lastModified = System.IO.File.GetLastWriteTime(filePath);
                string previousLine = "";
                using (StreamReader reader = new StreamReader(filePath))
                {
                    if (reader.BaseStream.Length > 1024)
                    {
                        reader.BaseStream.Seek(-1024, SeekOrigin.End);
                    }
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        previousLine = line;
                    }
                }
                TAssetETM asset = null;
                previousLine = previousLine.TrimStart();
                previousLine = previousLine.TrimEnd();
                Logger.Info("Line to be processed: " + previousLine);
                int length = previousLine.Length;
                //00196600000051686401OXFOPT021VCF-ES33EGIA 469ABCETM510000000600000006004589XXXXXXXX2019121318:40:2710.10.10.65
                if (previousLine.Length > 76)
                {
                    //00196600000051686401OXFOPT021VCF-ES33EGIA 469ABCETM510000000600000007004589DtySel672019121318:43:2810.10.10.65
                    asset = new TAssetETM();
                    int ipLength = (length - 99);
                    asset.ETMID = Convert.ToInt32(previousLine.Substring(0, 6));
                    asset.TrayID = Convert.ToInt32(previousLine.Substring(6, 6));
                    asset.ModuleID = Convert.ToInt32(previousLine.Substring(12, 6));
                    asset.ETMConfig = previousLine.Substring(37, 8);
                    asset.TimeBand = previousLine.Substring(69, 6);
                    asset.DutySel = previousLine.Substring(75, 8);
                    asset.TgxIpAddr = previousLine.Substring(99, ipLength);
                    asset.dat_LastUpdate = lastModified;
                }
                else
                {
                    //00698700000053318001EBS00001MCF-0001VCF-ES50EGIA 469ABCCND630000000200000002
                    asset = new TAssetETM
                    {
                        ETMID = Convert.ToInt32(previousLine.Substring(0, 6)),
                        TrayID = Convert.ToInt32(previousLine.Substring(6, 6)),
                        ModuleID = Convert.ToInt32(previousLine.Substring(12, 6)),
                        ETMConfig = previousLine.Substring(44, 8),
                        dat_LastUpdate = lastModified
                    };
                }
                //00249202080853271601OXFOPT021VCF-ES23EGIA 469ABCETM460000000100000001000148

                if (dbService.InsertOrUpdateAssetETM(asset, dbService.DoesRecordExist("TAssetETM", "ETMID", asset.ETMID.ToString(), dbName), dbName))
                {
                    helper.MoveSuccessStatusFile(filePath, dbName);
                    Logger.Info("**************Successfully Processed:" + filePath + "**************");
                }
                else
                {
                    Logger.Error("Issue while inserting or updating Asset ETM record with ETMID: " + asset.ETMID);
                    helper.MoveErrorStatusFile(filePath, dbName);
                }
            }
            catch (Exception ex)
            {
                string exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Logger.Error("Failed in Status ProcessFile");
                    Logger.Error("Exception:" + exception);
                }
                Logger.Error("Exception:" + exception);
                helper.MoveErrorStatusFile(filePath, dbName);
                if (Constants.EnableEmailTrigger)
                {
                    emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                }
                //MessageBox.Show(ex.Message);
                return result;
            }
            return result;
        }

        public bool ValidateFile(string filePath)
        {
            return !helper.IsFileLocked(filePath);
        }
    }
}
