using Logging.TraceSource;
using System;
using System.Diagnostics;

namespace Logging
{
    /// <summary>
    /// TraceSource extensions for the <see cref="ILoggerFactory"/>
    /// </summary>
    public static class TraceSourceFactoryExtensions
    {
        /// <summary>
        /// Enable the default TraceSouce as logging provider in a <see cref="ILoggerFactory"/>
        /// The default switch name is <see cref="TraceSourceLoggerProvider.RootTraceName"/>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/></param>
        /// <returns>The <see cref="ILoggerFactory"/> with the default TraceSource provider</returns>
        public static ILoggerFactory AddTraceSource(this ILoggerFactory factory)
        {
            factory.AddProvider(new TraceSourceLoggerProvider());

            return factory;
        }

        /// <summary>
        /// Enable TraceSouce as logging provider in a <see cref="ILoggerFactory"/>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/></param>
        /// <param name="switchName">The trace switch name</param>
        /// <returns>The <see cref="ILoggerFactory"/> with TraceSource provider</returns>
        public static ILoggerFactory AddTraceSource(this ILoggerFactory factory, string switchName, TraceListener listener = null)
        {
            SourceSwitch sourceSwitch = null;
            if (!string.IsNullOrEmpty(switchName))
            {
                sourceSwitch = new SourceSwitch(switchName);
            }

            return factory.AddTraceSource(sourceSwitch, listener);
        }

        /// <summary>
        /// Enable TraceSouce as logging provider in a <see cref="ILoggerFactory"/>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/></param>
        /// <param name="sourceSwitch">The <see cref="SourceSwitch"/></param>
        /// <param name="listener">The <see cref="TraceListener"/></param>
        /// <returns>The <see cref="ILoggerFactory"/> with TraceSource provider</returns>
        public static ILoggerFactory AddTraceSource(this ILoggerFactory factory, SourceSwitch sourceSwitch, TraceListener listener = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            factory.AddProvider(new TraceSourceLoggerProvider(sourceSwitch, listener));

            return factory;
        }
    }
}
