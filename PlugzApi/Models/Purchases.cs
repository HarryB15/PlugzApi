using System;
using Microsoft.Graph.Models;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Models;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Purchases: Base
    {
        public int purchaseId { get; set; }
        public int purchaseStatusId { get; set; }
        public string purchaseStatus { get; set; } = "";
        public decimal price { get; set; }
        public decimal fee { get; set; }
        public DateTime purchaseDatetime { get; set; }
        public DateTime? completionDatetime { get; set; }
        public int offerId { get; set; }
        public string? payIntentCS { get; set; }
        public bool userSharingLoc { get; set; }
        public string purchaseRef { get; set; } = "";
        public string? userName { get; set; }
        public bool isPurchase { get; set; }
        public string pickupDropoff { get; set; } = "";
        public Listings listing { get; set; } = new Listings();
        public Location location { get; set; } = new Location();
        public Location? deliveryLocation { get; set; }

        public async Task InsPurchases()
        {
            try
            {
                fee = (price < 5) ? (decimal)0.5 : price * (decimal)0.1;
                StripeService stripe = new StripeService();
                var paymentIntent = stripe.GetPaymentIntent((long)(fee + price) * 100);
                if (paymentIntent != null)
                {
                    con = await CommonService.Instance.Open();
                    if(pickupDropoff == "D" && deliveryLocation == null)
                    {
                        throw new Exception("Delivery address not entered");
                    }
                    else if(pickupDropoff == "D" && deliveryLocation != null)
                    {
                        await deliveryLocation.InsLocations(con);
                        if(deliveryLocation.locationId == 0)
                        {
                            throw new Exception("Error inserting delivery address");
                        }
                    }

                    cmd = new SqlCommand("InsPurchases", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listing.listingId;
                    cmd.Parameters.Add("@offerId", SqlDbType.Int).Value = (offerId > 0) ? offerId : null;
                    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                    cmd.Parameters.Add("@fee", SqlDbType.Decimal).Value = fee;
                    cmd.Parameters.Add("@payIntentId", SqlDbType.VarChar).Value = paymentIntent.Id;
                    cmd.Parameters.Add("@pickupDropoff", SqlDbType.Char).Value = pickupDropoff;
                    cmd.Parameters.Add("@deliveryAddressId", SqlDbType.Int).Value = deliveryLocation != null ? deliveryLocation.locationId : null;
                    sdr = await cmd.ExecuteReaderAsync();
                    if (sdr.Read())
                    {
                        payIntentCS = paymentIntent.ClientSecret;
                        purchaseId = (int)sdr["PurchaseId"];
                        purchaseStatusId = (int)sdr["PurchaseStatusId"];
                        purchaseStatus = (string)sdr["PurchaseStatus"];
                        purchaseDatetime = (DateTime)sdr["PurchaseDatetime"];
                        completionDatetime = null;
                        userSharingLoc = (bool)sdr["UserSharingLoc"];
                        isPurchase = true;
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        };
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            userId = (int)sdr["ListingUserId"],
                            userName = (string)sdr["ListingUserName"],
                            listingDesc = (string)sdr["ListingDesc"],
                            price = (decimal)sdr["ListingPrice"],
                            createdDatetime = (DateTime)sdr["CreatedDatetime"],
                            expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                            pickUpDropOff = (string)sdr["PickUpDropOff"],
                            pickupAddress = (sdr["PickupAddress"] != DBNull.Value) ? (string)sdr["PickupAddress"] : null,
                            pickupLocation = new Location()
                            {
                                lat = (sdr["PickupLat"] != DBNull.Value) ? (decimal)sdr["PickupLat"] : null,
                                lng = (sdr["PickupLng"] != DBNull.Value) ? (decimal)sdr["PickupLng"] : null
                            }
                        };

                        var hashedId = CommonService.HashString(purchaseId.ToString(), "purchaseRef");
                        purchaseRef = hashedId.Substring(0, 8).ToUpper();
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
        public async Task DeletePurchases()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("DeletePurchase", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@purchaseId", SqlDbType.Int).Value = purchaseId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task<List<Purchases>> GetUsersPurchases(bool liveOnly)
        {
            List<Purchases> purchases = new List<Purchases>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPurchases", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@liveOnly", SqlDbType.Bit).Value = liveOnly;
                cmd.Parameters.Add("@existingPurchaseIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while(sdr.Read())
                {
                    Purchases purchase = new Purchases()
                    {
                        purchaseId = (int)sdr["PurchaseId"],
                        purchaseStatusId = (int)sdr["PurchaseStatusId"],
                        purchaseStatus = (string)sdr["PurchaseStatus"],
                        price = (decimal)sdr["PurchasePrice"],
                        fee = (decimal)sdr["Fee"],
                        purchaseDatetime = (DateTime)sdr["PurchaseDatetime"],
                        completionDatetime = (sdr["CompletionDatetime"] != DBNull.Value) ? (DateTime)sdr["CompletionDatetime"] : null,
                        userSharingLoc = (bool)sdr["UserSharingLoc"],
                        isPurchase = true,
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        },
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            userId = (int)sdr["ListingUserId"],
                            userName = (string)sdr["ListingUserName"],
                            listingDesc = (string)sdr["ListingDesc"],
                            price = (decimal)sdr["ListingPrice"],
                            createdDatetime = (DateTime)sdr["CreatedDatetime"],
                            expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                            pickUpDropOff = (string)sdr["PickUpDropOff"],
                            pickupAddress = (sdr["PickupAddress"] != DBNull.Value) ? (string)sdr["PickupAddress"] : null,
                            pickupLocation = new Location()
                            {
                                lat = (sdr["PickupLat"] != DBNull.Value) ? (decimal)sdr["PickupLat"] : null,
                                lng = (sdr["PickupLng"] != DBNull.Value) ? (decimal)sdr["PickupLng"] : null
                            }
                        }
                    };
                    await purchase.listing.GetImages(true);
                    var hashedId = CommonService.HashString(purchase.purchaseId.ToString(), "purchaseRef");
                    purchase.purchaseRef = hashedId.Substring(0, 8).ToUpper();
                    purchases.Add(purchase);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return purchases;
        }
        public async Task<List<Purchases>> GetUsersPurchasesBasic()
        {
            List<Purchases> purchases = new List<Purchases>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersPurchasesBasic", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Purchases purchase = new Purchases()
                    {
                        purchaseId = (int)sdr["PurchaseId"],
                        purchaseStatusId = (int)sdr["PurchaseStatusId"],
                        purchaseStatus = (string)sdr["PurchaseStatus"],
                        price = (decimal)sdr["PurchasePrice"],
                        fee = (decimal)sdr["Fee"],
                        purchaseDatetime = (DateTime)sdr["PurchaseDatetime"],
                        isPurchase = true,
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            listingDesc = (string)sdr["ListingDesc"]
                        }
                    };
                    var hashedId = CommonService.HashString(purchase.purchaseId.ToString(), "purchaseRef");
                    purchase.purchaseRef = hashedId.Substring(0, 8).ToUpper();
                    purchases.Add(purchase);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return purchases;
        }

        public async Task<List<Purchases>> GetUsersSales(bool liveOnly)
        {
            List<Purchases> purchases = new List<Purchases>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersSales", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@liveOnly", SqlDbType.Bit).Value = liveOnly;
                cmd.Parameters.Add("@existingPurchaseIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Purchases purchase = new Purchases()
                    {
                        purchaseId = (int)sdr["PurchaseId"],
                        purchaseStatusId = (int)sdr["PurchaseStatusId"],
                        purchaseStatus = (string)sdr["PurchaseStatus"],
                        price = (decimal)sdr["PurchasePrice"],
                        fee = (decimal)sdr["Fee"],
                        purchaseDatetime = (DateTime)sdr["PurchaseDatetime"],
                        completionDatetime = (sdr["CompletionDatetime"] != DBNull.Value) ? (DateTime)sdr["CompletionDatetime"] : null,
                        userSharingLoc = (bool)sdr["UserSharingLoc"],
                        userId = (int)sdr["UserId"],
                        userName = (string)sdr["UserName"],
                        isPurchase = false,
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        },
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            listingDesc = (string)sdr["ListingDesc"],
                            price = (decimal)sdr["ListingPrice"],
                            createdDatetime = (DateTime)sdr["CreatedDatetime"],
                            expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                            pickUpDropOff = (string)sdr["PickUpDropOff"],
                            pickupAddress = (sdr["PickupAddress"] != DBNull.Value) ? (string)sdr["PickupAddress"] : null,
                            pickupLocation = new Location()
                            {
                                lat = (sdr["PickupLat"] != DBNull.Value) ? (decimal)sdr["PickupLat"] : null,
                                lng = (sdr["PickupLng"] != DBNull.Value) ? (decimal)sdr["PickupLng"] : null
                            }
                        }
                    };
                    await purchase.listing.GetImages(true);
                    var hashedId = CommonService.HashString(purchase.purchaseId.ToString(), "purchaseRef");
                    purchase.purchaseRef = hashedId.Substring(0, 8).ToUpper();
                    purchases.Add(purchase);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return purchases;
        }
        public async Task<List<Purchases>> GetUsersSalesBasic()
        {
            List<Purchases> purchases = new List<Purchases>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersSalesBasic", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Purchases purchase = new Purchases()
                    {
                        purchaseId = (int)sdr["PurchaseId"],
                        purchaseStatusId = (int)sdr["PurchaseStatusId"],
                        purchaseStatus = (string)sdr["PurchaseStatus"],
                        price = (decimal)sdr["PurchasePrice"],
                        fee = (decimal)sdr["Fee"],
                        purchaseDatetime = (DateTime)sdr["PurchaseDatetime"],
                        userId = (int)sdr["UserId"],
                        userName = (string)sdr["UserName"],
                        isPurchase = false,
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            listingDesc = (string)sdr["ListingDesc"]
                        }
                    };
                    var hashedId = CommonService.HashString(purchase.purchaseId.ToString(), "purchaseRef");
                    purchase.purchaseRef = hashedId.Substring(0, 8).ToUpper();
                    purchases.Add(purchase);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return purchases;
        }
    }
}