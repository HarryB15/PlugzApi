using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostMessageResponseController: ControllerBase
	{
        [HttpPost]
        public async Task<ActionResult> UpsertPostMessageResponses(PostMessageResponse response)
        {
            await response.UpsertPostMessageResponses();
            return (response.error == null) ? Ok() : StatusCode(response.error.errorCode, response.error);
        }
    }
}

