using System;

namespace EBusTGXImporter.DataProvider.Models
{
    public class TAssetETM
    {
        public int TAssetETM_id { get; set; }
        public Nullable<int> ETMID { get; set; } = null;
        public Nullable<int> ModuleID { get; set; } = null;
        public Nullable<int> TrayID { get; set; } = null;
        public string ETMConfig { get; set; } = null;
        public string DutySel { get; set; } = null;
        public string Hotlist { get; set; } = null;
        public string SmartText { get; set; } = null;
        public string TimeBand { get; set; } = null;
        public string PPProducts { get; set; } = null;
        public string SignOn { get; set; } = null;
        public string SCProducts { get; set; } = null;
        public string SimPhone { get; set; } = null;
        public string TgxIpAddr { get; set; } = null;
        public string SimId { get; set; } = null;
        public string SimIMEI { get; set; } = null;
        public string SimIMSI { get; set; } = null;
        public string ModemVers { get; set; } = null;
        public Nullable<System.DateTime> dat_LastUpdate { get; set; } = null;
    }
}

