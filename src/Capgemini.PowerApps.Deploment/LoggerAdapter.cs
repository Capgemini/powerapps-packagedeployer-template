using System;
using System.Diagnostics;
using Capgemini.DataMigration.Core;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.Deployment
{
    public class LoggerAdapter : ILogger
    {
        private readonly TraceLogger traceLogger;

        public LoggerAdapter(TraceLogger traceLogger)
        {
            this.traceLogger = traceLogger;
        }

        public void Error(string message)
        {
            this.traceLogger.Log(message, TraceEventType.Error);
        }

        public void Error(string message, Exception ex)
        {
            this.traceLogger.Log(message, TraceEventType.Error, ex);
        }

        public void Info(string message)
        {
            this.traceLogger.Log(message);
        }

        public void Verbose(string message)
        {
            this.traceLogger.Log(message, TraceEventType.Verbose);
        }

        public void Warning(string message)
        {
            this.traceLogger.Log(message, TraceEventType.Warning);
        }
    }
}
