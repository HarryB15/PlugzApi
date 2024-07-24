using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersStatsController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersStats(int userId)
        {
            UsersStats stats = new UsersStats();
            stats.userId = userId;
            await stats.GetUsersStats();
            return (stats.error == null) ? Ok(stats) : StatusCode(stats.error.errorCode, stats.error);
        }
    }
}

