using Capgemini.Xrm.Deployment.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Capgemini.Xrm.PackageDeployer.TestUI.Logging
{
    public class MessageLogger : ILogger, Capgemini.Xrm.DataMigration.Core.ILogger
    {
        private readonly SynchronizationContext _syncContext;
        private readonly TextBox _tbMessage;

        public MessageLogger(TextBox tbMessage, SynchronizationContext syncContext)
        {
            _tbMessage = tbMessage;
            _syncContext = syncContext;
        }

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
            this.WriteLine("Error:" + message);
        }

        public void Error(string message, Exception ex)
        {
            this.WriteLine("Error:" + message + ",Ex:" + ex.ToString());
        }

        public void Info(string message)
        {
            if (LogLevel > 1)
                this.WriteLine("Info:" + message);
        }

        public void Start(string message)
        {
            if (LogLevel > 1)
                this.WriteLine("Start:" + message);
        }

        public void Stop(string message)
        {
            if (LogLevel > 1)
                this.WriteLine("Stop:" + message);
        }

        public void Verbose(string message)
        {
            if (LogLevel > 2)
                this.WriteLine("Verbose:" + message);
        }

        public void Warning(string message)
        {
            if (LogLevel > 0)
                this.WriteLine("Warning:" + message);
        }

        public void WriteLogMessage(string message)
        {
            Info(message);
        }

        public void WriteLogMessage(string message, TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    Error(message);
                    break;

                case TraceEventType.Error:
                    Error(message);
                    break;

                case TraceEventType.Warning:
                    Warning(message);
                    break;

                case TraceEventType.Start:
                    Start(message);
                    break;

                case TraceEventType.Stop:
                    Stop(message);
                    break;

                default:
                    Info(message);
                    break;
            }
        }

        public void WriteLogMessage(string message, TraceEventType eventType, Exception ex)
        {
            WriteLogMessage(message + " error:" + ex, eventType);
        }

        private void WriteLine(string message)
        {
            _syncContext.Send(p =>
            {
                _tbMessage.AppendText(string.Format("{0} - {1}{2}", DateTime.Now, message, Environment.NewLine));
            }, null);
        }
    }
}