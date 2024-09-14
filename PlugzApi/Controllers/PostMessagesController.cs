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
        [HttpGet("{userId:int}/{maxPostMessageId:int?}")]
        public async Task<ActionResult> GetUsersPostMessages(int userId, int maxPostMessageId)
        {
            var message = new PostMessages();
            message.userId = userId;
            var messages = await message.GetUsersPostMessages(maxPostMessageId);
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
    }
}

