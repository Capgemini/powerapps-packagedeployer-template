using System;
using System.Diagnostics;
using Capgemini.DataMigration.Core;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    public class LoggerAdapter : ILogger
    {
        private readonly TraceLogger traceLogger;

        public LoggerAdapter(TraceLogger traceLogger)
        {
            this.traceLogger = traceLogger;
        }

        public void LogError(string message)
        {
            this.traceLogger.Log(message, TraceEventType.Error);
        }

        public void LogError(string message, Exception ex)
        {
            this.traceLogger.Log(message, TraceEventType.Error, ex);
        }

        public void LogInfo(string message)
        {
            this.traceLogger.Log(message);
        }

        public void LogVerbose(string message)
        {
            traceLogger.Log(message, TraceEventType.Verbose);
        }

        public void LogWarning(string message)
        {
            traceLogger.Log(message, TraceEventType.Warning);
        }
    }
}
