using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public void Activate(IEnumerable<string> defaultSlas)
        {
            if (defaultSlas == null || defaultSlas.Any())
            {
                this.logger.LogInformation("No default SLAs have been configured.");
                return;
            }

            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { 0 });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, 1, 2);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error activating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
                return;
            }

            var retrieveMultipleResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "name", defaultSlas);
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
                this.logger.LogError($"Error deactivating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

    }
}
