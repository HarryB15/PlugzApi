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
        [HttpGet("{userId:int}/{contactUserId:int}/{listingId:int}")]
        public async Task<ActionResult> GetListingMessages(int userId, int contactUserId, int listingId)
        {
            Messages message = new Messages();
            message.userId = userId;
            message.listingId = listingId;
            var messages = await message.GetListingMessages(contactUserId);
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
    }
}

