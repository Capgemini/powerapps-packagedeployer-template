namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Deployment functionality relating to processes.
    /// </summary>
    public class ProcessDeploymentService : IComponentStateSettingService
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

        /// <inheritdoc/>
        public void SetStatesBySolution(IEnumerable<string> solutions, IEnumerable<string> componentsToDeactivate = null)
        {
            this.logger.LogInformation("Setting process states in solution(s).");

            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation("No solutions were provided to activate processes for.");
                return;
            }

            var deployedProcesses = this.crmSvc.RetrieveDeployedSolutionComponents(
                solutions,
                Constants.SolutionComponent.ComponentTypeWorkflow,
                Constants.Workflow.LogicalName,
                new ColumnSet(Constants.Workflow.Fields.Name)).Entities;

            this.SetStates(deployedProcesses, componentsToDeactivate);
        }

        /// <inheritdoc/>
        public void SetStates(IEnumerable<string> componentsToActivate, IEnumerable<string> componentsToDeactivate = null)
        {
            this.logger.LogInformation("Setting process states.");

            if (componentsToActivate is null)
            {
                throw new ArgumentNullException(nameof(componentsToActivate));
            }

            var allProcesses = componentsToDeactivate != null ? componentsToActivate.Concat(componentsToDeactivate) : componentsToActivate;
            if (!allProcesses.Any())
            {
                this.logger.LogInformation($"No processes were provided.");
                return;
            }

            var processes = this.RetrieveProcesses(allProcesses).Entities;
            if (!processes.Any())
            {
                this.logger.LogInformation($"No processes were found with the names provided.");
                return;
            }

            this.logger.LogInformation($"Found {processes.Count} matching processes.");
            if (processes.Count != allProcesses.Count())
            {
                this.logger.LogWarning($"Found {processes.Count} deployed processes but expected {allProcesses.Count()}.");
            }

            this.SetStates(processes, componentsToDeactivate);
        }

        private void SetStates(IEnumerable<Entity> processes, IEnumerable<string> processesToDeactivate = null)
        {
            if (processes is null)
            {
                return;
            }

            foreach (var deployedProcess in processes)
            {
                var stateCode = Constants.Workflow.StateCodeActive;
                var statusCode = Constants.Workflow.StatusCodeActive;

                if (processesToDeactivate != null && processesToDeactivate.Contains(deployedProcess[Constants.Workflow.Fields.Name]))
                {
                    stateCode = Constants.Workflow.StateCodeInactive;
                    statusCode = Constants.Workflow.StatusCodeInactive;
                }

                this.logger.LogInformation($"Setting process status for {deployedProcess[Constants.Workflow.Fields.Name]} with statecode {stateCode} and statuscode {statusCode}");
                if (!this.crmSvc.UpdateStateAndStatusForEntity(Constants.Workflow.LogicalName, deployedProcess.Id, stateCode, statusCode))
                {
                    this.logger.LogError($"Status for process {deployedProcess.Attributes[Constants.Workflow.Fields.Name]} could not be set. Please check the processes for errors e.g. missing reference data.");
                }
            }
        }

        private EntityCollection RetrieveProcesses(IEnumerable<string> names)
        {
            var query = new QueryExpression(Constants.Workflow.LogicalName)
            {
                ColumnSet = new ColumnSet(false),
            };
            query.Criteria.AddCondition(Constants.Workflow.Fields.Name, ConditionOperator.In, names.ToArray<object>());
            query.Criteria.AddCondition(Constants.Workflow.Fields.Type, ConditionOperator.Equal, Constants.Workflow.TypeDefinition);

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} processes matching the {names.Count()} provided names.");

            return results;
        }
    }
}
