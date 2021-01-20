namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// An adapter from <see cref="DataMigration.Core.ILogger"/> to <see cref="ILogger"/>.
    /// </summary>
    public class DataMigratorLoggerAdapter : DataMigration.Core.ILogger
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMigratorLoggerAdapter"/> class.
        /// </summary>
        /// <param name="traceLogger">The <see cref="ILogger"/>.</param>
        public DataMigratorLoggerAdapter(ILogger traceLogger)
        {
            this.logger = traceLogger;
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogError(string message)
        {
            this.logger.LogError(message);
        }

        /// <summary>
        /// Logs an error with an inner exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">The exception to log.</param>
        public void LogError(string message, Exception ex)
        {
            this.logger.LogError(ex, message);
        }

        /// <summary>
        /// Logs an info message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogInfo(string message)
        {
            this.logger.LogInformation(message);
        }

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogVerbose(string message)
        {
            this.logger.LogDebug(message);
        }

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogWarning(string message)
        {
            this.logger.LogWarning(message);
        }
    }
}
