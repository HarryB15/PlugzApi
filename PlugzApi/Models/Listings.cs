using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models.Security;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Text;

namespace PlugzApi.Models
{
    public class Listings : Base
    {
        public int listingId { get; set; }
        public string listingDesc { get; set; } = "";
        public decimal price { get; set; }
        public byte minUserRatings { get; set; }
        public short? minPurchases { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public bool isPublic { get; set; }
        public DateTime createdDatetime { get; set; }
        public DateTime expiryDatetime { get; set; }
        public int expiryHours { get; set; }
        public string userName { get; set; } = "";
        public bool isConnected { get; set; }
        public List<string> images { get; set; } = new List<string>();

        public async Task InsListings()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsListings", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@listingDesc", SqlDbType.NVarChar).Value = listingDesc;
                cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@minUserRatings", SqlDbType.TinyInt).Value = minUserRatings;
                cmd.Parameters.Add("@minPurchases", SqlDbType.SmallInt).Value = minPurchases;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                cmd.Parameters.Add("@isPublic", SqlDbType.Bit).Value = isPublic;
                cmd.Parameters.Add("@expiryHours", SqlDbType.Int).Value = expiryHours;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    listingId = (int)sdr["listingId"];
                    await StoreImages();
                }
                else
                {
                    error = CommonService.Instance.GetUnexpectedErrrorMsg();
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        private async Task StoreImages()
        {
            try
            {
                var blobServiceClient = CommonService.Instance.GetBlobServiceClient();
                var containerClient = blobServiceClient.GetBlobContainerClient("listing-images");
                await containerClient.CreateIfNotExistsAsync();
                var hashedListingId =  CommonService.Instance.HashString(listingId.ToString(), "Listings");
                for(var i = 0; i < images.Count; i++)
                {
                    BlobClient blobClient = containerClient.GetBlobClient($"{hashedListingId}/{i}.txt");
                    using MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(images[i]));
                    await blobClient.UploadAsync(memoryStream, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
                await DeleteListing();
            }
        }
        public async Task DeleteListing()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DeleteListing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
        public async Task<List<Listings>> GetUsersListing()
        {
            List<Listings> listings = new List<Listings>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersListing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Listings listing = new Listings()
                    {
                        listingId = (int)sdr["ListingId"],
                        listingDesc = (string)sdr["ListingDesc"],
                        price = (decimal)sdr["Price"],
                        minUserRatings = (byte)sdr["MinUserRatings"],
                        minPurchases = (sdr["MinPurchases"] != DBNull.Value) ? (short)sdr["MinPurchases"] : null,
                        isPublic = (bool)sdr["IsPublic"],
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (DateTime)sdr["ExpiryDatetime"],
                    };
                    listings.Add(listing);
                }
                foreach(var listing in listings)
                {
                    await listing.GetImages();
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
            return listings;
        }
        public async Task GetImages()
        {
            try
            {
                var blobServiceClient = CommonService.Instance.GetBlobServiceClient();
                var containerClient = blobServiceClient.GetBlobContainerClient("listing-images");
                var hashedListingId = CommonService.Instance.HashString(listingId.ToString(), "Listings");
                var blobs = containerClient.GetBlobs(BlobTraits.None, BlobStates.All, prefix: hashedListingId);
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
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
        }
    }
}