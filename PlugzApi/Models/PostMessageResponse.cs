using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class PostMessageResponse: Base
	{
		public int responseId { get; set; }
        public int postMessageId { get; set; }
        public DateTime sentDatetime { get; set; }
        public bool messageRead { get; set; }
        public Listings listing { get; set; } = new Listings();


        public async Task<List<PostMessageResponse>> GetPostMessageResponses()
        {
            var responses = new List<PostMessageResponse>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetPostMessageResponses", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postMessageId", SqlDbType.Int).Value = postMessageId;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    responses.Add(new PostMessageResponse()
                    {
                        responseId = (int)sdr["ResponseId"],
                        postMessageId = postMessageId,
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        messageRead = (bool)sdr["MessageRead"],
                        listing = new Listings()
                        {
                            listingId = (int)sdr["ListingId"],
                            userId = (int)sdr["UserId"],
                            userName = (string)sdr["UserName"],
                            listingDesc = (string)sdr["ListingDesc"],
                            price = (decimal)sdr["Price"],
                            minUserRatings = (byte)sdr["MinUserRatings"],
                            minPurchases = (sdr["MinPurchases"] != DBNull.Value) ? (short)sdr["MinPurchases"] : null,
                            isPublic = (bool)sdr["IsPublic"],
                            createdDatetime = (DateTime)sdr["CreatedDatetime"],
                            expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                            pickUpDropOff = (string)sdr["PickUpDropOff"],
                            pickupLocation = (sdr["PickupLocationId"] == DBNull.Value) ? null : new Location()
                            {
                                locationId = (int)sdr["PickupLocationId"],
                                address = (string)sdr["PickupAddress"],
                                lat = (decimal)sdr["PickupLat"],
                                lng = (decimal)sdr["PickupLng"],
                            },
                            location = new Location()
                            {
                                lat = (decimal)sdr["Lat"],
                                lng = (decimal)sdr["Lng"],
                            },
                        }
                    });
                }
                foreach(var response in responses)
                {
                    await response.listing.GetImages();
                }

            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return responses;
        }

        public async Task UpsertPostMessageResponses()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpsertPostMessageResponses", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@postMessageId", SqlDbType.Int).Value = postMessageId;
                cmd.Parameters.Add("@listingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
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

