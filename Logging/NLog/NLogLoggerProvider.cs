using NLog;
using NLog.Config;

namespace Logging.NLog
{
    public class NLogLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name));
        }

        public void InstallTargets()
        {
            if(LogManager.Configuration == null)
            {
                // no configuration file found, create an empty configuration
                LogManager.Configuration = new LoggingConfiguration();
            }
            LogManager.Configuration.Install(new InstallationContext());
        }

        public void Dispose()
        {
        }
    }
}
