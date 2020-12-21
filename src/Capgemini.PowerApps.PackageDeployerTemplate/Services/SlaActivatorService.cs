using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SlaActivatorService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        public SlaActivatorService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void Activate(IEnumerable<string> defaultSlas = null)
        {
            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { 0 });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, 1, 2);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogInformation($"Error activating SLAs.", TraceEventType.Error);
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
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
                this.logger.LogInformation($"Error deactivating SLAs.", TraceEventType.Error);
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }
    }
}
