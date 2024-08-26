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
        public int totalListings { get; set; }
        public int liveListings { get; set; }
        public int totalConnections { get; set; }
        public decimal rejectedPerc { get; set; }
        public decimal cancelledPerc { get; set; }
        public string trustScore { get; set; } = "";
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
                    totalListings = (int)sdr["TotalListings"];
                    liveListings = (int)sdr["LiveListings"];
                    rating.avgRating = (decimal)sdr["avgRating"];
                    rating.totalRatings = (int)sdr["TotalRatings"];
                    totalConnections = (int)sdr["TotalConnections"];
                    var allSales = (int)sdr["AllSales"];
                    var allPurchases = (int)sdr["AllPurchases"];
                    cancelledPerc = (allSales + allPurchases > 0) ? ((int)sdr["Cancellations"] * 100) / (allSales + allPurchases) : 0;
                    rejectedPerc = (allSales > 0) ? ((int)sdr["Rejections"] * 100) / allSales : 0;
                    trustScore = GetTrustScore(allSales + allPurchases);
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                error = CommonService.GetUnexpectedErrrorMsg();
            }
            await CommonService.Close(con, sdr);
        }
        private string GetTrustScore(int allSalesPurchases)
        {
            var score = 0;
            score += GetPurcSaleConnScore(totalPurchases);
            score += GetPurcSaleConnScore(totalSales);
            score += GetPurcSaleConnScore(totalConnections);
            score += (allSalesPurchases > 0) ? GetCancelledRejectedScore(cancelledPerc) : 0;
            score += (allSalesPurchases > 0) ? (GetCancelledRejectedScore(rejectedPerc) / 2) : 0;
            score += GetRatingScore();

            if (score > 80)
            {
                return "Very High";
            }
            else if (score > 60)
            {
                return "High";
            }
            else if (score > 40)
            {
                return "Medium";
            }
            else return "Low";
        }
        private int GetPurcSaleConnScore(int value)
        {
            if(value > 50)
            {
                return 50;
            }
            else if(value > 20)
            {
                return 40;
            }
            return value * 2;
        }
        private int GetCancelledRejectedScore(decimal percentage)
        {
            if(percentage == 0)
            {
                return 20;
            }
            else if(percentage < 10)
            {
                return 10;
            }
            else if(percentage < 20)
            {
                return 0;
            }
            else if(percentage < 40)
            {
                return -10;
            }
            else if(percentage < 60)
            {
                return -20;
            }
            return -30;
        }
        private int GetRatingScore()
        {
            if(rating.avgRating > (decimal)4.5)
            {
                return 25;
            }
            else if (rating.avgRating > (decimal)4)
            {
                return 20;
            }
            else if (rating.avgRating > (decimal)3)
            {
                return 10;
            }
            else if(rating.avgRating > (decimal)2)
            {
                return 5;
            }
            return 0;
        }
	}
}

