using EBusTGXImporter.Core.Helpers;
using EBusTGXImporter.Core.Interfaces;
using EBusTGXImporter.DataProvider;
using EBusTGXImporter.DataProvider.Models;
using EBusTGXImporter.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace EBusTGXImporter.Core
{
    public class XmlImporter : IImporter
    {
        public static ILogService Logger { get; set; }

        private Helper helper = null;
        private EmailHelper emailHelper = null;
        private DBService dbService = null;
        public static object thisLock = new object();
        public XmlImporter(ILogService logger)
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
            bool result = true;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            DateTime todayDate = DateTime.Now;
            int batch = Convert.ToInt32(todayDate.Day.ToString().PadLeft(2, '0') + todayDate.Month.ToString().PadLeft(2, '0') + todayDate.Year.ToString());
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


                XDocument xdoc = XDocument.Load(filePath);
                List<XElement> nodes = xdoc.Root.Elements().ToList();

                #region Check for file completeness
                IEnumerable<XElement> fileClosureNode = nodes.Where((x => x.Attribute("STXID").Value.Equals("82")));
                if (fileClosureNode == null || !fileClosureNode.Any())
                {
                    Logger.Info("No file closure found, Moving file to error folder");
                    helper.MoveErrorFile(filePath, dbName);
                    if (Constants.EnableEmailTrigger)
                    {
                        emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                    }

                    return false;
                }
                #endregion

                #region Check for validity of ExtendedWaybill date
                if (Constants.DetailedLogging)
                { Logger.Info("Check for validity of ExtendedWaybill date - Start"); }
                IEnumerable<XElement> extendedWaybill = nodes.Where((x => x.Attribute("STXID").Value.Equals("122")));
                XElement tempDutyDetail = nodes.Where((x => x.Attribute("STXID").Value.Equals("151"))).FirstOrDefault();
                if (tempDutyDetail != null && extendedWaybill != null && extendedWaybill.Any())
                {
                    bool foundDataProblem = false;
                    DateTime dutyDate = helper.ConvertToInsertDateTimeString(tempDutyDetail.Element("DutyDate").Value, tempDutyDetail.Element("DutyTime").Value);
                    List<XElement> tempList = extendedWaybill.ToList();
                    for (int i = 0; i <= tempList.Count() - 1; i++)
                    {
                        DateTime? tempNextStartDate = null;
                        DateTime startDateTime = helper.ConvertToInsertDateTimeString(tempList[i].Element("StartDate").Value, tempList[i].Element("StartTime").Value);
                        DateTime stopDateTime = helper.ConvertToInsertDateTimeString(tempList[i].Element("StopDate").Value, tempList[i].Element("StopTime").Value);
                        if ((i + 1) <= tempList.Count() - 1)
                        {
                            tempNextStartDate = helper.ConvertToInsertDateTimeString(tempList[i + 1].Element("StartDate").Value, tempList[i + 1].Element("StartTime").Value);
                        }

                        if (DateTime.Compare(startDateTime, stopDateTime) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (i == 0 && DateTime.Compare(startDateTime, dutyDate) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (tempNextStartDate != null && DateTime.Compare(tempNextStartDate.Value, stopDateTime) >= 0)
                        {
                            foundDataProblem = true;
                        }

                        if (foundDataProblem)
                        {
                            Logger.Info("Date Problem found, Moving file to error folder.");
                            helper.MoveDateProblemFile(filePath, dbName);
                            if (Constants.EnableEmailTrigger)
                            {
                                emailHelper.SendMail(filePath, dbName, "", EmailType.DateProblem);
                            }

                            return false;
                        }
                    }
                }
                if (Constants.DetailedLogging)
                { Logger.Info("Check for validity of ExtendedWaybill date - End"); }
                #endregion

                #region Initialize Variables
                int latestModuleID = 0;
                int latestModulesID = 0;
                int latestDutyID = 0;
                int latestJourneyID = 0;
                int latestStageID = 0;
                int latestTransID = 0;
                int latestSCTransID = 0;
                int latestPosTransID = 0;
                int latestInspectorID = 0;
                int latestAuditFileStatus = 0;
                int latestdiagnosticRecord = 0;
                int latestBusChecklistID = 0;

                Module moduleDetail = null;
                Modules modulesDetail = null;
                List<Waybill> wayBillDetails = new List<Waybill>();
                Duty dutyDetail = null;
                AuditFileStatus auditFileDetail = null;
                List<AuditFileStatus> auditFileDetails = new List<AuditFileStatus>();
                DiagnosticRecord diagnosticRecord = null;
                List<DiagnosticRecord> diagnosticRecords = new List<DiagnosticRecord>();
                Journey journeyDetail = null;
                List<Journey> journeyDetails = new List<Journey>();
                List<Stage> stageDetails = new List<Stage>();
                Stage stageDetail = null;
                List<TempStage> tempStageDetails = new List<TempStage>();
                TempStage tempStage = null;
                Trans transDetail = null;
                GPSCoordinate gPSCoordinate = null;
                List<GPSCoordinate> gPSCoordinates = new List<GPSCoordinate>();
                List<Trans> transDetails = new List<Trans>();
                PosTrans posTransDetail = null;
                List<PosTrans> posTransDetails = new List<PosTrans>();
                SCTrans scTransDetail = null;
                List<SCTrans> scTransDetails = new List<SCTrans>();
                Staff staffDetail = null;
                Inspector inspectorDetail = null;
                List<Inspector> inspectorDetails = new List<Inspector>();
                BusChecklist busChecklistDetail = null;
                #endregion

                XAttribute way6OrTGXCheck = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault().Attribute("Position");

                if (way6OrTGXCheck != null)
                {
                    #region TGX file processing

                    #region Process Module Information

                    XElement node18 = nodes.Where(x => x.Attribute("STXID").Value.Equals("18")).FirstOrDefault();
                    if (node18 != null)
                    {

                        //Process Module details
                        moduleDetail = new Module
                        {
                            id_Module = latestModuleID,
                            int4_ModuleID = (int)node18.Element("ModuleESN"),
                            int4_DriverNo = (int)node18.Element("DriverNumber"),
                            int4_NetSVCash = 0,
                            int4_GrossSVCash = 0,
                            int4_NetSVTickets = 0,
                            int4_AnulledSVTickets = 0,
                            int4_GrossSVTickets = 0,
                            int4_SignOnSeq = 0,
                            int4_SignOffSeq = 0,
                            int_DutyControl = 0,
                            int4_OnReaderID = 12,
                            int4_OffReaderID = 12,
                            int4_CancelledCash = 0,
                            int4_AnulledSVCash = 0,
                            int4_TotalPasses = 0,
                            int4_CancelledTickets = 0,
                            int4_ModuleAnnulCash = 0,
                            int4_ModuleAnnulCount = 0,
                            int4_AnnulledSVCash = null,
                            int4_AnnulledSVTickets = null,
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node18.Element("SignOnDate"));
                        moduleDetail.dat_SignOnDate = date;
                        moduleDetail.dat_SignOnTime = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOnDate"), (string)node18.Element("SignOnTime"));
                        moduleDetail.int4_OffReaderID = (int)node18.Element("HomeDepotID");
                        date = helper.ConvertToInsertDateString((string)node18.Element("SignOffDate"));
                        moduleDetail.dat_SignOffDate = date;
                        DateTime time = helper.ConvertToInsertDateTimeString((string)node18.Element("SignOffDate"), (string)node18.Element("SignOffTime"));
                        moduleDetail.dat_SignOfftime = time;
                        moduleDetail.int4_NetCash = (int)node18.Element("TotalNetCashPaid");
                        moduleDetail.int4_NetTickets = (int)node18.Element("TotalNetCashPassengers");
                        moduleDetail.int4_GrossCash = (int)node18.Element("TotalGrossCash");
                        moduleDetail.int4_GrossTickets = (int)node18.Element("TotalGrossCashPassengers");
                        moduleDetail.int4_ModulePasses = 0;
                        moduleDetail.int4_ModuleNonRevenue = 0;
                        moduleDetail.int4_ModuleTransfer = 0;
                        moduleDetail.id_BatchNo = batch;
                        moduleDetail.int_Moduletype = (int)node18.Element("ModuleType");

                        if (!dbService.CheckForExistingModules(moduleDetail.int4_ModuleID, dbName))
                        {
                            //Process Modules details
                            modulesDetail = new Modules()
                            {
                                id_Module = latestModulesID,
                                str6_Modules = moduleDetail.int4_ModuleID.ToString(),
                                int_ModuleStatus = 1,
                                int_ModuleType = 0,
                                int_ModuleSerial = 0,
                                dat_FirstUsed = DateTime.Now,
                                dat_LastUsed = DateTime.Now
                            };
                        }
                    }


                    #endregion

                    #region Process Waybill Information

                    IEnumerable<XElement> nodes122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122"));
                    if (nodes122 != null && nodes122.Any())
                    {
                        nodes122.ToList().ForEach(x =>
                        {
                            XElement thisNode = x;
                            wayBillDetails.Add(new Waybill()
                            {
                                ModuleID = (int)thisNode.Element("ModuleNumber"),
                                id_Module = moduleDetail.id_Module,
                                dat_Start = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StartDate"), (string)thisNode.Element("StartTime")),
                                dat_End = helper.ConvertToInsertDateTimeString((string)thisNode.Element("StopDate"), (string)thisNode.Element("StopTime")),
                                int4_Operator = (int)thisNode.Element("OperatorNumber"),
                                str8_BusID = (string)thisNode.Element("BusNumber"),
                                str6_EtmID = (string)thisNode.Element("ETMNumber"),
                                int4_EtmGrandTotal = (int)thisNode.Element("ETMTotal"),
                                int4_Revenue = (int)thisNode.Element("DutyRevenue"),
                                dat_Match = null,
                                dat_Actual = null,
                                Imported_Operator = null,
                                Imported_BusID = null,
                                Imported_ETMID = null,
                                Imported_GT = null,
                                Imported_Revenue = null
                            });
                        });
                    }
                    #endregion

                    #region Process Duty Information   

                    XElement node151 = nodes.Where(x => x.Attribute("STXID").Value.Equals("151")).FirstOrDefault();
                    XElement node122 = nodes.Where(x => x.Attribute("STXID").Value.Equals("122")).FirstOrDefault();
                    IEnumerable<XElement> node153 = nodes.Where(x => x.Attribute("STXID").Value.Equals("153"));
                    XElement node154 = nodes.Where(x => x.Attribute("STXID").Value.Equals("154")).FirstOrDefault();
                    XElement node155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155")).FirstOrDefault();

                    if (node151 != null && node154 != null && node155 != null)
                    {
                        dutyDetail = new Duty
                        {
                            id_Duty = latestDutyID,
                            id_Module = moduleDetail.id_Module,
                            int4_DutyID = (int)node151.Element("DutyNo"),
                            int4_OperatorID = (int)node151.Element("DriverNumber"),
                            str_ETMID = (string)node122?.Element("ETMNumber"),
                            int4_GTValue = (int)node151.Element("ETMCashTotal"),
                            int4_NextTicketNumber = (int)node151.Element("NextTicketNo"),
                            int4_DutySeqNum = (int)node151.Element("DutySeqNo.")
                        };
                        DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                        dutyDetail.dat_DutyStartDate = date;
                        dutyDetail.dat_DutyStartTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node151.Element("DutyDate"), (string)node151.Element("DutyTime"));
                        dutyDetail.dat_DutyStopTime = helper.ConvertToInsertDateTimeStringWithOutSeconds((string)node154.Element("SignOffDate"), (string)node154.Element("SignOffTime"));
                        dutyDetail.str_BusID = node151.Element("FleetID").Value.TrimStart('0') == "" ? "0" : node151.Element("FleetID").Value.TrimStart('0');
                        dutyDetail.int4_DutyRevenue = (int)node154.Element("DutyCashTotal");
                        dutyDetail.int4_DutyTickets = (int)node154.Element("DutyTicketTotal");
                        dutyDetail.int4_DutyPasses = (int)node154.Element("DutyPassTotal");
                        dutyDetail.int4_DutyNonRevenue = 0;
                        dutyDetail.int4_DutyTransfer = 0;
                        dutyDetail.str_FirstRouteID = (string)node155.Element("RouteVariantNo");
                        dutyDetail.int2_FirstJourneyID = (short)node155.Element("JourneyNo");
                        if (node153 != null)
                        {
                            node153.ToList().ForEach(x =>
                            {
                                switch (x?.Element("VersionType").Value.ToString())
                                {
                                    case "255":
                                        dutyDetail.str_EpromVersion = (string)x.Element("Version");
                                        break;
                                    case "256":
                                        dutyDetail.str_SpecialVersion = (string)x.Element("Version");
                                        break;
                                    case "264":
                                        dutyDetail.str_OperatorVersion = (string)x.Element("Version");
                                        break;
                                    default:
                                        break;
                                }
                            });
                        }
                        dutyDetail.int4_DutyAnnulCash = 0;
                        dutyDetail.int4_DutyAnnulCount = 0;
                        dutyDetail.int4_Reconstructed = 0;
                    }

                    if (dbService.CheckForDutyDuplicates(dutyDetail.int4_OperatorID.Value, dutyDetail.dat_DutyStartTime.Value, dutyDetail.dat_DutyStopTime.Value, dbName))
                    {
                        Logger.Info("Error: Duplicate file found");
                        helper.MoveDuplicateFile(filePath, dbName);
                        return false;
                    }
                    #endregion

                    #region Process AuditFileStatus Information

                    IEnumerable<XElement> nodes125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125"));
                    if (nodes125 != null && nodes125.Count() == 2)
                    {
                        auditFileDetail = new AuditFileStatus();

                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[1];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = (int)nextnode.Element("AuditStatus");
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)nextnode.Element("Time"));
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                    }
                    else if (nodes125.Count() == 1)
                    {
                        auditFileDetail = new AuditFileStatus();
                        string lastEndOfJourneyTime = nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault() == null ? "000000" : nodes.Where(x => x.Attribute("STXID").Value.Equals("156")).LastOrDefault().Element("JourneyStopTime").Value;
                        XElement thisNode = nodes125.ToList()[0];
                        XElement nextnode = nodes125.ToList()[0];
                        auditFileDetail.Id_Status = latestAuditFileStatus;
                        auditFileDetail.id_duty = dutyDetail.id_Duty;
                        auditFileDetail.DriverAuditStatus1 = (int)thisNode.Element("AuditStatus");
                        auditFileDetail.DriverNumber1 = (int)thisNode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber1 = (string)thisNode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus1DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisNode.Element("Time"));
                        auditFileDetail.DriverAuditStatus2 = 02;
                        auditFileDetail.DriverNumber2 = (int)nextnode.Element("DriverNumber");
                        auditFileDetail.DriverCardSerialNumber2 = (string)nextnode.Element("DriverCardSerialNo");
                        auditFileDetail.DriverStatus2DateTime = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), lastEndOfJourneyTime);
                        auditFileDetail.DutySignOffMode = (int)node154.Element("SignOffMode");
                        auditFileDetail.RecordModified = todayDate;
                        auditFileDetail.AuditFileName = Path.GetFileName(filePath);
                        auditFileDetails.Add(auditFileDetail);
                        Logger.Info("AuditFileStatus Correction: No AuditFileStatus node found at the end, data is auto corrected");
                    }
                    else
                    {
                        Logger.Info("Error: No AuditFileStatus node found, Moving file to error folder");
                        helper.MoveErrorFile(filePath, dbName);
                        if (Constants.EnableEmailTrigger)
                        {
                            emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                        }

                        return false;
                    }

                    #endregion

                    #region Process Diagnostic Record

                    IEnumerable<XElement> nodes50 = nodes.Where(x => x.Attribute("STXID").Value.Equals("50"));
                    if (nodes50 != null && nodes50.Any())
                    {
                        nodes50.ToList().ForEach(x =>
                        {
                            diagnosticRecord = new DiagnosticRecord();
                            XElement thisRecord = x;
                            diagnosticRecord.Id_DiagnosticRecord = latestdiagnosticRecord;
                            diagnosticRecord.Id_Status = latestAuditFileStatus;
                            diagnosticRecord.TSN = (string)thisRecord.Element("TSN");
                            diagnosticRecord.EquipmentType = (string)thisRecord.Element("EquipmentType");
                            diagnosticRecord.DiagCode = (string)thisRecord.Element("DiagCode");
                            diagnosticRecord.DiagInfo = (string)thisRecord.Element("DiagInfo");
                            diagnosticRecord.Time = helper.ConvertToInsertTimeString((string)thisRecord.Element("Time"));
                            diagnosticRecords.Add(diagnosticRecord);
                            latestdiagnosticRecord++;
                        });
                    }
                    #endregion

                    #region Process Journey Information


                    IEnumerable<XElement> nodes156 = nodes.Where(x => x.Attribute("STXID").Value.Equals("156"));
                    IEnumerable<XElement> nodes155 = nodes.Where(x => x.Attribute("STXID").Value.Equals("155"));
                    if (nodes156 != null && nodes155 != null)
                    {
                        nodes155.ToList().ForEach(x =>
                        {
                            XElement startNode155 = x;
                            XElement endNode156 = nodes156.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(x.Attribute("Position").Value)).OrderBy(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();
                            List<XElement> eachJourneyNodes = nodes.Where(i => Convert.ToInt32(i.Attribute("Position").Value) > Convert.ToInt32(startNode155.Attribute("Position").Value) && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(endNode156.Attribute("Position").Value)).ToList();
                            if (endNode156 == null)
                            {
                                Logger.Info("Error: No end of journey node found for journey node with position-" + x.Attribute("Position").Value + ", Moving file to error folder");
                                helper.MoveErrorFile(filePath, dbName);
                                if (Constants.EnableEmailTrigger)
                                {
                                    emailHelper.SendMail(filePath, dbName, "", EmailType.Error);
                                }

                                return;
                            }
                            journeyDetail = new Journey
                            {
                                id_Journey = latestJourneyID,
                                id_Duty = dutyDetail.id_Duty,
                                //id_Module = moduleDetail.id_Module,
                                str_RouteID = (string)startNode155.Element("RouteVariantNo"),
                                int2_JourneyID = (short)startNode155.Element("JourneyNo"),
                                int2_Direction = (short)startNode155.Element("Direction")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                            journeyDetail.dat_JourneyStartDate = date;
                            journeyDetail.dat_JourneyStartTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)startNode155.Element("StartTime"));
                            journeyDetail.dat_JourneyStopTime = helper.ConvertToInsertDateTimeString((string)endNode156.Element("JourneyStopDate"), (string)endNode156.Element("JourneyStopTime"));
                            journeyDetail.int4_Distance = 0;
                            journeyDetail.int4_JourneyRevenue = (int)endNode156.Element("JourneyCashTotal");
                            journeyDetail.int4_JourneyTickets = (int)endNode156.Element("JourneyTicketTotal");
                            journeyDetail.int4_JourneyPasses = 0;
                            journeyDetail.int4_JourneyNonRevenue = 0;
                            journeyDetail.int4_JourneyTransfer = 0;
                            journeyDetail.dat_JourneyMoveTime = null;
                            journeyDetail.dat_JourneyArrivalTime = null;
                            journeyDetail.int4_GPSDistance = null;
                            journeyDetail.int4_DBDistance = 0;
                            journeyDetails.Add(journeyDetail);

                            #region Process Stage Information

                            IEnumerable<XElement> nodes113 = eachJourneyNodes.Where(i => i.Attribute("STXID").Value.Equals("113"));

                            if (nodes113 != null)
                            {
                                List<XElement> listNodes113 = nodes113.ToList();
                                int count = listNodes113.Count();
                                for (int i = 0; i < count; i++)
                                {
                                    XElement thisNode113 = listNodes113[i];
                                    stageDetail = new Stage();
                                    tempStage = new TempStage();
                                    stageDetail.id_Stage = latestStageID;
                                    stageDetail.id_Journey = journeyDetail.id_Journey;
                                    stageDetail.id_Duty = dutyDetail.id_Duty;
                                    stageDetail.int2_StageID = (short)thisNode113.Element("BoardingStage");
                                    date = helper.ConvertToInsertDateString((string)startNode155.Element("StartDate"));
                                    stageDetail.dat_StageDate = date;
                                    stageDetail.dat_StageTime = helper.ConvertToInsertDateTimeString((string)startNode155.Element("StartDate"), (string)thisNode113.Element("Time"));
                                    stageDetail.int4_Data1 = (int)thisNode113.Element("StageChangeIndicator");
                                    stageDetails.Add(stageDetail);

                                    tempStage.id_Stage = latestStageID;
                                    tempStage.TSN = (string)thisNode113.Element("TSN");
                                    tempStage.RecordedTime = (string)thisNode113.Element("Time");
                                    tempStageDetails.Add(tempStage);

                                    latestStageID++;

                                    #region Process Trans Information
                                    int nextPosition = 0;
                                    if ((i + 1) < count)
                                    {
                                        nextPosition = Convert.ToInt32(listNodes113[i + 1].Attribute("Position").Value);
                                    }
                                    else
                                    {
                                        nextPosition = Convert.ToInt32(endNode156.Attribute("Position").Value);
                                    }

                                    List<XElement> eachStageNodes = eachJourneyNodes.Where(j => Convert.ToInt32(j.Attribute("Position").Value) > Convert.ToInt32(thisNode113.Attribute("Position").Value) && Convert.ToInt32(j.Attribute("Position").Value) < nextPosition).ToList();

                                    IEnumerable<XElement> cashTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("157"));
                                    IEnumerable<XElement> smartCardTransNodes = eachStageNodes.Where(j => j.Attribute("STXID").Value.Equals("188"));

                                    if ((cashTransNodes != null && cashTransNodes.Any()) || (smartCardTransNodes != null && smartCardTransNodes.Any()))
                                    {
                                        cashTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            transDetail.id_Trans = latestTransID;
                                            transDetail.id_SCTrans = null;
                                            transDetail.id_Stage = stageDetail.id_Stage;
                                            transDetail.id_Journey = journeyDetail.id_Journey;
                                            transDetail.id_Duty = dutyDetail.id_Duty;
                                            transDetail.id_Module = moduleDetail.id_Module;
                                            transDetail.int2_BoardingStageID = stageDetail.int2_StageID;
                                            transDetail.int2_AlightingStageID = (short)thisTrans.Element("StageNo");
                                            transDetail.int2_Class = Convert.ToInt16(ticketType, 16);
                                            transDetail.int4_Fare = (int)thisTrans.Element("Fare");
                                            transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                            transDetail.int4_NonRevenue = 0;

                                            switch (ticketType)
                                            {
                                                case "0011":
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    break;
                                                case "0021":
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    break;
                                                case "0012":
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    break;
                                                case "0041":
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    break;
                                                case "0013":
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    break;
                                                case "00B2":
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            transDetail.int2_PassCount = 0;

                                            transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                            transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                            transDetail.int2_AnnulCount = null;
                                            transDetail.int4_AnnulCash = null;
                                            transDetails.Add(transDetail);
                                            latestTransID++;
                                        });

                                        #region SmartCard Transaction
                                        smartCardTransNodes.ToList().ForEach(t =>
                                        {
                                            XElement thisTrans = t;
                                            transDetail = new Trans();
                                            string ticketType = t.Element("TicketType").Value.Trim();
                                            string productData = (string)thisTrans.Element("Product1Data");
                                            transDetail.id_Trans = latestTransID;
                                            transDetail.id_SCTrans = latestSCTransID;
                                            transDetail.id_Stage = stageDetail.id_Stage;
                                            transDetail.id_Journey = journeyDetail.id_Journey;
                                            transDetail.id_Duty = dutyDetail.id_Duty;
                                            transDetail.id_Module = moduleDetail.id_Module;
                                            transDetail.int2_BoardingStageID = stageDetail.int2_StageID;
                                            transDetail.int2_AlightingStageID = (short)thisTrans.Element("StageNo");
                                            transDetail.dat_TransDate = helper.ConvertToInsertDateString((string)thisTrans.Element("IssueDate"));
                                            transDetail.dat_TransTime = helper.ConvertToInsertDateTimeString((string)thisTrans.Element("IssueDate"), (string)thisTrans.Element("IssueTime"));
                                            string serialNumber = (string)thisTrans.Element("ESN");
                                            transDetail.str_SerialNumber = serialNumber;
                                            transDetail.int2_AnnulCount = 0;
                                            transDetail.int4_AnnulCash = 0;
                                            //transDetail.id_SCTrans = null;

                                            #region GPS CO-Ordinates
                                            if ((t.Element("Latitude") != null && t.Element("Longitude") != null) && (t.Element("Latitude").Value.Trim() != string.Empty && t.Element("Longitude").Value.Trim() != string.Empty))
                                            {
                                                string latitude = t.Element("Latitude").Value.Trim();
                                                string longitude = t.Element("Longitude").Value.Trim();
                                                if (latitude.Length > 9 || longitude.Length > 9)
                                                {
                                                    latitude = latitude.TrimStart('0');
                                                    longitude = longitude.TrimStart('0');
                                                }
                                                gPSCoordinate = new GPSCoordinate
                                                {
                                                    id_Trans = latestTransID,
                                                    id_Stage = stageDetail.id_Stage,
                                                    id_Journey = journeyDetail.id_Journey,
                                                    id_Duty = dutyDetail.id_Duty,
                                                    id_Module = moduleDetail.id_Module,
                                                    Latitude = latitude,
                                                    LatDegree = Convert.ToInt32(latitude.Substring(0, 2)),
                                                    LatMinutes = Convert.ToInt32(latitude.Substring(2, 2)),
                                                    LatSeconds = (Convert.ToDecimal(latitude.Substring(5, 3)) / 1000) * 60,
                                                    LatDir = latitude.Substring(8, 1),
                                                    Longitude = longitude,
                                                    LongDegree = Convert.ToInt32(longitude.Substring(0, 2)),
                                                    LongMinutes = Convert.ToInt32(longitude.Substring(2, 2)),
                                                    LongSeconds = (Convert.ToDecimal(longitude.Substring(5, 3)) / 1000) * 60,
                                                    LongDir = longitude.Substring(8, 1)
                                                };
                                                gPSCoordinates.Add(gPSCoordinate);
                                            }
                                            #endregion

                                            #region Process Ticket Type 
                                            int nonRevenue = 0;
                                            switch (ticketType)
                                            {
                                                case "2328":
                                                    //AdulT MJ Cancelation
                                                    transDetail.int2_Class = 995;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Adult MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 999;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;

                                                    break;
                                                case "2329":
                                                    //Scholar MJ Transfer
                                                    transDetail.int2_Class = 996;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Scholar MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 997;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "232B":
                                                    //Disabled MJ Cancelation
                                                    transDetail.int2_Class = 994;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_RevenueBal = 0;
                                                    if (!helper.IsTransferTransaction(productData))
                                                    {
                                                        //Disabled MJ Transfer
                                                        nonRevenue = dbService.GetNonRevenueFromPosTransTable(serialNumber, dbName);
                                                        transDetail.int2_Class = 993;
                                                        transDetail.int2_PassCount = 1;
                                                        transDetail.int2_Transfers = 0;
                                                    }
                                                    transDetail.int4_NonRevenue = nonRevenue;
                                                    break;
                                                case "03F9":
                                                    //Stored Value Adult Cancelation
                                                    transDetail.int2_Class = 701;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "03FA":
                                                    //Stored Value Pensioner Cancelation
                                                    transDetail.int2_Class = 703;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "03FB":
                                                    //Stored Value Penalty Cancelation
                                                    transDetail.int2_Class = 708;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "0409":
                                                    //Stored Value Child Cancelation
                                                    transDetail.int2_Class = 702;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "040A":
                                                    //Stored Value Scholar Cancelation
                                                    transDetail.int2_Class = 704;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "0429":
                                                    //Stored Value Parcel 1
                                                    transDetail.int2_Class = 705;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "27C2":
                                                    //Stored Value Transfer
                                                    transDetail.int2_Class = 709;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "049A":
                                                    //Stored Value Transfer
                                                    transDetail.int2_Class = 709;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 1;
                                                    transDetail.int4_NonRevenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "042A":
                                                    //Stored Value Parcel 2
                                                    transDetail.int2_Class = 706;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "042B":
                                                    //Stored Value Parcel 3
                                                    transDetail.int2_Class = 707;
                                                    transDetail.int4_Revenue = 0;
                                                    transDetail.int2_TicketCount = 0;
                                                    transDetail.int2_PassCount = 1;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_NonRevenue = helper.GetNonRevenueFromProductData(productData);
                                                    transDetail.int4_RevenueBal = helper.GetRevenueBalanceFromProductData(productData);
                                                    break;
                                                case "2715":
                                                    //Adult MJ 10 Recharge
                                                    transDetail.int2_Class = 711;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2718":
                                                    //Adult MJ 12 Recharge
                                                    transDetail.int2_Class = 712;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2727":
                                                    //Adult MJ 14 Recharge
                                                    transDetail.int2_Class = 713;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271B":
                                                    //Adult MJ 40 Recharge
                                                    transDetail.int2_Class = 714;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2722":
                                                    //Adult MJ 44 Recharge
                                                    transDetail.int2_Class = 715;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271E":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 716;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2723":
                                                    //Adult MJ 52 Recharge
                                                    transDetail.int2_Class = 717;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B18":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 718;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2B19":
                                                    //Adult MJ 48 Recharge
                                                    transDetail.int2_Class = 719;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2716":
                                                    //Scholar MJ 10 Recharge
                                                    transDetail.int2_Class = 721;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2717":
                                                    //Scholar MJ 10 Recharge
                                                    transDetail.int2_Class = 722;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "271C":
                                                    //Scholar MJ 44 Recharge
                                                    transDetail.int2_Class = 722;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = 0;
                                                    posTransDetail.TripsRecharged = helper.GetTripRechargedFromProductData(productData);
                                                    break;
                                                case "2729":
                                                    //SV 10 Recharge
                                                    transDetail.int2_Class = 752;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2726":
                                                    //SV 10 Recharge
                                                    transDetail.int2_Class = 741;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2728":
                                                    //SV 20 Recharge
                                                    transDetail.int2_Class = 742;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2714":
                                                    //SV 50 Recharge
                                                    transDetail.int2_Class = 743;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "272A":
                                                    //SV 100 Recharge
                                                    transDetail.int2_Class = 753;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2710":
                                                    //SV 100 Recharge
                                                    transDetail.int2_Class = 744;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "271F":
                                                    //SV 200 Recharge
                                                    transDetail.int2_Class = 748;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2711":
                                                    //SV 200 Recharge
                                                    transDetail.int2_Class = 745;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2712":
                                                    //SV 300 Recharge
                                                    transDetail.int2_Class = 746;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2713":
                                                    //SV 400 Recharge
                                                    transDetail.int2_Class = 747;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2AF9":
                                                    //Adult MJ Deposit
                                                    transDetail.int2_Class = 731;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    break;
                                                case "2AFA":
                                                    //Scholar MJ Deposit
                                                    transDetail.int2_Class = 732;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    break;
                                                case "2AFE":
                                                    transDetail.int2_Class = 733;
                                                    transDetail.int4_Revenue = helper.GetHalfProductData(productData, true);
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = helper.GetHalfProductData(productData, false);
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    posTransDetail.AmountRecharged = helper.GetHalfProductData(productData, true); ;
                                                    posTransDetail.TripsRecharged = 0;
                                                    scTransDetail = MapTransToScTrans(transDetail);
                                                    scTransDetail.id_SCTrans = latestSCTransID;
                                                    break;
                                                case "2B17":
                                                    transDetail.int2_Class = 751;
                                                    transDetail.int4_Revenue = (int)thisTrans.Element("Fare");
                                                    transDetail.int4_NonRevenue = 0;
                                                    transDetail.int2_TicketCount = 1;
                                                    transDetail.int2_PassCount = 0;
                                                    transDetail.int2_Transfers = 0;
                                                    transDetail.int4_RevenueBal = 0;
                                                    posTransDetail = MapTransToPosTrans(transDetail);
                                                    posTransDetail.id_PosTrans = latestPosTransID;
                                                    break;
                                                default:
                                                    break;
                                            }
                                            #endregion

                                            if (scTransDetail != null)
                                            {
                                                scTransDetails.Add(scTransDetail);
                                                latestSCTransID++;
                                                scTransDetail = null;
                                            }

                                            if (posTransDetail != null)
                                            {
                                                posTransDetails.Add(posTransDetail);
                                                latestPosTransID++;
                                                posTransDetail = null;
                                            }

                                            if (transDetail != null && ticketType.Trim() != "2AF8")
                                            {
                                                transDetails.Add(transDetail);
                                                latestTransID++;
                                                transDetail = null;
                                            }
                                        });
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            latestJourneyID++;
                        });
                    }
                    #endregion

                    #region Process Staff Information

                    if (!dbService.DoesRecordExist("Staff", "int4_StaffID", dutyDetail.int4_OperatorID.Value, dbName))
                    {

                        XElement node125 = nodes.Where(x => x.Attribute("STXID").Value.Equals("125")).FirstOrDefault();
                        if (node151 != null && node125 != null)
                        {
                            staffDetail = new Staff
                            {
                                int4_StaffID = dutyDetail.int4_OperatorID.Value
                            };
                            staffDetail.str50_StaffName = "New Staff" + " - " + staffDetail.int4_StaffID;
                            staffDetail.bit_InUse = true;
                            staffDetail.int4_StaffTypeID = 5;
                            staffDetail.int4_StaffSubTypeID = 1;
                            staffDetail.dat_RecordMod = DateTime.Now;
                            //var runningBoard = dutyDetail.str_OperatorVersion;
                            staffDetail.str4_LocationCode = "0001";//runningBoard.Substring(runningBoard.Length - 4, 4);
                            staffDetail.str2_LocationCode = null;
                        }
                    }
                    #endregion

                    #region Process Inspector Information
                    IEnumerable<XElement> nodes34 = nodes.Where(x => x.Attribute("STXID").Value.Equals("34"));
                    if (nodes34 != null)
                    {
                        nodes34.ToList().ForEach(x =>
                        {
                            XElement thisInspector = x;
                            XElement inspectorStage = nodes.Where(i => i.Attribute("STXID").Value.Equals("113") && Convert.ToInt32(i.Attribute("Position").Value) < Convert.ToInt32(thisInspector.Attribute("Position").Value)).OrderByDescending(i => Convert.ToInt32(i.Attribute("Position").Value)).FirstOrDefault();
                            int tempStageID = tempStageDetails.Where(i => i.TSN.Equals(inspectorStage.Element("TSN").Value) && i.RecordedTime.Equals(inspectorStage.Element("Time").Value)).FirstOrDefault().id_Stage;
                            inspectorDetail = new Inspector
                            {
                                id_Inspector = latestInspectorID,
                                id_Stage = tempStageID,
                                id_InspectorID = (int)thisInspector.Element("InspectorNo")
                            };
                            DateTime date = helper.ConvertToInsertDateString((string)node151.Element("DutyDate"));
                            inspectorDetail.datTimeStamp = helper.ConvertToInsertDateTimeString((string)node151.Element("DutyDate"), (string)thisInspector.Element("Time"));
                            inspectorDetails.Add(inspectorDetail);
                            latestInspectorID++;
                        });

                    }
                    #endregion
                    #endregion
                }

                #region Update Dependencies

                if (journeyDetails.Any())
                {
                    journeyDetails.ForEach(x =>
                    {
                        x.int4_JourneyTickets = transDetails.Where(i => i.id_Journey == x.id_Journey && i.int2_TicketCount == 1).Count();
                        x.int4_JourneyPasses = transDetails.Where(i => i.id_Journey == x.id_Journey && i.int2_PassCount == 1).Count();
                        x.int4_JourneyNonRevenue = transDetails.Where(i => i.id_Journey == x.id_Journey).Sum(i => i.int4_NonRevenue ?? 0);
                        x.int4_JourneyTransfer = transDetails.Where(i => i.id_Journey == x.id_Journey && i.int2_Transfers == 1).Count();
                    });
                }

                if (dutyDetail != null && journeyDetails.Any())
                {
                    dutyDetail.int4_DutyTickets = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyTickets ?? 0);
                    dutyDetail.int4_DutyPasses = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyPasses ?? 0);
                    dutyDetail.int4_DutyNonRevenue = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyNonRevenue ?? 0);
                    dutyDetail.int4_DutyTransfer = journeyDetails.Where(i => i.id_Duty == dutyDetail.id_Duty).Sum(i => i.int4_JourneyTransfer ?? 0);
                }

                if (moduleDetail != null && dutyDetail != null)
                {
                    List<Duty> dutyDetails = new List<Duty>() { dutyDetail };
                    moduleDetail.int4_ModulePasses = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyPasses ?? 0);
                    moduleDetail.int4_ModuleNonRevenue = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyNonRevenue ?? 0);
                    moduleDetail.int4_ModuleTransfer = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTransfer ?? 0);
                    moduleDetail.int4_ModuleTickets = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTickets ?? 0);
                    moduleDetail.int4_NetTickets = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTickets ?? 0);
                    moduleDetail.int4_GrossTickets = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTickets ?? 0);
                    if (posTransDetails != null && posTransDetails.Any())
                    {
                        moduleDetail.int4_ModuleTickets = dutyDetails.Where(i => i.id_Module == moduleDetail.id_Module).Sum(i => i.int4_DutyTickets ?? 0);
                    }
                }

                #endregion

                #region DB Insertion Section
                int fallBackCount = 0;
            FALLBACK:
                try
                {

                    lock (thisLock)
                    {
                        latestModuleID = dbService.GetLatestIDUsed("Module", "id_Module", dbName);
                        latestModulesID = dbService.GetLatestIDUsed("Modules", "id_Module", dbName);
                        latestDutyID = dbService.GetLatestIDUsed("Duty", "id_Duty", dbName);
                        latestJourneyID = dbService.GetLatestIDUsed("Journey", "id_Journey", dbName);
                        latestStageID = dbService.GetLatestIDUsed("Stage", "id_Stage", dbName);
                        latestTransID = dbService.GetLatestIDUsed("Trans", "id_Trans", dbName);
                        latestPosTransID = dbService.GetLatestIDUsed("PosTrans", "id_PosTrans", dbName);
                        latestInspectorID = dbService.GetLatestIDUsed("Inspector", "id_Inspector", dbName);
                        latestAuditFileStatus = dbService.GetLatestIDUsed("AuditFileStatus", "Id_Status", dbName);
                        latestdiagnosticRecord = dbService.GetLatestIDUsed("DiagnosticRecord", "Id_DiagnosticRecord", dbName);
                        latestSCTransID = dbService.GetLatestIDUsed("SCTrans", "id_SCTrans", dbName);
                        #region Update Actual ID's
                        moduleDetail.id_Module = moduleDetail.id_Module + latestModuleID;
                        dutyDetail.id_Module = dutyDetail.id_Module + latestModuleID;
                        dutyDetail.id_Duty = dutyDetail.id_Duty + latestDutyID;
                        if (modulesDetail != null)
                        {
                            modulesDetail.id_Module = modulesDetail.id_Module + latestModulesID;
                        }

                        if (busChecklistDetail != null)
                        {
                            busChecklistDetail.id_BusChecklist = busChecklistDetail.id_BusChecklist + latestBusChecklistID;
                        }

                        wayBillDetails.ForEach(x =>
                        {
                            x.id_Module = x.id_Module + latestModuleID;
                        });

                        journeyDetails.ForEach(x =>
                        {
                            x.id_Journey = x.id_Journey + latestJourneyID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                        });

                        stageDetails.ForEach(x =>
                        {
                            x.id_Stage = x.id_Stage + latestStageID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                            x.id_Journey = x.id_Journey + latestJourneyID;
                        });

                        transDetails.ForEach(x =>
                        {
                            x.id_Trans = x.id_Trans + latestTransID;
                            x.id_Module = x.id_Module + latestModuleID;
                            x.id_SCTrans = x.id_SCTrans + latestSCTransID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                            x.id_Journey = x.id_Journey + latestJourneyID;
                            x.id_Stage = x.id_Stage + latestStageID;
                        });

                        gPSCoordinates.ForEach(x =>
                        {
                            x.id_Trans = x.id_Trans + latestTransID;
                            x.id_Module = x.id_Module + latestModuleID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                            x.id_Journey = x.id_Journey + latestJourneyID;
                            x.id_Stage = x.id_Stage + latestStageID;
                        });


                        posTransDetails.ForEach(x =>
                        {
                            x.id_PosTrans = x.id_PosTrans + latestPosTransID;
                            x.id_Module = x.id_Module + latestModuleID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                            x.id_Journey = x.id_Journey + latestJourneyID;
                            x.id_Stage = x.id_Stage + latestStageID;
                        });

                        scTransDetails.ForEach(x =>
                        {
                            x.id_SCTrans = x.id_SCTrans + latestSCTransID;
                            x.id_Module = x.id_Module + latestModuleID;
                            x.id_Duty = x.id_Duty + latestDutyID;
                            x.id_Journey = x.id_Journey + latestJourneyID;
                            x.id_Stage = x.id_Stage + latestStageID;
                        });

                        inspectorDetails.ForEach(x =>
                        {
                            x.id_Stage = x.id_Stage + latestStageID;
                            x.id_Inspector = x.id_Inspector + latestInspectorID;
                        });

                        auditFileDetails.ForEach(x =>
                        {
                            x.Id_Status = x.Id_Status + latestAuditFileStatus;
                            x.id_duty = x.id_duty + latestDutyID;
                        });

                        diagnosticRecords.ForEach(x =>
                        {
                            x.Id_Status = x.Id_Status + latestAuditFileStatus;
                            x.Id_DiagnosticRecord = x.Id_DiagnosticRecord + latestdiagnosticRecord;
                        });
                        #endregion

                        XmlDataToImport xmlDataToImport = new XmlDataToImport()
                        {
                            Modules = moduleDetail != null ? new List<Module>() { moduleDetail } : new List<Module>(),
                            Moduless = modulesDetail != null ? new List<Modules>() { modulesDetail } : new List<Modules>(),
                            Duties = dutyDetail != null ? new List<Duty>() { dutyDetail } : new List<Duty>(),
                            Waybills = wayBillDetails,
                            Journeys = journeyDetails,
                            Stages = stageDetails,
                            Trans = transDetails,
                            PosTrans = posTransDetails,
                            Staffs = staffDetail != null ? new List<Staff>() { staffDetail } : new List<Staff>(),
                            Inspectors = inspectorDetails,
                            AuditFileStatuss = auditFileDetails,
                            DiagnosticRecords = diagnosticRecords,
                            BusChecklistRecords = busChecklistDetail != null ? new List<BusChecklist>() { busChecklistDetail } : new List<BusChecklist>(),
                            GPSCoordinates = gPSCoordinates,
                            SCTrans = scTransDetails,
                        };

                        result = dbService.InsertXmlFileData(xmlDataToImport, dbName);
                        helper.MoveSuccessFile(filePath, dbName);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Violation of PRIMARY KEY"))
                    {                        
                        if (fallBackCount < 3)
                        {
                            Logger.Info("Violation of PRIMARY KEY occured fallback activated: " + fallBackCount);
                            int milliseconds = 60000;
                            Thread.Sleep(milliseconds);
                            fallBackCount++;
                            goto FALLBACK;
                        }
                        Logger.Info("Violation of PRIMARY KEY occured fallback failed: " + fallBackCount);
                    }
                    throw;
                }
                #endregion
            }
            catch (Exception ex)
            {
                string exception = JsonConvert.SerializeObject(ex).ToString();
                if (Constants.DetailedLogging)
                {
                    Logger.Error("Failed in XML ProcessFile");
                    Logger.Error("Exception:" + exception);
                }
                Logger.Error("Exception:" + exception);
                helper.MoveErrorFile(filePath, dbName);
                if (Constants.EnableEmailTrigger)
                {
                    emailHelper.SendMail(filePath, dbName, exception, EmailType.Error);
                }
                //MessageBox.Show(ex.Message);
                return result;
            }
            return result;
        }

        public void CopyPropertyValues(object source, object destination)
        {
            System.Reflection.PropertyInfo[] destProperties = destination.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo sourceProperty in source.GetType().GetProperties())
            {
                foreach (System.Reflection.PropertyInfo destProperty in destProperties)
                {
                    if (destProperty.Name == sourceProperty.Name &&
                destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        destProperty.SetValue(destination, sourceProperty.GetValue(
                            source, new object[] { }), new object[] { });

                        break;
                    }
                }
            }
        }

        private PosTrans MapTransToPosTrans(Trans transDetail)
        {
            return new PosTrans()
            {
                dat_TransDate = transDetail.dat_TransDate,
                id_Duty = transDetail.id_Duty,
                dat_TransTime = transDetail.dat_TransTime,
                id_Journey = transDetail.id_Journey,
                id_Module = transDetail.id_Module,
                //id_SCTrans = transDetail.id_SCTrans,
                id_Stage = transDetail.id_Stage,
                int2_AlightingStageID = transDetail.int2_AlightingStageID,
                int2_AnnulCount = transDetail.int2_AnnulCount,
                int2_BoardingStageID = transDetail.int2_BoardingStageID,
                int2_Class = transDetail.int2_Class,
                int2_PassCount = transDetail.int2_PassCount,
                int2_TicketCount = transDetail.int2_TicketCount,
                int2_Transfers = transDetail.int2_Transfers,
                int4_AnnulCash = transDetail.int4_AnnulCash,
                int4_NonRevenue = transDetail.int4_NonRevenue,
                int4_Revenue = transDetail.int4_Revenue,
                int4_RevenueBal = transDetail.int4_RevenueBal,
                int4_TicketSerialNumber = null,
                int4_TripBal = null,
                str_LocationCode = "",
                str_SerialNumber = transDetail.str_SerialNumber
            };
        }


        private SCTrans MapTransToScTrans(Trans transDetail)
        {
            return new SCTrans()
            {
                id_Stage = transDetail.id_Stage,
                id_Journey = transDetail.id_Journey,
                id_Duty = transDetail.id_Duty,
                id_Module = transDetail.id_Module,
                int2_Class = transDetail.int2_Class,
                str_SerialNumber = transDetail.str_SerialNumber,
                int4_RevenueBal = transDetail.int4_RevenueBal,
                int4_TripBal = 0,
                int4_ProcessCode = 0,
                dat_TransTime = transDetail.dat_TransTime,
                int4_FieldID = 1,
                int4_GroupID = 1,
                bit_CreditFlag = 0,
                int4_TransactionRef = 0,
                int4_RevenueSubtracted = 0,
                int4_LoyaltySubtracted = null,
                int4_LoyaltyBal = null,
                dat_PassStart = null,
                dat_PassExpiry = null,
                int4_UserValue = null,
                int4_TripSubtracted = null,
                int4_TransferSubtracted = null,
                int4_TransferBal = null,
                int4_StartStage = null,
                int4_EndStage = null,
                int4_PayMethod = null,
                int4_DriverNo = null,
                int4_Tendered = null,
                int4_TokenOID = null,
                str_PrintedSerialNum = null,
                dat_UpdateDate = null,
                int4_BoardingStage = null,
                int4_AlightingStage = null,
                int4_ControlSeq = null,
                int4_UnsafeTransaction = null,
                str_HexSerialNumber = null,
            };
        }

        public bool ValidateFile(string filePath)
        {
            return !helper.IsFileLocked(filePath);
        }
    }
}
