using RWICPreceiverApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RWICPreceiverApp.Services
{
    public interface IStationService
    {
       Task<StationDTO> GetStationData();
    }

    /*Waiting to work to determine what information to send to the main website.*/
    public class StationService
    {
        public async Task<StationDTO> GetStationData()
        {
            StationDTO stationDTO = null;

            //using (RiverWatchEntities _db = new RiverWatchEntities())
            //{
            //    stationDTO = (from sti in _db.StationImages
            //                  select new StationDTO
            //                  {

            //                  })
            //}

           return stationDTO;
        }
    }
}