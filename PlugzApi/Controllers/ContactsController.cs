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
        [HttpGet("{userId:int}/{contactUserId:int}")]
        public async Task<ActionResult<Contacts>> GetContact(int userId, int contactUserId)
        {
            Contacts contact = new Contacts();
            contact.userId = userId;
            contact.contactUser.userId = contactUserId;
            await contact.GetContact();
            return (contact.error == null) ? Ok(contact) : StatusCode(contact.error.errorCode, contact.error);
        }
        [HttpPatch]
        public async Task<ActionResult> UpdUserContactIsConnected(Contacts contact)
        {
            await contact.UpdUserContactIsConnected();
            return (contact.error == null) ? Ok() : StatusCode(contact.error.errorCode, contact.error);
        }
    }
}

