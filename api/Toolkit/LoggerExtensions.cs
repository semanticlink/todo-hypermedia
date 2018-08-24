using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Toolkit
{
    /// <summary>
    ///     Log a message at a given level with an exception. Automatically append
    ///     the exception message and exception class for the format string to use.
    ///     IMPORTANT: This method will provide the following additional message format arguments to the
    ///     format message:
    ///     <ul>
    ///         <li>the exception message</li>
    ///         <li>the exception class type</li>
    ///     </ul>
    /// </summary>
    public static class LoggerExtensions
    {
        [StringFormatMethod("format")]
        public static void LogFormat(
            this ILogger logger,
            LogLevel level,
            string format,
            params object[] args)
        {
            logger.Log(level, (Exception) null, format, args);
        }

        [StringFormatMethod("format")]
        public static void TraceExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogTrace(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        [StringFormatMethod("format")]
        public static void DebugExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogDebug(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        /// <seealso cref = "ErrorExceptionFormat" />
        [StringFormatMethod("format")]
        public static void InfoExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogInformation(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        /// <seealso cref = "ErrorExceptionFormat" />
        [StringFormatMethod("format")]
        public static void WarnExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogWarning(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        [StringFormatMethod("format")]
        public static void ErrorExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogError(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        /// <seealso cref = "ErrorExceptionFormat" />
        [StringFormatMethod("format")]
        public static void FatalExceptionFormat(
            this ILogger logger,
            Exception ex,
            string format,
            params object[] args)
        {
            logger.LogCritical(ex, format, args.Concatenate(ex.Message, ex.GetType().Name));
        }

        [StringFormatMethod("format")]
        public static void InfoFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void DebugFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Debug, format, args);
        }

        [StringFormatMethod("format")]
        public static void WarnFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void ErrorFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Error, format, args);
        }

        [StringFormatMethod("format")]
        public static void FatalFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Critical, format, args);
        }

        [StringFormatMethod("format")]
        public static void TraceFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Trace, format, args);
        }

        public static void Trace(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Trace, message);
        }

        public static void Debug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        public static void Info(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Information, message);
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Error, message);
        }

        public static void Fata(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Critical, message);
        }

        public static void Warn(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        /// <summary>
        ///     <para>
        ///         Run an action and log a caller provided message
        ///     </para>
        ///     <para>
        ///         The caller provides the logging level and the message. This
        ///         utility will inject an additional (magic) parameter
        ///         into the log message which is the timespan
        ///     </para>
        /// </summary>
        [StringFormatMethod("message")]
        public static void StopWatch(
            this ILogger logger,
            Action a,
            Func<TimeSpan, LogLevel> level,
            string message,
            params object[] args)
        {
            var sw = LogStopwatchStart();
            try
            {
                a();
            }
            finally
            {
                LogStopwatchEnd(sw, logger, level, message, args);
            }
        }

        /// <summary>
        ///     <para>
        ///         Run an async action and log a caller provided message
        ///     </para>
        ///     <para>
        ///         The caller provides the logging level and the message. This utility
        ///         will inject an additional (magic) parameter
        ///         into the log message which is the timespan
        ///     </para>
        /// </summary>
        [StringFormatMethod("message")]
        public static async Task StopWatchAsync(
            this ILogger logger,
            Func<Task> a,
            Func<TimeSpan, LogLevel> level,
            string message,
            params object[] args)
        {
            var sw = LogStopwatchStart();
            try
            {
                await a()
                    .ConfigureAwait(false);
            }
            finally
            {
                LogStopwatchEnd(sw, logger, level, message, args);
            }
        }

        /// <summary>
        ///     <para>
        ///         Run an action and log a caller provided message
        ///     </para>
        ///     <para>
        ///         The caller provides the logging level and the message. This
        ///         utility will inject an additional (magic) parameter
        ///         into the log message which is the timespan
        ///     </para>
        /// </summary>
        [StringFormatMethod("message")]
        public static T StopWatch<T>(
            this ILogger logger,
            Func<T> f,
            Func<TimeSpan, LogLevel> level,
            string message,
            params object[] args)
        {
            var sw = LogStopwatchStart();
            try
            {
                return f();
            }
            finally
            {
                LogStopwatchEnd(sw, logger, level, message, args);
            }
        }

        /// <summary>
        ///     <para>
        ///         Run an async action and log a caller provided message
        ///     </para>
        ///     <para>
        ///         The caller provides the logging level and the message. This
        ///         utility will inject an additional (magic) parameter
        ///         into the log message which is the timespan
        ///     </para>
        /// </summary>
        [StringFormatMethod("message")]
        public static async Task<T> StopWatchAsync<T>(
            this ILogger logger,
            Func<Task<T>> f,
            Func<TimeSpan, LogLevel> level,
            string message,
            params object[] args)
        {
            var sw = LogStopwatchStart();
            try
            {
                return await f()
                    .ConfigureAwait(false);
            }
            finally
            {
                LogStopwatchEnd(sw, logger, level, message, args);
            }
        }

        public static Stopwatch LogStopwatchStart()
        {
            return Stopwatch.StartNew();
        }

        public static void LogStopwatchEnd(
            Stopwatch sw,
            ILogger logger,
            Func<TimeSpan,
                LogLevel> level,
            string message,
            params object[] args)
        {
            sw.Stop();
            var elapsed = sw.Elapsed;
            logger.Log(level(elapsed), message, args.Concatenate((long) elapsed.TotalMilliseconds));
        }
    }
}