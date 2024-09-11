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
        public int listingId { get; set; }
        public int postId { get; set; }
        public string? messageText { get; set; } = null;
        public int senderUserId { get; set; }
        public int receiverUserId { get; set; }
        public bool userIsSender { get; set; }
        public DateTime sentDatetime { get; set; }
        public bool messageRead { get; set; }
        public Offer? offer { get; set; }

        public async Task<List<Messages>> GetListingMessages(int contactUserId)
        {
            List<Messages> messages = new List<Messages>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetListingMessages", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@contactUserId", SqlDbType.Int).Value = contactUserId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    messages.Add(new Messages()
                    {
                        messageId = (int)sdr["MessageId"],
                        messageText = (sdr["MessageText"] != DBNull.Value) ? (string)sdr["MessageText"] : null,
                        senderUserId = (int)sdr["SenderUserId"],
                        receiverUserId = (int)sdr["ReceiverUserId"],
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                    });
                }

                await sdr.NextResultAsync();
                while (sdr.Read())
                {
                    messages.Add(new Messages()
                    {
                        messageId = 0,
                        messageText = (sdr["OfferText"] != DBNull.Value) ? (string)sdr["OfferText"] : null,
                        senderUserId = (int)sdr["UserId"],
                        receiverUserId = (int)sdr["ReceiverUserId"],
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        offer = new Offer()
                        {
                            offerId = (int)sdr["OfferId"],
                            listingId = listingId,
                            userId = (int)sdr["UserId"],
                            offerValue = (decimal)sdr["OfferValue"],
                            responseType = (sdr["ResponseType"] != DBNull.Value) ? (string)sdr["ResponseType"] : null,
                            oriOfferId = (sdr["OriOfferId"] != DBNull.Value) ? (int)sdr["OriOfferId"] : null,
                            pickUpDropOff = (string)sdr["PickupDropoff"],
                            pickupLocation = (sdr["LocationId"] == DBNull.Value) ? null : new Location()
                            {
                                locationId = (int)sdr["LocationId"],
                                address = (string)sdr["Address"],
                                lat = (decimal)sdr["Lat"],
                                lng = (decimal)sdr["Lng"],
                            }
                        }
                    });
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

        public async Task InsMessage()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsMessage", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@messageText", SqlDbType.NVarChar).Value = messageText;
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserId", SqlDbType.Int).Value = receiverUserId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId > 0 ? listingId : null;
                cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId > 0 ? postId : null;
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
                cmd.Parameters.Add("@senderUserId", SqlDbType.Int).Value = senderUserId;
                cmd.Parameters.Add("@receiverUserIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId > 0 ? listingId : null;
                cmd.Parameters.Add("@postId", SqlDbType.Int).Value = postId > 0 ? postId : null;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
    }
}