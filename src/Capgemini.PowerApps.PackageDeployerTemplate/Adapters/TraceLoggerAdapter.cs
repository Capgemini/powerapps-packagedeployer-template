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
        private readonly LoggerSettings loggerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLoggerAdapter"/> class.
        /// </summary>
        /// <param name="traceLogger">The <see cref="TraceLogger"/>.</param>
        public TraceLoggerAdapter(TraceLogger traceLogger)
            : this(traceLogger, new LoggerSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLoggerAdapter"/> class.
        /// </summary>
        /// <param name="traceLogger">The <see cref="TraceLogger"/>.</param>
        /// <param name="loggerSettings">The <see cref="LoggerSettings"/>.</param>
        public TraceLoggerAdapter(TraceLogger traceLogger, LoggerSettings loggerSettings)
        {
            if (traceLogger is null)
            {
                throw new ArgumentNullException(nameof(traceLogger));
            }

            (this.traceLogger, this.loggerSettings, this.Warnings, this.Errors) = (traceLogger, loggerSettings, new List<string>(), new List<string>());
        }

        /// <summary>
        /// Gets warning messages logged.
        /// </summary>
        public IList<string> Warnings { get; }

        /// <summary>
        /// Gets error messages logged.
        /// </summary>
        public IList<string> Errors { get; }

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

            var message = formatter(state, exception);
            if (logLevel == LogLevel.Warning)
            {
                this.Warnings.Add(message);
            }
            else if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            {
                this.Errors.Add(message);
            }

            this.traceLogger.Log(
                $"{this.GetLoggingCommand(logLevel)}{message} {(exception != null ? exception.StackTrace : string.Empty)}",
                LogLevelMap[logLevel]);
        }

        private string GetLoggingCommand(LogLevel logLevel)
        {
            if (!this.loggerSettings.LoggingCommands)
            {
                return string.Empty;
            }

            switch (logLevel)
            {
                case LogLevel.Warning:
                    return "##[task.logissue type=warning]";
                case LogLevel.Error:
                case LogLevel.Critical:
                    return "##[task.logissue type=error]";
                default:
                    return string.Empty;
            }
        }
    }
}