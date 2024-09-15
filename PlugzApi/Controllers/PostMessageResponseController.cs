using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostMessageResponseController: ControllerBase
	{
        [HttpGet]
        public async Task<ActionResult> GetPostMessageResponses(int postMessageId)
        {
            PostMessageResponse response = new PostMessageResponse();
            response.postMessageId = postMessageId;
            var responses = await response.GetPostMessageResponses();
            return (response.error == null) ? Ok(responses) : StatusCode(response.error.errorCode, response.error);
        }
        [HttpPost]
        public async Task<ActionResult> UpsertPostMessageResponses(PostMessageResponse response)
        {
            await response.UpsertPostMessageResponses();
            return (response.error == null) ? Ok() : StatusCode(response.error.errorCode, response.error);
        }
    }
}

