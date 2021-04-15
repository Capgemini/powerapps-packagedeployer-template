namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Crm.Sdk.Messages;
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
        /// Activates processes in the given solutions unless otherwise specified.
        /// </summary>
        /// <param name="solutions">The solutions to activate processes in.</param>
        /// <param name="componentsToDeactivate">The names of any processes to deactivate.</param>
        /// <param name="user">The username of the user to impersonate (use this when activating flows if you're authenticated as an application user).</param>
        public void SetStatesBySolution(IEnumerable<string> solutions, IEnumerable<string> componentsToDeactivate = null, string user = null)
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

            this.SetStates(deployedProcesses, componentsToDeactivate, user);
        }

        /// <summary>
        /// Sets the states of processes.
        /// </summary>
        /// <param name="componentsToActivate">The processes to activate.</param>
        /// <param name="componentsToDeactivate">The processes to deactivate.</param>
        /// <param name="user">The username of the user to impersonate (use this when activating flows if you're authenticated as an application user).</param>
        public void SetStates(IEnumerable<string> componentsToActivate, IEnumerable<string> componentsToDeactivate = null, string user = null)
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

            this.SetStates(processes, componentsToDeactivate, user);
        }

        private void SetStates(IEnumerable<Entity> processes, IEnumerable<string> processesToDeactivate = null, string user = null)
        {
            if (processes is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(user))
            {
                this.logger.LogInformation($"Activating processes as {user}.");
            }

            foreach (var deployedProcess in processes)
            {
                var stateCode = new OptionSetValue(Constants.Workflow.StateCodeActive);
                var statusCode = new OptionSetValue(Constants.Workflow.StatusCodeActive);

                if (processesToDeactivate != null && processesToDeactivate.Contains(deployedProcess[Constants.Workflow.Fields.Name]))
                {
                    stateCode.Value = Constants.Workflow.StateCodeInactive;
                    statusCode.Value = Constants.Workflow.StatusCodeInactive;
                }

                this.logger.LogInformation($"Setting process status for {deployedProcess[Constants.Workflow.Fields.Name]} with statecode {stateCode.Value} and statuscode {statusCode.Value}");

                // SetStateRequest is supposedly deprecated but UpdateRequest doesn't work for deactivating active flows
                var setStateRequest = new SetStateRequest { EntityMoniker = deployedProcess.ToEntityReference(), State = stateCode, Status = statusCode };

                try
                {
                    if (!string.IsNullOrEmpty(user))
                    {
                        this.logger.LogInformation($"Impersonating {user} to activate processes.");

                        this.crmSvc.Execute<SetStateResponse>(setStateRequest, user, fallbackToExistingUser: true);
                    }
                    else
                    {
                        this.crmSvc.Execute(setStateRequest);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Status for process {deployedProcess.Attributes[Constants.Workflow.Fields.Name]} could not be set. Please check the processes for errors e.g. missing reference data or connections.");
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
