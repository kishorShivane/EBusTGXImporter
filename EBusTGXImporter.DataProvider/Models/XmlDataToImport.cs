using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public class XmlDataToImport
    {
        public List<Module> Modules { get; set; } = new List<Module>();
        public List<Modules> Moduless { get; set; } = new List<Modules>();
        public List<Duty> Duties { get; set; } = new List<Duty>();
        public List<Waybill> Waybills { get; set; } = new List<Waybill>();
        public List<Journey> Journeys { get; set; } = new List<Journey>();
        public List<Stage> Stages { get; set; } = new List<Stage>();
        public List<Staff> Staffs { get; set; } = new List<Staff>();
        public List<Inspector> Inspectors { get; set; } = new List<Inspector>();
        public List<Trans> Trans { get; set; } = new List<Trans>();
        public List<SCTrans> SCTrans { get; set; } = new List<SCTrans>();
        public List<PosTrans> PosTrans { get; set; } = new List<PosTrans>();
        public List<AuditFileStatus> AuditFileStatuss { get; set; } = new List<AuditFileStatus>();
        public List<DiagnosticRecord> DiagnosticRecords { get; set; } = new List<DiagnosticRecord>();
        public List<BusChecklist> BusChecklistRecords { get; set; } = new List<BusChecklist>();
        public List<GPSCoordinate> GPSCoordinates { get; set; } = new List<GPSCoordinate>();
    }
}
