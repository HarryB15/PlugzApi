using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<List<Contacts>>> GetUsersContacts(int userId)
        {
            Contacts contact = new Contacts();
            contact.userId = userId;
            var contacts = await contact.GetUsersContacts();
            return (contact.error == null) ? Ok(contacts) : StatusCode(contact.error.errorCode, contact.error);
        }
    }
}

