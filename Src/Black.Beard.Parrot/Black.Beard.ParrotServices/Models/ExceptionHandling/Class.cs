using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace Bb.Models.ExceptionHandling
{

    public interface IProblemDetailsFactory
    {
        ProblemDetailsWithNotifications BadRequest(List<Notification> notifications);
        ProblemDetails InternalServerError();
    }

    internal class ProblemDetailsConfiguration
    {
        public string UrnName { get; set; }
    }

    internal class ProblemDetailsFactory : IProblemDetailsFactory
    {

        public const string BadRequestId = "69244389-3C4E-4D94-ABDC-C05E703E3DBD";

        private readonly ProblemDetailsConfiguration _configuration;

        public ProblemDetailsFactory(ProblemDetailsConfiguration configuration) => _configuration = configuration;

        public ProblemDetailsWithNotifications BadRequest(List<Notification> notifications)
        {
            // ProblemDetails implements the RFC7807 standards.
            var problemDetails = new ProblemDetailsWithNotifications
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "Review the notifications for details.",
                Instance = $"urn:{_configuration.UrnName}:error:{BadRequestId}",
                Notifications = notifications
            };

            problemDetails.Extensions["traceId"] = Tracking.TraceId;
            return problemDetails;
        }

        public ProblemDetails InternalServerError()
        {
            const string uncaughtExceptionId = "D1537B75-D85A-48CF-8A02-DF6C614C3198";

            // ProblemDetails implements the RFC7807 standards.
            var problemDetails = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "Internal Server Error",
                Instance = $"urn:{_configuration.UrnName}:error:{uncaughtExceptionId}"
            };

            problemDetails.Extensions["traceId"] = Tracking.TraceId;
            return problemDetails;
        }
    }

    public class ProblemDetailsWithNotifications : ProblemDetails
    {
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public class Notification
    {
        public string Id { get; set; }
        public string Severity { get; set; }
        public string Detail { get; set; }

        public List<Notification> Notifications { get; } = new List<Notification>();

        public static Notification CreateError(string id, string detail) => new Notification { Id = id, Detail = detail, Severity = SeverityType.Error };
        public static Notification CreateInfo(string id, string detail) => new Notification { Id = id, Detail = detail, Severity = SeverityType.Info };
        public static Notification CreateWarning(string id, string detail) => new Notification { Id = id, Detail = detail, Severity = SeverityType.Warning };

        public static class SeverityType
        {
            public const string Error = "Error";
            public const string Info = "Info";
            public const string Warning = "Warning";
        }
    }

    public static class Tracking
    {
        // https://www.w3.org/TR/trace-context/
        // TraceParentStateString and TraceStateString values are not validated here.
        // If these fields are critical for your tracking, then follow the detailed guidelines in the W3C trace-context.
        public static string TraceParentId => Activity.Current?.ParentId;
        public static string TraceId => Activity.Current?.Id;
        public static string TraceParentStateString => Activity.Current?.TraceStateString;
        public static string TraceStateString(string traceStateStringValue) => $"{ApplicationName}={traceStateStringValue}{ParseTraceParentStateString()}";

        private static readonly string ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
        private static string ParseTraceParentStateString()
        {
            var state = TraceParentStateString?.Split(',').ToList().FirstOrDefault();
            return !string.IsNullOrEmpty(state) ? $",{state}" : string.Empty;
        }
    }

}
