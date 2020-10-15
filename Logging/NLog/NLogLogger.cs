using System;
using nsNLog = global::NLog;

namespace Logging.NLog
{
    internal class NLogLogger : ILogger
    {
        private readonly nsNLog.Logger _logger;

        /// <summary>
        /// The <see cref="ILogger"/> name
        /// </summary>
        public string Name
        {
            get
            {
                return _logger.Name;
            }
        }

        /// <summary>
        /// Indicates that <see cref="ILogger"/> can write logs.
        /// </summary>
        public Func<bool> IsActive { get; set; }

        /// <summary>
        /// Create a new logger based on <see cref="nsNLog.Logger"/>
        /// </summary>
        /// <param name="logger">The <see cref="nsNLog.Logger"/></param>
        public NLogLogger(nsNLog.Logger logger)
        {
            _logger = logger;
            IsActive = () => true;
        }

        /// <summary>
        /// Map an <see cref="LogLevel"/> to a <see cref="nsNLog.LogLevel"/>
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <returns>The <see cref="nsNLog.LogLevel"/></returns>
        private nsNLog.LogLevel ConvertLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: return nsNLog.LogLevel.Trace;
                case LogLevel.Debug: return nsNLog.LogLevel.Debug;
                case LogLevel.Information: return nsNLog.LogLevel.Info;
                case LogLevel.Warning: return nsNLog.LogLevel.Warn;
                case LogLevel.Error: return nsNLog.LogLevel.Error;
                case LogLevel.Critical: return nsNLog.LogLevel.Fatal;
                case LogLevel.Off: return nsNLog.LogLevel.Off;
                default: return nsNLog.LogLevel.Debug;
            }
        }

        /// <summary>
        /// Indicates whether the given <see cref="LogLevel"/> is active in any of the loggers
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <returns>True if the <see cref="LogLevel"/> is active, otherwise False</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(ConvertLogLevel(logLevel));
        }

        /// <summary>
        /// Log a message
        /// </summary>
        /// <typeparam name="TState">The log message state object type</typeparam>
        /// <param name="logLevel">The log level</param>
        /// <param name="eventId">The log event id</param>
        /// <param name="state">The log message state object</param>
        /// <param name="exception">The log error message</param>
        /// <param name="formatter">The log message formatter</param>
        public void Log<TState>(LogLevel logLevel, int eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsActive())
            {
                return;
            }

            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = string.Empty;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else
            {
                if (state != null)
                {
                    message += state;
                }
            }

            var eventInfo = nsNLog.LogEventInfo.Create(ConvertLogLevel(logLevel), _logger.Name, message);
            eventInfo.Exception = exception;
            eventInfo.SetStackTrace(new System.Diagnostics.StackTrace(true), 4);
            eventInfo.Properties["EventId"] = eventId;
            _logger.Log(eventInfo);
        }

        /// <summary>
        /// Start a logical logging scope
        /// </summary>
        /// <typeparam name="TState">The log state object type</typeparam>
        /// <param name="state">The log state object</param>
        /// <returns><see cref="IDisposable"/> that can be called to end the logical logging scope</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            return nsNLog.NestedDiagnosticsContext.Push(state);
        }
    }
}
