using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public class CsvDataToImport
    {
        public List<ReaderActivity> ReaderActivities { get; set; } = new List<ReaderActivity>();
        public List<Cashier> Cashiers { get; set; } = new List<Cashier>();
        public List<CashierDetail> CashierDetails { get; set; } = new List<CashierDetail>();
        public List<CashierSigonSignoff> CashierSigonSignoffs { get; set; } = new List<CashierSigonSignoff>();
        public List<CashierStaffESN> CashierStaffESNs { get; set; } = new List<CashierStaffESN>();
        public List<Staff> Staffs { get; set; } = new List<Staff>();
    }
}
