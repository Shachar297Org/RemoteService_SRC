using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DiagnosticsTraceSource = System.Diagnostics.TraceSource;

namespace Logging.TraceSource
{
    /// <summary>
    /// The logger based on <see cref="DiagnosticsTraceSource"/>
    /// </summary>
    public class TraceSourceLogger : ILogger
    {
        /// <summary>
        /// The <see cref="DiagnosticsTraceSource"/>
        /// </summary>
        private readonly DiagnosticsTraceSource _traceSource;

        /// <summary>
        /// The <see cref="ILogger"/> name
        /// </summary>
        public string Name
        {
            get
            {
                return _traceSource.Name;
            }
        }

        /// <summary>
        /// Indicates that <see cref="ILogger"/> can write logs.
        /// </summary>
        public Func<bool> IsActive { get; set; }

        /// <summary>
        /// Create a new logger based on <see cref="DiagnosticsTraceSource"/>
        /// </summary>
        /// <param name="traceSource">The <see cref="DiagnosticsTraceSource"/></param>
        public TraceSourceLogger(DiagnosticsTraceSource traceSource)
        {
            _traceSource = traceSource;
            IsActive = () => true;
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

                if (exception != null)
                {
                    message += Environment.NewLine + exception;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                _traceSource.TraceEvent(GetEventType(logLevel), eventId, string.Format("{0}|{1}", DateTime.Now, message));
            }
        }

        /// <summary>
        /// Indicates whether the given <see cref="LogLevel"/> is active in any of the loggers
        /// </summary>
        /// <param name="logLevel">The log level</param>
        /// <returns>True if the log level is active, otherwise False</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            var traceEventType = GetEventType(logLevel);
            return _traceSource.Switch.ShouldTrace(traceEventType);
        }

        /// <summary>
        /// Map an <see cref="LogLevel"/> to a <see cref="TraceEventType"/>
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <returns>The <see cref="TraceEventType"/></returns>
        private static TraceEventType GetEventType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical: return TraceEventType.Critical;
                case LogLevel.Error: return TraceEventType.Error;
                case LogLevel.Warning: return TraceEventType.Warning;
                case LogLevel.Information: return TraceEventType.Information;
                case LogLevel.Trace:
                default: return TraceEventType.Verbose;
            }
        }

        /// <summary>
        /// Start a logical logging scope
        /// </summary>
        /// <typeparam name="TState">The log state object type</typeparam>
        /// <param name="state">The log state object</param>
        /// <returns><see cref="IDisposable"/> that can be called to end the logical logging scope</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return new TraceSourceScope(state);
        }
    }
}
