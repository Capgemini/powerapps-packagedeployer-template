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
    /// Functionality related to deploying flows.
    /// </summary>
    public class FlowActivationService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowActivationService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public FlowActivationService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Ensures that all flows in the provided solutions are active or inactive.
        /// </summary>
        /// <param name="flowsToDeactivate">Flows which should not be active.</param>
        /// <param name="solutions">The solutions containing the flows.</param>
        public void ActivateFlows(IEnumerable<string> flowsToDeactivate, IEnumerable<string> solutions)
        {
            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation($"No solutions to activate flows for");
                return;
            }

            foreach (var item in solutions)
            {
                var solutionId = this.GetSolutionIdByUniqueName(item);
                if (!solutionId.HasValue)
                {
                    return;
                }

                var solutionWorkflowIds = this.GetSolutionComponentObjectIdsByType(solutionId.Value, Constants.SolutionComponent.WorkflowTypeFlow);
                if (!solutionWorkflowIds.Any())
                {
                    return;
                }

                var solutionFlowIds = this.GetDeployedFlows(solutionWorkflowIds, new ColumnSet("name"));

                this.logger.LogInformation($"Flows found {solutionFlowIds.Count()}");

                foreach (var solutionFlow in solutionFlowIds)
                {
                    string matchedFlow = null;

                    if (flowsToDeactivate != null)
                    {
                        matchedFlow = flowsToDeactivate.FirstOrDefault(f => f == solutionFlow.Attributes["name"].ToString());
                    }

                    var stateCode = matchedFlow == null ? Constants.Process.StateCodeActive : Constants.Process.StateCodeInactive;
                    var statusCode = matchedFlow == null ? Constants.Process.StatusCodeActive : Constants.Process.StatusCodeInactive;

                    this.logger.LogInformation($"Setting flow status for {solutionFlow["name"]} with statecode {stateCode} and statuscode {statusCode}");

                    if (!this.crmSvc.UpdateStateAndStatusForEntity("workflow", solutionFlow.Id, stateCode, statusCode))
                    {
                        this.logger.LogInformation($"Status for flow {solutionFlow.Attributes["name"]} could not be set. Please ensure the connection is set in the environment as this can stop the flow from being activated / deactivated.");
                    }
                }
            }

            this.logger.LogInformation($"{nameof(FlowActivationService)}: Flow Connections completed");
        }

        private Guid? GetSolutionIdByUniqueName(string solution)
        {
            this.logger.LogInformation($"Getting solution ID for solution {solution}.");

            if (string.IsNullOrEmpty(solution))
            {
                throw new ArgumentException("You must provide a solution unique name.", nameof(solution));
            }

            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solution);

            var result = this.crmSvc.RetrieveMultiple(query).Entities.FirstOrDefault()?.Id;
            this.logger.LogInformation($"Solution ID: {result}.");

            return result;
        }

        private IEnumerable<Guid> GetSolutionComponentObjectIdsByType(Guid solutionId, int componentType)
        {
            this.logger.LogInformation($"Getting solution components of type {componentType} for solution {solutionId}.");

            var queryExpression = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet(new string[] { "objectid" }),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            queryExpression.Criteria.AddCondition("componenttype", ConditionOperator.Equal, componentType);
            queryExpression.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            var results = this.crmSvc.RetrieveMultiple(queryExpression);

            this.logger.LogInformation($"Found {results.Entities.Count} matching components.");

            return results.Entities.Select(e => e.GetAttributeValue<Guid>("objectid")).ToArray();
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
            var flowQuery = new QueryExpression("workflow")
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            flowQuery.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
            flowQuery.Criteria.AddCondition("workflowid", ConditionOperator.In, guids.Cast<object>().ToArray());

            var results = this.crmSvc.RetrieveMultiple(flowQuery);

            this.logger.LogInformation($"Found {results.Entities.Count} matching flows.");

            return results.Entities;
        }
    }
}