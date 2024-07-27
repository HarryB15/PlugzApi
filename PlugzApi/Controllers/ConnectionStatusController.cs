using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionStatusController : ControllerBase
	{
        [HttpGet("{userId:int}/{contactUserId:int}")]
        public async Task<ActionResult> GetConnectionStatus(int userId, int contactUserId)
        {
            ConnectionStatus status = new ConnectionStatus();
            status.userId = userId;
            status.contactUserId = contactUserId;
            await status.GetConnectionStatus();
            return (status.error == null) ? Ok(status) : StatusCode(status.error.errorCode, status.error);
        }
    }
}

