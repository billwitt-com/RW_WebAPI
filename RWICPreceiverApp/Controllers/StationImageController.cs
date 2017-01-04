using RWICPreceiverApp.Models;
using RWICPreceiverApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace RWICPreceiverApp.Controllers
{
    [RoutePrefix("api/StationImage")]
    public class StationImageController : ApiController
    {
        // Interface in place so you can resolve with IoC container of your choice
        private readonly IBlobService _service = new BlobService();

        /// <summary>
        /// Uploads one or more blob files.
        /// </summary>
        /// <param name="stationID">The ID of the station.</param>
        /// <param name="user">The name of the user uploading the image.</param>
        /// <returns></returns>
        [ResponseType(typeof(List<BlobUploadModel>))]
        [Route("PostStationImage/{stationID:int}/{user}")]
        [HttpPost]
        public async Task<IHttpActionResult> PostStationImage(int stationID, string user)
        {
            try
            {
                //add?
                //bool validated = false;
                //HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 

                //ValidateCredentials VC = new ValidateCredentials();
                //validated = VC.checkCreds(request);
                //if (!validated)
                //    return Unauthorized();
                var requestt = Request;
                // This endpoint only supports multipart form data
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                {
                    return StatusCode(HttpStatusCode.UnsupportedMediaType);
                }

                if(stationID <= 0)
                {
                    return BadRequest();
                }

                // Call service to perform upload, then check result to return as content
                var result = await _service.UploadBlobs(Request.Content, stationID, user);
                if (result != null && result.Count > 0)
                {
                    return Ok(result);
                }

                // Otherwise
                return BadRequest();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Downloads a blob file.
        /// </summary>
        /// <param name="stationID">The ID of the blob.</param>
        /// <returns>A Stations Image</returns>
        [Route("GetStationImage/{stationID}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetStationImage(int stationID)
        {
            // IMPORTANT: This must return HttpResponseMessage instead of IHttpActionResult

            try
            {
                var result = await _service.DownloadBlob(stationID);
                if (result == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                // Reset the stream position; otherwise, download will not work
                result.BlobStream.Position = 0;

                // Create response message with blob stream as its content
                var message = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(result.BlobStream)
                };

                // Set content headers
                message.Content.Headers.ContentLength = result.BlobLength;
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(result.BlobContentType);
                message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = HttpUtility.UrlDecode(result.BlobFileName),
                    Size = result.BlobLength
                };

                return message;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }
    }
}
