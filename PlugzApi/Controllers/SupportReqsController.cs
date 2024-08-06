using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PlugzApi.Interfaces;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportReqsController: ControllerBase
    {
        private readonly IEmailService _emailService;
        public SupportReqsController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersSupportRequests(int userId)
        {
            SupportReqs reqs = new SupportReqs();
            reqs.userId = userId;
            var result = await reqs.GetUsersSupportRequests();
            return (reqs.error == null) ? Ok(result) : StatusCode(reqs.error.errorCode, reqs.error);
        }
        [HttpPost]
        public async Task<ActionResult> InsSupportRequests(SupportReqs reqs)
        {
            await reqs.InsSupportRequests();
            if (reqs.error == null)
            {
                var emailError = _emailService.SendSupportEmail(reqs);
                return (emailError == null) ? Ok(reqs) : StatusCode(emailError.errorCode, emailError);
            }
            else
            {
                return StatusCode(reqs.error.errorCode, reqs.error);
            }
        }
    }
}

