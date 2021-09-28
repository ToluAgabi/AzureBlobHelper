using System;
using Microsoft.Azure.Storage.Blob;

namespace AzureBlobHelper.AzureBlobServices
{
    public class AzureBlobItem
    {
        private IListBlobItem Item { get; }

        public AzureBlobItem(IListBlobItem item)
        {
            this.Item = item;
            // Constructor Initialization
        }


        private bool IsBlockBlob => Item.GetType() == typeof(CloudBlockBlob);
        private bool IsPageBlob => Item.GetType() == typeof(CloudPageBlob);
        private bool IsDirectory => Item.GetType() == typeof(CloudBlobDirectory);

        private string BlobName => IsBlockBlob ? ((CloudBlockBlob)Item).Name :
                                  IsPageBlob ? ((CloudPageBlob)Item).Name :
                                  IsDirectory ? ((CloudBlobDirectory)Item).Prefix :
                                  "";

        public string Folder => BlobName.Contains("/") ?
                         BlobName.Substring(0, BlobName.LastIndexOf("/", StringComparison.Ordinal)) : "";

        public string Name => BlobName.Contains("/") ?
                         BlobName.Substring(BlobName.LastIndexOf("/", StringComparison.Ordinal) + 1) : BlobName;
    }
}
