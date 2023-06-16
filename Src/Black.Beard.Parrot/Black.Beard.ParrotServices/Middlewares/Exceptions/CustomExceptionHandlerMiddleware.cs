using System.Diagnostics;
using Microsoft.Extensions.Options;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using System.Text;
using Bb.Mock;

namespace Bb.Middlewares.Exceptions
{

    /// <sum
    /// mary>
    /// A middleware for handling exceptions in the application.
    /// </summary>
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ExceptionHandlerOptions _options;
        private readonly ILogger _logger;
        //private readonly Func<object, Task> _clearCacheHeadersDelegate;
        private readonly DiagnosticListener _diagnosticListener;

        /// <summary>
        /// Creates a new <see cref="CustomExceptionHandlerMiddleware"/>
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
        /// <param name="options">The options for configuring the middleware.</param>
        /// <param name="diagnosticListener">The <see cref="DiagnosticListener"/> used for writing diagnostic messages.</param>
        public CustomExceptionHandlerMiddleware
        (
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            IOptions<ExceptionHandlerOptions> options,
            DiagnosticListener diagnosticListener)
        {
            _next = next;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<CustomExceptionHandlerMiddleware>();
            //_clearCacheHeadersDelegate = ClearCacheHeaders;
            _diagnosticListener = diagnosticListener;
            if (_options.ExceptionHandler == null)
            {
                //if (_options.ExceptionHandlingPath == null)
                //{
                //    throw new InvalidOperationException("ExceptionHandlerOptions_NotConfiguredCorrectly");
                //}
                //else
                //{
                    _options.ExceptionHandler = _next;
                //}
            }
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        public Task Invoke(HttpContext context)
        {
            ExceptionDispatchInfo edi;
            try
            {
                var task = _next(context);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(this, context, task);
                }

                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                // Get the Exception, but don't continue processing in the catch block as its bad for stack usage.
                edi = ExceptionDispatchInfo.Capture(exception);
            }

            return HandleException(context, edi);
            
        }


        static async Task Awaited(CustomExceptionHandlerMiddleware middleware, HttpContext context, Task task)
        {
            ExceptionDispatchInfo? edi = null;
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                // Get the Exception, but don't continue processing in the catch block as its bad for stack usage.
                edi = ExceptionDispatchInfo.Capture(exception);
            }

            if (edi != null)
            {
                await middleware.HandleException(context, edi);
            }
        }

        private async Task HandleException(HttpContext context, ExceptionDispatchInfo edi)
        {
            _logger.LocalUnhandledException(edi.SourceException, context);
            // We can't do anything if the response has already started, just abort.
            if (context.Response.HasStarted)
            {
                _logger.LocalResponseStartedErrorHandler(context);
                edi.Throw();
            }

            PathString originalPath = context.Request.Path;
            if (_options.ExceptionHandlingPath.HasValue)
            {
                context.Request.Path = _options.ExceptionHandlingPath;
            }
            try
            {
                var exceptionHandlerFeature = new ExceptionHandlerFeature()
                {
                    Error = edi.SourceException,
                    Path = originalPath.Value!,
                    Endpoint = context.GetEndpoint(),
                    RouteValues = context.Features.Get<IRouteValuesFeature>()?.RouteValues
                };

                await ClearHttpContext(context);

                context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
                context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);

                //await _options.ExceptionHandler!(context);

                // If the response has already started, assume exception handler was successful.
                if (context.Response.HasStarted || context.Response.StatusCode != StatusCodes.Status404NotFound || _options.AllowStatusCode404Response)
                {
                    if (_diagnosticListener.IsEnabled() && _diagnosticListener.IsEnabled("Microsoft.AspNetCore.Diagnostics.HandledException"))
                    {
                        _diagnosticListener.Write("Microsoft.AspNetCore.Diagnostics.HandledException", new { httpContext = context, exception = edi.SourceException });
                    }

                    return;
                }

                edi = ExceptionDispatchInfo.Capture(new InvalidOperationException($"The exception handler configured on {nameof(ExceptionHandlerOptions)} produced a 404 status response. " +
                    $"This {nameof(InvalidOperationException)} containing the original exception was thrown since this is often due to a misconfigured {nameof(ExceptionHandlerOptions.ExceptionHandlingPath)}. " +
                    $"If the exception handler is expected to return 404 status responses then set {nameof(ExceptionHandlerOptions.AllowStatusCode404Response)} to true.", edi.SourceException));
            }
            catch (Exception ex2)
            {
                // Suppress secondary exceptions, re-throw the original.
                _logger.LocalErrorHandlerException(ex2, context);
            }
            finally
            {
                context.Request.Path = originalPath;
            }

            edi.Throw(); // Re-throw wrapped exception or the original if we couldn't handle it
        }

        private static async Task ClearHttpContext(HttpContext context)
        {
            context.Response.Clear();

            // An endpoint may have already been set. Since we're going to re-invoke the middleware pipeline we need to reset
            // the endpoint and route values to ensure things are re-calculated.
            context.SetEndpoint(endpoint: null);
            var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
            if (routeValuesFeature != null)
            {
                routeValuesFeature.RouteValues = null!;
            }

            var headers = context.Response.Headers;
            headers.CacheControl = "no-cache,no-store";
            headers.Pragma = "no-cache";
            headers.Expires = "-1";
            headers["TraceIdentifier"] = context.TraceIdentifier;
            headers["Message"] = "Sorry, an error has occurred. Please contact our customer service with TraceIdentifier for assistance.";

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.StartAsync();   //.OnStarting(ClearCacheHeaders, context.Response);

        }

        private static Task ClearCacheHeaders(object state)
        {
            var response = (HttpResponse)state;
            var headers = response.Headers;
            headers.CacheControl = "no-cache,no-store";
            headers.Pragma = "no-cache";
            headers.Expires = "-1";
            headers.ETag = default;
            return Task.CompletedTask;
        }

    }


}
