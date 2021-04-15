namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Deployment functionality related to SLAs.
    /// </summary>
    public class SlaDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlaDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public SlaDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Sets the provided SLAs as default.
        /// </summary>
        /// <param name="defaultSlas">The names of the SLAs.</param>
        public void SetDefaultSlas(IEnumerable<string> defaultSlas)
        {
            if (defaultSlas == null || !defaultSlas.Any())
            {
                this.logger.LogInformation("No default SLAs have been configured.");
                return;
            }

            var retrieveMultipleResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.Sla.LogicalName, Constants.Sla.Fields.Name, defaultSlas);
            foreach (var defaultSla in retrieveMultipleResponse.Entities)
            {
                defaultSla[Constants.Sla.Fields.IsDefault] = true;
                this.crmSvc.Update(defaultSla);
            }
        }

        /// <summary>
        /// Activates all SLAs.
        /// </summary>
        public void ActivateAll()
        {
            var queryResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.Sla.LogicalName, Constants.Sla.Fields.StateCode, new object[] { Constants.Sla.StateCodeInactive });
            var executeMultipleResponse = this.crmSvc.UpdateStateAndStatusForEntityInBatch(queryResponse, Constants.Sla.StateCodeActive, Constants.Sla.StatusCodeActive);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error activating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        /// <summary>
        /// Deactivates all SLAs.
        /// </summary>
        public void DeactivateAll()
        {
            var queryResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.Sla.LogicalName, Constants.Sla.Fields.StateCode, new object[] { Constants.Sla.StateCodeActive });
            var executeMultipleResponse = this.crmSvc.UpdateStateAndStatusForEntityInBatch(queryResponse, Constants.Sla.StateCodeInactive, Constants.Sla.StatusCodeInactive);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error deactivating SLAs.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }
    }
}
