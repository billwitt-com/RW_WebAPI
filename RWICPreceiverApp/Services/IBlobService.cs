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
        Task<List<StationImageUploadModel>> UploadBlobs(HttpContent httpContent, int stationID, string user, bool primary);
        Task<StationImageDownloadModel> DownloadBlob(int stationID);
        Task<DeleteStationImageModel> DeleteBlob(DeleteStationImageModel model);
        Task<UpdatePrimaryStationImageModel> UpdatedStationImagePrimaryStatus(UpdatePrimaryStationImageModel model);
    }

    public class BlobService : IBlobService
    {
        public async Task<List<StationImageUploadModel>> UploadBlobs(HttpContent httpContent, int stationID, string user, bool primary)
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
            if (!SaveImage(uploadList, stationID, user, primary))
            {
                return null;
            }

            return uploadList;
        }

        public async Task<StationImageDownloadModel> DownloadBlob(int ID)
        {
            // TODO: You must implement this helper method. It should retrieve blob info
            // from your database, based on the blobId. The record should contain the
            // blobName, which you should return as the result of this helper method.
            var imageName = GetImageName(ID);

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
                var downloadModel = new StationImageDownloadModel
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

        private string GetImageName(int ID)
        {
            string imageName = string.Empty;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                imageName = _db.StationImages
                               .Where(si => si.ID == ID)
                               .Select(si => si.FileName)
                               .FirstOrDefault();

            }
            return imageName;
        }

        private bool SaveImage(List<StationImageUploadModel> uploadList, int stationID, string user, bool primary)
        {
            bool exists = true;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                int stationIDFromStationTable = _db.Stations
                                                   .Where(s => s.ID == stationID)
                                                   .Select(s => s.ID)
                                                   .FirstOrDefault();
                if (stationIDFromStationTable > 0)
                {
                    StationImage newStationImage = new StationImage();
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
                            Primary = primary
                        };

                        _db.StationImages.Add(stationImage);
                        _db.SaveChanges();

                        newStationImage = stationImage;
                    }

                    //set all other to not primary if the new one is primary
                    if (primary)
                    {
                        var stationImages = _db.StationImages
                                               .Where(si => si.StationID == stationID &&
                                                      !si.FileName.Equals(newStationImage.FileName))
                                               .ToList<StationImage>();

                        foreach (var stationImage in stationImages)
                        {
                            stationImage.Primary = false;
                            _db.SaveChanges();
                        }
                    }
                }
                else
                {
                    exists = false;
                }
            }

            return exists;
        }

        public async Task<DeleteStationImageModel> DeleteBlob(DeleteStationImageModel model)
        {
            var deleteStationImageModel = new DeleteStationImageModel();

            if (!String.IsNullOrEmpty(model.FileName))
            {
                var container = BlobHelper.GetBlobContainer();
                var blob = container.GetBlockBlobReference(model.FileName);
                bool deleted = false;
                string errorMessage = string.Empty;

                using (RiverWatchEntities _db = new RiverWatchEntities())
                {
                    var sameImageUrls = _db.StationImages
                                            .Where(si => si.FileUrl.Equals(model.FileUrl))
                                            .ToList();

                    //if there is only one file, then delete it from the blob storage.
                    if (sameImageUrls.Count() == 1)
                    {
                        await blob.DeleteIfExistsAsync();
                    }

                    var stationImageToDelete = _db.StationImages.Find(model.ID);
                    _db.StationImages.Remove(stationImageToDelete);
                    _db.SaveChanges();

                    deleted = true;
                }

                if (!deleted)
                {
                    errorMessage = "Image could not be deleted. Contact an Administrator if this problem continues.";
                }

                deleteStationImageModel = new DeleteStationImageModel()
                {
                    ID = model.ID,
                    FileName = model.FileName,
                    FileUrl = model.FileUrl,
                    Deleted = deleted,
                    ErrorMessage = errorMessage
                };

                return deleteStationImageModel;
            }

            // Otherwise
            deleteStationImageModel = new DeleteStationImageModel()
            {
                ID = model.ID,
                FileName = model.FileName,
                Deleted = false,
                ErrorMessage = "Image could not be found. Contact an Administrator if this problem continues."
            };

            return deleteStationImageModel;
        }

        public async Task<UpdatePrimaryStationImageModel> UpdatedStationImagePrimaryStatus(UpdatePrimaryStationImageModel model)
        {
            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                var updatePrimaryStationImageModel = new UpdatePrimaryStationImageModel();

                var stationImages = _db.StationImages
                                        .Where(si => si.StationID == model.StationID)
                                        .ToList<StationImage>();
                if (stationImages.Count() == 0)
                {
                    updatePrimaryStationImageModel = new UpdatePrimaryStationImageModel()
                    {
                        ID = model.ID,
                        StationID = model.StationID,
                        Primary = model.Primary,
                        Updated = false,
                        ErrorMessage = "No Station Images were found to update."
                    };
                }
                else
                {
                    foreach (var stationImage in stationImages)
                    {
                        if (stationImage.ID == model.ID)
                        {
                            stationImage.Primary = model.Primary;
                        }
                        else if (model.Primary && (stationImage.ID != model.ID))
                        {
                            stationImage.Primary = false;
                        }

                        await _db.SaveChangesAsync();

                        updatePrimaryStationImageModel = new UpdatePrimaryStationImageModel()
                        {
                            ID = model.ID,
                            StationID = model.StationID,
                            Primary = model.Primary,
                            Updated = true,
                            ErrorMessage = string.Empty
                        };
                    }
                }
                return updatePrimaryStationImageModel;
            }
        }
    }
}