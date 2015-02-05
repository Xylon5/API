using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models.OData
{
    public class Article
    {
        [Key]
        public int Code { get; set; }
        public string FullName { get; set; }

        public string ShortName { get; set; }

    }
}