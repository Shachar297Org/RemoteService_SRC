using System;
using Logging.NLog;
using nsNLog = global::NLog;

namespace Logging
{
    /// <summary>
    /// NLog extensions for the <see cref="ILoggerFactory"/>
    /// </summary>
    public static class NLogLoggerFactoryExtensions
    {
        /// <summary>
        /// Enable NLog as logging provider in a <see cref="ILoggerFactory"/>
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/></param>
        /// <returns>The <see cref="ILoggerFactory"/> with NLog provider</returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            using (var provider = new NLogLoggerProvider())
            {
                factory.AddProvider(provider);

                provider.InstallTargets();
            }
            return factory;
        }
    }
}
