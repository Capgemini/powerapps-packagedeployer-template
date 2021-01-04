using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SdkStepDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        private const int STATECODE_INACTIVE = 1;
        private const int STATUSCODE_INACTIVE = 2;

        public SdkStepDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
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
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, STATECODE_INACTIVE, STATUSCODE_INACTIVE);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error deactivating SDK Message Processing Steps.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

    }
}
