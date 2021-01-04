using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class SlaDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        private const int STATECODE_ACTIVE = 1;
        private const int STATECODE_INACTIVE = 0;
        private const int STATUSCODE_ACTIVE = 2;
        private const int STATUSCODE_INACTIVE = 1;

        public SlaDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void SetDefaultSlas(IEnumerable<string> defaultSlas)
        {
            if (defaultSlas == null || !defaultSlas.Any())
            {
                this.logger.LogInformation("No default SLAs have been configured.");
                return;
            }

            var retrieveMultipleResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "name", defaultSlas);
            foreach (var defaultSla in retrieveMultipleResponse.Entities)
            {
                defaultSla["isdefault"] = true;
                this.crmSvc.Update(defaultSla);
            }
        }

        public void ActivateAll()
        {
            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { STATECODE_INACTIVE });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, STATECODE_ACTIVE, STATUSCODE_ACTIVE);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error activating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        public void DeactivateAll()
        {
            var queryResponse = this.crmSvc.QueryRecordsBySingleAttributeValue("sla", "statecode", new object[] { STATECODE_ACTIVE });
            var executeMultipleResponse = this.crmSvc.SetRecordsStateInBatch(queryResponse, STATECODE_INACTIVE, STATUSCODE_INACTIVE);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error deactivating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

    }
}
