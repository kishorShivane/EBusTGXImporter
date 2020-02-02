using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class AuditFileStatus
    {
        public int Id_Status { get; set; }
        public int id_duty { get; set; }
        public int DriverAuditStatus1 { get; set; }
        public long DriverNumber1 { get; set; }
        public string DriverCardSerialNumber1 { get; set; }
        public System.DateTime DriverStatus1DateTime { get; set; }
        public int DriverAuditStatus2 { get; set; }
        public long DriverNumber2 { get; set; }
        public string DriverCardSerialNumber2 { get; set; }
        public System.DateTime DriverStatus2DateTime { get; set; }
        public int DutySignOffMode { get; set; }
        public System.DateTime RecordModified { get; set; }
        public string AuditFileName { get; set; }
    }
}
