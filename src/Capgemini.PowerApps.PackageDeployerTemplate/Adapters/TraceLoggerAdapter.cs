namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// An adapter class from <see cref="TraceLogger"/> to <see cref="ILogger"/>.
    /// </summary>
    internal class TraceLoggerAdapter : ILogger
    {
        private static readonly Dictionary<LogLevel, TraceEventType> LogLevelMap = new Dictionary<LogLevel, TraceEventType>
        {
            { LogLevel.Trace, TraceEventType.Verbose },
            { LogLevel.Debug, TraceEventType.Verbose },
            { LogLevel.Information,  TraceEventType.Information },
            { LogLevel.Warning, TraceEventType.Warning },
            { LogLevel.Error, TraceEventType.Error },
            { LogLevel.Critical, TraceEventType.Critical },
        };

        private readonly TraceLogger traceLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLoggerAdapter"/> class.
        /// </summary>
        /// <param name="traceLogger">The <see cref="TraceLogger"/>.</param>
        public TraceLoggerAdapter(TraceLogger traceLogger)
        {
            this.traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => default;

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            var logMessage = string.Format("{0} {1}", formatter(state, exception), exception != null ? exception.StackTrace : string.Empty);
            var traceEventType = LogLevelMap[logLevel];

            this.traceLogger.Log(logMessage, traceEventType);
        }
    }
}
