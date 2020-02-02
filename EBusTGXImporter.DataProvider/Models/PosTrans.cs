using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class PosTrans
    {

        public int id_PosTrans { get; set; }
        public int id_Stage { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public string str_LocationCode { get; set; }
        public Nullable<short> int2_BoardingStageID { get; set; }
        public Nullable<short> int2_AlightingStageID { get; set; }
        public Nullable<short> int2_Class { get; set; }
        public Nullable<int> int4_Revenue { get; set; }
        public Nullable<int> int4_NonRevenue { get; set; }
        public Nullable<short> int2_TicketCount { get; set; }
        public Nullable<short> int2_PassCount { get; set; }
        public Nullable<short> int2_Transfers { get; set; }
        public Nullable<System.DateTime> dat_TransDate { get; set; }
        public Nullable<System.DateTime> dat_TransTime { get; set; }
        public string str_SerialNumber { get; set; }
        public Nullable<int> int4_RevenueBal { get; set; }
        public Nullable<int> int4_TripBal { get; set; }
        public Nullable<int> int2_AnnulCount { get; set; }
        public Nullable<int> int4_AnnulCash { get; set; }
        public Nullable<int> id_SCTrans { get; set; }
        public Nullable<int> int4_TicketSerialNumber { get; set; }
        public Nullable<int> TripsRecharged { get; set; }
        public Nullable<int> AmountRecharged { get; set; }
    }
}
