using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Cashier
    {
        public int ID { get; set; }
        public string StaffNumber { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
        public string Revenue { get; set; }
        public string CashierID { get; set; }
        public string ImportDateTime { get; set; }
        public string CashOnCard { get; set; }
    }
}
