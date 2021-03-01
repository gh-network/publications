using GhostNetwork.Publications.AzureBlobStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GhostNetwork.Publications.Azure
{
    public class ImagesStorage : IImagesStorage
    {
        private readonly BlobStorageSettings storageSettings;

        public ImagesStorage(BlobStorageSettings storageSettings)
        {
            this.storageSettings = storageSettings;
        }

        public async Task<string> UploadImagesAsync(Stream stream, string fileName)
        {
            var containerClient = storageSettings.BlobServiceClient.GetBlobContainerClient(storageSettings.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream);
            return containerClient.Uri.AbsoluteUri + "/" + fileName;
        }

        public async Task DeleteImagesAsync(string imagesUrl)
        {
            var containerClient = storageSettings.BlobServiceClient.GetBlobContainerClient(storageSettings.ContainerName);
            var arr = imagesUrl.Split(new char[] { '/' });
            await containerClient.DeleteBlobIfExistsAsync(arr[arr.Length - 1]);
        }
    }
}
