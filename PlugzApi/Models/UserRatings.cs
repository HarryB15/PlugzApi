﻿using System;
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
                await cmd.ExecuteNonQueryAsync();
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