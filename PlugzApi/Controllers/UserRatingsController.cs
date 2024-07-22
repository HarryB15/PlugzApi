using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRatingsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> UpdInsUserRatings(UserRatings rating)
        {
            await rating.UpdInsUserRatings();
            return (rating.error == null) ? Ok() : StatusCode(rating.error.errorCode, rating.error);
        }
    }
}

