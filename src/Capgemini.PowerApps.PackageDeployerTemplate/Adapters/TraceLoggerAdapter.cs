using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    internal class TraceLoggerAdapter : ILogger
    {
        private readonly TraceLogger traceLogger;

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

            var logRecord = string.Format("{0} [{1}] {2} {3}", "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]", logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : "");

            traceLogger.Log(logRecord);
        }
    }
}
