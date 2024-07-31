using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models.Security;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Text;
using Microsoft.Graph.Models;
using System.Text.RegularExpressions;

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
        public string pickUpDropOff { get; set; } = "";
        public List<string> images { get; set; } = new List<string>();
        public List<Keywords> keywords { get; set; } = new List<Keywords>();

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
                cmd.Parameters.Add("@pickUpDropOff", SqlDbType.Char).Value = pickUpDropOff;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    listingId = (int)sdr["listingId"];
                    await StoreImages();
                    if(keywords.Count > 0)
                    {
                        await sdr.CloseAsync();
                        await InsKeywords();
                    }
                }
                else
                {
                    error = CommonService.GetUnexpectedErrrorMsg();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        private async Task InsKeywords()
        {
            try
            {
                var dt = new DataTable();
                dt.Columns.Add("String", typeof(string));
                foreach (var keyword in keywords)
                {
                    dt.Rows.Add(keyword.keyword);
                }

                cmd = new SqlCommand("InsKeywords", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                cmd.Parameters.Add("@keywords", SqlDbType.Structured).Value = dt;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
        }
        private async Task StoreImages()
        {
            try
            {
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedListingId = CommonService.HashString(listingId.ToString(), "Listings");
                for (var i = 0; i < images.Count; i++)
                {
                    if (!await azureStorageService.StoreImage(images[i], "listing-images", $"{hashedListingId}/{i}.txt"))
                    {
                        error = CommonService.GetUnexpectedErrrorMsg();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
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
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
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
                cmd.Parameters.Add("@existingListingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Listings listing = new Listings()
                    {
                        listingId = (int)sdr["ListingId"],
                        userId = userId,
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
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return listings;
        }
        public async Task GetImages()
        {
            try
            {
                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedListingId = CommonService.HashString(listingId.ToString(), "Listings");
                var azureImages = await azureStorageService.GetPhotos("listing-images", hashedListingId);
                if (azureImages != null)
                {
                    images = azureImages; 
                }
                else
                {
                    error = CommonService.GetUnexpectedErrrorMsg();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
        }
        public async Task<List<Listings>> SearchListings(string searchValue)
        {
            List<Listings> listings = new List<Listings>();
            try
            {
                searchValue = Regex.Replace(searchValue, "[^a-zA-Z0-9 ]", "");
                List<string> searchWords = searchValue.Split(" ").Where(sv => sv != "").ToList();
                var dt = new DataTable();
                dt.Columns.Add("String", typeof(string));
                foreach (var word in searchWords)
                {
                    dt.Rows.Add(word);
                }

                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("SearchListings", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@searchWords", SqlDbType.Structured).Value = dt;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                cmd.Parameters.Add("@existingListingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Listings listing = new Listings()
                    {
                        listingId = (int)sdr["ListingId"],
                        userId = (int)sdr["UserId"],
                        listingDesc = (string)sdr["ListingDesc"],
                        price = (decimal)sdr["Price"],
                        lat = (decimal)sdr["Lat"],
                        lng = (decimal)sdr["Lng"],
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (DateTime)sdr["ExpiryDatetime"],
                        userName = (string)sdr["UserName"]
                    };
                    listings.Add(listing);
                }
                foreach (var listing in listings)
                {
                    await listing.GetImages();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return listings;
        }
    }
}