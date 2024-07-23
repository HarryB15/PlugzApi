using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilePhotosController: ControllerBase
	{
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetProfilePhoto(int userId)
        {
            ProfilePhotos profilePhoto = new ProfilePhotos();
            profilePhoto.userId = userId;
            await profilePhoto.GetProfilePhoto();
            return (profilePhoto.error == null) ? Ok(profilePhoto) : StatusCode(profilePhoto.error.errorCode, profilePhoto.error);
        }
		[HttpPost]
        public async Task<ActionResult> UpdateProfilePhoto(ProfilePhotos profilePhoto)
        {
            await profilePhoto.UpdateProfilePhoto();
            return (profilePhoto.error == null) ? Ok() : StatusCode(profilePhoto.error.errorCode, profilePhoto.error);
        }
    }
}

