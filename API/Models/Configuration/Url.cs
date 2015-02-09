using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class Url
    {
        [Key]
        public string Path { get; set; }
        public bool IsCritical { get; set; }
        public bool IsOffice365 { get; set; }

        public bool IsServiceUrl { get; set; }
    }
}