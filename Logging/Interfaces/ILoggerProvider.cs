using System;

namespace Logging
{
    /// <summary>
    /// A provider of <see cref="ILogger"/> instance types.
    /// </summary>
    public interface ILoggerProvider : IDisposable
    {
        /// <summary>
        /// Create a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="name">The name for messages produced by the logger.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        ILogger CreateLogger(string name);
    }
}
