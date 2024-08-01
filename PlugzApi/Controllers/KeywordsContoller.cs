using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeywordsContoller : ControllerBase
    {
        [HttpGet("{listingId:int}")]
        public async Task<ActionResult> GetKeywords(int listingId)
        {
            var keywordsObj = new Keywords();
            var keywords = await keywordsObj.GetKeywords(listingId);
            return (keywordsObj.error == null) ? Ok(keywords) : StatusCode(keywordsObj.error.errorCode, keywordsObj.error);
        }
    }
}

