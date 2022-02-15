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

            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation("No solutions were provided to activate processes for.");
                return;
            }

            var deployedProcesses = this.crmSvc.RetrieveDeployedSolutionComponents(
                solutions,
                Constants.SolutionComponent.ComponentTypeWorkflow,
                Constants.Workflow.LogicalName,
                new ColumnSet(Constants.Workflow.Fields.Name, Constants.Workflow.Fields.StateCode)).Entities;

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

            var requests = this.GetSetStateRequests(processes, processesToDeactivate);

            if (!requests.Any())
            {
                return;
            }

            this.ExecuteSetStateRequests(requests, user);
        }

        private void ExecuteSetStateRequests(IDictionary<Entity, UpdateRequest> requests, string user = null)
        {
            var remainingRequests = requests;

            IDictionary<Entity, UpdateRequest> failedRequests;
            IDictionary<Entity, UpdateRequest> successfulRequests;
            IDictionary<Entity, FaultException<OrganizationServiceFault>> errors;

            do
            {
                failedRequests = new Dictionary<Entity, UpdateRequest>();
                successfulRequests = new Dictionary<Entity, UpdateRequest>();
                errors = new Dictionary<Entity, FaultException<OrganizationServiceFault>>();

                foreach (var req in remainingRequests)
                {
                    if (req.Value == null)
                    {
                        this.logger.LogInformation($"Process {req.Key[Constants.Workflow.Fields.Name]} already has desired state. Skipping.");
                        continue;
                    }

                    if (req.Value.Target.GetAttributeValue<OptionSetValue>(Constants.Workflow.Fields.StateCode).Value == 1)
                    {
                        this.logger.LogInformation($"Activating {req.Key[Constants.Workflow.Fields.Name]}.");
                    }
                    else
                    {
                        this.logger.LogInformation($"Deactivating {req.Key[Constants.Workflow.Fields.Name]}.");
                    }

                    try
                    {
                        var response = string.IsNullOrEmpty(user) ?
                            (UpdateResponse)this.crmSvc.Execute(req.Value) : this.crmSvc.Execute<UpdateResponse>(req.Value, user, true);

                        successfulRequests.Add(req.Key, req.Value);
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        failedRequests.Add(req.Key, req.Value);
                        errors.Add(req.Key, ex);
                    }
                }

                remainingRequests = failedRequests;
            }
            while (remainingRequests.Count > 0 && successfulRequests.Any());

            foreach (var error in errors)
            {
                this.logger.LogError($"Failed to set state for process {error.Key[Constants.Workflow.Fields.Name]} with the following error: {error.Value.Message}.");
            }
        }

        private IDictionary<Entity, UpdateRequest> GetSetStateRequests(IEnumerable<Entity> processes, IEnumerable<string> processesToDeactivate)
        {
            return processes.ToDictionary(p => p, p =>
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
                    return null;
                }

                return new UpdateRequest
                {
                    Target = new Entity(p.LogicalName, p.Id)
                    {
                        Attributes =
                            {
                                { "statecode", stateCode },
                                { "statuscode", statusCode },
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
            query.Criteria.AddCondition(Constants.Workflow.Fields.Type, ConditionOperator.Equal, Constants.Workflow.TypeDefinition);

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} processes matching the {names.Count()} provided names.");

            return results;
        }
    }
}
