using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using RWICPreceiverApp.Models;
using System.Diagnostics.Tracing;

namespace RWICPreceiverApp.Controllers
{
    public class InboundICPFinals1Controller : ApiController
    {
        private RiverWatchEntities db = new RiverWatchEntities();

        // GET: api/InboundICPFinals1
        public IQueryable<InboundICPFinal> GetInboundICPFinals()
        {
            return db.InboundICPFinals;
        }

        // GET: api/InboundICPFinals1/5
        [ResponseType(typeof(InboundICPFinal))]
        public IHttpActionResult GetInboundICPFinal(int id)
        {
            InboundICPFinal inboundICPFinal = db.InboundICPFinals.Find(id);
            if (inboundICPFinal == null)
            {
                return NotFound();
            }
            return Ok(inboundICPFinal);
        }

        // PUT: api/InboundICPFinals1/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutInboundICPFinal(int id, InboundICPFinal inboundICPFinal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != inboundICPFinal.ID)
            {
                return BadRequest();
            }

            db.Entry(inboundICPFinal).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InboundICPFinalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/InboundICPFinals1
        [ResponseType(typeof(InboundICPFinal))]
        [AllowAnonymous]
        public  IHttpActionResult PostInboundICPFinal(InboundICPFinal inboundICPFinal)
        {          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.InboundICPFinals.Add(inboundICPFinal);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = inboundICPFinal.ID }, inboundICPFinal);
        }

        // DELETE: api/InboundICPFinals1/5
        [ResponseType(typeof(InboundICPFinal))]
        public IHttpActionResult DeleteInboundICPFinal(int id)
        {
            InboundICPFinal inboundICPFinal = db.InboundICPFinals.Find(id);
            if (inboundICPFinal == null)
            {
                return NotFound();
            }

            db.InboundICPFinals.Remove(inboundICPFinal);
            db.SaveChanges();

            return Ok(inboundICPFinal);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool InboundICPFinalExists(int id)
        {
            return db.InboundICPFinals.Count(e => e.ID == id) > 0;
        }
    }
}