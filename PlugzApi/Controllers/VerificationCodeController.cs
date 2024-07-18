using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.ODataErrors;
using PlugzApi.Interfaces;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationCodeController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public VerificationCodeController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpPost]
        public async Task<ActionResult> SendVerificationCode(VerificationCodes verificationCode)
        {
            await verificationCode.InsVerificationCode();
            if (verificationCode.error == null)
            {
                var emailError = _emailService.SendVerificationCodeEmail(verificationCode.email, verificationCode.code);
                return (emailError == null) ? Ok() : StatusCode(emailError.errorCode, emailError);
            }
            else
            {
                return StatusCode(verificationCode.error.errorCode, verificationCode.error);
            }
        }
        [HttpPut]
        public async Task<ActionResult> ValidateVerificationCode(VerificationCodes verificationCode)
        {
            await verificationCode.ValidateVerificationCode();
            return (verificationCode.error == null) ? Ok(verificationCode) : StatusCode(verificationCode.error.errorCode, verificationCode.error);
        }
    }
}

