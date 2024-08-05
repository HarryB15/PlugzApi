using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrivacyOptionsController: ControllerBase
	{

        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUsersPrivacyOptions(int userId)
        {
            PrivacyOptions privacyOptions = new PrivacyOptions();
            privacyOptions.userId = userId;
            var result = await privacyOptions.GetUsersPrivacyOptions();
            return (privacyOptions.error == null) ? Ok(result) : StatusCode(privacyOptions.error.errorCode, privacyOptions.error);
        }
        [HttpPost]
        public async Task<ActionResult> UpdInsUsersPrivacyOptions(List<PrivacyOptions> privacyOptions)
        {
            foreach(var privacyOption in privacyOptions)
            {
                await privacyOption.UpdInsUsersPrivacyOptions();
            }
            var privacyOptionError = privacyOptions.FirstOrDefault(po => po.error != null);
            return (privacyOptionError == null) ? Ok() : StatusCode(privacyOptionError.error!.errorCode, privacyOptionError.error);
        }
    }
}

