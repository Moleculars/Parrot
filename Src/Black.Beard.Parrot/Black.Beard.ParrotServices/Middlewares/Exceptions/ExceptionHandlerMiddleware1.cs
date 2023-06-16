using Bb.Extensions;
using Microsoft.AspNetCore.Mvc;
using Bb.Models.ExceptionHandling;
using System.Text.Json;

namespace Bb.Middlewares.Exceptions
{


    // Inspiration: https://code-maze.com/global-error-handling-aspnetcore/#builtinmiddleware
    internal class ExceptionHandlerMiddleware1
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware1> _logger;
        private readonly IProblemDetailsFactory _problemDetailsFactory;

        public ExceptionHandlerMiddleware1(RequestDelegate next, ILogger<ExceptionHandlerMiddleware1> logger, IProblemDetailsFactory problemDetailsFactory)
        {
            _next = next;
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            string id = Guid.NewGuid().ToString();
            context.TraceIdentifier = id;
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var problemDetails = _problemDetailsFactory.InternalServerError();
                await WriteResponseAsync(context, problemDetails);
                LogError(context, problemDetails, exception);
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, ProblemDetails problemDetails)
        {
            // Add same headers returned by the built-in exception handler.
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "-1";
            context.Response.StatusCode = problemDetails.Status ?? 0;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        private void LogError(HttpContext context, ProblemDetails problemDetails, Exception exception)
        {
            _logger.LogAction("Internal Server Error", () =>
            {
                _logger.LogError("{ProblemDetails}", JsonSerializer.Serialize(problemDetails));
                _logger.LogError(exception, "Uncaught Exception");
                _logger.LogResponse(context);
            });
        }
    }

}
