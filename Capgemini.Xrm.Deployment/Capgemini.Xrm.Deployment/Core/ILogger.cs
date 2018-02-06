using System;

namespace Capgemini.Xrm.Deployment.Core
{
    public interface ILogger
    {
        void WriteLogMessage(string message);

        void WriteLogMessage(string message, System.Diagnostics.TraceEventType eventType);

        void WriteLogMessage(string message, System.Diagnostics.TraceEventType eventType, Exception ex);

        void Error(string message);

        void Error(string message, Exception ex);

        void Info(string message);

        void Verbose(string message);

        void Warning(string message);
    }
}