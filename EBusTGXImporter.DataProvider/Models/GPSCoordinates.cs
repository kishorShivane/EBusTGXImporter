using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBusTGXImporter.DataProvider.Models
{
    public class GPSCoordinate
    {
        //public int ID { get; set; }
       
        public int id_Module { get; set; }
        public int id_Duty { get; set; }
        public int id_Journey { get; set; }
        public int id_Stage { get; set; }
        public int id_Trans { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int LatDegree { get; set; }
        public int LatMinutes { get; set; }
        public decimal LatSeconds { get; set; }
        public string LatDir { get; set; }
        public int LongDegree { get; set; }
        public int LongMinutes { get; set; }
        public decimal LongSeconds { get; set; }
        public string LongDir { get; set; }
    }
}
