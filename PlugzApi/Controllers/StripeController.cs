using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Services;
using Stripe;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController: ControllerBase
	{
        [HttpPost]
        public async Task<IActionResult> PaymentComplete()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, CommonService.Instance.GetConfig("StripeSK"));
                StripeService stripeService = new StripeService();
                if (await stripeService.PaymentComplete(stripeEvent))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (StripeException ex)
            {
                CommonService.Log(ex);
                return StatusCode(500);
            }
        }
    }
}

