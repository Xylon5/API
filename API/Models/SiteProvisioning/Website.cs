using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class Website
    {
        public string Title { get; set; }
        public string Url { get; set; }

        [Key]
        public Guid ID { get; set; }
        public WebsiteType Type { get; set; }
        public bool IsOffice365 { get; set; }
        public int Locale { get; set; }

        internal bool CanBeDeleted { get; set; }
    }

    public enum WebsiteType
    {
        Administration,
        Collaboration,
        Publishing,
        Form
    }
}