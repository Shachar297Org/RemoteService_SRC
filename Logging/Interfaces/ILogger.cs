using System;

namespace Logging
{
    /// <summary>
    /// The logging API.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write a log entry
        /// </summary>
        /// <param name="logLevel">The level of the log entry.</param>
        /// <param name="eventId">The Id of the event log</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
        void Log<TState>(LogLevel logLevel, int eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);

        /// <summary>
        /// The <see cref="ILogger"/> name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates that <see cref="ILogger"/> can write logs.
        /// </summary>
        Func<bool> IsActive { get; set; }

        /// <summary>
        /// Checks if the given <paramref name="logLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
        IDisposable BeginScope<TState>(TState state);
    }
}
