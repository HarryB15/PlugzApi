using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersLocation(int userId)
        {
            Location location = new Location();
            location.userId = userId;
            await location.GetUsersLocation();
            return (location.error == null) ? Ok(location) : StatusCode(location.error.errorCode, location.error);
        }
    }
}

