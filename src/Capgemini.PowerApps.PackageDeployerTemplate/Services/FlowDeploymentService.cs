namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Functionality related to deploying flows.
    /// </summary>
    public class FlowDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public FlowDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Updates connection references to used the provided connection names.
        /// </summary>
        /// <param name="connectionMap">Connection name by connection reference ID.</param>
        public void ConnectConnectionReferences(IDictionary<string, string> connectionMap)
        {
            if (connectionMap is null)
            {
                throw new ArgumentNullException(nameof(connectionMap));
            }

            var updateRequests = this
                .GetConnectionReferences(connectionMap.Keys.ToArray())
                .Select(e => new UpdateRequest
                {
                    Target = new Entity(Constants.ConnectionReference.LogicalName)
                    {
                        Id = e.Id,
                        Attributes =
                        {
                            {
                                Constants.ConnectionReference.Fields.ConnectionReferenceId,
                                e.Id
                            },
                            {
                                Constants.ConnectionReference.Fields.ConnectionId,
                                connectionMap[e.GetAttributeValue<string>(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName)]
                            },
                        },
                    },
                }).ToList();

            var result = this.crmSvc.ExecuteMultiple(updateRequests);
            if (result.IsFaulted)
            {
                this.logger.LogExecuteMultipleErrors(result);
            }
        }

        /// <summary>
        /// Ensures that all flows in the provided solutions are active or inactive.
        /// </summary>
        /// <param name="solutions">The solutions containing the flows.</param>
        /// <param name="flowsToDeactivate">Flows which should not be active.</param>
        public void ActivateFlows(IEnumerable<string> solutions, IEnumerable<string> flowsToDeactivate = null)
        {
            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation($"No solutions to activate flows for.");
                return;
            }

            foreach (var solution in solutions)
            {
                var solutionWorkflowIds = this.crmSvc.GetSolutionComponentObjectIdsByType(solution, Constants.SolutionComponent.ComponentTypeFlow);
                if (!solutionWorkflowIds.Any())
                {
                    this.logger.LogInformation($"No flows found for solution '{solution}'.");
                    continue;
                }

                var solutionFlowIds = this.GetDeployedFlows(solutionWorkflowIds, new ColumnSet(Constants.Workflow.Fields.Name));

                this.logger.LogInformation($"{solutionFlowIds.Count()} flows found for solution '{solution}'.");

                foreach (var solutionFlow in solutionFlowIds)
                {
                    var toDeactivate = (flowsToDeactivate?.Any(f => f == solutionFlow.Attributes[Constants.Workflow.Fields.Name].ToString())).GetValueOrDefault();

                    var stateCode = toDeactivate ? Constants.Workflow.StateCodeInactive : Constants.Workflow.StateCodeActive;
                    var statusCode = toDeactivate ? Constants.Workflow.StatusCodeInactive : Constants.Workflow.StatusCodeActive;

                    this.logger.LogInformation($"Setting flow status for {solutionFlow[Constants.Workflow.Fields.Name]} with statecode {stateCode} and statuscode {statusCode}");

                    if (!this.crmSvc.UpdateStateAndStatusForEntity(Constants.Workflow.LogicalName, solutionFlow.Id, stateCode, statusCode))
                    {
                        this.logger.LogInformation($"Status for flow {solutionFlow.Attributes[Constants.Workflow.Fields.Name]} could not be set. Please check the flows for errors e.g. missing connections.");
                    }
                }
            }

            this.logger.LogInformation($"{nameof(FlowDeploymentService)}.{nameof(this.ActivateFlows)} completed.");
        }

        private IEnumerable<Entity> GetConnectionReferences(params string[] logicalNames)
        {
            if (logicalNames is null)
            {
                throw new ArgumentNullException(nameof(logicalNames));
            }

            return this.crmSvc.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    logicalNames,
                    new ColumnSet(true)).Entities.ToList();
        }

        private IEnumerable<Entity> GetDeployedFlows(IEnumerable<Guid> guids, ColumnSet columnSet)
        {
            if (guids is null)
            {
                throw new ArgumentNullException(nameof(guids));
            }

            if (!guids.Any())
            {
                throw new ArgumentException("You must provide at least one workflow ID.");
            }

            this.logger.LogInformation($"Getting deployed flows matching the following workflow IDs: {string.Join("\n - ", guids)}");
            var flowQuery = new QueryExpression(Constants.Workflow.LogicalName)
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(LogicalOperator.And),
            };

            flowQuery.Criteria.AddCondition(Constants.Workflow.Fields.Category, ConditionOperator.Equal, Constants.Workflow.CategoryModernFlow);
            flowQuery.Criteria.AddCondition(Constants.Workflow.Fields.WorkflowId, ConditionOperator.In, guids.Cast<object>().ToArray());

            var results = this.crmSvc.RetrieveMultiple(flowQuery);

            this.logger.LogInformation($"Found {results.Entities.Count} matching flows.");

            return results.Entities;
        }
    }
}