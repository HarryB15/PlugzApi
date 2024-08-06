using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportReqsController: ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersSupportRequests(int userId)
        {
            SupportReqs reqs = new SupportReqs();
            reqs.userId = userId;
            var result = await reqs.GetUsersSupportRequests();
            return (reqs.error == null) ? Ok(result) : StatusCode(reqs.error.errorCode, reqs.error);
        }
        [HttpPost]
        public async Task<ActionResult> GetMessages(SupportReqs reqs)
        {
            await reqs.InsSupportRequests();
            return (reqs.error == null) ? Ok(reqs.supportRequestId) : StatusCode(reqs.error.errorCode, reqs.error);
        }
    }
}

