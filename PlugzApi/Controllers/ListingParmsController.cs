using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingParmsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> GetListings(ListingParms parms)
        {
            await parms.GetListings();
            return (parms.error == null) ? Ok(parms.results) : StatusCode(parms.error.errorCode, parms.error);
        }
    }
}

