using System;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Trans
    {
        public int id_Trans { get; set; }
        public Nullable<int> id_SCTrans { get; set; }
        public int id_Stage { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public Nullable<short> int2_Class { get; set; }
        public Nullable<int> int4_Fare { get; set; }
        public Nullable<short> int2_BoardingStageID { get; set; }
        public Nullable<short> int2_AlightingStageID { get; set; }

        public Nullable<int> int4_Revenue { get; set; }
        public Nullable<int> int4_NonRevenue { get; set; }
        public Nullable<short> int2_TicketCount { get; set; }
        public Nullable<short> int2_PassCount { get; set; }
        public Nullable<short> int2_Transfers { get; set; }
        public Nullable<System.DateTime> dat_TransDate { get; set; }
        public Nullable<System.DateTime> dat_TransTime { get; set; }

        public Nullable<int> int2_AnnulCount { get; set; }
        public Nullable<int> int4_AnnulCash { get; set; }
        public string str_SerialNumber { get; set; }
        public Nullable<int> int4_RevenueBal { get; set; }

    }
}
