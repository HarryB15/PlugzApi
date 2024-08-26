using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRatingsController : ControllerBase
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetUserRatings(int userId)
        {
            UserRatings rating = new UserRatings();
            rating.userId = userId;
            await rating.GetUserRating();
            return (rating.error == null) ? Ok(rating) : StatusCode(rating.error.errorCode, rating.error);
        }
        [HttpPost]
        public async Task<ActionResult> UpdInsUserRatings(UserRatings rating)
        {
            await rating.UpdInsUserRatings();
            return (rating.error == null) ? Ok() : StatusCode(rating.error.errorCode, rating.error);
        }
        [HttpGet("{userId:int}/{listingId:int}")]
        public async Task<ActionResult> GetRating(int userId, int purchaseId)
        {
            UserRatings rating = new UserRatings();
            rating.userId = userId;
            rating.purchaseId = purchaseId;
            await rating.GetRating();
            return (rating.error == null) ? Ok(rating) : StatusCode(rating.error.errorCode, rating.error);
        }
        [HttpGet("Detail/{userId:int}")]
        public async Task<ActionResult> GetUserRatingDetail(int userId)
        {
            UserRatings rating = new UserRatings();
            rating.userId = userId;
            var ratings = await rating.GetUserRatingDetail();
            return (rating.error == null) ? Ok(ratings) : StatusCode(rating.error.errorCode, rating.error);
        }
    }
}

