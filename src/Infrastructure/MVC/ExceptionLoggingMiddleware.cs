using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using safnet.Common.GenericExtensions;

namespace safnet.Identity.Api.Infrastructure.MVC
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
        {
            _next = next.MustNotBeNull(nameof(next));
            _logger = logger.MustNotBeNull(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            context.MustNotBeNull(nameof(context));

            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception on {Url}", context.Request.GetDisplayUrl());
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
