﻿using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionRequestsController : ControllerBase
	{
        [HttpPost]
        public async Task<ActionResult> InsConnectionRequests(ConnectionRequests request)
        {
            await request.InsConnectionRequests();
            return (request.error == null) ? Ok(request) : StatusCode(request.error.errorCode, request.error);
        }
        [HttpPut]
        public async Task<ActionResult> ConnectionRequestResponse(ConnectionRequests request)
        {
            await request.ConnectionRequestResponse();
            return (request.error == null) ? Ok() : StatusCode(request.error.errorCode, request.error);
        }
        [HttpPost("UsersRequests")]
        public async Task<ActionResult> GetUserConnectionRequests(ConnectionRequests request)
        {
            var requests = await request.GetUserConnectionRequests();
            return (request.error == null) ? Ok(requests) : StatusCode(request.error.errorCode, request.error);
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteConnectionRequest(ConnectionRequests request)
        {
            await request.DeleteConnectionRequest();
            return (request.error == null) ? Ok() : StatusCode(request.error.errorCode, request.error);
        }
    }
}

