using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models.Configuration
{
    public class Url
    {
        public string Path { get; set; }
        public bool IsCritical { get; set; }
        public bool IsOffice365 { get; set; }
    }
}