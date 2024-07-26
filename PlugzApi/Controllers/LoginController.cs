using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using PlugzApi.Interfaces;
using PlugzApi.Models;
using PlugzApi.Services;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public LoginController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpPost]
        public async Task<ActionResult<Login>> Login(Login login)
        {
            var valid = await login.ValidateUser();
            if (valid && login.error == null)
            {
                return Ok(login);
            }
            else if (login.error != null)
            {
                return StatusCode(login.error.errorCode, login.error);
            }
            else
            {
                return StatusCode(500, "Unexpected error please try again later");
            }
        }
        [HttpPatch("Forgot")]
        public async Task<ActionResult> ForgotPassword(Login login)
        {
            var user = await login.ForgotPassword();
            if(login.error == null)
            {
                var emailError = _emailService.SendResetPasswordEmail(login.password, user.userName, user.email);
                return (emailError == null) ? Ok() : StatusCode(emailError.errorCode, emailError);
            }
            else
            {
                return StatusCode(login.error.errorCode, login.error);
            }
        }
        [HttpPatch("Reset")]
        public async Task<ActionResult> ResetPassword(Login login)
        {
            await login.UpdUsersPassword();
            return (login.error == null) ? Ok() : StatusCode(login.error.errorCode, login.error);
        }
    }
}

