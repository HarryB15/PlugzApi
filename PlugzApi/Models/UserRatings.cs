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
        public int purchaseId { get; set; }
        public byte rating { get; set; }
        public int totalRatings { get; set; }
        public decimal avgRating { get; set; }
        public string? message { get; set; }
        public DateTime ratingDatetime { get; set; }
        public int raterUserId { get; set; }
        public string raterUserName { get; set; } = "";
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
                cmd.Parameters.Add("@purchaseId", SqlDbType.Int).Value = purchaseId;
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
                cmd.Parameters.Add("@purchaseId", SqlDbType.Int).Value = purchaseId;
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
        public async Task<List<UserRatings>> GetUserRatingDetail()
        {
            List<UserRatings> ratings = new List<UserRatings>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUserRatingDetail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@existingRatingIds", SqlDbType.Structured).Value = CommonService.AddListInt(ids);
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    ratings.Add(new UserRatings()
                    {
                        ratingId = (int)sdr["RatingId"],
                        rating = (byte)sdr["Rating"],
                        ratingDatetime = (DateTime)sdr["RatingDatetime"],
                        message = (sdr["Message"] != DBNull.Value) ? (string)sdr["Message"] : null,
                        raterUserId = (int)sdr["raterUserId"],
                        raterUserName = (string)sdr["raterUserName"]
                    });
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
            return ratings;
        }
    }
}