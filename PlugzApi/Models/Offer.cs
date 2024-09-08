using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class Offer: Base
	{
		public int offerId { get; set; }
		public int listingId { get; set; }
		public string? offerText { get; set; }
		public decimal offerValue { get; set; }
        public string? responseType { get; set; }
        public int? oriOfferId { get; set; }
        public string pickUpDropOff { get; set; } = "";
        public int receiverUserId { get; set; }
        public bool userIsSender { get; set; }
        public DateTime sentDatetime { get; set; }
        public string userName { get; set; } = "";
        public Listings listing { get; set; } = new Listings();
        public Location? pickupLocation { get; set; }
        public async Task InsOffer(int receiverUserId)
		{
            try
            {
                con = await CommonService.Instance.Open();
                if (pickUpDropOff == "P" && pickupLocation == null)
                {
                    throw new Exception("Pickup address not entered");
                }
                else if (pickUpDropOff == "P" && pickupLocation != null)
                {
                    await pickupLocation.InsLocations(con);
                    if (pickupLocation.locationId == 0)
                    {
                        throw new Exception("Error inserting delivery address");
                    }
                }

                cmd = new SqlCommand("InsOffer", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@receiverUserId", SqlDbType.Int).Value = receiverUserId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                cmd.Parameters.Add("@offerValue", SqlDbType.Decimal).Value = offerValue;
                cmd.Parameters.Add("@offerText", SqlDbType.NVarChar).Value = offerText;
                cmd.Parameters.Add("@oriOfferId", SqlDbType.Int).Value = oriOfferId;
                cmd.Parameters.Add("@pickUpDropOff", SqlDbType.Char).Value = pickUpDropOff;
                if (pickUpDropOff == "P" && pickupLocation != null)
                {
                    cmd.Parameters.Add("@pickupLocationId", SqlDbType.Int).Value = pickupLocation.locationId;
                }

                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    offerId = (int)sdr["OfferId"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task UpdOfferResponse()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdOfferResponse", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@offerId", SqlDbType.Int).Value = oriOfferId;
                cmd.Parameters.Add("@responseType", SqlDbType.Char).Value = responseType;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task<List<Offer>> GetUsersOffers()
        {
            var offers = new List<Offer>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersOffers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@existingListingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();

                while (sdr.Read())
                {
                    offers.Add(new Offer()
                    {
                        offerId = (int)sdr["OfferId"],
                        listingId = (int)sdr["ListingId"],
                        userId = (int)sdr["UserId"],
                        receiverUserId = (int)sdr["ReceiverUserId"],
                        offerValue = (decimal)sdr["OfferValue"],
                        responseType = sdr["ResponseType"] != DBNull.Value ? (string)sdr["ResponseType"] : null,
                        offerText = sdr["OfferText"] != DBNull.Value ? (string)sdr["OfferText"] : null,
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        userName = (string)sdr["UserName"],
                        userIsSender = (userId == (int)sdr["UserId"])   
                    });
                    foreach(var offer in offers)
                    {
                        offer.listing.listingId = offer.listingId;
                        await listing.GetImages(true);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return offers;
        }
    }
}

