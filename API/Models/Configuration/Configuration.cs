using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models.Configuration
{
    public class Configuration
    {
        public Url Publishing { get; set; }
        public Url Collaboration { get; set; }
        public Url MySite { get; set; }

        public TimeSpan CacheRefreshInterval { get; set; }
    }
}