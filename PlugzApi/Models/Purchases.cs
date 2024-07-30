using System;
using Microsoft.Graph.Models;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Models;
using PlugzApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlugzApi.Models
{
	public class Purchases: Base
    {
        public int purchaseId { get; set; }
        public int purchaseStatusId { get; set; }
        public string purchaseStatus { get; set; } = "";
        public decimal price { get; set; }
        public decimal fee { get; set; }
        public DateTime purchaseDatetime { get; set; }
        public DateTime completionDatetime { get; set; }
        public Listings listing { get; set; } = new Listings();

        public async Task InsPurchases()
        {
            try
            {
                con = await CommonService.Instance.Open();
                cmd = new SqlCommand("InsPurchases", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listing.listingId;
                cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                cmd.Parameters.Add("@fee", SqlDbType.Decimal).Value = fee;
                await cmd.ExecuteNonQueryAsync();
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