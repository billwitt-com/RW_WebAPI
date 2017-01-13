using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using RWICPreceiverApp.Models;
using System.Runtime.Serialization;
using System.Data.Sql;
using System.Data.SqlClient; 

// controller that works to update incoming icp data to both final and origional tables
// use this BW 08/25/2016

namespace RWICPreceiverApp.Controllers
{
    [RoutePrefix("api/InboundICPFinals")]
    public class InboundICPFinalsController : ApiController
    {
        // short queries for fort collins:

        [ResponseType(typeof(bool))]
        [Route("GetInboundICPFinalExists")]
        [HttpGet]
        public IHttpActionResult GetInboundICPFinalExists(int id)
        {
            bool validated = false;
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 

            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request);
            if (!validated)
                return Unauthorized();

            RiverWatchEntities RWDE = new RiverWatchEntities();
            return Ok(RWDE.InboundICPFinals.Count(e => e.ID == id) > 0);
        }

        [ResponseType(typeof(bool))]
        [Route("GetICPBarCodeExists")]
        [HttpGet]
        public IHttpActionResult GetICPBarCodeExists(string Bcode)
        {
            bool validated = false;
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 

            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request);
            if (!validated)
                return Unauthorized();

            RiverWatchEntities RWDE = new RiverWatchEntities();           
            return Ok((RWDE.InboundICPFinals.Count(e => e.CODE == Bcode) > 0));
        }

        [ResponseType(typeof(bool))]
        [Route("GetNewExpBarCodeExists")]
        [HttpGet]
        public IHttpActionResult GetNewExpBarCodeExists(string Bcode)
        {
            bool validated = false;
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request

            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request);
            if (!validated)
                return Unauthorized();

            RiverWatchEntities RWDE = new RiverWatchEntities();
            return Ok(RWDE.NEWexpWaters.Count(e => e.MetalsBarCode == Bcode) > 0);
        }

        [ResponseType(typeof(bool))]
        [Route("GetSampleExists")]
        [HttpGet]
        public IHttpActionResult GetSampleExists(string samplenumber)
        {
            bool validated = false;
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request

            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request);
            if (!validated)
                return Unauthorized();

            RiverWatchEntities RWDE = new RiverWatchEntities();
            return Ok(RWDE.Samples.Count(e => e.SampleNumber == samplenumber) > 0);
        }

        // GET: api/InboundICPFinals1/5
        [ResponseType(typeof(InboundICPFinal))]
        [HttpGet]
        public IHttpActionResult GetInboundICPFinal(int id)
        {
            bool validated = false;
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 
           
            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request); 
            if(!validated)
                return Unauthorized();

            // user is good to go..
            
            // now do something
            RiverWatchEntities RWDE = new RiverWatchEntities();
            InboundICPFinal inboundICPFinal = RWDE.InboundICPFinals.Find(id);
            if (inboundICPFinal == null)
            {
                return NotFound();
            }
            return Ok(inboundICPFinal);
        }


        [ResponseType(typeof(InboundICPFinal))]
        [Route("PostInboundICPFinal")]
        [HttpPost]
        public async Task<IHttpActionResult> PostInboundICPFinal(InboundICPFinal inboundICPFinal)    
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RiverWatchEntities RWDE = new RiverWatchEntities();
            InboundICPOrigional IO = new InboundICPOrigional();
            int res = 0;
            int id = 0;
            bool validated = false;
            string conStr = RWDE.Database.Connection.ConnectionString; 
           
            HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 
          
            ValidateCredentials VC = new ValidateCredentials();
            validated = VC.checkCreds(request);
            if (!validated)
                return Unauthorized();

           try
           {
               RWDE.InboundICPFinals.Add(inboundICPFinal);
           //    RWDE.InboundICPOrigionals.Add(inboundICPFinal);
               res = await RWDE.SaveChangesAsync();
               id = inboundICPFinal.ID; // get from last write    
               // now copy to origional table and save
               // would rather do this than use a stored proc that could get lost later

               IO.AL_D = inboundICPFinal.AL_D;
               IO.AL_T = inboundICPFinal.AL_T;
               IO.ANADATE = inboundICPFinal.ANADATE;
               IO.AS_D = inboundICPFinal.AS_D;
               IO.AS_T = inboundICPFinal.AS_T;
               IO.CA_D = inboundICPFinal.CA_D;
               IO.CA_T = inboundICPFinal.CA_T;
               IO.CD_D = inboundICPFinal.CD_D;
               IO.CD_T = inboundICPFinal.CD_T;
               IO.CODE = inboundICPFinal.CODE;
               IO.Comments = inboundICPFinal.Comments;
               IO.COMPLETE = inboundICPFinal.COMPLETE;
               IO.CreatedBy = inboundICPFinal.CreatedBy;
               IO.CreatedDate = inboundICPFinal.CreatedDate;
               IO.CU_D = inboundICPFinal.CU_D;
               IO.CU_T = inboundICPFinal.CU_T;
               IO.DATE_SENT = inboundICPFinal.DATE_SENT;
               IO.DUPLICATE = inboundICPFinal.DUPLICATE;
               IO.Edited = inboundICPFinal.Edited;
               IO.FE_D = inboundICPFinal.FE_D;
               IO.FE_T = inboundICPFinal.FE_T;
               IO.K_D = inboundICPFinal.K_D;
               IO.K_T = inboundICPFinal.K_T;
               IO.MG_D = inboundICPFinal.MG_D;
               IO.MG_T = inboundICPFinal.MG_T;
               IO.MN_D = inboundICPFinal.MN_D;
               IO.MN_T = inboundICPFinal.MN_T;
               IO.NA_D = inboundICPFinal.NA_D;
               IO.NA_T = inboundICPFinal.NA_T;
               IO.PB_D = inboundICPFinal.PB_D;
               IO.PB_T = inboundICPFinal.PB_T; 
               IO.Saved = inboundICPFinal.Saved;
               IO.SE_D = inboundICPFinal.SE_D;
               IO.SE_T = inboundICPFinal.SE_T;
               IO.tblSampleID = inboundICPFinal.tblSampleID;
               IO.Valid = inboundICPFinal.Valid;
               IO.ZN_D = inboundICPFinal.ZN_D;
               IO.ZN_T = inboundICPFinal.ZN_T;

               RWDE.InboundICPOrigionals.Add(IO); 
               RWDE.SaveChanges(); 
               
           }
           catch (Exception ex)
           {
               string msg = string.Format("Data base write failed with error: {0}", ex.Message);
               return InternalServerError(ex);
           }

            //return updated inboundICPFinal object with Id
            return Ok(inboundICPFinal);

            //not sure what this was being used for so I commented it out? returning the URI of the created resource?
            //return CreatedAtRoute("DefaultApi", new { id = inboundICPFinal.ID }, inboundICPFinal);
        }

        [Route("PostTest")]
        [HttpPost]
        public async Task<IHttpActionResult> PostTest(Test test)    // async
        {
            if (!ModelState.IsValid)
            {
                BadRequest("Model State is not Valid.");
            }
            test.Id = "55";
            return Ok(test);
        }
    }
}