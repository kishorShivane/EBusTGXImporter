using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class ReaderActivity
    {
        public int int4_SequenceNum { get; set; }
        public Nullable<int> byt_ActivityType { get; set; }
        public Nullable<int> int4_CashierID { get; set; }
        public Nullable<int> int4_ThisReaderID { get; set; }
        public Nullable<System.DateTime> dat_ActivityDate { get; set; }
        public Nullable<System.DateTime> dat_ActivityTime { get; set; }
        public Nullable<int> int4_Supervisor { get; set; }
        public Nullable<int> int4_TicketCount { get; set; }
        public Nullable<int> int4_AnnulCash { get; set; }
        public Nullable<int> int4_AnnulCount { get; set; }
        public Nullable<System.DateTime> dat_SignOnDate { get; set; }
        public Nullable<int> int4_OperatorID { get; set; }
        public Nullable<int> int4_ModuleCash { get; set; }
        public Nullable<int> int4_SignOffCash { get; set; }
        public Nullable<int> int4_SignOffAnnulCash { get; set; }
        public Nullable<int> int4_SignOffAnnulQty { get; set; }
        public Nullable<System.DateTime> dat_RecordMod { get; set; }
        public Nullable<int> int4_RecNum { get; set; }
        public Nullable<int> int4_ModuleID { get; set; }
        public Nullable<int> int4_CouponCash { get; set; }
        public Nullable<int> int4_CouponCount { get; set; }
        public Nullable<int> int4_SignOffCouponCash { get; set; }
        public Nullable<int> int4_SignOffCouponQty { get; set; }
        public Nullable<int> int4_SmartLastSinglePassCash { get; set; }
        public Nullable<int> int4_SmartLastSinglePassCount { get; set; }
        public Nullable<int> int4_LRMDriverSinglePasses { get; set; }
        public Nullable<int> int4_LRMNoSinglePasses { get; set; }
    }
}
