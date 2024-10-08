﻿using System;
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
        public string pickUpDropOff { get; set; } = "";
        public Location location { get; set; } = new Location();
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
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = location.lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = location.lng;
                cmd.Parameters.Add("@maxPrice", SqlDbType.Decimal).Value = maxPrice;
                cmd.Parameters.Add("@minPrice", SqlDbType.Decimal).Value = minPrice;
                cmd.Parameters.Add("@connectionsOnly", SqlDbType.Bit).Value = connectionsOnly;
                cmd.Parameters.Add("@maxDist", SqlDbType.Int).Value = maxDist;
                cmd.Parameters.Add("@pickUpDropOff", SqlDbType.Char).Value = pickUpDropOff;
                cmd.Parameters.Add("@existingListingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                listings = Listings.ReadListings(sdr);
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
        }
        public async Task GetPosts()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetPosts", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = location.lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = location.lng;
                cmd.Parameters.Add("@maxPrice", SqlDbType.Decimal).Value = maxPrice;
                cmd.Parameters.Add("@minPrice", SqlDbType.Decimal).Value = minPrice;
                cmd.Parameters.Add("@connectionsOnly", SqlDbType.Bit).Value = connectionsOnly;
                cmd.Parameters.Add("@maxDist", SqlDbType.Int).Value = maxDist;
                cmd.Parameters.Add("@existingPostIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
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
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                        userName = (string)sdr["UserName"],
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        }
                    };
                    posts.Add(post);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task GetListingsMap()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetListingsMap", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = location.lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = location.lng;
                sdr = await cmd.ExecuteReaderAsync();
                listings = Listings.ReadListings(sdr);
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
        }
        public async Task GetPostsMap()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetPostsMap", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = location.lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = location.lng;
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
                        expiryDatetime = (sdr["ExpiryDatetime"] != DBNull.Value) ? (DateTime)sdr["ExpiryDatetime"] : null,
                        userName = (string)sdr["UserName"],
                        location = new Location()
                        {
                            lat = (decimal)sdr["Lat"],
                            lng = (decimal)sdr["Lng"],
                        }
                    };
                    posts.Add(post);
                }
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

