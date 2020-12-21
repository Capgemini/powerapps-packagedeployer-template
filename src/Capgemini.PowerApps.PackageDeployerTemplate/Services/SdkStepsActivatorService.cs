using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SdkStepsActivatorService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        public SdkStepsActivatorService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void Deactivate(IEnumerable<string> sdkStepsToDeactivate)
        {
            if (sdkStepsToDeactivate is null || !sdkStepsToDeactivate.Any())
            {
                this.logger.LogInformation("No SDK steps to deactivate have been configured.");
                return;
            }

            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sdkmessageprocessingstep", "name", sdkStepsToDeactivate);
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, 1, 2);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogInformation($"Error deactivating SDK Message Processing Steps.", TraceEventType.Error);
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

    }
}
