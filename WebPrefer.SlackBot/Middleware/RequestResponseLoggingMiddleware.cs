using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPrefer.SlackBot.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory
                      .CreateLogger<RequestResponseLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogInformation($"Request {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            await _next(context);
            _logger.LogInformation($"Response {context.Request.Method} {context.Request.Path}{context.Request.QueryString}: {context.Response.StatusCode}");
        }
    }
}
