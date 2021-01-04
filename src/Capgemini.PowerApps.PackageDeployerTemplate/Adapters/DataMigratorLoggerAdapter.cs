using Microsoft.Extensions.Logging;
using System;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    public class DataMigratorLoggerAdapter : Capgemini.DataMigration.Core.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        public DataMigratorLoggerAdapter(Microsoft.Extensions.Logging.ILogger traceLogger)
        {
            this.logger = traceLogger;
        }

        public void LogError(string message)
        {
            this.logger.LogError(message);
        }

        public void LogError(string message, Exception ex)
        {
            this.logger.LogError(ex, message);
        }

        public void LogInfo(string message)
        {
            this.logger.LogInformation(message);
        }

        public void LogVerbose(string message)
        {
            logger.LogDebug(message);
        }

        public void LogWarning(string message)
        {
            logger.LogWarning(message);
        }
    }
}
