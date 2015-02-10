using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class Website
    {
        [Key]
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string BaseUrl { get; set; }
        public string ServerRelativeUrl { get; set; }
        public WebsiteType Type { get; set; }
        public bool IsOffice365 { get; set; }
        public uint Locale { get; set; }
        internal bool CanBeDeleted { get; set; }

        internal string Url
        {
            get { return string.Format("{0}/{1}", this.BaseUrl, this.ServerRelativeUrl); }
        }

        public string OwnerLogin { get; set; }
    }

    public enum WebsiteType
    {
        Administration,
        Collaboration,
        Publishing,
        Form
    }
}