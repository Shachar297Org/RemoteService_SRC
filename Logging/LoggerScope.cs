using System;
using System.Collections.Generic;

namespace Logging
{
    /// <summary>
    /// An activity scope for loggers
    /// A collection of <see cref="IDisposable"/>s to notify when scope is ended
    /// </summary>
    internal class LoggerScope : IDisposable
    {
        /// <summary>
        /// The registered logger scope activities
        /// </summary>
        private List<IDisposable> _disposables = new List<IDisposable>();

        /// <summary>
        /// Register an <see cref="IDisposable"/> scope activity
        /// </summary>
        /// <param name="disposable">The <see cref="IDisposable"/> scope activity</param>
        public void RegisterScope(IDisposable disposable)
        {
            if(disposable == null)
            {
                return;
            }

            _disposables.Add(disposable);
        }

        /// <summary>
        /// Notify all scope activities by disposing
        /// Clear registered activities
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
    }
}
