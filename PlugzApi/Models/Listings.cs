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
        public bool isPublic { get; set; }
        public DateTime createdDatetime { get; set; }
        public DateTime expiryDatetime { get; set; }
        public int expiryHours { get; set; }
        public string userName { get; set; } = "";
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
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedListingId = CommonService.Instance.HashString(listingId.ToString(), "Listings");
                for (var i = 0; i < images.Count; i++)
                {
                    if (!await azureStorageService.StoreImage(images[i], "listing-images", $"{hashedListingId}/{i}.txt"))
                    {
                        error = CommonService.Instance.GetUnexpectedErrrorMsg();
                        break;
                    }
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
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedListingId = CommonService.Instance.HashString(listingId.ToString(), "Listings");
                var azureImages = await azureStorageService.GetPhotos("listing-images", hashedListingId);
                if (azureImages != null)
                {
                    images = azureImages; 
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
        }
    }
}