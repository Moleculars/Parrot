using System.Collections;

namespace Bb.Middlewares.Exceptions
{

    internal static partial class DiagnosticsCustomLoggerExtensions
    {

        private static readonly Action<ILogger, Exception?> __UnhandledExceptionCallback
            = Define
            (
                LogLevel.Error,
                new EventId(1, "UnhandledException"),
                "An unhandled exception has occurred while executing the request.",
                new LogDefineOptions() { SkipEnabledCheck = true }
            );

        public static void LocalUnhandledException(this ILogger logger, Exception exception, HttpContext context)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                __UnhandledExceptionCallback(logger, exception);
            }
        }

        private static readonly Action<ILogger, Exception?> __ResponseStartedErrorHandlerCallback
            = Define
            (
                LogLevel.Warning,
                new EventId(2, "ResponseStarted"),
                "The response has already started, the error handler will not be executed.",
                new LogDefineOptions() { SkipEnabledCheck = true }
            );

        public static void LocalResponseStartedErrorHandler(this ILogger logger, HttpContext context)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                __ResponseStartedErrorHandlerCallback(logger, null);
            }
        }
        private static readonly Action<ILogger, Exception?> __ErrorHandlerExceptionCallback =
            Define(LogLevel.Error, new EventId(3, "Exception"), "An exception was thrown attempting to execute the error handler.", new LogDefineOptions() { SkipEnabledCheck = true });

        public static void LocalErrorHandlerException(this
            ILogger logger, Exception exception, HttpContext context)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                __ErrorHandlerExceptionCallback(logger, exception);
            }
        }






        /// <summary>
        /// Creates a delegate which can be invoked for logging a message.
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <param name="eventId">The event id</param>
        /// <param name="formatString">The named format string</param>
        /// <param name="options">The <see cref="LogDefineOptions"/></param>
        /// <returns>A delegate which when invoked creates a log message.</returns>
        public static Action<ILogger, Exception?> Define(LogLevel logLevel, EventId eventId, string formatString, LogDefineOptions? options)
        {
            LogValuesFormatter formatter = CreateLogValuesFormatter(formatString, expectedNamedParameterCount: 0);

            void Log(ILogger logger, Exception? exception)
            {
                logger.Log(logLevel, eventId, new LogValues(formatter), exception, LogValues.Callback);
            }

            if (options != null && options.SkipEnabledCheck)
            {
                return Log;
            }

            return (logger, exception) =>
            {
                if (logger.IsEnabled(logLevel))
                {
                    Log(logger, exception);
                }
            };
        }

        private static LogValuesFormatter CreateLogValuesFormatter(string formatString, int expectedNamedParameterCount)
        {
            var logValuesFormatter = new LogValuesFormatter(formatString);

            int actualCount = logValuesFormatter.ValueNames.Count;
            if (actualCount != expectedNamedParameterCount)
            {
                throw new ArgumentException(String.Format("Unexpected {2} of named parameters {1}. {0}", formatString, expectedNamedParameterCount, actualCount));
            }

            return logValuesFormatter;
        }


        private readonly struct LogValues : IReadOnlyList<KeyValuePair<string, object?>>
        {
            public static readonly Func<LogValues, Exception?, string> Callback = (state, exception) => state.ToString();

            private readonly LogValuesFormatter _formatter;

            public LogValues(LogValuesFormatter formatter)
            {
                _formatter = formatter;
            }

            public KeyValuePair<string, object?> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object?>("{OriginalFormat}", _formatter.OriginalFormat);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
            }

            public int Count => 1;

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            {
                yield return this[0];
            }

            public override string ToString() => _formatter.Format();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }

}
