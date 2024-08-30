using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockedAccountsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> InsBlockedAccounts(BlockedAccounts blockedAccount)
        {
            await blockedAccount.InsBlockedAccounts();
            return (blockedAccount.error == null) ? Ok() : StatusCode(blockedAccount.error.errorCode, blockedAccount.error);
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteBlockedAccount(BlockedAccounts blockedAccount)
        {
            await blockedAccount.DeleteBlockedAccount();
            return (blockedAccount.error == null) ? Ok() : StatusCode(blockedAccount.error.errorCode, blockedAccount.error);
        }
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersBlockedAccounts(int userId)
        {
            BlockedAccounts blockedAccount = new BlockedAccounts();
            blockedAccount.userId = userId;
            var blockedAccounts = await blockedAccount.GetUsersBlockedAccounts();
            return (blockedAccount.error == null) ? Ok(blockedAccounts) : StatusCode(blockedAccount.error.errorCode, blockedAccount.error);
        }
    }
}

