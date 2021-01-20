namespace Capgemini.PowerApps.PackageDeployerTemplate.Extensions
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk.Messages;

    /// <summary>
    /// Extensions to <see cref="ILogger"/>.
    /// </summary>
    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs the errors from a <see cref="ExecuteMultipleResponse"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="executeMultipleResponse">The response to log errors from.</param>
        public static void LogExecuteMultipleErrors(this ILogger logger, ExecuteMultipleResponse executeMultipleResponse)
        {
            foreach (var response in executeMultipleResponse.Responses.Where(r => r.Fault != null).Select(r => r.Fault))
            {
                logger.LogError(response.Message);
            }
        }
    }
}
