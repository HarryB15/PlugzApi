using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController: ControllerBase
	{
        [HttpPost]
        public async Task<ActionResult> InsPosts(Posts post)
        {
            await post.InsPosts();
            return (post.error == null) ? Ok() : StatusCode(post.error.errorCode, post.error);
        }
        [HttpDelete]
        public async Task<ActionResult> DeletePost(Posts post)
        {
            await post.DeletePost();
            return (post.error == null) ? Ok() : StatusCode(post.error.errorCode, post.error);
        }
        [HttpPost("UsersPosts/{incExpired:bool}")]
        public async Task<ActionResult> GetUsersPosts(Posts post, bool incExpired)
        {
            var posts = await post.GetUsersPosts(incExpired);
            return (post.error == null) ? Ok(posts) : StatusCode(post.error.errorCode, post.error);
        }
        [HttpPut]
        public async Task<ActionResult> UpdPost(Posts post)
        {
            await post.UpdPost();
            return (post.error == null) ? Ok() : StatusCode(post.error.errorCode, post.error);
        }
    }
}

