using System;
using System.Diagnostics;

namespace Logging.TraceSource
{
    /// <summary>
    /// Provides an IDisposable that represents a logical operation scope based on System.Diagnostics LogicalOperationStack
    /// </summary>
    public class TraceSourceScope : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Pushes state onto the LogicalOperationStack by calling
        /// <see cref="CorrelationManager.StartLogicalOperation(object)"/>
        /// </summary>
        /// <param name="state">The logical operation state object.</param>
        public TraceSourceScope(object state)
        {
            Trace.CorrelationManager.StartLogicalOperation(state);
        }

        /// <summary>
        /// Disposes the current instance and pops a state off the LogicalOperationStack by calling
        /// <see cref="CorrelationManager.StopLogicalOperation()"/>
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Trace.CorrelationManager.StopLogicalOperation();

                _isDisposed = true;
            }
        }
    }
}
