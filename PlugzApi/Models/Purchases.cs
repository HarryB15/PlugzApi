using System;
using Microsoft.Graph.Models;
using System.Data;
using System.Data.SqlClient;
using PlugzApi.Models;
using PlugzApi.Services;

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
        public int listingId { get; set; }
        public int offerId { get; set; }
        public string? payIntentId { get; set; }

        public async Task InsPurchases()
        {
            try
            {
                fee = (price < 5) ? (decimal)0.5 : price * (decimal)0.1;
                StripeService stripe = new StripeService();
                payIntentId = stripe.GetPaymentIntent((long)(fee + price) * 100);
                if (payIntentId != null)
                {
                    con = await CommonService.Instance.Open();
                    cmd = new SqlCommand("InsPurchases", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@listingId", SqlDbType.Int).Value = listingId;
                    cmd.Parameters.Add("@offerId", SqlDbType.Int).Value = (offerId > 0) ? offerId : null;
                    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                    cmd.Parameters.Add("@fee", SqlDbType.Decimal).Value = fee;
                    cmd.Parameters.Add("@payIntentId", SqlDbType.VarChar).Value = payIntentId;
                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    error = CommonService.GetUnexpectedErrrorMsg();
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