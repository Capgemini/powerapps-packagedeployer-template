using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class ProcessActivatorService
    {
        private readonly TraceLogger packageLog;
        private readonly CrmServiceAdapter crmSvc;

        public ProcessActivatorService(TraceLogger packageLog, CrmServiceAdapter crmSvc)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void Activate(IEnumerable<string> processesToActivate)
        {
            if (processesToActivate is null || !processesToActivate.Any())
            {
                this.packageLog.Log("No processes to activate have been configured.");
                return;
            }

            var executeMultipleResponse = this.crmSvc.SetRecordStateByAttribute("workflow", 1, 2, "name", processesToActivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error activating processes.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        public void Deactivate(IEnumerable<string> processesToDeactivate)
        {
            if (processesToDeactivate is null || !processesToDeactivate.Any())
            {
                this.packageLog.Log("No processes to deactivate have been configured.");
                return;
            }

            var executeMultipleResponse = this.crmSvc.SetRecordStateByAttribute("workflow", 0, 1, "name", processesToDeactivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error deactivating processes.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }     
    }
}
