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
        public int locationId { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
        public string address { get; set; } = "";
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
        public List<Location>? SearchLocation()
        {
            AzureMapsService azureMapsService = new AzureMapsService();
            var locations = azureMapsService.SearchAddress(address);
            if (locations == null)
            {
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            return locations;
        }
        public async Task InsLocations(SqlConnection con)
        {
            try
            {
                cmd = new SqlCommand("InsLocations", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@address", SqlDbType.VarChar).Value = address;
                cmd.Parameters.Add("@lat", SqlDbType.Decimal).Value = lat;
                cmd.Parameters.Add("@lng", SqlDbType.Decimal).Value = lng;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    locationId = (int)sdr["LocationId"];
                }
                sdr.Close();
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
        }
    }
}

