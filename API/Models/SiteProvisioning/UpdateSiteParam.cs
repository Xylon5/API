using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class UpdateSiteParam
    {
        public bool All { get; set; }
        public bool Pilot { get; set; }
        public bool Single { get; set; }
        public Guid Id { get; set; }
    }
}