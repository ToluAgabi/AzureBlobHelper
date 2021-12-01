using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace AzureBlobHelper.AzureBlobServices
{
    public class AzureBlobStorage : IAzureBlobStorage
    {
        #region " Public "

        private CloudStorageAccount StorageAccount { get; }

        public AzureBlobStorage(AzureBlobSettings settings)
        {
            if (CloudStorageAccount.TryParse(settings.StorageConnectionString, out  var storageAccount))
            {
                StorageAccount = storageAccount;
                
            }
            else
            {
                throw new Exception("unable to parse connection string");
            }
        }
        public async Task<CloudBlobContainer> CreateContainerAsync(string containerName)
        {

            var blobClient = StorageAccount.CreateCloudBlobClient();


            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();


            var permissions = await blobContainer.GetPermissionsAsync();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;

            await blobContainer.SetPermissionsAsync(permissions);




            return blobContainer;
        }



        public async Task<List<CloudBlobContainer>> ListContainersAsync()
        {
           
            var client = StorageAccount.CreateCloudBlobClient();
            BlobContinuationToken continuationToken = null;
            var containers = new List<CloudBlobContainer>();

            do
            {
                var response = await client.ListContainersSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                containers.AddRange(response.Results);

            } while (continuationToken != null);

            return containers;
        }

        public async Task UploadAsync(string blobName, string filePath, CloudBlobContainer blobContainer)
        {
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);

            //Upload
            await using var fileStream = File.Open(filePath, FileMode.Open);
            fileStream.Position = 0;
            await blockBlob.UploadFromStreamAsync(fileStream);
        }

        public async Task<string> UploadAsync(string blobName, Stream stream, CloudBlobContainer blobContainer, string contentType)
        {
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);
            blockBlob.Properties.ContentType = contentType;
            //Upload
            //  IDictionary<string, string> dict = new Dictionary<string, string>();

            stream.Position = 0;
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri.AbsoluteUri;
        }

        public async Task<DownloadViewModel> DownloadAsync(string blobName, CloudBlobContainer blobContainer)
        {
            var model = new DownloadViewModel();
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);

            //Download
            var stream = new MemoryStream {Position = 0};
            await blockBlob.DownloadToStreamAsync(stream);
            await blockBlob.FetchAttributesAsync();


            model.Stream = stream;
            return model;

        }


        public async Task DownloadAsync(string blobName, string path, CloudBlobContainer blobContainer)
        {
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);

            //Download
            await blockBlob.DownloadToFileAsync(path, FileMode.Create);
        }

        public async Task DeleteAsync(string blobName, CloudBlobContainer blobContainer)
        {
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);

            //Delete
            await blockBlob.DeleteAsync();
        }
   public async Task DeleteBlobByUrlAsync(string blobUrl)
        {
            //Blob
            var blob = new CloudBlockBlob(new Uri(blobUrl), StorageAccount.Credentials);
            
            //Delete
            await blob.DeleteAsync();
        }

        public async Task<bool> ExistsAsync(string blobName, CloudBlobContainer blobContainer)
        {
            //Blob
            var blockBlob =  GetBlockBlobAsync(blobName, blobContainer);

            //Exists
            return await blockBlob.ExistsAsync();
        }

        public async Task<List<AzureBlobItem>> ListAsync(CloudBlobContainer blobContainer)
        {
            return await GetBlobListAsync(blobContainer);
        }

        public async Task<List<AzureBlobItem>> ListAsync(string rootFolder, CloudBlobContainer blobContainer)
        {
            switch (rootFolder)
            {
                case "*":
                    return await ListAsync(blobContainer); //All Blobs
                case "/":
                    rootFolder = "";          //Root Blobs
                    break;
            }

            var list = await GetBlobListAsync(blobContainer);
            return list.Where(i => i.Folder == rootFolder).ToList();
        }

        public async Task<List<string>> ListFoldersAsync(CloudBlobContainer blobContainer)
        {
            var list = await GetBlobListAsync(blobContainer);
            return list.Where(i => !string.IsNullOrEmpty(i.Folder))
                       .Select(i => i.Folder)
                       .Distinct()
                       .OrderBy(i => i)
                       .ToList();
        }

        public async Task<List<string>> ListFoldersAsync(string rootFolder, CloudBlobContainer blobContainer)
        {
            if (rootFolder == "*" || rootFolder == "") return await ListFoldersAsync(blobContainer); //All Folders

            var list = await GetBlobListAsync(blobContainer);
            return list.Where(i => i.Folder.StartsWith(rootFolder))
                       .Select(i => i.Folder)
                       .Distinct()
                       .OrderBy(i => i)
                       .ToList();
        }

        #endregion

        #region " Private "




        public CloudBlobContainer GetContainerAsync(string containerName)
        {
          
            //Client
            var blobClient = StorageAccount.CreateCloudBlobClient();

            //Container
            var blobContainer = blobClient.GetContainerReference(containerName);
            // await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }

        private  CloudBlockBlob GetBlockBlobAsync(string blobName, CloudBlobContainer blobContainer)
        {

            //Blob
            var blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }


        public async Task<List<AzureBlobItem>> GetBlobListAsync(CloudBlobContainer blobContainer, bool useFlatListing = true)
        {
            //Container

            //List
            var list = new List<AzureBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                var resultSegment =
                    await blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (var item in resultSegment.Results)
                {
                    list.Add(new AzureBlobItem(item));
                }
            } while (token != null);

            return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
        }
        public async Task DeleteFile(CloudBlobContainer container, string uniqueFileIdentifier)
        {

            var blob = container.GetBlockBlobReference(uniqueFileIdentifier);
            await blob.DeleteIfExistsAsync();
        }

        public string GetContainerSasUri(CloudBlobContainer container, string storedPolicyName = null)
        {
            string sasContainerToken;

            if (storedPolicyName == null)
            {

                var adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // Set start time to five minutes before now to avoid clock skew.
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List
                };

                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {

                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            return container.Uri + sasContainerToken;
        }
        public string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;


            var blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {

                var adHocSas = new SharedAccessBlobPolicy()
                {
                    // Set start time to five minutes before now to avoid clock skew.
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
                };

                sasBlobToken = blob.GetSharedAccessSignature(adHocSas);
            }
            else
            {

                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            return blob.Uri + sasBlobToken;
        }
        static async void CreateSharedAccessPolicy(CloudBlobContainer container,
          string policyName)
        {
            //Create a new shared access policy and define its constraints.
            var sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List |
                    SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Delete
            };

            //Get the container's existing permissions.
            var permissions = await container.GetPermissionsAsync();

            //Add the new policy to the container's permissions, and set the container's permissions.
            permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
            await container.SetPermissionsAsync(permissions);
        }

      


        #endregion

    }

}
