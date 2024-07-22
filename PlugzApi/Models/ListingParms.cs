using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class ListingParms: Base
    {
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public decimal maxPrice { get; set; }
        public decimal minPrice { get; set; }
        public bool connectionsOnly { get; set; }
        public string buyOrsell { get; set; } = "";
        public int maxDist { get; set; }
        public List<Listings> results { get; set; } = new List<Listings>();

        public async Task GetListings()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetListings", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                cmd.Parameters.Add("@maxPrice", SqlDbType.Decimal).Value = maxPrice;
                cmd.Parameters.Add("@minPrice", SqlDbType.Decimal).Value = minPrice;
                cmd.Parameters.Add("@connectionsOnly", SqlDbType.Bit).Value = connectionsOnly;
                cmd.Parameters.Add("@buyOrsell", SqlDbType.VarChar).Value = buyOrsell;
                cmd.Parameters.Add("@maxDist", SqlDbType.Int).Value = maxDist;
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
                        buyOrSell = Convert.ToChar(sdr["BuyOrSell"]),
                        sentDatetime = (DateTime)sdr["SentDatetime"],
                        expiryDatetime = (DateTime)sdr["ExpiryDatetime"],
                        userName = (string)sdr["UserName"],
                        isConnected = (bool)sdr["IsConnected"]
                    };
                    results.Add(listing);
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
    }
}

