using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class SCTrans
    {
        public int id_SCTrans { get; set; }
        public Nullable<int> id_Stage { get; set; }
        public Nullable<int> id_Journey { get; set; }
        public Nullable<int> id_Duty { get; set; }
        public Nullable<int> id_Module { get; set; }
        public Nullable<int> int2_Class { get; set; }
        public string str_SerialNumber { get; set; }
        public Nullable<int> int4_RevenueBal { get; set; }
        public Nullable<int> int4_TripBal { get; set; }
        public Nullable<int> int4_ProcessCode { get; set; }
        public Nullable<System.DateTime> dat_TransTime { get; set; }
        public Nullable<int> int4_FieldID { get; set; }
        public Nullable<int> int4_GroupID { get; set; }
        public Nullable<int> bit_CreditFlag { get; set; }
        public Nullable<int> int4_TransactionRef { get; set; }
        public Nullable<int> int4_RevenueSubtracted { get; set; }
        public Nullable<int> int4_LoyaltySubtracted { get; set; }
        public Nullable<int> int4_LoyaltyBal { get; set; }
        public Nullable<System.DateTime> dat_PassStart { get; set; }
        public Nullable<System.DateTime> dat_PassExpiry { get; set; }
        public Nullable<int> int4_UserValue { get; set; }
        public Nullable<int> int4_TripSubtracted { get; set; }
        public Nullable<int> int4_TransferSubtracted { get; set; }
        public Nullable<int> int4_TransferBal { get; set; }
        public Nullable<int> int4_StartStage { get; set; }
        public Nullable<int> int4_EndStage { get; set; }
        public Nullable<int> int4_PayMethod { get; set; }
        public Nullable<int> int4_DriverNo { get; set; }
        public Nullable<int> int4_Tendered { get; set; }
        public Nullable<int> int4_TokenOID { get; set; }
        public string str_PrintedSerialNum { get; set; }
        public Nullable<System.DateTime> dat_UpdateDate { get; set; }
        public Nullable<int> int4_BoardingStage { get; set; }
        public Nullable<int> int4_AlightingStage { get; set; }
        public Nullable<int> int4_ControlSeq { get; set; }
        public Nullable<int> int4_UnsafeTransaction { get; set; }
        public string str_HexSerialNumber { get; set; }
    }
}
