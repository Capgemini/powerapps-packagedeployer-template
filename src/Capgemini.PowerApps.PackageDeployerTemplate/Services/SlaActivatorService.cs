using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SlaActivatorService
    {
        private readonly TraceLogger packageLog;
        private readonly CrmServiceAdapter crmSvc;

        public SlaActivatorService(TraceLogger packageLog, CrmServiceAdapter crmSvc)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }


        public void Activate(IEnumerable<string> defaultSlas = null)
        {
            var executeMultipleResponse = this.crmSvc.SetRecordStateByAttribute("sla", 1, 2, "statecode", new object[] { 0 });
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error activating SLAs.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }

            if (defaultSlas == null || defaultSlas?.Count() == 0)
            {
                return;
            }

            var defaultSlasQuery = new QueryByAttribute("sla") { ColumnSet = new ColumnSet(false) };
            foreach (var sla in defaultSlas)
            {
                defaultSlasQuery.AddAttributeValue("name", sla);
            }
            var retrieveMultipleResponse = this.crmSvc.RetrieveMultiple(defaultSlasQuery);

            foreach (var defaultSla in retrieveMultipleResponse.Entities)
            {
                defaultSla["isdefault"] = true;
                this.crmSvc.Update(defaultSla);
            }
        }

        public void Deactivate()
        {
            var executeMultipleResponse = this.crmSvc.SetRecordStateByAttribute("sla", 0, 1, "statecode", new object[] { 1 });
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error deactivating SLAs.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }
    }
}
