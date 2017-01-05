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
        /// <param name="primary">Wheather the image is primary or not.</param>
        /// <returns></returns>
        [ResponseType(typeof(List<StationImageUploadModel>))]
        [Route("PostStationImage/{stationID:int}/{user}/{primary:bool}")]
        [HttpPost]
        public async Task<IHttpActionResult> PostStationImage(int stationID, string user, bool primary)
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
                var result = await _service.UploadBlobs(Request.Content, stationID, user, primary);
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
        /// Downloads a station image file.
        /// </summary>
        /// <param name="ID">The ID of the Station image.</param>
        /// <returns>A Stations Image</returns>
        [Route("GetStationImage/{ID}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetStationImage(int ID)
        {
            // IMPORTANT: This must return HttpResponseMessage instead of IHttpActionResult

            try
            {
                var result = await _service.DownloadBlob(ID);
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

        /// <summary>
        /// Deletes the specified Station Image
        /// </summary>
        /// <param name="model">The DeleteStationImageModel to be deleted.</param>
        /// <returns>IHttpActionResult</returns>
        [ResponseType(typeof(DeleteStationImageModel))]
        [Route("DeleteStationImage")]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteStationImage(DeleteStationImageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //add?
                //bool validated = false;
                //HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 

                //ValidateCredentials VC = new ValidateCredentials();
                //validated = VC.checkCreds(request);
                //if (!validated)
                //    return Unauthorized();               

                if (model.ID <= 0)
                {
                    return BadRequest();
                }

                // Call service to perform delete, then check result to return as content
                var deleteStationImageModel = await _service.DeleteBlob(model);
                if (deleteStationImageModel != null)
                {
                    if (deleteStationImageModel.Deleted)
                    {
                        return Ok(deleteStationImageModel);
                    }
                    else
                    {
                        return BadRequest(deleteStationImageModel.ErrorMessage);
                    }
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
        /// Updates the primary status of thespecified Station Image
        /// </summary>
        /// <param name="model">The UpdatePrimaryStationImageModel for Updating the Primary Status.</param>
        /// <returns>IHttpActionResult</returns>
        [ResponseType(typeof(UpdatePrimaryStationImageModel))]
        [Route("UpdateStationImagePrimaryStatus")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateStationImagePrimaryStatus(UpdatePrimaryStationImageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //add?
                //bool validated = false;
                //HttpRequestMessage request = ControllerContext.Request; // this seems to be same as request 

                //ValidateCredentials VC = new ValidateCredentials();
                //validated = VC.checkCreds(request);
                //if (!validated)
                //    return Unauthorized();               

                if (model.ID <= 0)
                {
                    return BadRequest();
                }

                // Call service to perform update, then check result to return as content
                var updatePrimaryStationImageModel = await _service.UpdatedStationImagePrimaryStatus(model);
                if (updatePrimaryStationImageModel != null)
                {
                    if (updatePrimaryStationImageModel.Updated)
                    {
                        return Ok(updatePrimaryStationImageModel);
                    }
                    else
                    {
                        return BadRequest(updatePrimaryStationImageModel.ErrorMessage);
                    }
                }

                // Otherwise
                return BadRequest();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
