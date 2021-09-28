using System.IO;
using Microsoft.AspNetCore.Http;

namespace AzureBlobHelper.AzureBlobServices
{
    public class DownloadViewModel
    {
        public MemoryStream Stream { get; set; }
        public IFormFile File { get; set; }
    }
}
