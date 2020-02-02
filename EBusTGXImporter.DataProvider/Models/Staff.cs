using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Staff
    {
        public int int4_StaffID { get; set; }
        public string str50_StaffName { get; set; }
        public bool bit_InUse { get; set; }
        public System.DateTime dat_RecordMod { get; set; }
        public int int4_StaffTypeID { get; set; }
        public int int4_StaffSubTypeID { get; set; }
        public string str4_LocationCode { get; set; }
        public string str2_LocationCode { get; set; }
    }
}
