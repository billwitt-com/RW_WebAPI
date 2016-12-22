using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RWICPreceiverApp.Helpers
{
    public class BlobHelper
    {
        public static CloudBlobContainer GetBlobContainer()
        {
            // Pull these from config
            var blobStorageConnectionString = ConfigurationManager.AppSettings["BlobStorageConnectionString"];
            var blobStorageContainerName = ConfigurationManager.AppSettings["BlobStorageContainerName"];

            // Create blob client and return reference to the container
            var blobStorageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            var blobClient = blobStorageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(blobStorageContainerName);
        }
    }
}