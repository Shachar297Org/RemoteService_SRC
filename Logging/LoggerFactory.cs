using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    /// <summary>
    /// The Default logger factory
    /// </summary>
    public class LoggerFactory : ILoggerFactory
    {
        /// <summary>
        /// The known loggers
        /// </summary>
        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>(StringComparer.Ordinal);

        /// <summary>
        /// The registered logger providers
        /// </summary>
        private List<ILoggerProvider> _providers = new List<ILoggerProvider>();

        /// <summary>
        /// Synchronization object for collection manipulation
        /// </summary>

        private readonly object _sync = new object();
        private volatile bool _disposed;

        private static readonly ILoggerFactory defaultFactory = new LoggerFactory();
        private static readonly ILogger _listener = new LogListener("SmartLens");

        /// <summary>
        /// Get the default <see cref="ILoggerFactory"/>
        /// </summary>
        public static ILoggerFactory Default
        {
            get { return defaultFactory; }
        }

        /// <summary>
        /// Default C'tor
        /// </summary>
        public LoggerFactory()
        {
            // register NLog provider by default
            this.AddNLog();

            // register TraceSource provider by default
            //this.AddTraceSource();
        }

        /// <summary>
        /// Register a logger provider
        /// </summary>
        /// <param name="provider">The logger provider</param>
        public void AddProvider(ILoggerProvider provider)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("LoggerFactory");
            }

            lock (_sync)
            {
                _providers.Add(provider);

                foreach (var logger in _loggers.Values)
                {
                    logger.AddProvider(provider);
                }
            }
        }

        /// <summary>
        /// Create a named logger for using each known provider
        /// </summary>
        /// <param name="name">The logger name</param>
        /// <returns>The created logger</returns>
        public ILogger CreateLogger(string name)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("LoggerFactory");
            }

            Logger logger;
            lock (_sync)
            {
                if (!_loggers.TryGetValue(name, out logger))
                {
                    logger = new Logger(this, name, _listener);
                    _loggers[name] = logger;
                }
            }
            return logger;
        }

        /// <summary>
        /// Get the <see cref="ILogListener"/>
        /// </summary>
        /// <returns>The <see cref="ILogListener"/></returns>
        public ILogListener GetLogListener()
        {
            return _listener as ILogListener;
        }

        /// <summary>
        /// Get the registered logger providers
        /// </summary>
        /// <returns>The logger providers</returns>
        internal ILoggerProvider[] GetProviders()
        {
            return _providers.ToArray();
        }

        /// <summary>
        /// Dispose the registered loggers and providers
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                foreach (var provider in _providers)
                {
                    try
                    {
                        provider.Dispose();
                    }
                    catch
                    {
                        // Swallow exceptions on dispose
                    }
                }

                _providers.Clear();
            }
        }
    }
}
