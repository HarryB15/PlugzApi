using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Models;

namespace PlugzApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
	{
        [HttpGet("{countryCode}/{city}")]
        public async Task<ActionResult> GetCities(string countryCode, string city)
        {
            Cities citiesObj = new Cities();
            citiesObj.countryCode = countryCode;
            citiesObj.city = city;
            var cities = await citiesObj.GetCities();
            return (citiesObj.error == null) ? Ok(cities) : StatusCode(citiesObj.error.errorCode, citiesObj.error);
        }
    }
}

