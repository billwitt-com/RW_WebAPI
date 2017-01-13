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
        Task<List<StationImageUploadModel>> UploadBlobs(IList<HttpContent> files, StationImageUploadModel model);
        Task<StationImageDownloadModel> DownloadBlob(int stationID);
        Task<DeleteStationImageModel> DeleteBlob(DeleteStationImageModel model);
        Task<UpdatePrimaryStationImageModel> UpdatedStationImage(UpdatePrimaryStationImageModel model);
    }

    public class BlobService : IBlobService
    {
        private List<StationImageUploadModel> Uploads { get; set; }
        public async Task<List<StationImageUploadModel>> UploadBlobs(IList<HttpContent> files, StationImageUploadModel model)
        {            
            Uploads = new List<StationImageUploadModel>();
            Uploads = await UploadBlobsToAzure(files, model);

            //Task taskUpload = await UploadBlobsToAzure(files, model)
            //    .ContinueWith(task =>
            //    {
            //        Task<List<StationImageUploadModel>> uploadList = null;

            //        if (task.IsFaulted || task.IsCanceled)
            //        {
            //            throw task.Exception;
            //        }

            //        if (task.Status == TaskStatus.RanToCompletion)
            //        {                       
            //            var result = task.Result;
            //            Uploads = result.ToList();
            //        }
                        
            //        return Uploads;
            //    });

            //taskUpload.Wait();
           
            if (!SaveImage(Uploads))
            {
                return null;
            }

            return Uploads;
        }

        private async Task<List<StationImageUploadModel>> UploadBlobsToAzure(IList<HttpContent> files, StationImageUploadModel uploadedModel)
        {
            // NOTE: FileData is a property of MultipartFileStreamProvider and is a list of multipart
            // files that have been uploaded and saved to disk in the Path.GetTempPath() location.
            foreach (var fileData in files)
            {
                if (!string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                {
                    string mediaType = fileData.Headers.ContentType.MediaType.ToString();

                    //If the file an image media type
                    if (System.Text.RegularExpressions.Regex.IsMatch(mediaType, "image/\\S+"))
                    {
                        // Sometimes the filename has a leading and trailing double-quote character
                        // when uploaded, so we trim it; otherwise, we get an illegal character exception
                        var fileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));
                        var path = HttpRuntime.AppDomainAppPath;
                        string directoryName = Path.Combine(path, "StationImage");

                        if (!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(@directoryName);
                        }

                        string filePath = Path.Combine(directoryName, fileName);
                        //Deletion exists file  
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        // Retrieve reference to a blob
                        var blobContainer = BlobHelper.GetBlobContainer();
                        var blob = blobContainer.GetBlockBlobReference(fileName);

                        bool blobExists = blob.Exists();

                        //if is doesn't exist, then add it.
                        if (!blobExists)
                        {
                            // Set the blob content type
                            blob.Properties.ContentType = fileData.Headers.ContentType.MediaType;

                            Stream input = await fileData.ReadAsStreamAsync();
                            //Directory.CreateDirectory(@directoryName);  
                            using (Stream file = File.OpenWrite(filePath))
                            {
                                input.CopyTo(file);
                                //close file  
                                file.Close();
                            }

                            // Upload file into blob storage, basically copying it from local disk into Azure
                            using (var fs = File.OpenRead(filePath))
                            {    
                                long fileSizeInKB = (long)(fs.Length / 1024);
                                //If the image is greater than 1 MB don't save it 
                                if (fileSizeInKB > 1001)
                                {
                                    continue;
                                }

                                blob.UploadFromStream(fs);                                
                            }

                            // Delete local file from disk
                            File.Delete(filePath);
                        }

                        // Create blob upload model with properties from blob info
                        var stationImageUploadModel = new StationImageUploadModel
                        {
                            FileName = blob.Name,
                            FileUrl = blob.Uri.AbsoluteUri,
                            FileSizeInBytes = blob.Properties.Length,
                            StationID = uploadedModel.StationID,
                            User = uploadedModel.User,
                            Primary = uploadedModel.Primary,
                            ImageTypeID = uploadedModel.ImageTypeID,
                            Description = uploadedModel.Description,
                            PhysHabYear = uploadedModel.PhysHabYear
                        };

                        // Add uploaded blob to the list
                        Uploads.Add(stationImageUploadModel);
                    }
                }
            }

            return Uploads;
        }

        public async Task<StationImageDownloadModel> DownloadBlob(int ID)
        {           
            var imageName = GetImageName(ID);

            if (!String.IsNullOrEmpty(imageName))
            {
                var container = BlobHelper.GetBlobContainer();
                var blob = container.GetBlockBlobReference(imageName);
               
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

        private bool SaveImage(List<StationImageUploadModel> uploadList)
        {
            bool exists = true;
            int stationID = uploadList[0].StationID;

            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                int stationIDFromStationTable = 0;
                if (uploadList.Count > 0)
                {
                    
                    stationIDFromStationTable = _db.Stations
                                                   .Where(s => s.ID == stationID)
                                                   .Select(s => s.ID)
                                                   .FirstOrDefault();
                }

                if (stationIDFromStationTable > 0)
                {
                    StationImage newStationImage = new StationImage();
                    bool primary = false;
                    bool enforcePrimary = false;
                    foreach (var uploadModel in uploadList)
                    {
                        string fileExt = Path.GetExtension(uploadModel.FileName);
                        long fileSizeInKb = uploadModel.FileSizeInKb;
                        primary = uploadModel.Primary;

                        enforcePrimary = _db.StationImageTypes.Find(uploadModel.ImageTypeID).EnforcePrimary;

                        if (!enforcePrimary)
                        {
                            primary = false;
                        }
                        else
                        {
                            primary = uploadModel.Primary;
                        }

                        var stationImage = new StationImage()
                        {
                            StationID = uploadModel.StationID,
                            FileName = uploadModel.FileName,
                            FileUrl = uploadModel.FileUrl,
                            FileExt = fileExt,
                            FileSizeInBytes = uploadModel.FileSizeInBytes,
                            FileSizeInKb = uploadModel.FileSizeInKb,
                            Primary = primary,
                            ImageType = uploadModel.ImageTypeID,
                            Description = uploadModel.Description,
                            PhysHabYear = uploadModel.PhysHabYear,
                            CreatedBy = uploadModel.User,
                            CreatedDate = DateTime.Now
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

        public async Task<UpdatePrimaryStationImageModel> UpdatedStationImage(UpdatePrimaryStationImageModel model)
        {
            using (RiverWatchEntities _db = new RiverWatchEntities())
            {
                var updatePrimaryStationImageModel = new UpdatePrimaryStationImageModel();
                bool enforcePrimary = false;
                enforcePrimary = _db.StationImageTypes.Find(model.ImageTypeID).EnforcePrimary;

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
                        ImageTypeID = model.ImageTypeID,
                        Description = model.Description,
                        PhysHabYear = model.PhysHabYear,
                        Updated = false,
                        ErrorMessage = "No Station Images were found to update."
                    };
                }
                else
                {
                    foreach (var stationImage in stationImages)
                    {
                        //the station image being updated
                        if (stationImage.ID == model.ID)
                        {
                            if (!enforcePrimary)
                            {
                                stationImage.Primary = false;
                            }
                            else
                            {
                                stationImage.Primary = model.Primary;
                            }
                            stationImage.ImageType = model.ImageTypeID;
                            stationImage.Description = model.Description;
                            stationImage.ModifiedBy = model.ModifiedBy;
                            stationImage.ModifiedDate = DateTime.Now;
                            stationImage.PhysHabYear = model.PhysHabYear;
                        }                       
                        else if (model.Primary && enforcePrimary && (stationImage.ID != model.ID))
                        {
                            stationImage.Primary = false;
                        }

                        await _db.SaveChangesAsync();

                        updatePrimaryStationImageModel = new UpdatePrimaryStationImageModel()
                        {
                            ID = model.ID,
                            StationID = model.StationID,
                            Primary = model.Primary,
                            ImageTypeID = model.ImageTypeID,
                            Description = model.Description,
                            PhysHabYear = model.PhysHabYear,
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