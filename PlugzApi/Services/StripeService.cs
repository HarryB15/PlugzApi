﻿using System;
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
        public string? GetPaymentIntent(long amount)
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
                var paymentIntent = service.Create(options);
                return paymentIntent.Id;
            }
            catch (Exception ex)
            {
                CommonService.Log(ex);
                return null;
            }
        }
    }
}
