using System;

namespace Logging
{
    /// <summary>
    /// An <see cref="ILogger"/> type used for listening for log message
    /// An action can be registered to be invoked on logs
    /// </summary>
    public class LogListener : ILogListener, ILogger
    {
        private event EventHandler<string> OnLogMessage;

        private void RaiseLogMessage(string msg)
        {
            var handler = OnLogMessage;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        /// <summary>
        /// Indicates whether the given <see cref="LogLevel"/> is active
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/></param>
        /// <returns>True if the <see cref="LogLevel"/> is active, otherwise False</returns>
        public bool IsEnabled(LogLevel level)
        {
            var handler = OnLogMessage;
            return handler != null;
        }

        /// <summary>
        /// The <see cref="ILogger"/> name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Indicates that <see cref="ILogger"/> can write logs.
        /// </summary>
        public Func<bool> IsActive { get; set; }

        /// <summary>
        /// Create a new logger based on <see cref="LogListener"/>
        /// </summary>
        /// <param name="logger">The <see cref="LogListener"/></param>
        public LogListener(string name)
        {
            Name = name;
            IsActive = () => true;
        }

        /// <summary>
        /// Register an action to be invoked on the logs
        /// </summary>
        /// <param name="logAction">The action to be invoked on the log</param>
        /// <param name="filter">A filter on the <see cref="ILogger"/></param>
        public void RegisterLogAction(Action<string> logAction, string filter = "")
        {
            if (logAction == null)
            {
                throw new ArgumentNullException("logAction");
            }

            OnLogMessage += (s, msg) => logAction(msg);
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
                RaiseLogMessage(string.Format("{0}|{1}|{2}|{3}", DateTime.Now, logLevel, Name, message));
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
            return EmptyDisposable.Instance;
        }

        /// <summary>
        /// An empty disposable
        /// </summary>
        private class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();

            static EmptyDisposable() { }
            private EmptyDisposable() { }

            public void Dispose()
            {
            }
        }
    }
}
