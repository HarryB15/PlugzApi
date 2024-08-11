using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> InsPurchases(Purchases purchase)
        {
            var payIntentClientSecret = await purchase.InsPurchases();
            return (purchase.error == null) ? Ok(payIntentClientSecret) : StatusCode(purchase.error.errorCode, purchase.error);
        }
    }
}

