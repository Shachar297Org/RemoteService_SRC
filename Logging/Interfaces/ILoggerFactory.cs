using System;

namespace Logging
{
    /// <summary>
    /// A factory to create instances of <see cref="ILogger"/> from the registered <see cref="ILoggerProvider"/>s
    /// </summary>
    public interface ILoggerFactory : IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="name">The name for messages produced by the logger.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        ILogger CreateLogger(string name);

        /// <summary>
        /// Get the <see cref="ILogListener"/>
        /// </summary>
        /// <returns>The <see cref="ILogListener"/></returns>
        ILogListener GetLogListener();

        /// <summary>
        /// Add an <see cref="ILoggerProvider"/> to the logging system.
        /// </summary>
        /// <param name="provider">The <see cref="ILoggerProvider"/>.</param>
        void AddProvider(ILoggerProvider provider);
    }
}
