using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBusTGXImporter.DataProvider.Models
{
    public partial class Journey
    {

        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public string str_RouteID { get; set; }
        public Nullable<short> int2_JourneyID { get; set; }
        public Nullable<short> int2_Direction { get; set; }
        public Nullable<System.DateTime> dat_JourneyStartDate { get; set; }
        public Nullable<System.DateTime> dat_JourneyStartTime { get; set; }
        public Nullable<System.DateTime> dat_JourneyStopTime { get; set; }
        public Nullable<int> int4_Distance { get; set; }
        public Nullable<int> int4_JourneyRevenue { get; set; }
        public Nullable<int> int4_JourneyTickets { get; set; }
        public Nullable<int> int4_JourneyPasses { get; set; }
        public Nullable<int> int4_JourneyNonRevenue { get; set; }
        public Nullable<int> int4_JourneyTransfer { get; set; }
        public Nullable<System.DateTime> dat_JourneyMoveTime { get; set; }
        public Nullable<System.DateTime> dat_JourneyArrivalTime { get; set; }
        public Nullable<int> int4_JourneyAnnulCash { get; set; }
        public Nullable<int> int4_JourneyAnnulCount { get; set; }
        public Nullable<int> int4_GPSDistance { get; set; }
        public Nullable<int> int4_DBDistance { get; set; }

    }
}
