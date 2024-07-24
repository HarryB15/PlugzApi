using System;
using PlugzApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace PlugzApi.Models
{
	public class UsersStats: Base
	{
		public int totalPurchases { get; set; }
        public int livePurchases { get; set; }
        public int awaitingAccPurchases { get; set; }
        public int totalSales { get; set; }
        public int liveSales { get; set; }
        public int awaitingAccSales { get; set; }
        public UserRatings rating { get; set; } = new UserRatings();
		public async Task GetUsersStats()
		{

            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("GetUsersStats", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                sdr = await cmd.ExecuteReaderAsync();
                if (sdr.Read())
                {
                    totalPurchases = (int)sdr["TotalPurchases"];
                    livePurchases = (int)sdr["LivePurchases"];
                    awaitingAccPurchases = (int)sdr["AwaitingAccPurchases"];
                    totalSales = (int)sdr["TotalSales"];
                    liveSales = (int)sdr["LiveSales"];
                    awaitingAccSales = (int)sdr["AwaitingAccSales"];
                    rating.avgRating = (decimal)sdr["avgRating"];
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

