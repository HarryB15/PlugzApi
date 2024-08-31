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
        [HttpPost("Multiple")]
        public async Task<ActionResult> InsMessageMultiple(Messages message)
        {
            await message.InsMessageMultiple();
            return (message.error == null) ? Ok() : StatusCode(message.error.errorCode, message.error);
        }
        [HttpPost("{contactUserId:int}")]
        public async Task<ActionResult> GetMessages(Messages message, int contactUserId)
        {
            var messages = await message.GetMessages(contactUserId);
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
    }
}

