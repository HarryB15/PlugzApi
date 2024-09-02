﻿using System;
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
        public string? pickupAddress { get; set; }
        public Location? pickupLocation { get; set; }
        public Listings listing { get; set; } = new Listings();
        public async Task InsOffer(int receiverUserId)
		{
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsOffer", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                cmd.Parameters.Add("@offerValue", SqlDbType.Decimal).Value = offerValue;
                cmd.Parameters.Add("@offerText", SqlDbType.NVarChar).Value = offerText;
                cmd.Parameters.Add("@oriOfferId", SqlDbType.Int).Value = oriOfferId;
                cmd.Parameters.Add("@pickUpDropOff", SqlDbType.Char).Value = pickUpDropOff;
                if (pickUpDropOff == "P" && pickupLocation != null)
                {
                    cmd.Parameters.Add("@pickupAddress", SqlDbType.VarChar).Value = pickupAddress;
                    cmd.Parameters.Add("@pickupLat", SqlDbType.Decimal).Value = pickupLocation.lat;
                    cmd.Parameters.Add("@pickupLng", SqlDbType.Decimal).Value = pickupLocation.lng;
                }
                cmd.Parameters.Add("@receiverUserId", SqlDbType.Int).Value = receiverUserId;
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
	}
}

