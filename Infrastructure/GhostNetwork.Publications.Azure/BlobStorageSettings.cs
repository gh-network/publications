using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GhostNetwork.Publications.Azure
{
    public class BlobStorageSettings
    {
        public BlobStorageSettings(BlobServiceClient blobServiceClient, string containerName)
        {
            BlobServiceClient = blobServiceClient;
            ContainerName = containerName;
        }

        public BlobServiceClient BlobServiceClient { get; }

        public string ContainerName { get; }
    }
}
