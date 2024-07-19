using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
		[HttpPost]
        public async Task<ActionResult> InsMessage(Messages message)
        {
            await message.InsMessage();
            return (message.error == null) ? Ok() : StatusCode(message.error.errorCode, message.error);
        }
    }
}

