using System;
using System.IO;

namespace RWICPreceiverApp.Models
{
    public class StationImageUploadModel
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSizeInBytes { get; set; }
        public long FileSizeInKb { get { return (long)Math.Ceiling((double)FileSizeInBytes / 1024); } }
        public int StationID { get; set; }
        public string User { get; set; }
        public bool Primary { get; set; }
        public int ImageTypeID { get; set; }
        public string ImageTypeType { get; set; }
        public string Description { get; set; }
        public int PhysHabYear { get; set; }
    }

    public class StationImageDownloadModel
    {
        public MemoryStream BlobStream { get; set; }
        public string BlobFileName { get; set; }
        public string BlobContentType { get; set; }
        public long BlobLength { get; set; }
    }

    public class DeleteStationImageModel
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public bool Deleted { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UpdatePrimaryStationImageModel
    {
        public int ID { get; set; }
        public int StationID { get; set; }
        public bool Primary { get; set; }
        public string ModifiedBy { get; set; }
        public int ImageTypeID { get; set; }
        public string ImageTypeType { get; set; }
        public string Description { get; set; }
        public int PhysHabYear { get; set; }
        public bool Updated { get; set; }
        public string ErrorMessage { get; set; }
    }
}