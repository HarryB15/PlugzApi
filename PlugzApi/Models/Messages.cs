using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models;
using PlugzApi.Services;

namespace PlugzApi.Models
{
	public class Messages: Base
	{
        public int messageId { get; set; }
        public byte messageTypeId { get; set; }
        public string? messageText { get; set; } = null;
        public int senderUserId { get; set; }
        public int receiverUserId { get; set; }
        public bool userIsSender { get; set; }
        public DateTime sentDatetime { get; set; }
        public bool messageRead { get; set; }
        public int? extId { get; set; }
        public Posts? post { get; set; }
        public Listings? listing { get; set; }
        public Offer? offer { get; set; }
        public async Task InsMessage()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsMessage", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@messageTypeId", SqlDbType.TinyInt).Value = messageTypeId;
                cmd.Parameters.Add("@messageText", SqlDbType.NVarChar).Value = messageText;
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserId", SqlDbType.Int).Value = receiverUserId;
                cmd.Parameters.Add("@extId", SqlDbType.Int).Value = extId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task InsMessageMultiple()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsMessageMultiple", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@messageTypeId", SqlDbType.TinyInt).Value = messageTypeId;
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                cmd.Parameters.Add("@extId", SqlDbType.Int).Value = extId;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task<List<Messages>> GetMessages(int contactUserId)
        {
            List<Messages> messages = new List<Messages>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@contactUserId", SqlDbType.Int).Value = contactUserId;
                cmd.Parameters.Add("@existingMessageIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Messages message = new Messages()
                    {
                        messageId = (int)sdr["MessageId"],
                        messageTypeId = (byte)sdr["MessageTypeId"],
                        messageText = (sdr["MessageText"] != DBNull.Value) ? (string)sdr["MessageText"] : null,
                        senderUserId = (int)sdr["SenderUserId"],
                        receiverUserId = (int)sdr["ReceiverUserId"],
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        extId = (sdr["ExtId"] != DBNull.Value) ? (int)sdr["ExtId"] : null,
                    };
                    message.userIsSender = (message.senderUserId == userId);
                    if(message.messageTypeId == 2)
                    {
                        message.post = new Posts()
                        {
                            postId = (int)message.extId!,
                            userId = (int)sdr["PostUserId"],
                            postText = (string)sdr["PostText"],
                            price = (decimal)sdr["PostPrice"],
                            createdDatetime = (DateTime)sdr["PostCreatedDatetime"]
                        };
                    }
                    else if(message.messageTypeId == 3)
                    {
                        var existingListing = messages.Where(m => m.listing != null).Select(m => m.listing!)
                            .FirstOrDefault(l => l.listingId == message.extId);
                        if(existingListing != null)
                        {
                            message.listing = existingListing;
                        }
                        else
                        {
                            message.listing = new Listings()
                            {
                                listingId = (int)message.extId!,
                                userId = (int)sdr["ListingUserId"],
                                listingDesc = (string)sdr["ListingDesc"],
                                price = (decimal)sdr["ListingPrice"],
                                createdDatetime = (DateTime)sdr["ListingCreated"],
                                expiryDatetime = (sdr["ListingExpiry"] != DBNull.Value) ? (DateTime)sdr["ListingExpiry"] : null,
                                pickUpDropOff = (string)sdr["PickUpDropOff"],
                                pickupLocation = (sdr["PickupLocationId"] == DBNull.Value) ? null : new Location()
                                {
                                    locationId = (int)sdr["PickupLocationId"],
                                    address = (string)sdr["PickupAddress"],
                                    lat = (decimal)sdr["PickupLat"],
                                    lng = (decimal)sdr["PickupLng"],
                                },
                            };
                            await message.listing.GetImages();
                        }
                    }
                    else if(message.messageTypeId == 4)
                    {
                        message.offer = new Offer()
                        {
                            offerId = (int)message.extId!,
                            userId = (int)sdr["OfferUserId"],
                            listingId = (int)sdr["OfferListingId"],
                            offerValue = (decimal)sdr["OfferValue"],
                            responseType = (sdr["ResponseType"] != DBNull.Value) ? (string)sdr["ResponseType"] : null,
                            oriOfferId = (sdr["OriOfferId"] != DBNull.Value) ? (int)sdr["OriOfferId"] : null,
                            pickUpDropOff = (string)sdr["OfferPickUpDropOff"],
                            pickupLocation = (sdr["OfferPickupLocationId"] == DBNull.Value) ? null : new Location()
                            {
                                locationId = (int)sdr["OfferPickupLocationId"],
                                address = (string)sdr["OfferPickupAddress"],
                                lat = (decimal)sdr["OfferPickupLat"],
                                lng = (decimal)sdr["OfferPickupLng"],
                            }
                        };

                        var existingListing = messages.Where(m => m.offer != null).Select(m => m.offer!.listing)
                            .FirstOrDefault(l => l.listingId == message.offer.listingId);
                        if(existingListing != null)
                        {
                            message.offer.listing = existingListing;
                        }
                        else
                        {
                            message.offer.listing = new Listings()
                            {
                                listingId = message.offer.listingId,
                                userId = (int)sdr["OfferListingUserId"],
                                listingDesc = (string)sdr["OfferListingDesc"],
                                price = (decimal)sdr["OfferListingPrice"],
                                createdDatetime = (DateTime)sdr["OfferListingCreated"],
                                expiryDatetime = (sdr["OfferListingExpiry"] != DBNull.Value) ? (DateTime)sdr["ListingExpiry"] : null,
                                pickUpDropOff = (string)sdr["OfferPickUpDropOff"],
                                pickupLocation = (sdr["OfferListingPickupLocationId"] == DBNull.Value) ? null : new Location()
                                {
                                    locationId = (int)sdr["OfferListingPickupLocationId"],
                                    address = (string)sdr["OfferListingPickupAddress"],
                                    lat = (decimal)sdr["OfferListingPickupLat"],
                                    lng = (decimal)sdr["OfferListingPickupLng"],
                                },
                            };
                            await message.offer.listing.GetImages();
                        }
                    }
                    messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return messages.OrderBy(m => m.sentDatetime).ToList();
        }
    }
}