namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// An adapter class from <see cref="TraceLogger"/> to <see cref="ILogger"/> for use on Azure DevOps.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureDevOpsTraceLoggerAdapter : TraceLoggerAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDevOpsTraceLoggerAdapter"/> class.
        /// </summary>
        /// <param name="traceLogger">The <see cref="TraceLogger"/>.</param>
        public AzureDevOpsTraceLoggerAdapter(TraceLogger traceLogger)
            : base(traceLogger)
        {
        }

        /// <inheritdoc/>
        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            base.Log(logLevel, eventId, state, exception, formatter);

            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Warning:
                    Console.WriteLine($"##vso[task.logissue type=warning]{message}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Console.WriteLine($"##vso[task.logissue type=error]{message}");
                    break;
            }
        }
    }
}
