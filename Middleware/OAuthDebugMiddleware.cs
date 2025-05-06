using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TiengAnh.Middleware
{
    public class OAuthDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OAuthDebugMiddleware> _logger;

        public OAuthDebugMiddleware(RequestDelegate next, ILogger<OAuthDebugMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log OAuth callback requests
            if (context.Request.Path.StartsWithSegments("/signin-google") || 
                context.Request.Path.StartsWithSegments("/Account/ExternalLoginCallback"))
            {
                _logger.LogInformation($"========== OAUTH DEBUG ==========");
                _logger.LogInformation($"OAuth Request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
                _logger.LogInformation($"Host: {context.Request.Host}");
                _logger.LogInformation($"Scheme: {context.Request.Scheme}");
                _logger.LogInformation($"PathBase: {context.Request.PathBase}");
                _logger.LogInformation($"Remote IP: {context.Connection.RemoteIpAddress}");
                _logger.LogInformation($"IsHttps: {context.Request.IsHttps}");
                
                _logger.LogInformation($"Headers:");
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogInformation($"  {header.Key}={header.Value}");
                }

                _logger.LogInformation($"Cookies:");
                foreach (var cookie in context.Request.Cookies)
                {
                    _logger.LogInformation($"  {cookie.Key}=Length:{cookie.Value?.Length ?? 0}");
                }

                _logger.LogInformation($"Query String:");
                foreach (var query in context.Request.Query)
                {
                    _logger.LogInformation($"  {query.Key}={query.Value}");
                }
                
                _logger.LogInformation($"========== END OAUTH DEBUG ==========");
            }

            await _next(context);
        }
    }
}
