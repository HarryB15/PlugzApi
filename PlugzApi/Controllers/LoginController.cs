using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [HttpPost()]
        public async Task<ActionResult> Login(Login login)
        {
            var valid = await login.ValidateUser();
            if (valid && login.error == null)
            {
                return Ok();
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
    }
}

