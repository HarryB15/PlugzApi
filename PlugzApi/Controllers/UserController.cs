using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;
using Microsoft.Graph.Models;
using PlugzApi.Interfaces;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly VerificationCodeController _verificationCodeController;
        public UserController(IEmailService emailService)
        {
            _verificationCodeController = new VerificationCodeController(emailService);
        }
        [HttpGet("{jwt}")]
        public async Task<ActionResult<Users>> GetUser(string jwt)
        {
            Users user = new Users();
            user.jwt = jwt;
            await user.GetUser();
            return (user.error == null) ? Ok(user) : StatusCode(user.error.errorCode, user.error);
        }
        [HttpPost]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            await user.CreateUser();
            if(user.error == null)
            {
                var verificationCode = new VerificationCodes()
                {
                    userId = user.userId,
                    email = user.email
                };
                await _verificationCodeController.SendVerificationCode(verificationCode);
                return Ok(user);
            }
            else
            {
                return StatusCode(user.error.errorCode, user.error);
            }
        }
    }
}

