using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Extensions
{
    public static class TraceLoggerExtensions
    {
        public static void LogExecuteMultipleErrors(this TraceLogger traceLogger, ExecuteMultipleResponse executeMultipleResponse)
        {
            foreach (var response in executeMultipleResponse.Responses.Where(r => r.Fault != null).Select(r => r.Fault))
            {
                traceLogger.Log(response.Message, TraceEventType.Error);
            }
        }
    }
}
