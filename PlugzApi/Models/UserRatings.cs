using System;
using Microsoft.Graph.Models;
using PlugzApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class UserRatings : Base
	{
		public int ratingId { get; set; }
        public int listingId { get; set; }
        public byte rating { get; set; }
        public int totalRatings { get; set; }
        public decimal avgRating { get; set; }
        public string? message { get; set; }
        public async Task GetUserRating()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUserRating", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    avgRating = (decimal)sdr["AvgRating"];
                    totalRatings = (int)sdr["TotalRatings"];
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task UpdInsUserRatings()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("UpdInsUserRatings", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                cmd.Parameters.Add("@rating", SqlDbType.TinyInt).Value = rating;
                cmd.Parameters.Add("@message", SqlDbType.VarChar).Value = message;
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        public async Task GetRating()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetRating", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    ratingId = (int)sdr["RatingId"];
                    rating = (byte)sdr["Rating"];
                    message = (sdr["Message"] != DBNull.Value) ? (string)sdr["Message"] : null;
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