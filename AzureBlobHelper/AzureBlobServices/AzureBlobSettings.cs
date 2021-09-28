using System;

namespace AzureBlobHelper.AzureBlobServices
{
    public  class AzureBlobSettings
    {
        public string StorageConnectionString { get; }

        public AzureBlobSettings(string storageConnectionString)
        {
            if (string.IsNullOrEmpty(storageConnectionString))
                throw new ArgumentNullException(nameof(StorageConnectionString));

 

            this.StorageConnectionString = storageConnectionString;
        }

    }
}
