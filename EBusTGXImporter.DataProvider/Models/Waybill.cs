using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Waybill
    {
        public int ModuleID { get; set; }
        public int id_Module { get; set; }  
        public Nullable<System.DateTime> dat_Start { get; set; }
        public Nullable<System.DateTime> dat_End { get; set; }
        public Nullable<int> int4_Operator { get; set; }
        public string str8_BusID { get; set; }
        public string str6_EtmID { get; set; }
        public Nullable<int> int4_EtmGrandTotal { get; set; }
        public Nullable<int> int4_Revenue { get; set; }
        public Nullable<System.DateTime> dat_Match { get; set; }
        public Nullable<System.DateTime> dat_Actual { get; set; }
        public Nullable<int> Imported_Operator { get; set; }
        public string Imported_BusID { get; set; }
        public string Imported_ETMID { get; set; }
        public Nullable<int> Imported_GT { get; set; }
        public Nullable<int> Imported_Revenue { get; set; }
    }
}
