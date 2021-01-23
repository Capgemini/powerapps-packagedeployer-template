namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class FlowDeploymentServiceTests
    {
        private const string SolutionUniqueName = "solution";

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmSvc;

        private readonly FlowDeploymentService flowDeploymentService;

        public FlowDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmSvc = new Mock<ICrmServiceAdapter>();

            this.flowDeploymentService = new FlowDeploymentService(this.loggerMock.Object, this.crmSvc.Object);
        }

        [Fact]
        public void ActivateFlows_NoSolutions_LogsNoSolutions()
        {
            this.flowDeploymentService.ActivateFlows(Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(x => x.LogInformation("No solutions to activate flows for."));
        }

        [Fact]
        public void ActivateFlows_NoFlows_LogsNoFlows()
        {
            this.flowDeploymentService.ActivateFlows(new string[] { SolutionUniqueName });

            this.loggerMock.VerifyLog(x => x.LogInformation($"No flows found for solution '{SolutionUniqueName}'."));
        }

        [Fact]
        public void ActivateFlows_NoFlowsToDeactivatePassed_ActivatesAllFlows()
        {
            var solutionComponentObjectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var workflows = new EntityCollection(solutionComponentObjectIds.Select(id => new Entity(Constants.Workflow.LogicalName, id) { Attributes = { { Constants.Workflow.Fields.Name, string.Empty } } }).ToList());
            this.crmSvc
                .Setup(c => c.GetSolutionComponentObjectIdsByType(SolutionUniqueName, Constants.SolutionComponent.ComponentTypeFlow))
                .Returns(solutionComponentObjectIds);
            this.crmSvc
                .Setup(c => c.RetrieveMultiple(It.Is<QueryExpression>(q => q.EntityName == Constants.Workflow.LogicalName)))
                .Returns(workflows);

            this.flowDeploymentService.ActivateFlows(new string[] { SolutionUniqueName });

            this.crmSvc.Verify(
                c => c.UpdateStateAndStatusForEntity(
                    Constants.Workflow.LogicalName,
                    It.Is<Guid>(g => solutionComponentObjectIds.Contains(g)),
                    Constants.Workflow.StateCodeActive,
                    Constants.Workflow.StatusCodeActive),
                Times.Exactly(workflows.Entities.Count));
        }

        [Fact]
        public void ActivateFlows_FlowsToDeactivatePassed_DeactivatesFlows()
        {
            var solutionComponentObjectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var workflows = new EntityCollection(solutionComponentObjectIds.Select(
                    id => new Entity(Constants.Workflow.LogicalName, id)
                    {
                        Attributes =
                        {
                            {
                                Constants.Workflow.Fields.Name,
                                string.Empty
                            },
                        },
                    }).ToList());
            var workflowToDeactivateName = "When this flow is deployed -> Deactivate it";
            var workflowToDeactivate = workflows.Entities.First();
            workflowToDeactivate.Attributes[Constants.Workflow.Fields.Name] = workflowToDeactivateName;
            this.crmSvc
                .Setup(c => c.GetSolutionComponentObjectIdsByType(SolutionUniqueName, Constants.SolutionComponent.ComponentTypeFlow))
                .Returns(solutionComponentObjectIds);
            this.crmSvc
                .Setup(c => c.RetrieveMultiple(It.Is<QueryExpression>(q => q.EntityName == Constants.Workflow.LogicalName)))
                .Returns(workflows);

            this.flowDeploymentService.ActivateFlows(new string[] { SolutionUniqueName }, new string[] { workflowToDeactivateName });

            this.crmSvc.Verify(
                c => c.UpdateStateAndStatusForEntity(
                    Constants.Workflow.LogicalName,
                    workflowToDeactivate.Id,
                    Constants.Workflow.StateCodeInactive,
                    Constants.Workflow.StatusCodeInactive), Times.Once());
        }

        [Fact]
        public void ConnectConnectionReferences_NullConnectionMap_Throws()
        {
            Action callingConnectConnectionReferencesWithNullConnectionMap = () => this.flowDeploymentService.ConnectConnectionReferences(null);

            callingConnectConnectionReferencesWithNullConnectionMap.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConnectConnectionReferences_ConnectionMappingPassed_UpdatesConnectionReferences()
        {
            var connectionMap = new Dictionary<string, string>
            {
                { "pdt_sharedapprovals_d7dcb", "12038109da0wud01" },
            };
            var connectionReferences = new EntityCollection(
                connectionMap.Keys.Select(k =>
                {
                    var entity = new Entity(Constants.ConnectionReference.LogicalName, Guid.NewGuid());
                    entity.Attributes.Add(Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName, k);
                    return entity;
                }).ToList());
            this.crmSvc.Setup(
                c => c.RetrieveMultipleByAttribute(
                    Constants.ConnectionReference.LogicalName,
                    Constants.ConnectionReference.Fields.ConnectionReferenceLogicalName,
                    connectionMap.Keys,
                    It.IsAny<ColumnSet>()))
                .Returns(connectionReferences);
            this.crmSvc.Setup(c => c.ExecuteMultiple(
                    It.Is<List<UpdateRequest>>(
                        reqs => reqs.All(r =>
                            r.Target.LogicalName == Constants.ConnectionReference.LogicalName &&
                            connectionReferences.Entities.Any(e => e.Id == r.Target.Id) &&
                            connectionMap.Values.Contains(r.Target.Attributes[Constants.ConnectionReference.Fields.ConnectionId]))),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new ExecuteMultipleResponse { Results = { { "IsFaulted", false } } })
                .Verifiable();

            this.flowDeploymentService.ConnectConnectionReferences(connectionMap);

            this.crmSvc.VerifyAll();
        }
    }
}
