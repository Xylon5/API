using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Spatial;
using System.Web;

namespace API.Models.OData
{
    public class Store
    {
        [Key]
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Adress { get; set; }
        public DbGeography Location { get; set; }

        public double Longitude
        {
            get
            {
                return Location.Longitude.HasValue ? Location.Longitude.Value : 0.0;
            }
        }
        public double Latitude
        {
            get
            {
                return Location.Latitude.HasValue ? Location.Latitude.Value : 0.0;
            }
        }
    }
}