using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Messages;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Extensions
{
    public static class ILoggerExtensions
    {
        public static void LogExecuteMultipleErrors(this ILogger logger, ExecuteMultipleResponse executeMultipleResponse)
        {
            foreach (var response in executeMultipleResponse.Responses.Where(r => r.Fault != null).Select(r => r.Fault))
            {
                logger.LogError(response.Message);
            }
        }
    }
}
