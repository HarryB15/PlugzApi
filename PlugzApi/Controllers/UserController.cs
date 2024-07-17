using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;
using Microsoft.Graph.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Users>> GetUser(int userId)
        {
            Users user = new Users();
            user.userId = userId;
            await user.GetUser();
            return (user.error == null) ? Ok(user) : StatusCode(user.error.errorCode, user.error.errorMsg);
        }
        [HttpPost]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            await user.CreateUser();
            return (user.error == null) ? Ok(user) : StatusCode(user.error.errorCode, user.error.errorMsg);
        }
    }
}

