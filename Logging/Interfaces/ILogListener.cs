using System;

namespace Logging
{
    /// <summary>
    /// An <see cref="ILogger"/> type used for listening for log message
    /// An action cna be registered to be invoked on logs
    /// </summary>
    public interface ILogListener
    {
        /// <summary>
        /// Register an action to be invoked on the logs
        /// </summary>
        /// <param name="logAction">The action to be invoked on the log</param>
        /// <param name="filter">A filter on the <see cref="ILogger"/></param>
        void RegisterLogAction(Action<string> logAction, string filter = null);
    }
}
