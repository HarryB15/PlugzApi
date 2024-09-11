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
        [HttpPost]
        public async Task<ActionResult> GetUsersContacts(Contacts contact)
        {
            var contacts = await contact.GetUsersContacts();
            return (contact.error == null) ? Ok(contacts) : StatusCode(contact.error.errorCode, contact.error);
        }
        [HttpPost("Search/{searchValue}")]
        public async Task<ActionResult> SearchContacts(Contacts contact, string searchValue)
        {
            var contacts = await contact.SearchContacts(searchValue);
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
        [HttpGet("Basic/{userId:int}")]
        public async Task<ActionResult<Contacts>> GetUsersContactsBasic(int userId)
        {
            Contacts contact = new Contacts();
            contact.userId = userId;
            var result = await contact.GetUsersContactsBasic();
            return (contact.error == null) ? Ok(result) : StatusCode(contact.error.errorCode, contact.error);
        }
    }
}

