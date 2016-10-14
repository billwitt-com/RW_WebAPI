using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using RWICPreceiverApp.Models;

namespace RWICPreceiverApp.Controllers
{
    public class InboundICPFinals2Controller : ApiController
    {
        private RiverWatchEntities db = new RiverWatchEntities();

        // GET: api/InboundICPFinals2
        public IQueryable<InboundICPFinal> GetInboundICPFinals()
        {
            return db.InboundICPFinals;
        }

        // GET: api/InboundICPFinals2/5
        [ResponseType(typeof(InboundICPFinal))]
        public async Task<IHttpActionResult> GetInboundICPFinal(int id)
        {
            InboundICPFinal inboundICPFinal = await db.InboundICPFinals.FindAsync(id);
            if (inboundICPFinal == null)
            {
                return NotFound();
            }

            return Ok(inboundICPFinal);
        }

        // PUT: api/InboundICPFinals2/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutInboundICPFinal(int id, InboundICPFinal inboundICPFinal)
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
                await db.SaveChangesAsync();
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

        // POST: api/InboundICPFinals2
        [HttpPost]
        [ResponseType(typeof(InboundICPFinal))]
        public async Task<IHttpActionResult> PostInboundICPFinal(InboundICPFinal inboundICPFinal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.InboundICPFinals.Add(inboundICPFinal);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = inboundICPFinal.ID }, inboundICPFinal);
        }

        // DELETE: api/InboundICPFinals2/5
        [ResponseType(typeof(InboundICPFinal))]
        public async Task<IHttpActionResult> DeleteInboundICPFinal(int id)
        {
            InboundICPFinal inboundICPFinal = await db.InboundICPFinals.FindAsync(id);
            if (inboundICPFinal == null)
            {
                return NotFound();
            }

            db.InboundICPFinals.Remove(inboundICPFinal);
            await db.SaveChangesAsync();

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