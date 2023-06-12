using Bb.ParrotServices;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;

namespace Bb.Extensions
{


    public static class LoggerExtensions
    {
        public static void LogAction(this ILogger logger, string scope, Action method)
        {
            using (logger.BeginScopeWithApiTraceId())
            using (logger.BeginScopeWithApiTraceStateString(scope))
            using (logger.BeginScopeWithApiScope(scope))
            {
                method();
            }
        }

        private static IDisposable BeginScopeWithApiTraceId(this ILogger logger) =>
            logger.BeginScope("{Api.TraceId}", Tracking.TraceId);

        private static IDisposable BeginScopeWithApiTraceStateString(this ILogger logger, string traceStateStringValue) =>
            logger.BeginScope("{Api.TraceStateString}", Tracking.TraceStateString(traceStateStringValue));

        private static IDisposable BeginScopeWithApiTraceStateStringEncoded(this ILogger logger, string traceStateStringValue) =>
            logger.BeginScope("{Api.TraceStateString}", Tracking.TraceStateString(EncodeTraceStateStringValue(traceStateStringValue)));

        private static IDisposable BeginScopeWithApiScope(this ILogger logger, string scope) =>
            logger.BeginScope("{Api.Scope}", scope);

        private static string EncodeTraceStateStringValue(string traceStateStringValue) =>
            Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(traceStateStringValue));
  
        public static void LogRequest(this ILogger logger, HttpContext context, string apiKeyOwner) =>
            logger.LogInformation(
                "{RequestMethod} {Url} | ApiKey.Owner: {ApiKeyOwner} | Remote IP Address: {RemoteIpAddress} | TraceParentId: {TraceParentId} | TraceParentStateString: {TraceParentStateString}",
                context.Request?.Method, context.Request?.GetDisplayUrl(),
                apiKeyOwner, context.Connection.RemoteIpAddress,
                Tracking.TraceParentId, Tracking.TraceParentStateString);

        public static void LogResponse(this ILogger logger, HttpContext context) =>
            logger.LogInformation(
                "{RequestProtocol} {StatusCode} {StatusCodeName}",
                context.Request?.Protocol,
                context.Response?.StatusCode,
                GetHttpStatusCodeName(context.Response?.StatusCode));

        private static string GetHttpStatusCodeName(int? statusCode) =>
            statusCode != null ? Enum.GetName(typeof(HttpStatusCode), statusCode) : string.Empty;

    }

}
