using System;
using PlugzApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace PlugzApi.Models
{
	public class Location: Base
	{
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
        public async Task GetUsersLocation()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersLocation", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    lat = (decimal)sdr["Lat"];
                    lng = (decimal)sdr["Lng"];
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

