using Bb.ParrotServices.Controllers;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;

namespace Bb.Services
{

    public class Log4netTraceListener : System.Diagnostics.TraceListener
    {

        private readonly ILogger<Log4netTraceListener> _log;

        public Log4netTraceListener(ILogger<Log4netTraceListener> logger)
        {
            _log = logger;
        }

        public override void Fail(string? message)
        {
            _log.Log(LogLevel.Error, message);
        }

        public override void Fail(string? message, string? detailMessage)
        {
            _log.Log(LogLevel.Error, message);
        }

        public override void Write(object? o)
        {
            base.Write(o);
        }

        public override void Write(object? o, string? category)
        {

            switch (category != null ? category.ToLower() : string.Empty)
            {

                case "verbose":
                case "info":
                case "infos":
                    _log.LogInformation("info", o);
                    break;

                case "warn":
                case "warning":
                case "warnings":
                    _log.LogWarning("warn", o);
                    break;

                case "fatal":
                case "error":
                case "critic":
                case "critical":
                    if (o is Exception e)
                        _log.LogError(e.Message, e);
                    else
                        _log.LogError("error", o, null);
                    break;

                case "":
                case "debug":
                default:
                    _log.LogDebug("debug", o);
                    break;
            }

        }

        public override void WriteLine(object? o)
        {
            _log.LogDebug("debug", o);
            base.WriteLine(o);
        }

        public override void WriteLine(object? o, string? category)
        {
            switch (category != null ? category.ToLower() : string.Empty)
            {

                case "verbose":
                case "info":
                case "infos":
                    _log.LogInformation("info", o);
                    break;

                case "warn":
                case "warning":
                case "warnings":
                    _log.LogWarning("warn", o);
                    break;

                case "fatal":
                case "error":
                case "critic":
                case "critical":
                    if (o is Exception e)
                        _log.LogError(e.Message, e);
                    else
                        _log.LogError("error", o, null);
                    break;

                case "debug":
                    _log.LogDebug("debug", o);
                    break;

                default:
                    _log.LogTrace("debug", o);
                    break;

            }

        }

        public override void WriteLine(string? message, string? category)
        {
            switch (category != null ? category.ToLower() : string.Empty)
            {

                case "verbose":
                case "info":
                case "infos":
                    _log.LogInformation(message);
                    break;

                case "fatal":
                case "error":
                    _log.LogError(message);
                    break;

                default:
                    _log.LogTrace(message);
                    break;
            }

        }

        public override void Write(string message)
        {
            _log.LogTrace(message);
        }

        public override void WriteLine(string message)
        {
            _log.LogTrace(message);

        }

    }

}
