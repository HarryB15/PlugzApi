﻿using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> InsListings(Listings listing)
        {
            await listing.InsListings();
            return (listing.error == null) ? Ok(listing.listingId) : StatusCode(listing.error.errorCode, listing.error);
        }
        [HttpPost("UsersListings")]
        public async Task<ActionResult> GetUsersListing(Listings listing)
        {
            var listings = await listing.GetUsersListing();
            return (listing.error == null) ? Ok(listings) : StatusCode(listing.error.errorCode, listing.error);
        }
        [HttpPost("{searchValue}")]
        public async Task<ActionResult> SearchListings(string searchValue, Listings listing)
        {
            var listings = await listing.SearchListings(searchValue);
            return (listing.error == null) ? Ok(listings) : StatusCode(listing.error.errorCode, listing.error);
        }
        [HttpPut]
        public async Task<ActionResult> UpdListing(Listings listing)
        {
            await listing.UpdListing();
            return (listing.error == null) ? Ok() : StatusCode(listing.error.errorCode, listing.error);
        }
        [HttpDelete("{listingId:int}")]
        public async Task<ActionResult> DeleteListing(int listingId)
        {
            Listings listing = new Listings();
            listing.listingId = listingId;
            await listing.DeleteListing();
            return (listing.error == null) ? Ok() : StatusCode(listing.error.errorCode, listing.error);
        }
    }
}

