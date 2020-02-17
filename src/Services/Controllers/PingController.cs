using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;

namespace safnet.Identity.Api.Services.Controllers
{
    /// <summary>
    /// Returns the current date time, thus confirming that the API service is running
    /// </summary>
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PingController : ControllerBase
    {
        // Uncomment the code below to have a quick route inspector

        //private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        //public PingController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        //{
        //    _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        //}

        //[HttpGet]
        //public IActionResult Get()
        //{
        //    var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(x => new {
        //        Action = x.RouteValues["Action"],
        //        Controller = x.RouteValues["Controller"],
        //        Name = x.AttributeRouteInfo.Name,
        //        Template = x.AttributeRouteInfo.Template
        //    }).ToList();
        //    return Ok(routes);
        //}

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }
    }
}
