using System;
using System.Globalization;

namespace Logging
{
    /// <summary>
    /// Extentions methods for <see cref="ILogger"/>
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Validate that <see cref="ILogger"/> is not null, throw if it is
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        private static void ValidateNotNull(this ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
        }

        #region Trace
        /// <summary>
        /// Writes a <see cref="LogLevel.Trace"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Trace(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Trace, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Trace"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Trace, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Trace"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Trace(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Trace, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Trace"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Trace(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Trace, exception, format, args);
        }
        #endregion

        #region Debug
        /// <summary>
        /// Writes a <see cref="LogLevel.Debug"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Debug(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Debug, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Debug"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Debug, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Debug"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Debug(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Debug, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Debug"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Debug, exception, format, args);
        }
        #endregion

        #region Information
        /// <summary>
        /// Writes a <see cref="LogLevel.Information"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Information(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Information, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Information"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Information(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Information, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Information"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Information(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Information, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Information"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Information(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Information, exception, format, args);
        }
        #endregion

        #region Warning
        /// <summary>
        /// Writes a <see cref="LogLevel.Warning"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Warning(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Warning, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Warning"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warning(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Warning, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Warning"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Warning(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Warning, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Warning"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warning(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Warning, exception, format, args);
        }
        #endregion

        #region Error
        /// <summary>
        /// Writes a <see cref="LogLevel.Error"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Error(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Error, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Error"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Error, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Error"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Error(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Error, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Error"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Error, exception, format, args);
        }
        #endregion

        #region Critical
        /// <summary>
        /// Writes a <see cref="LogLevel.Critical"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        public static void Critical(this ILogger logger, string message)
        {
            logger.WriteLog(LogLevel.Critical, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Critical"/> log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Critical(this ILogger logger, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Critical, format, args);
        }

        /// <summary>
        /// Writes a <see cref="LogLevel.Critical"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The log message.</param>
        public static void Critical(this ILogger logger, Exception exception, string message = null)
        {
            logger.WriteLog(LogLevel.Critical, exception, message);
        }

        /// <summary>
        /// Writes a formatted <see cref="LogLevel.Critical"/> log message with an exception.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="format">Format string of the log message.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Critical(this ILogger logger, Exception exception, string format, params object[] args)
        {
            logger.WriteLog(LogLevel.Critical, exception, format, args);
        }
        #endregion

        #region Helpers
        private static void WriteLog(this ILogger logger, LogLevel logLevel, string message)
        {
            logger.ValidateNotNull();
            logger.Log(logLevel, 0, message, null, null);
        }

        private static void WriteLog(this ILogger logger, LogLevel logLevel, string format, params object[] args)
        {
            logger.ValidateNotNull();
            logger.Log(logLevel, 0, string.Empty, null, (msg, err) => string.Format(CultureInfo.InvariantCulture, format, args));
        }

        private static void WriteLog(this ILogger logger, LogLevel logLevel, Exception exception, string message)
        {
            logger.ValidateNotNull();
            logger.Log(logLevel, 0, message, exception, null);
        }

        private static void WriteLog(this ILogger logger, LogLevel logLevel, Exception exception, string format, params object[] args)
        {
            logger.ValidateNotNull();
            logger.Log(logLevel, 0, string.Empty, exception, (msg, err) => string.Format(CultureInfo.InvariantCulture, format, args));
        }
        #endregion
    }
}
