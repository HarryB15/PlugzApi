using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using Stripe;
using Stripe.Checkout;
namespace PlugzApi.Services
{
	public class StripeService
	{
		public StripeService()
		{
			StripeConfiguration.ApiKey = CommonService.Instance.GetConfig("StripeSK");
        }
        public PaymentIntent? GetPaymentIntent(long amount)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "gbp",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                };
                var service = new PaymentIntentService();
                return service.Create(options);
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return null;
            }
        }
        public async Task<bool> PaymentComplete(Event stripeEvent)
        {
            bool success = false;
            try
            {
                var paymentIntent = new PaymentIntent();
                paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    success = await HandlePaymentComplete(paymentIntent.Id);
                    if (!success)
                    {
                        await InsPayCompFailures(paymentIntent.Id, paymentIntent.Amount / 100.0m);
                        SentrySdk.CaptureMessage("Error with payment: " + paymentIntent.Id, SentryLevel.Fatal);
                    }
                }
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
            return success;
        }
        private static async Task<bool> HandlePaymentComplete(string payIntentId)
        {
            bool success = false;
            try
            {
                var con = await CommonService.Instance.Open();
                var cmd = new SqlCommand("PaymentCompleted", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@payIntentId", SqlDbType.VarChar).Value = payIntentId;
                await cmd.ExecuteNonQueryAsync();
                success = true;
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
            return success;
        }
        private static async Task<bool> InsPayCompFailures(string payIntentId, decimal amount)
        {
            bool success = false;
            try
            {
                var con = await CommonService.Instance.Open();
                var cmd = new SqlCommand("InsPayCompFailures", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@payIntentId", SqlDbType.VarChar).Value = payIntentId;
                cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
                await cmd.ExecuteNonQueryAsync();
                success = true;
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
            }
            return success;
        }
    }
}

