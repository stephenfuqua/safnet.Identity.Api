using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace safnet.Identity.Api.Services.Controllers
{
    /// <summary>
    /// Returns the current date time, thus confirming that the API service is running
    /// </summary>
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }
    }
}
