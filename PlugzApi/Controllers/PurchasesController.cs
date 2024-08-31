using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
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
            await purchase.InsPurchases();
            return (purchase.error == null) ? Ok(purchase) : StatusCode(purchase.error.errorCode, purchase.error);
        }
        [HttpDelete("{purchaseId:int}")]
        public async Task<ActionResult> DeletePurchases(int purchaseId)
        {
            var purchase = new Purchases();
            purchase.purchaseId = purchaseId;
            await purchase.DeletePurchases();
            return (purchase.error == null) ? Ok() : StatusCode(purchase.error.errorCode, purchase.error);
        }
        [HttpPost("GetUsersPurchases/{liveOnly:bool}")]
        public async Task<ActionResult> GetUsersPurchases(Purchases purchase, bool liveOnly)
        {
            var purchases = await purchase.GetUsersPurchases(liveOnly);
            return (purchase.error == null) ? Ok(purchases) : StatusCode(purchase.error.errorCode, purchase.error);
        }
        [HttpPost("GetUsersSales/{liveOnly:bool}")]
        public async Task<ActionResult> GetUsersSales(Purchases purchase, bool liveOnly)
        {
            var purchases = await purchase.GetUsersSales(liveOnly);
            return (purchase.error == null) ? Ok(purchases) : StatusCode(purchase.error.errorCode, purchase.error);
        }
        [HttpGet("PurchasesBasic/{userId:int}")]
        public async Task<ActionResult> GetUsersPurchasesBasic(int userId)
        {
            Purchases purchase = new Purchases();
            purchase.userId = userId;
            var purchases = await purchase.GetUsersPurchasesBasic();
            return (purchase.error == null) ? Ok(purchases) : StatusCode(purchase.error.errorCode, purchase.error);
        }
        [HttpGet("SalesBasic/{userId:int}")]
        public async Task<ActionResult> GetUsersSalesBasic(int userId)
        {
            Purchases purchase = new Purchases();
            purchase.userId = userId;
            var purchases = await purchase.GetUsersSalesBasic();
            return (purchase.error == null) ? Ok(purchases) : StatusCode(purchase.error.errorCode, purchase.error);
        }
    }
}

