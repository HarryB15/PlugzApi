using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
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
        [HttpGet("{userId:int}/{contactUserId:int}")]
        public async Task<ActionResult<List<Messages>>> GetMessages(int userId, int contactUserId)
        {
            Messages message = new Messages();
            var messages = await message.GetMessages(userId, contactUserId);
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
    }
}

