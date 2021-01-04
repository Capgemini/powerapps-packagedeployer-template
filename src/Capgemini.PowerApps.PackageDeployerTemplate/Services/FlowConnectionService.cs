﻿using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class FlowConnectionService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        public FlowConnectionService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }      

        public void SetFlowConnections(IEnumerable<ConnectionReference> connectionReferences, IEnumerable<FlowConnection> flows, List<string> solutions)
        {       
            foreach (var item in solutions)
            {
                var solutionId = GetSolutionIdByUniqueName(item);
                if (!solutionId.HasValue) return;
              
                var solutionWorkflowIds = this.GetSolutionComponentObjectIdsByType(solutionId.Value, 29);
                if (!solutionWorkflowIds.Any()) return;

                var solutionFlowIds = this.GetDeployedFlows(solutionWorkflowIds, new ColumnSet(false)).Select(f => f.Id);
                foreach (var flowId in solutionFlowIds)
                {
                  //Activate Flow
                }
            }
                    
            logger.LogInformation($"{nameof(FlowConnectionService)}: Flow Connections completed");
        }

        public Guid? GetSolutionIdByUniqueName(string solution)
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

            //     var result = this.crmSvc.RetrieveMultiple(query).Entities.FirstOrDefault()?.Id;
            //     this.logger.LogInformation($"Solution ID: {result}.");

            //      return result;
            return null;
        }

        public IEnumerable<Guid> GetSolutionComponentObjectIdsByType(Guid solutionId, int componentType)
        {
            this.logger.LogInformation($"Getting solution components of type {componentType} for solution {solutionId}.");

            var queryExpression = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid"),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            queryExpression.Criteria.AddCondition("componenttype", ConditionOperator.Equal, componentType);
            queryExpression.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            //    var results = this.crmSvc.RetrieveMultiple(queryExpression);

            //    this.logger.LogInformation($"Found {results.Entities.Count} matching components.");

            //        return results.Entities.Select(e => e.GetAttributeValue<Guid>("objectid")).ToArray();
            return null;
        }

        public IEnumerable<Entity> GetDeployedFlows(IEnumerable<Guid> guids, ColumnSet columnSet)
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

            //   var results = this.crmSvc.RetrieveMultiple(flowQuery);

            //    this.logger.LogInformation($"Found {results.Entities.Count} matching flows.");

            //      return results.Entities;
            return null;
        }


    }
}