using Newtonsoft.Json;
using RWICPreceiverApp.Helpers;
using RWICPreceiverApp.Models;
using RWICPreceiverApp.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RWICPreceiverApp.Services
{
    public interface IBlobService
    {
        Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent, int stationID, string user);
        Task<BlobDownloadModel> DownloadBlob(int stationID);
    }

    public class BlobService : IBlobService
    {
        public async Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent, int stationID, string user)
        {
            var blobUploadProvider = new BlobStorageUploadProvider();          

            var uploadList = await httpContent.ReadAsMultipartAsync(blobUploadProvider)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        throw task.Exception;
                    }

                    var provider = task.Result;
                    return provider.Uploads.ToList();
                });
                       
            // TODO: Use data in the list to store blob info in your
            // database so that you can always retrieve it later.
            if(!SaveImage(uploadList, stationID, user))
            {
                return null;
            }

            return uploadList;
        }

        public async Task<BlobDownloadModel> DownloadBlob(int stationID)
        {
            // TODO: You must implement this helper method. It should retrieve blob info
            // from your database, based on the blobId. The record should contain the
            // blobName, which you should return as the result of this helper method.
            var imageName = GetImageName(stationID);

            if (!String.IsNullOrEmpty(imageName))
            {
                var container = BlobHelper.GetBlobContainer();
                var blob = container.GetBlockBlobReference(imageName);

                // Download the blob into a memory stream. Notice that we're not putting the memory
                // stream in a using statement. This is because we need the stream to be open for the
                // API controller in order for the file to actually be downloadable. The closing and
                // disposing of the stream is handled by the Web API framework.
                var ms = new MemoryStream();
                await blob.DownloadToStreamAsync(ms);

                // Strip off any folder structure so the file name is just the file name
                var lastPos = blob.Name.LastIndexOf('/');
                var fileName = blob.Name.Substring(lastPos + 1, blob.Name.Length - lastPos - 1);

                // Build and return the download model with the blob stream and its relevant info
                var downloadModel = new BlobDownloadModel
                {
                    BlobStream = ms,
                    BlobFileName = fileName,
                    BlobLength = blob.Properties.Length,
                    BlobContentType = blob.Properties.ContentType
                };

                return downloadModel;
            }

            // Otherwise
            return null;
        }

        private string GetImageName(int stationID)
        {
            string imageName = string.Empty;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                imageName = _db.StationImages
                               .Where(si => si.StationID == stationID && si.Primary == true)
                               .Select(si => si.FileName)
                               .FirstOrDefault();

            }
            return imageName;
        }

        private bool SaveImage(List<BlobUploadModel> uploadList, int stationID, string user)
        {
            bool exists = true;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                int stationIDFromStationTable = _db.Stations
                                                   .Where(s => s.ID == stationID)
                                                   .Select(s => s.ID)
                                                   .FirstOrDefault();
                if(stationIDFromStationTable > 0)
                {
                    foreach (var file in uploadList)
                    {
                        string fileExt = Path.GetExtension(file.FileName);
                        long fileSizeInKb = file.FileSizeInKb;
                        var stationImage = new StationImage()
                        {
                            StationID = stationID,
                            FileName = file.FileName,
                            FileUrl = file.FileUrl,
                            FileExt = fileExt,
                            FileSizeInBytes = file.FileSizeInBytes,
                            FileSizeInKb = file.FileSizeInKb,
                            CreatedBy = user,
                            CreatedDate = DateTime.Now,
                            Primary = true
                        };

                        _db.StationImages.Add(stationImage);
                        _db.SaveChanges();
                        //if ((fileExt.ToLower().Equals(".jpg") || fileExt.ToLower().Equals(".png")) && fileSizeInKb < 1001)
                        //{

                        //}
                        //else
                        //{
                        //    exists = false;
                        //}                        
                    }
                }
                else
                {
                    exists = false;
                }                
            }

            return exists;
        }
    }
}