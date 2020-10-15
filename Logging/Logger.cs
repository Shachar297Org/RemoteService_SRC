using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Logging
{
    /// <summary>
    /// The default logger
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly LoggerFactory _loggerFactory;

        /// <summary>
        /// The registered logger providers
        /// </summary>
        private List<ILogger> _loggers = new List<ILogger>();

        private static ILogger _logListener { get; set; }

        /// <summary>
        /// The <see cref="ILogger"/> name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Indicates that <see cref="ILogger"/> can write logs.
        /// </summary>
        public Func<bool> IsActive { get; set; }

        /// <summary>
        /// Create a new logger using the given factory and name
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="name">The logger name</param>
        public Logger(LoggerFactory loggerFactory, string name, ILogger logListener)
        {
            _loggerFactory = loggerFactory;
            Name = name;
            _logListener = logListener;
            IsActive = () => true;

            var providers = loggerFactory.GetProviders();
            if(providers == null)
            {
                return;
            }

            foreach(var provider in providers)
            {
                _loggers.Add(provider.CreateLogger(name));
            }
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

            List<Exception> exceptions = null;
            foreach (var logger in _loggers)
            {
                try
                {
                    logger.Log(logLevel, eventId, state, exception, formatter);
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            try
            {
                _logListener.Log(logLevel, eventId, state, exception, formatter);
            }
            catch (Exception ex)
            {
                if (exceptions == null)
                {
                    exceptions = new List<Exception>();
                }

                exceptions.Add(ex);
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException("An error occurred while writing log messages", exceptions);
            }
        }

        /// <summary>
        /// Indicates whether the given <see cref="LogLevel"/> is active in any of the loggers
        /// </summary>
        /// <param name="logLevel">The log level</param>
        /// <returns>True if the log level is active, otherwise False</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            List<Exception> exceptions = null;
            foreach (var logger in _loggers)
            {
                try
                {
                    if (logger.IsEnabled(logLevel))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException("An error occurred while checking enabled loggers", exceptions);
            }

            return false;
        }

        /// <summary>
        /// Start a logical logging scope
        /// </summary>
        /// <typeparam name="TState">The log state object type</typeparam>
        /// <param name="state">The log state object</param>
        /// <returns><see cref="IDisposable"/> that can be called to end the logical logging scope</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (_loggers.Count == 1)
            {
                return _loggers[0].BeginScope(state);
            }

            var scope = new LoggerScope();
            int index = 0;

            List<Exception> exceptions = null;
            foreach(var logger in _loggers)
            {
                try
                {
                    var disposable = logger.BeginScope(state);
                    scope.RegisterScope(disposable);

                    index++;
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException("An error occurred while beginning a logger scope", exceptions);
            }

            return scope;
        }

        /// <summary>
        /// Add a registered logging provider
        /// </summary>
        /// <param name="provider">The logging provider</param>
        internal void AddProvider(ILoggerProvider provider)
        {
            var logger = provider.CreateLogger(Name);
            _loggers.Add(logger);
        }
    }
}
