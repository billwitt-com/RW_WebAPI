using RWICPreceiverApp.Helpers;
using RWICPreceiverApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace RWICPreceiverApp.Providers
{

    public class BlobStorageUploadProvider : MultipartFileStreamProvider
    {
        public List<StationImageUploadModel> Uploads { get; set; }

        public BlobStorageUploadProvider() : base(Path.GetTempPath())
        {
            Uploads = new List<StationImageUploadModel>();
        }

        public override Task ExecutePostProcessingAsync()
        {            
            // NOTE: FileData is a property of MultipartFileStreamProvider and is a list of multipart
            // files that have been uploaded and saved to disk in the Path.GetTempPath() location.
            foreach (var fileData in FileData)
            {
                if (!string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                {                   
                    string mediaType = fileData.Headers.ContentType.MediaType.ToString();
                    //If the file an image media type
                    if (System.Text.RegularExpressions.Regex.IsMatch(mediaType, "image/\\S+") )
                    {
                        // Sometimes the filename has a leading and trailing double-quote character
                        // when uploaded, so we trim it; otherwise, we get an illegal character exception
                        var fileName = Path.GetFileName(fileData.Headers.ContentDisposition.FileName.Trim('"'));

                        // Retrieve reference to a blob
                        var blobContainer = BlobHelper.GetBlobContainer();
                        var blob = blobContainer.GetBlockBlobReference(fileName);

                        bool blobExists = blob.Exists();
                        //if is doesn't exist, then add it.
                        if (!blobExists)
                        {
                            // Set the blob content type
                            blob.Properties.ContentType = fileData.Headers.ContentType.MediaType;                    

                            // Upload file into blob storage, basically copying it from local disk into Azure
                            using (var fs = File.OpenRead(fileData.LocalFileName))
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
                            File.Delete(fileData.LocalFileName);                            
                        }

                        // Create blob upload model with properties from blob info
                        var blobUpload = new StationImageUploadModel
                        {
                            FileName = blob.Name,
                            FileUrl = blob.Uri.AbsoluteUri,
                            FileSizeInBytes = blob.Properties.Length
                        };

                        // Add uploaded blob to the list
                        Uploads.Add(blobUpload);
                    }
                }                
            }

            return base.ExecutePostProcessingAsync();
        }      
    }
}