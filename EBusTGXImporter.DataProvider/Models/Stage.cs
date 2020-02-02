using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Stage
    {
        public int id_Stage { get; set; }
        public Nullable<int> id_Journey { get; set; }
        public Nullable<short> int2_StageID { get; set; }
        public Nullable<System.DateTime> dat_StageDate { get; set; }
        public Nullable<System.DateTime> dat_StageTime { get; set; }
        public Nullable<int> int4_Data1 { get; set; }
        public Nullable<int> id_Duty { get; set; }
    }

    public class TempStage
    {
        public int id_Stage { get; set; }
        public string TSN { get; set; }
        public string RecordedTime { get; set; }
    }
}
