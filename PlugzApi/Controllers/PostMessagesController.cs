using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostMessagesController: ControllerBase
	{
        [HttpPost]
        public async Task<ActionResult> InsPostMessages(PostMessages message)
        {
            await message.InsPostMessages();
            return (message.error == null) ? Ok() : StatusCode(message.error.errorCode, message.error);
        }
    }
}

