﻿using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingPostsParmsController : ControllerBase
    {
        [HttpPost("listings")]
        public async Task<ActionResult> GetListings(ListingPostsParms parms)
        {
            await parms.GetListings();
            return (parms.error == null) ? Ok(parms.listings) : StatusCode(parms.error.errorCode, parms.error);
        }
        [HttpPost("posts")]
        public async Task<ActionResult> GetPosts(ListingPostsParms parms)
        {
            await parms.GetPosts();
            return (parms.error == null) ? Ok(parms.posts) : StatusCode(parms.error.errorCode, parms.error);
        }
        [HttpPost("ListingsMap")]
        public async Task<ActionResult> GetListingsMap(ListingPostsParms parms)
        {
            await parms.GetListingsMap();
            return (parms.error == null) ? Ok(parms.listings) : StatusCode(parms.error.errorCode, parms.error);
        }
        [HttpPost("PostsMap")]
        public async Task<ActionResult> GetPostsMap(ListingPostsParms parms)
        {
            await parms.GetPostsMap();
            return (parms.error == null) ? Ok(parms.posts) : StatusCode(parms.error.errorCode, parms.error);
        }
    }
}

