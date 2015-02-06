using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class SPSite
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public Guid ID { get; set; }
        public SPSiteType Type { get; set; }
        public bool IsOffice365 { get; set; }
        public int Locale { get; set; }

        internal bool CanBeDeleted { get; set; }
    }

    public enum SPSiteType
    {
        Administration,
        Collaboration,
        Publishing,
        Form
    }
}