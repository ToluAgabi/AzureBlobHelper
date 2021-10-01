
# AzureBlobHelper
Wrapper for Azure Blob Storage SDK V11
# Nuget
https://www.nuget.org/packages/AzureBlobStorageHelper


# Usage
ASP.NET CORE

### Startup .cs
 ```   services.AddScoped<IAzureBlobStorage>(_ => new AzureBlobStorage(new AzureBlobSettings(storageConnectionString: "Configuration["Blob_StorageAccount"]"))); ```
 
#### DI

     private readonly IAzureBlobStorage blobHelper;
      public ConstructorName(IAzureBlobStorage helper)
            {
    			blobHelper = helper;
    		}

 
 
 
 ### Direct Initialisation
 
 ``` var blobHelper = new AzureBlobStorage(new AzureBlobSettings("storage connection string")); ```
 

**Create Container**

    var response = await blobHelper.ContainerAsync(string containerName);

**Get Container**

    var response = await blobHelper.GetContainerAsync(string containerName);

### Documentation Ongoing

**Helper Methods**

		    Task<List<AzureBlobItem>> GetBlobListAsync(CloudBlobContainer blobContainer, bool useFlatListing);
            Task UploadAsync(string blobName, string filePath, CloudBlobContainer blobContainer);
            Task<string> UploadAsync(string blobName, Stream stream, CloudBlobContainer blobContainer, string contentType = "application/octet-stream");
            Task DownloadAsync(string blobName, CloudBlobContainer blobContainer);
            Task DownloadAsync(string blobName, string path, CloudBlobContainer blobContainer);
            Task DeleteAsync(string blobName, CloudBlobContainer blobContainer);
            Task<bool> ExistsAsync(string blobName, CloudBlobContainer blobContainer);
            Task<List<AzureBlobItem>> ListAsync(CloudBlobContainer blobContainer);
            Task<List<AzureBlobItem>> ListAsync(string rootFolder, CloudBlobContainer blobContainer);
            Task<List<string>> ListFoldersAsync(CloudBlobContainer blobContainer);
            Task<List<string>> ListFoldersAsync(string rootFolder, CloudBlobContainer blobContainer);
            Task<CloudBlobContainer> CreateContainerAsync(string containerName);
            Task<List<CloudBlobContainer>> ListContainersAsync();
            CloudBlobContainer GetContainerAsync(string containerName);
            string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null);
            string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null);
            Task DeleteFile(CloudBlobContainer container, string uniqueFileIdentifier);

