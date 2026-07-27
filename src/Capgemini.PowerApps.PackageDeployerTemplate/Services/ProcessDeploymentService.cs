namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
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

            var categoryOrder = new[]
            {
                Constants.WorkflowCategory.BusinessRule,
                Constants.WorkflowCategory.Action,
                Constants.WorkflowCategory.Workflow,
                Constants.WorkflowCategory.BusinessProcessFlow,
                Constants.WorkflowCategory.ModernFlow,
                Constants.WorkflowCategory.Dialog,
                Constants.WorkflowCategory.DesktopFlow,
                Constants.WorkflowCategory.AiFlow,
                Constants.WorkflowCategory.WebClientApiFlow,
            };

            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation("No solutions were provided to activate processes for.");
                return;
            }

            var deployedProcesses = this.crmSvc.RetrieveDeployedSolutionComponents(
                solutions,
                Constants.SolutionComponent.ComponentTypeWorkflow,
                Constants.Workflow.LogicalName,
                new ColumnSet(
                    Constants.Workflow.Fields.Name,
                    Constants.Workflow.Fields.Category,
                    Constants.Workflow.Fields.StateCode)).Entities;

            foreach (var category in categoryOrder)
            {
                this.logger.LogInformation($"Checking for deployed processes in solution(s) with category {category}.");

                var processesInCategory = deployedProcesses.Where(p => p.GetAttributeValue<OptionSetValue>(Constants.Workflow.Fields.Category).Value == category);
                if (!processesInCategory.Any())
                {
                    this.logger.LogInformation($"No deployed processes were found in solution(s) with category {category}.");
                    continue;
                }

                this.logger.LogInformation($"Found {processesInCategory.Count()} deployed processes in solution(s) with category {category}.");

                this.SetStates(processesInCategory, componentsToDeactivate, user);
            }
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

            var requests = this.GetRequestByProcess(processes, processesToDeactivate);

            if (!requests.Any())
            {
                return;
            }

            this.ExecuteUpdateRequests(requests, user);
        }

        private void ExecuteUpdateRequests(IDictionary<Entity, UpdateRequest> requestsByProcess, string user = null)
        {
            var nameMap = requestsByProcess.Keys
                .ToDictionary(e => e.Id, e => e.GetAttributeValue<string>(Constants.Workflow.Fields.Name));

            var remainingRequests = requestsByProcess.Values.Where(r => r != null);
            if (!remainingRequests.Any())
            {
                return;
            }

            this.logger.LogInformation($"Updating the states of {remainingRequests.Count()} processes.");

            var iteration = 1;
            var iterationSuccessfulRequestCount = 0;
            var errorMessages = new List<string>();
            do
            {
                errorMessages = new List<string>();
                var responses = this.crmSvc.ExecuteManySolutionHistoryOperation(
                    remainingRequests,
                    user,
                    (r, ex) =>
                    {
                        errorMessages.Add($"Failed to update status of process {nameMap[((UpdateRequest)r).Target.Id]} with the following error: {((FaultException<OrganizationServiceFault>)ex).Detail.Message}");
                    });

                remainingRequests = responses
                    .Where(kvp => kvp.Value is null)
                    .Select(kvp => kvp.Key)
                    .Cast<UpdateRequest>();

                iterationSuccessfulRequestCount = responses.Values.Where(v => v != null).Count();
                this.logger.LogInformation($"Successfully updated the state of {iterationSuccessfulRequestCount} processes in iteration {iteration}.");
                iteration++;
            }
            while (remainingRequests.Any() && iterationSuccessfulRequestCount > 0);

            foreach (var errorMessage in errorMessages)
            {
                this.logger.LogError(errorMessage);
            }
        }

        private IDictionary<Entity, UpdateRequest> GetRequestByProcess(IEnumerable<Entity> processes, IEnumerable<string> processesToDeactivate)
        {
            return processes.ToDictionary(
                p => p,
                p =>
                {
                    var stateCode = new OptionSetValue(Constants.Workflow.StateCodeActive);
                    var statusCode = new OptionSetValue(Constants.Workflow.StatusCodeActive);

                    if (processesToDeactivate != null && processesToDeactivate.Contains(p[Constants.Workflow.Fields.Name]))
                    {
                        stateCode.Value = Constants.Workflow.StateCodeInactive;
                        statusCode.Value = Constants.Workflow.StatusCodeInactive;
                    }

                    if (stateCode.Value == p.GetAttributeValue<OptionSetValue>(Constants.Workflow.Fields.StateCode).Value)
                    {
                        this.logger.LogInformation($"Process {p[Constants.Workflow.Fields.Name]} will be skipped. Already has desired state.");
                        return null;
                    }

                    this.logger.LogInformation($"Process {p[Constants.Workflow.Fields.Name]} will be {(stateCode.Value == Constants.Workflow.StateCodeActive ? "activated" : "deactivated")}.");

                    return new UpdateRequest
                    {
                        Target = new Entity(Constants.Workflow.LogicalName, p.Id)
                        {
                            Attributes =
                            {
                                [Constants.Workflow.Fields.StateCode] = stateCode,
                                [Constants.Workflow.Fields.StatusCode] = statusCode,
                            },
                        },
                    };
                });
        }

        private EntityCollection RetrieveProcesses(IEnumerable<string> names)
        {
            var query = new QueryExpression(Constants.Workflow.LogicalName)
            {
                ColumnSet = new ColumnSet(Constants.Workflow.Fields.Name, Constants.Workflow.Fields.StateCode, Constants.Workflow.Fields.Type),
            };
            query.Criteria.AddCondition(Constants.Workflow.Fields.Name, ConditionOperator.In, names.ToArray<object>());
            query.Criteria.AddCondition(Constants.Workflow.Fields.Type, ConditionOperator.Equal, Constants.WorkflowType.Definition);

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} processes matching the {names.Count()} provided names.");

            return results;
        }
    }
}