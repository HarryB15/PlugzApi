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
                            postText = (string)sdr["PostText"],
                            price = (decimal)sdr["PostPrice"],
                            createdDatetime = (DateTime)sdr["PostCreatedDatetime"],
                            userId = (message.userIsSender) ? message.senderUserId : message.receiverUserId
                        };
                    }
                    else if(message.messageTypeId == 3)
                    {
                        message.listing = new Listings()
                        {
                            listingId = (int)message.extId!,
                            userId = (message.userIsSender) ? message.senderUserId : message.receiverUserId,
                            listingDesc = (string)sdr["ListingDesc"],
                            price = (decimal)sdr["ListingPrice"],
                            createdDatetime = (DateTime)sdr["ListingCreatedDatetime"],
                            expiryDatetime = (sdr["ListingExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ListingExpiryDatetime"] : null
                        };
                        await message.listing.GetImages();
                    }
                    else if(message.messageTypeId == 4)
                    {
                        message.offer = new Offer()
                        {
                            offerId = (int)message.extId!,
                            userId = (message.userIsSender) ? message.senderUserId : message.receiverUserId,
                            listingId = (int)sdr["OfferListingId"],
                            offerValue = (decimal)sdr["OfferValue"],
                            responseType = (sdr["ResponseType"] != DBNull.Value) ? (string)sdr["ResponseType"] : null,
                            oriOfferId = (sdr["OriOfferId"] != DBNull.Value) ? (int)sdr["OriOfferId"] : null
                        };
                        message.offer.listing = new Listings()
                        {
                            listingId = message.offer.listingId,
                            userId = message.offer.userId,
                            listingDesc = (string)sdr["OfferListingDesc"],
                            price = (decimal)sdr["OfferListingPrice"],
                            createdDatetime = (DateTime)sdr["OfferListingCreated"],
                            expiryDatetime = (sdr["OfferListingExpiry"] != DBNull.Value) ? (DateTime)sdr["ListingExpiry"] : null
                        };
                        await message.offer.listing.GetImages();
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