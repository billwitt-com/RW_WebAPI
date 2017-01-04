using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RWICPreceiverApp.Models
{
    /// <summary>
    /// Used for saving images.
    /// </summary>
    public class PostStationImageDTO
    {
        public int StationID { get; set; }
        public string CreatedBy { get; set; }
    }
}