using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GhostNetwork.Publications.AzureBlobStorage
{
    public interface IImagesStorage
    {
        Task<string> UploadImagesAsync(Stream stream, string fileName);

        Task DeleteImagesAsync(string imagesUrl);
    }
}
