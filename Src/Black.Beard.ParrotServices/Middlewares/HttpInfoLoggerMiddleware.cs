﻿

using Bb.Extensions;
using Bb.Models.Security;
using Microsoft.AspNetCore.Http.Extensions;
using NLog.LayoutRenderers;
using System.Diagnostics;
using System.Text;

namespace Bb.ParrotServices.Middlewares
{
    internal class HttpInfoLoggerMiddleware
    {

        public HttpInfoLoggerMiddleware(RequestDelegate next, ILogger<HttpInfoLoggerMiddleware> logger, IApiKeyRepository apiKeyRepository)
        {
            _next = next;
            _logger = logger;
            _apiKeyRepository = apiKeyRepository;
        }

        // RemoteIpAddress = ::1 is localhost
        // https://stackoverflow.com/questions/28664686/how-do-i-get-client-ip-address-in-asp-net-core

        public async Task InvokeAsync(HttpContext context)
        {

            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            using (_logger.BeginScope(new[] { 
                new KeyValuePair<string, object>("{url}", context.Request?.GetDisplayUrl()), 
                new KeyValuePair<string, object>("{RemoteIpAddress}", context.Connection.RemoteIpAddress),
            }))
            {
                LogRequest(context);
                await _next(context);
                LogResponse(context);
            }

        }

        private void LogRequest(HttpContext context)
        {
            var apiKeyOwner = _apiKeyRepository.GetApiKeyFromHeaders(context)?.Owner;
            //_logger.LogAction("Processing Request", () => _logger.LogRequest(context, apiKeyOwner));
        }

        private void LogResponse(HttpContext context)
        {

            //            _logger.LogAction("Returning Response", () => _logger.LogResponse(context));
        }

        private readonly RequestDelegate _next;
        private readonly ILogger<HttpInfoLoggerMiddleware> _logger;
        private readonly IApiKeyRepository _apiKeyRepository;

    }


    ///// <summary>
    ///// Render all headers for ASP.NET Core
    ///// </summary>
    ///// <example>
    ///// <code lang="NLog Layout Renderer">
    ///// ${aspnet-request-all-headers}
    ///// </code>
    ///// </example>
    //[LayoutRenderer("aspnet-request-all-headers")]
    //public class AspNetRequestAllHeadersLayoutRenderer : NLog.Web.LayoutRenderers.AspNetLayoutRendererBase
    //{
    //    public AspNetRequestAllHeadersLayoutRenderer()
    //    {
    //    }
    //    protected override void DoAppend(StringBuilder builder, NLog.LogEventInfo logEvent)
    //    {
    //        var httpRequest = HttpContextAccessor.HttpContext.Request;
    //        if (httpRequest == null || httpRequest.Headers == null)
    //        {
    //            return;
    //        }
    //        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in httpRequest.Headers)
    //        {
    //            builder.Append(header.Key);
    //            builder.Append("=");
    //            builder.Append(header.Value);
    //        }
    //    }
    //}

}


