using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using DiagnosticsTraceSource = System.Diagnostics.TraceSource;

namespace Logging.TraceSource
{
    /// <summary>
    /// Provides an <see cref="ILoggerFactory"/> based on <see cref="TraceSource"/>
    /// </summary>
    public class TraceSourceLoggerProvider : ILoggerProvider
    {
        internal const string RootTraceName = "SmartLens";
        private readonly SourceSwitch _rootSourceSwitch;
        private readonly TraceListener _rootTraceListener;

        private readonly ConcurrentDictionary<string, DiagnosticsTraceSource> _sources = new ConcurrentDictionary<string, DiagnosticsTraceSource>(StringComparer.OrdinalIgnoreCase);

        private bool _disposed = false;

        #region Private Methods
        /// <summary>
        /// Get or Add a <see cref="DiagnosticsTraceSource"/> to the collection
        /// </summary>
        /// <param name="name">The trace source name</param>
        /// <returns>The <see cref="DiagnosticsTraceSource"/></returns>
        private DiagnosticsTraceSource GetOrAddTraceSource(string name)
        {
            return _sources.GetOrAdd(name, InitializeTraceSource);
        }

        /// <summary>
        /// Initialize a <see cref="DiagnosticsTraceSource"/> given its name
        /// </summary>
        /// <param name="traceSourceName">The trace source name</param>
        /// <returns>The initialized <see cref="DiagnosticsTraceSource"/></returns>
        private DiagnosticsTraceSource InitializeTraceSource(string traceSourceName)
        {
            var traceSource = new DiagnosticsTraceSource(traceSourceName);
            if (traceSourceName == RootTraceName)
            {
                if (HasDefaultSwitch(traceSource))
                {
                    traceSource.Switch = _rootSourceSwitch;
                }
                if (_rootTraceListener != null)
                {
                    traceSource.Listeners.Add(_rootTraceListener);
                }
            }
            else
            {
                string parentSourceName = ParentSourceName(traceSourceName);
                if (HasDefaultListeners(traceSource))
                {
                    var parentTraceSource = GetOrAddTraceSource(parentSourceName);
                    traceSource.Listeners.Clear();
                    traceSource.Listeners.AddRange(parentTraceSource.Listeners);
                }
                if (HasDefaultSwitch(traceSource))
                {
                    var parentTraceSource = GetOrAddTraceSource(parentSourceName);
                    traceSource.Switch = parentTraceSource.Switch;
                }
            }

            return traceSource;
        }

        /// <summary>
        /// Get the parent source name
        /// </summary>
        /// <param name="traceSourceName"></param>
        /// <returns></returns>
        private static string ParentSourceName(string traceSourceName)
        {
            var indexOfLastDot = traceSourceName.LastIndexOf('.');
            return indexOfLastDot == -1 ? RootTraceName : traceSourceName.Substring(0, indexOfLastDot);
        }

        /// <summary>
        /// Indicated whether the given <see cref="DiagnosticsTraceSource"/> has any only the default trace listener
        /// </summary>
        /// <param name="traceSource">The <see cref="DiagnosticsTraceSource"/></param>
        /// <returns>True if only the default listener is registered, otherwise False</returns>
        private static bool HasDefaultListeners(DiagnosticsTraceSource traceSource)
        {
            return traceSource.Listeners.Count == 1 && traceSource.Listeners[0] is DefaultTraceListener;
        }

        /// <summary>
        /// Indicated whether the given <see cref="DiagnosticsTraceSource"/> has an empty default switch
        /// </summary>
        /// <param name="traceSource">The <see cref="DiagnosticsTraceSource"/></param>
        /// <returns>True if only the default switch is active, otherwise False</returns>
        private static bool HasDefaultSwitch(DiagnosticsTraceSource traceSource)
        {
            return string.IsNullOrEmpty(traceSource.Switch.DisplayName) == string.IsNullOrEmpty(traceSource.Name) &&
                traceSource.Switch.Level == SourceLevels.Off;
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceLoggerProvider"/> class using the default source switch name.
        /// </summary>
        public TraceSourceLoggerProvider()
            : this(new SourceSwitch(RootTraceName), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceLoggerProvider"/> class using the given source switch name.
        /// </summary>
        /// <param name="rootSourceSwitch">The source switch name</param>
        public TraceSourceLoggerProvider(SourceSwitch rootSourceSwitch)
            : this(rootSourceSwitch, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceLoggerProvider"/> class.
        /// </summary>
        /// <param name="rootSourceSwitch">The source switch name</param>
        /// <param name="rootTraceListener">The trace listener</param>
        public TraceSourceLoggerProvider(SourceSwitch rootSourceSwitch, TraceListener rootTraceListener)
        {
            _rootSourceSwitch = rootSourceSwitch ?? new SourceSwitch(RootTraceName);
            _rootTraceListener = rootTraceListener;
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> using the given name.
        /// </summary>
        /// <param name="name">The logger name</param>
        /// <returns>The created <see cref="ILogger"/></returns>
        public ILogger CreateLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = RootTraceName;
            }
            return new TraceSourceLogger(GetOrAddTraceSource(name));
        }

        /// <summary>
        /// Dispose the current provider
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_rootTraceListener != null)
                {
                    _rootTraceListener.Flush();
                    _rootTraceListener.Dispose();
                }
            }
        }
    }
}
