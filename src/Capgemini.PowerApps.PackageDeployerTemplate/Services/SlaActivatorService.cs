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
            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { 0 });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, 1, 2);
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
            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { 1 });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, 0, 1);
            if (executeMultipleResponse.IsFaulted)
            {
                this.packageLog.Log($"Error deactivating SLAs.", TraceEventType.Error);
                this.packageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }
    }
}
