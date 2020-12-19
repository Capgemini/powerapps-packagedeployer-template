using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SdkStepsActivatorService
    {
        private readonly TraceLogger packageLog;
        private readonly CrmServiceAdapter crmSvc;

        public SdkStepsActivatorService(TraceLogger packageLog, CrmServiceAdapter crmSvc)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void Deactivate(IEnumerable<string> sdkStepsToDeactivate)
        {
            if (sdkStepsToDeactivate is null || !sdkStepsToDeactivate.Any())
            {
                this.packageLog.Log("No SDK steps to deactivate have been configured.");
                return;
            }

            var executeMultipleResponse = this.crmSvc.SetRecordStateByAttribute("sdkmessageprocessingstep", 1, 2, "name", sdkStepsToDeactivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error deactivating SDK Message Processing Steps.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

    }
}
