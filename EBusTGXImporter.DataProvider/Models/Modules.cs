using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Modules
    {
        public int id_Module { get; set; }
        public string str6_Modules { get; set; }
        public Nullable<byte> int_ModuleStatus { get; set; }
        public byte int_ModuleType { get; set; }
        public int int_ModuleSerial { get; set; }
        public System.DateTime dat_FirstUsed { get; set; }
        public System.DateTime dat_LastUsed { get; set; }
    }
}
