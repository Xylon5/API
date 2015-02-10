using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class Config
    {
        [Key]
        public int ConfigID { get; set; }

        public List<Url> Urls { get; set; }

        public List<Url> EndPoints { get; set; }

        public MiscConfigs Miscellanous { get; set; }
    }

    public class MiscConfigs
    {
        public TimeSpan CacheRefreshInterval { get; set; }

        public bool AllowBetaFeatures { get; set; }

        public bool IsMaintenance { get; set; }

    }
}