using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Inspector
    {
        public int id_Inspector { get; set; }
        public int id_Stage { get; set; }
        public int id_InspectorID { get; set; }
        public Nullable<System.DateTime> datTimeStamp { get; set; }
        public Nullable<int> intSerialNo { get; set; }
    }
}
