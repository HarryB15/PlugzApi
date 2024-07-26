using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class Cities: Base
	{
		public int cityId { get; set; }
		public string city { get; set; } = "";
		public string? county { get; set; }
		public string countryCode { get; set; } = "";

		public async Task<List<Cities>> GetCities()
		{
			List<Cities> cities = new List<Cities>();
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetCities", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@countryCode", SqlDbType.Char).Value = countryCode;
                cmd.Parameters.Add("@city", SqlDbType.VarChar).Value = city;
                sdr = await cmd.ExecuteReaderAsync();
                while (sdr.Read())
                {
                    Cities city = new Cities()
                    {
                        cityId = (int)sdr["CityId"],
                        city = (string)sdr["City"],
                        county = (sdr["County"] != DBNull.Value) ? (string)sdr["County"] : null,
                        countryCode = countryCode,
                        lat = (decimal)sdr["Lat"],
                        lng = (decimal)sdr["Lng"],
                    };
                    cities.Add(city);
                }
            }
            catch (Exception ex)
            {
                CommonService.Instance.Log(ex);
                error = CommonService.Instance.GetUnexpectedErrrorMsg();
            }
            await CommonService.Instance.Close(con, sdr);
            return cities;
        }
    }
}
