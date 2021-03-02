namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
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
        protected override string GetPrefix(LogLevel logLevel)
        {
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