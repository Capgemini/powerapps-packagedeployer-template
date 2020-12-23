using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    internal class TraceLoggerAdapter : ILogger
    {
        private readonly TraceLogger traceLogger;

        private static readonly Dictionary<LogLevel, TraceEventType> logLevelMap = new Dictionary<LogLevel, TraceEventType>
        {
            { LogLevel.Trace, TraceEventType.Verbose },
            { LogLevel.Debug, TraceEventType.Verbose },
            { LogLevel.Information,  TraceEventType.Information },
            { LogLevel.Warning, TraceEventType.Warning },
            { LogLevel.Error, TraceEventType.Error },
            { LogLevel.Critical, TraceEventType.Critical },
        };

        public TraceLoggerAdapter(TraceLogger traceLogger)
        {
            this.traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logMessage = string.Format("{0} {1}", formatter(state, exception), exception != null ? exception.StackTrace : "");
            var traceEventType = logLevelMap[logLevel];

            traceLogger.Log(logMessage, traceEventType);
        }
    }
}
