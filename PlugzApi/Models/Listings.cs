using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.Graph.Models;

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
        public DateTime? expiryDatetime { get; set; }
        public int expiryHours { get; set; }
        public string userName { get; set; } = "";
        public string pickUpDropOff { get; set; } = "";
        public Location location { get; set; } = new Location();
        public List<Images> images { get; set; } = new List<Images>();
        public List<Keywords> keywords { get; set; } = new List<Keywords>();

        public async Task InsListings(bool shareWithContacts)
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
                    if (shareWithContacts)
                    {
                        var contactObj = new Contacts();
                        contactObj.userId = userId;
                        var contacts = await contactObj.GetUsersContactsBasic();

                        var messages = new Messages();
                        messages.messageTypeId = 3;
                        messages.senderUserId = userId;
                        messages.extId = listingId;
                        foreach (var contact in contacts.Where(c => c.isConnected))
                        {
                            messages.receiverUserId = contact.contactUser.userId;
                            await messages.InsMessage();
                        }
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
                foreach (var keyword in keywords.Where(k => k.keywordId == 0 && !k.isDeleted))
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
                    if (!await azureStorageService.StoreImage(images[i].dataUrl, "listing-images", $"{hashedListingId}/{i}.txt"))
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

                AzureStorageService storageService = new AzureStorageService();
                var hashedListingId = CommonService.HashString(listingId.ToString(), "Listings");
                await storageService.DeleteImages("listing-images", hashedListingId);
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
        }
        public async Task<List<Listings>> GetUsersListing(bool incExpired)
        {
            List<Listings> listings = new List<Listings>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersListing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@incExpired", SqlDbType.Bit).Value = incExpired;
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
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                        pickUpDropOff = (string)sdr["PickUpDropOff"]
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
                    for(var i = 0; i < azureImages.Count; i++)
                    {
                        images.Add(new Images()
                        {
                            dataUrl = azureImages[i],
                            id = i
                        });
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
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = location.lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = location.lng;
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
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                        userName = (string)sdr["UserName"],
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        },
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
        public async Task UpdListing()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdListing", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                cmd.Parameters.Add("@listingDesc", SqlDbType.NVarChar).Value = listingDesc;
                cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@minUserRatings", SqlDbType.TinyInt).Value = minUserRatings;
                cmd.Parameters.Add("@minPurchases", SqlDbType.SmallInt).Value = minPurchases;
                cmd.Parameters.Add("@isPublic", SqlDbType.Bit).Value = isPublic;
                cmd.Parameters.Add("@expiryHours", SqlDbType.Int).Value = expiryHours;
                cmd.Parameters.Add("@pickUpDropOff", SqlDbType.Char).Value = pickUpDropOff;
                await cmd.ExecuteNonQueryAsync();
                if (keywords.Count > 0)
                {
                    await DeleteKeywords();
                    await InsKeywords();
                }

                AzureStorageService azureStorageService = new AzureStorageService();
                var hashedListingId = CommonService.HashString(listingId.ToString(), "Listings");
                await azureStorageService.UpdateListingImages(images, hashedListingId);
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        private async Task DeleteKeywords()
        {
            try
            {
                var deletedKeywords = keywords.Where(k => k.isDeleted && k.keywordId > 0).ToList();
                if(deletedKeywords != null)
                {
                    var deletedKeywordIds = deletedKeywords.Select(k => k.keywordId).ToList();
                    cmd = new SqlCommand("DeleteKeywords", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                    cmd.Parameters.Add("@keywordIds", SqlDbType.Structured).Value = CommonService.AddListInt(deletedKeywordIds);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
        }
    }
}