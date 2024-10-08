﻿using System;
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
            var postMessageIds = await message.InsPostMessages();
            return (message.error == null) ? Ok(postMessageIds) : StatusCode(message.error.errorCode, message.error);
        }
        [HttpPost("GetUsersPostMessages")]
        public async Task<ActionResult> GetUsersPostMessages(PostMessages message)
        {
            var messages = await message.GetUsersPostMessages();
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
        [HttpPost("Search/{searchValue}")]
        public async Task<ActionResult> SearchPostMessages(PostMessages message, string searchValue)
        {
            var messages = await message.SearchPostMessages(searchValue);
            return (message.error == null) ? Ok(messages) : StatusCode(message.error.errorCode, message.error);
        }
        [HttpGet("Offers/{postMessageId:int}")]
        public async Task<ActionResult> GetPostMessageOffers(int postMessageId)
        {
            var postMessage = new PostMessages();
            postMessage.postMessageId = postMessageId;
            var listingIds = await postMessage.GetPostMessageOffers();
            return (postMessage.error == null) ? Ok(listingIds) : StatusCode(postMessage.error.errorCode, postMessage.error);
        }
    }
}

