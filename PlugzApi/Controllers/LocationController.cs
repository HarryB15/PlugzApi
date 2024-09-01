using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly Location _location;
        public LocationController()
        {
            _location = new Location();
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersLocation(int userId)
        {
            _location.userId = userId;
            await _location.GetUsersLocation();
            return (_location.error == null) ? Ok(_location) : StatusCode(_location.error.errorCode, _location.error);
        }
        [HttpGet("Search/{address}")]
        public ActionResult SearchLocation(string address)
        {
            _location.address = address;
            var locations = _location.SearchLocation();
            return (_location.error == null) ? Ok(locations) : StatusCode(_location.error.errorCode, _location.error);
        }
    }
}

