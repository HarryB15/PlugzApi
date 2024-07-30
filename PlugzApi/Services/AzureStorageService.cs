using System;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PlugzApi.Services
{
	public class AzureStorageService
	{
        public async Task<List<string>?> GetPhotos(string blobContainer, string prefix)
        {
            List<string> images = new List<string>();
            try
            {
                var blobServiceClient = CommonService.Instance.GetBlobServiceClient();
                var containerClient = blobServiceClient.GetBlobContainerClient(blobContainer);
                var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.All, prefix: prefix);
                BlobClient blobClient;
                Azure.Response<BlobDownloadInfo> response;
                foreach (var blob in blobs)
                {
                    blobClient = containerClient.GetBlobClient(blob.Name);
                    response = await blobClient.DownloadAsync();
                    var image = "";
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            image += await streamReader.ReadLineAsync();
                        }
                        images.Add(image);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return null;
            }
            return images;
        }
        public async Task<bool> StoreImage(string image, string blobContainer, string path)
        {
            var success = false;
            try
            {
                var blobServiceClient = CommonService.Instance.GetBlobServiceClient();
                var containerClient = blobServiceClient.GetBlobContainerClient(blobContainer);
                await containerClient.CreateIfNotExistsAsync();
                BlobClient blobClient = containerClient.GetBlobClient(path);
                using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(image));
                await blobClient.UploadAsync(memoryStream, overwrite: true);
                success = true;
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return false;
            }
            return success;
        }
    }
}

