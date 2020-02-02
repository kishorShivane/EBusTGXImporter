using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Module
    {
        public int id_Module { get; set; }
        public Nullable<int> int4_ModuleID { get; set; }
        public Nullable<int> int4_DriverNo { get; set; }
        public Nullable<System.DateTime> dat_SignOnDate { get; set; }
        public Nullable<System.DateTime> dat_SignOnTime { get; set; }
        public Nullable<int> int4_OnReaderID { get; set; }
        public Nullable<System.DateTime> dat_SignOffDate { get; set; }
        public Nullable<System.DateTime> dat_SignOfftime { get; set; }
        public Nullable<int> int4_OffReaderID { get; set; }
        public Nullable<int> int_Moduletype { get; set; }
        public Nullable<int> int4_NetCash { get; set; }
        public Nullable<int> int4_CancelledCash { get; set; }
        public Nullable<int> int4_AnulledSVCash { get; set; }
        public Nullable<int> int4_NetTickets { get; set; }
        public Nullable<int> int4_TotalPasses { get; set; }
        public Nullable<int> int4_CancelledTickets { get; set; }
        public Nullable<int> int4_GrossTickets { get; set; }
        public Nullable<int> int4_GrossCash { get; set; }
        public Nullable<int> int4_NetSVCash { get; set; }
        public Nullable<int> int4_GrossSVCash { get; set; }
        public Nullable<int> int4_NetSVTickets { get; set; }
        public Nullable<int> int4_AnulledSVTickets { get; set; }
        public Nullable<int> int4_GrossSVTickets { get; set; }
        public Nullable<int> int4_SignOnSeq { get; set; }
        public Nullable<int> int4_SignOffSeq { get; set; }
        public Nullable<short> int_DutyControl { get; set; }
        public Nullable<int> id_BatchNo { get; set; }
        public Nullable<int> int4_ModuleRevenue { get; set; }
        public Nullable<int> int4_ModuleNonRevenue { get; set; }
        public Nullable<int> int4_ModuleTickets { get; set; }
        public Nullable<int> int4_ModulePasses { get; set; }
        public Nullable<int> int4_ModuleTransfer { get; set; }
        public Nullable<int> int4_ModuleAnnulCash { get; set; }
        public Nullable<int> int4_ModuleAnnulCount { get; set; }
        public Nullable<int> int4_AnnulledSVCash { get; set; }
        public Nullable<int> int4_AnnulledSVTickets { get; set;}
    }
}
