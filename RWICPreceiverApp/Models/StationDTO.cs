using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RWICPreceiverApp.Models
{
    public class StationDTO
    {
        public int StationID { get; set; }
        public int StationNumber { get; set; }
        public string StationName { get; set; }
        public double Longtitude { get; set; }
        public double Latitude { get; set; }
        public int ImageID { get; set; }
        public string ImageName { get; set; }
        public string ImageUrl { get; set; }
        public int ImageTypeID { get; set; }
    }
}