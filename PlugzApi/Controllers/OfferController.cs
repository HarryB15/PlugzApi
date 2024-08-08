using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfferController: ControllerBase
	{
        [HttpPost]
        public async Task<ActionResult> InsOffer(Offer offer)
        {
            await offer.InsOffer();
            return (offer.error == null) ? Ok(offer.offerId) : StatusCode(offer.error.errorCode, offer.error);
        }
    }
}

