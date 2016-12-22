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
        Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent, int stationNum, string user);
        Task<BlobDownloadModel> DownloadBlob(int blobId);
    }

    public class BlobService : IBlobService
    {
        public async Task<List<BlobUploadModel>> UploadBlobs(HttpContent httpContent, int stationNum, string user)
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
            if(!SaveBlob(uploadList, stationNum, user))
            {
                return null;
            }

            return uploadList;
        }

        public async Task<BlobDownloadModel> DownloadBlob(int blobId)
        {
            // TODO: You must implement this helper method. It should retrieve blob info
            // from your database, based on the blobId. The record should contain the
            // blobName, which you should return as the result of this helper method.
            var blobName = GetBlobName(blobId);

            if (!String.IsNullOrEmpty(blobName))
            {
                var container = BlobHelper.GetBlobContainer();
                var blob = container.GetBlockBlobReference(blobName);

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
                var download = new BlobDownloadModel
                {
                    BlobStream = ms,
                    BlobFileName = fileName,
                    BlobLength = blob.Properties.Length,
                    BlobContentType = blob.Properties.ContentType
                };

                return download;
            }

            // Otherwise
            return null;
        }

        private string GetBlobName(int blobId)
        {
            return "RWLogo.png";
        }

        private bool SaveBlob(List<BlobUploadModel> uploadList, int stationNum, string user)
        {
            bool exists = true;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                int stationID = _db.Stations
                                   .Where(s => s.StationNumber == stationNum)
                                   .Select(s => s.ID)
                                   .FirstOrDefault();
                if(stationID > 0)
                {
                    foreach (var file in uploadList)
                    {
                        var stationFile = new StationFile()
                        {
                            StationID = stationID,
                            FileName = file.FileName,
                            FileUrl = file.FileUrl,
                            FileExt = Path.GetExtension(file.FileName),
                            FileSizeInBytes = file.FileSizeInBytes,
                            FileSizeInKb = file.FileSizeInKb,
                            CreatedBy = user,
                            CreatedDate = DateTime.Now,
                            Primary = true
                        };

                        _db.StationFiles.Add(stationFile);
                        _db.SaveChanges();
                    }
                }
                else
                {
                    return exists = false;
                }                
            }

            return exists;
        }
    }
}