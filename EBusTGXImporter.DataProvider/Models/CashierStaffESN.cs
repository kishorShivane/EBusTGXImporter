using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class CashierStaffESN
    {
        public int ID { get; set; }
        public string ESN { get; set; }
        public Nullable<long> PSN { get; set; }
        public Nullable<int> OperatorID { get; set; }
        public Nullable<System.DateTime> ImportDateTime { get; set; }
    }
}
