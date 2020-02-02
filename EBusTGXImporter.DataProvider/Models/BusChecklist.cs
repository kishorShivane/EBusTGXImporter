using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class BusChecklist
    {
        public int id_BusChecklist { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public bool SignOnCheckItem1 { get; set; }
        public bool SignOnCheckItem2 { get; set; }
        public bool SignOnCheckItem3 { get; set; }
        public bool SignOnCheckItem4 { get; set; }
        public bool SignOnCheckItem5 { get; set; }
        public bool SignOnCheckItem6 { get; set; }
        public bool SignOnCheckItem7 { get; set; }
        public bool SignOnCheckItem8 { get; set; }
        public int SignOnDeviceDefective { get; set; }
        public string SignOnTime { get; set; }

        public bool SignOffCheckItem1 { get; set; }
        public bool SignOffCheckItem2 { get; set; }
        public bool SignOffCheckItem3 { get; set; }
        public bool SignOffCheckItem4 { get; set; }
        public bool SignOffCheckItem5 { get; set; }
        public bool SignOffCheckItem6 { get; set; }
        public bool SignOffCheckItem7 { get; set; }
        public bool SignOffCheckItem8 { get; set; }
        public int SignOffDeviceDefective { get; set; }
        public string SignOffTime { get; set; }
    }
}
