using Capgemini.Xrm.Deployment.Core;
using System;
using System.Diagnostics;

namespace Capgemini.Xrm.Deployment.IntegrationTests.Core
{
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// LogLevel
        /// 0 - only Errors
        /// 1 - Warnings
        /// 2 - Info
        /// 3 - Verbose
        /// </summary>
        public static int LogLevel { get; set; } = 2;

        public void Error(string message)
        {
            Console.WriteLine("Error:" + message);
        }

        public void Error(string message, Exception ex)
        {
            if (ex == null)
                Error(message);

            Console.WriteLine("Error:" + message + ",Ex:" + ex.ToString());
        }

        public void Info(string message)
        {
            if (LogLevel > 1)
                Console.WriteLine("Info:" + message);
        }

        public void Verbose(string message)
        {
            if (LogLevel > 2)
                Console.WriteLine("Verbose:" + message);
        }

        public void Warning(string message)
        {
            if (LogLevel > 0)
                Console.WriteLine("Warning:" + message);
        }

        public void WriteLogMessage(string message)
        {
            if (LogLevel > 2)
                Console.WriteLine("Warning:" + message);
        }

        public void WriteLogMessage(string message, TraceEventType eventType)
        {
            if (eventType <= TraceEventType.Error)
                Error(message);
            else if (eventType == TraceEventType.Information)
                Info(message);
            else
                Verbose(message);
        }

        public void WriteLogMessage(string message, TraceEventType eventType, Exception ex)
        {
            if (eventType <= TraceEventType.Error)
                Error(message, ex);
            else if (eventType == TraceEventType.Information)
                Info(message);
            else
                Verbose(message);
        }
    }
}