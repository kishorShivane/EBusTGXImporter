using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusTGXImporter.DataProvider.Models
{
    public class DiagnosticRecord
    {
        public int Id_DiagnosticRecord { get; set; }
        public int Id_Status { get; set; }
        public string TSN { get; set; }
        public string EquipmentType { get; set; }
        public string DiagCode { get; set; }
        public string DiagInfo { get; set; }
        public string Time { get; set; }
    }
}
