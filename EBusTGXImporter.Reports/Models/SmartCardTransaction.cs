using System;

namespace EBusTGXImporter.Reports.Models
{
    public class SmartCardTransaction
    {
        //public string Type { get; set; }
        //public string Duty { get; set; }
        //public string Route { get; set; }
        //public string Driver { get; set; }
        //public string BusID { get; set; }
        //public string Date { get; set; }
        //public string Time { get; set; }
        //public string Revenue { get; set; }
        //public string NonRevenue { get; set; }
        //public string RechargeQuantity { get; set; }
        //public string RevenueBalance { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string NonRevenue { get; set; }
        public string Revenue { get; set; }
        public DateTime TransDate { get; set; }
        public string SerialNumber { get; set; }
        public string SerialNumberHex { get; set; }
        public string RouteID { get; set; }
        public string JourneyID { get; set; }
        public string OperatorID { get; set; }
        public string OperatorName { get; set; }
        public string ETMID { get; set; }
        public string BusID { get; set; }
        public string DutyID { get; set; }
        public string ModuleID { get; set; }
        public string RevenueBalance { get; set; }
        public string TransDay { get; set; }

        public string TDate { get; set; }
        public string TTime { get; set; }

        public string DateRangeFilter { get; set; }
        public string CardIdFilter { get; set; }
        public string AmountRecharged { get; set; }
        public string TripsRecharged { get; set; }
    }
}