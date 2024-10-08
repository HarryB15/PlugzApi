﻿using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfferController: ControllerBase
	{
        [HttpPost("{receiverUserId:int}")]
        public async Task<ActionResult> InsOffer(Offer offer, int receiverUserId)
        {
            await offer.InsOffer(receiverUserId);
            return (offer.error == null) ? Ok(offer.offerId) : StatusCode(offer.error.errorCode, offer.error);
        }
        [HttpPatch]
        public async Task<ActionResult> UpdOfferResponse(Offer offer)
        {
            await offer.UpdOfferResponse();
            return (offer.error == null) ? Ok() : StatusCode(offer.error.errorCode, offer.error);
        }
        [HttpPost("GetUsersOffers")]
        public async Task<ActionResult> GetUsersOffers(Offer offer)
        {
            var offers = await offer.GetUsersOffers();
            return (offer.error == null) ? Ok(offers) : StatusCode(offer.error.errorCode, offer.error);
        }
        [HttpPost("Search/{searchValue}")]
        public async Task<ActionResult> SearchOffers(Offer offer, string searchValue)
        {
            var offers = await offer.SearchOffers(searchValue);
            return (offer.error == null) ? Ok(offers) : StatusCode(offer.error.errorCode, offer.error);
        }
    }
}

