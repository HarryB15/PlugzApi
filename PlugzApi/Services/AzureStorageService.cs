using System;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PlugzApi.Models;

namespace PlugzApi.Services
{
	public class AzureStorageService
	{
        public async Task<List<string>?> GetPhotos(string blobContainer, string prefix)
        {
            List<string> images = new List<string>();
            try
            {
                var containerClient = GetBlobContainer(blobContainer);
                var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix: prefix);
                BlobClient blobClient;
                Azure.Response<BlobDownloadInfo> response;
                foreach (var blob in blobs)
                {
                    blobClient = containerClient.GetBlobClient(blob.Name);
                    if (blobClient.Exists())
                    {
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
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return null;
            }
            return images;
        }
        public async Task<string?> GetPhoto(string blobContainer, string blobName)
        {
            string? image = null;
            try
            {
                var containerClient = GetBlobContainer(blobContainer);
                var blob = containerClient.GetBlobClient(blobName);
                Azure.Response<BlobDownloadInfo> response;
                if (blob.Exists())
                {
                    response = await blob.DownloadAsync();
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            image += await streamReader.ReadLineAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return null;
            }
            return image;
        }
        public async Task<bool> StoreImage(string image, string blobContainer, string path)
        {
            var success = false;
            try
            {
                var containerClient = GetBlobContainer(blobContainer);
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
        public async Task DeleteImages(string blobContainer, string prefix)
        {
            try
            {
                var containerClient = GetBlobContainer(blobContainer);
                var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix: prefix);
                foreach (var blob in blobs)
                {
                    await containerClient.DeleteBlobIfExistsAsync(blob.Name);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
        }
        public async Task UpdateListingImages(List<Images> images, string prefix)
        {
            try
            {
                var containerClient = GetBlobContainer("listing-images");
                var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.None, prefix: prefix);
                foreach (var blob in blobs)
                {
                    var imageIndex = int.Parse(blob.Name.Substring(blob.Name.LastIndexOf('/') + 1).Split('.')[0]);
                    if(imageIndex + 1 > images.Count)
                    {
                        await containerClient.DeleteBlobIfExistsAsync(blob.Name);
                    }
                }

                for(var i = 0; i < images.Count; i++)
                {
                    if (images[i].id != i)
                    {
                        await StoreImage(images[i].dataUrl, "listing-images", $"{prefix}/{i}.txt");
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
        }
        private BlobContainerClient GetBlobContainer(string containerName)
        {
            var blobServiceClient = CommonService.Instance.GetBlobServiceClient();
            return blobServiceClient.GetBlobContainerClient(containerName);
        }
    }
}

