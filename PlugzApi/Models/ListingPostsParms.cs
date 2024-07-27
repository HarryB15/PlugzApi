using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;

namespace PlugzApi.Models
{
	public class ListingPostsParms: Base
    {
        public decimal maxPrice { get; set; }
        public decimal minPrice { get; set; }
        public bool connectionsOnly { get; set; }
        public int maxDist { get; set; }
        public List<Listings> listings { get; set; } = new List<Listings>();
        public List<Posts> posts { get; set; } = new List<Posts>();

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
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (DateTime)sdr["ExpiryDatetime"],
                        userName = (string)sdr["UserName"],
                        connectionStatus = (byte)sdr["ConnectionStatus"]
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
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
        }
        public async Task GetPosts()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetPosts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                cmd.Parameters.Add("@maxPrice", SqlDbType.Decimal).Value = maxPrice;
                cmd.Parameters.Add("@minPrice", SqlDbType.Decimal).Value = minPrice;
                cmd.Parameters.Add("@connectionsOnly", SqlDbType.Bit).Value = connectionsOnly;
                cmd.Parameters.Add("@maxDist", SqlDbType.Int).Value = maxDist;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Posts post = new Posts()
                    {
                        postId = (int)sdr["PostId"],
                        userId = (int)sdr["UserId"],
                        postText = (string)sdr["PostText"],
                        price = (decimal)sdr["Price"],
                        isPublic = (bool)sdr["IsPublic"],
                        createdDatetime = (DateTime)sdr["CreatedDatetime"],
                        expiryDatetime = (DateTime)sdr["ExpiryDatetime"],
                        userName = (string)sdr["UserName"],
                        lat = (decimal)sdr["Lat"],
                        lng = (decimal)sdr["Lng"],
                        connectionStatus = (byte)sdr["ConnectionStatus"]
                    };
                    posts.Add(post);
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

