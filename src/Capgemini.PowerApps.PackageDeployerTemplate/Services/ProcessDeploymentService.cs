namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Deployment functionality relating to processes.
    /// </summary>
    public class ProcessDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public ProcessDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Activates the provided processes.
        /// </summary>
        /// <param name="processesToActivate">The names of the processes to activate.</param>
        public void Activate(IEnumerable<string> processesToActivate)
        {
            if (processesToActivate is null || !processesToActivate.Any())
            {
                this.logger.LogInformation("No processes to activate have been configured.");
                return;
            }

            var queryResponse = this.QueryWorkflowsByName(processesToActivate);
            var executeMultipleResponse = this.crmSvc.UpdateStateAndStatusForEntityInBatch(queryResponse, Constants.Process.StateCodeActive, Constants.Process.StatusCodeActive);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error activating processes.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        /// <summary>
        /// Deactivates the provided processes.
        /// </summary>
        /// <param name="processesToDeactivate">The names of the processes to deactivate.</param>
        public void Deactivate(IEnumerable<string> processesToDeactivate)
        {
            if (processesToDeactivate is null || !processesToDeactivate.Any())
            {
                this.logger.LogInformation("No processes to deactivate have been configured.");
                return;
            }

            var queryResponse = this.QueryWorkflowsByName(processesToDeactivate);
            var executeMultipleResponse = this.crmSvc.UpdateStateAndStatusForEntityInBatch(queryResponse, Constants.Process.StateCodeInactive, Constants.Process.StatusCodeInactive);
            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error deactivating processes.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        /// <summary>
        /// Get workflows with the provided names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>An <see cref="EntityCollection"/> containing the workflow records.</returns>
        public EntityCollection QueryWorkflowsByName(IEnumerable<object> names)
        {
            var query = new QueryByAttribute("workflow")
            {
                Attributes = { "name" },
                ColumnSet = new ColumnSet(false),
            };
            query.Values.AddRange(names);
            query.AddAttributeValue("type", 1);

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} of {names.Count()} workflows found.");
            return results;
        }
    }
}
