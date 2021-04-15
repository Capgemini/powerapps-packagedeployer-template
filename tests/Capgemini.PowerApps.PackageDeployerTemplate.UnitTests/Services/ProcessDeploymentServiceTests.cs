namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using FluentAssertions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class ProcessDeploymentServiceTests
    {
        private static readonly string[] Solutions = new string[] { "new_SolutionA", "new_SolutionB" };

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly ProcessDeploymentService processDeploymentSvc;

        public ProcessDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.processDeploymentSvc = new ProcessDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Action constructWithNullLogger = () => new ProcessDeploymentService(null, this.crmServiceAdapterMock.Object);

            constructWithNullLogger.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Action constructWithNullCrmSvc = () => new ProcessDeploymentService(this.loggerMock.Object, null);

            constructWithNullCrmSvc.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetStatesBySolution_NullSolutionsArgument_LogsNoSolutions()
        {
            this.processDeploymentSvc.SetStatesBySolution(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No solutions were provided to activate processes for."));
        }

        [Fact]
        public void SetStatesBySolution_ProcessInComponentsToDeactivateList_DeactivatesProcess()
        {
            var processToDeactivateName = "Process to deactivate";
            var solutionProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName)
                {
                    Id = Guid.NewGuid(),
                    Attributes =
                    {
                        { Constants.Workflow.Fields.Name, processToDeactivateName },
                    },
                },
            };
            this.MockBySolutionProcesses(solutionProcesses);

            this.processDeploymentSvc.SetStatesBySolution(Solutions, new List<string> { processToDeactivateName });

            this.crmServiceAdapterMock.Verify(
                svc => svc.Execute(
                    It.Is<SetStateRequest>(u =>
                        u.EntityMoniker.LogicalName == Constants.Workflow.LogicalName &&
                        u.EntityMoniker.Id == solutionProcesses[0].Id &&
                        u.State.Value == Constants.Workflow.StateCodeInactive &&
                        u.Status.Value == Constants.Workflow.StatusCodeInactive)),
                Times.Once());
        }

        [Fact]
        public void SetStatesBySolution_WithUserParameter_ExecutesAsUser()
        {
            var userToImpersonate = "licenseduser@domaincom";
            var solutionProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName)
                {
                    Id = Guid.NewGuid(),
                    Attributes =
                    {
                        { Constants.Workflow.Fields.Name, "A process" },
                    },
                },
            };
            this.MockBySolutionProcesses(solutionProcesses);

            this.processDeploymentSvc.SetStatesBySolution(Solutions, user: userToImpersonate);

            this.crmServiceAdapterMock.Verify(svc => svc.Execute<SetStateResponse>(It.IsAny<SetStateRequest>(), userToImpersonate, true));
        }

        [Fact]
        public void SetStates_ComponentsToActivateNull_ThrowsArgumentNullException()
        {
            Action setStatesWithoutComponentsToActivate = () => this.processDeploymentSvc.SetStates(null);

            setStatesWithoutComponentsToActivate.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_ComponentsToDeactivateNull_DoesNotThrow()
        {
            Action setStatesWithoutComponentsToDeactivate = () => this.processDeploymentSvc.SetStates(Enumerable.Empty<string>());

            setStatesWithoutComponentsToDeactivate.Should().NotThrow<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_NoProcessesPassed_LogsNoProcessesProvided()
        {
            this.processDeploymentSvc.SetStates(Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogInformation("No processes were provided."));
        }

        [Fact]
        public void SetStates_NoProcessesFound_LogsNoProcessesFound()
        {
            this.MockSetStatesProcesses(new List<Entity>());

            this.processDeploymentSvc.SetStates(new List<string> { "Not found process" });

            this.loggerMock.VerifyLog(l => l.LogInformation("No processes were found with the names provided."));
        }

        [Fact]
        public void SetStates_FoundProcessCountDiffersFromProvidedNamesCount_LogsMismatchWarning()
        {
            var foundProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName) { Attributes = { { Constants.Workflow.Fields.Name, "Found process" } } },
            };
            this.MockSetStatesProcesses(foundProcesses);
            var processesToFind = new List<string>
            {
                foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
                "Not found process",
            };

            this.processDeploymentSvc.SetStates(processesToFind);

            this.loggerMock.VerifyLog(l => l.LogWarning($"Found {foundProcesses.Count} deployed processes but expected {processesToFind.Count}."));
        }

        [Fact]
        public void SetStates_ProcessInComponentsToActivateFound_ActivatesProcess()
        {
            var foundProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName) { Attributes = { { Constants.Workflow.Fields.Name, "Found process" } } },
            };
            this.MockSetStatesProcesses(foundProcesses);

            this.processDeploymentSvc.SetStates(new List<string>
            {
                foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
            });

            this.crmServiceAdapterMock.Verify(
                svc => svc.Execute(
                    It.Is<SetStateRequest>(u =>
                        u.EntityMoniker.LogicalName == Constants.Workflow.LogicalName &&
                        u.EntityMoniker.Id == foundProcesses.First().Id &&
                        u.State.Value == Constants.Workflow.StateCodeActive &&
                        u.Status.Value == Constants.Workflow.StatusCodeActive)),
                Times.Once());
        }

        [Fact]
        public void SetStates_ProcessInComponentsToDeactivateFound_DectivatesProcess()
        {
            var foundProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName) { Attributes = { { Constants.Workflow.Fields.Name, "Found process" } } },
            };
            this.MockSetStatesProcesses(foundProcesses);

            this.processDeploymentSvc.SetStates(Enumerable.Empty<string>(), new List<string>
            {
                foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
            });

            this.crmServiceAdapterMock.Verify(
                svc => svc.Execute(
                    It.Is<SetStateRequest>(u =>
                        u.EntityMoniker.LogicalName == Constants.Workflow.LogicalName &&
                        u.EntityMoniker.Id == foundProcesses.First().Id &&
                        u.State.Value == Constants.Workflow.StateCodeInactive &&
                        u.Status.Value == Constants.Workflow.StatusCodeInactive)),
                Times.Once());
        }

        [Fact]
        public void SetStates_WithUserParameter_ExecutesAsUser()
        {
            var foundProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName) { Attributes = { { Constants.Workflow.Fields.Name, "Found process" } } },
            };
            this.MockSetStatesProcesses(foundProcesses);
            var userToImpersonate = "licenseduser@domaincom";

            this.processDeploymentSvc.SetStates(
                new List<string>
                {
                    foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
                },
                Enumerable.Empty<string>(),
                userToImpersonate);

            this.crmServiceAdapterMock.Verify(svc => svc.Execute<SetStateResponse>(It.IsAny<SetStateRequest>(), userToImpersonate, true));
        }

        [Fact]
        public void SetStates_WithError_LogsError()
        {
            var foundProcesses = new List<Entity>
            {
                new Entity(Constants.Workflow.LogicalName) { Attributes = { { Constants.Workflow.Fields.Name, "Found process" } } },
            };
            this.MockSetStatesProcesses(foundProcesses);
            var exception = new FaultException();
            this.crmServiceAdapterMock
                .Setup(svc => svc.Execute(It.IsAny<SetStateRequest>()))
                .Throws(exception);

            this.processDeploymentSvc.SetStates(
                new List<string>
                {
                    foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
                },
                Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogError(exception, It.IsAny<string>()));
        }

        private void MockSetStatesProcesses(IList<Entity> processes)
        {
            this.crmServiceAdapterMock.Setup(
                svc => svc.RetrieveMultiple(
                    It.Is<QueryExpression>(
                        q =>
                        q.EntityName == Constants.Workflow.LogicalName &&
                        q.Criteria.Conditions.Any(
                            c =>
                            c.AttributeName == Constants.Workflow.Fields.Name &&
                            c.Operator == ConditionOperator.In))))
                .Returns(new EntityCollection(processes));
        }

        private void MockBySolutionProcesses(IList<Entity> processes)
        {
            this.crmServiceAdapterMock
                .Setup(c => c.RetrieveDeployedSolutionComponents(
                    Solutions,
                    Constants.SolutionComponent.ComponentTypeWorkflow,
                    Constants.Workflow.LogicalName,
                    It.IsAny<ColumnSet>()))
                .Returns(new EntityCollection(processes));
        }
    }
}
