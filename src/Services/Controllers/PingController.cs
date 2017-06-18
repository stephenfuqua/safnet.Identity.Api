using System;
using System.Web.Http;

namespace FlightNode.Identity.Services.Controllers
{
    /// <summary>
    /// Returns the current date time, thus confirming that the API service is running
    /// </summary>
    public class PingController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(DateTime.Now.ToString());
        }
    }
}
