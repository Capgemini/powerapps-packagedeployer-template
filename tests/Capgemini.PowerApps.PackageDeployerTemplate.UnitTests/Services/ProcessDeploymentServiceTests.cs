namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using FluentAssertions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
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
            var solutionProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeActive) };
            this.MockBySolutionProcesses(solutionProcesses);
            this.MockExecuteMultipleResponse(
                null,
                svc => svc.ExecuteMultiple(
                It.Is<IEnumerable<OrganizationRequest>>(
                    reqs => reqs.Cast<SetStateRequest>().Any(
                        req =>
                        req.EntityMoniker.LogicalName == Constants.Workflow.LogicalName &&
                        req.EntityMoniker.Id == solutionProcesses.First().Id &&
                        req.State.Value == Constants.Workflow.StateCodeInactive &&
                        req.Status.Value == Constants.Workflow.StatusCodeInactive)),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
                true);

            this.processDeploymentSvc.SetStatesBySolution(
                Solutions,
                new List<string>
                {
                    solutionProcesses.First().GetAttributeValue<string>("name"),
                });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStatesBySolution_WithUserParameter_ExecutesAsUser()
        {
            var userToImpersonate = "licenseduser@domaincom";
            var solutionProcesses = new List<Entity>
            {
                GetProcess(Constants.Workflow.StateCodeInactive),
            };
            this.MockBySolutionProcesses(solutionProcesses);
            this.MockExecuteMultipleResponse(
                null,
                svc => svc.ExecuteMultiple(
                    It.IsAny<IEnumerable<OrganizationRequest>>(),
                    userToImpersonate,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()));

            this.processDeploymentSvc.SetStatesBySolution(
                Solutions, user: userToImpersonate);

            this.crmServiceAdapterMock.VerifyAll();
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
            var foundProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeActive) };
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
            var foundProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeInactive) };
            this.MockSetStatesProcesses(foundProcesses);
            this.MockExecuteMultipleResponse();

            this.processDeploymentSvc.SetStates(new List<string>
            {
                foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
            });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_ProcessInComponentsToDeactivateFound_DectivatesProcess()
        {
            var foundProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeActive) };
            this.MockSetStatesProcesses(foundProcesses);
            this.MockExecuteMultipleResponse(
                null,
                svc => svc.ExecuteMultiple(
                It.Is<IEnumerable<OrganizationRequest>>(
                    reqs => reqs.Cast<SetStateRequest>().Any(
                        req =>
                        req.EntityMoniker.LogicalName == Constants.Workflow.LogicalName &&
                        req.EntityMoniker.Id == foundProcesses.First().Id &&
                        req.State.Value == Constants.Workflow.StateCodeInactive &&
                        req.Status.Value == Constants.Workflow.StatusCodeInactive)),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
                true);

            this.processDeploymentSvc.SetStates(Enumerable.Empty<string>(), new List<string>
            {
                foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
            });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_WithUserParameter_ExecutesAsUser()
        {
            var foundProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeInactive) };
            this.MockSetStatesProcesses(foundProcesses);
            var userToImpersonate = "licenseduser@domaincom";
            this.MockExecuteMultipleResponse(
               null,
               svc => svc.ExecuteMultiple(
               It.IsAny<IEnumerable<OrganizationRequest>>(),
               userToImpersonate,
               It.IsAny<bool>(),
               It.IsAny<bool>()),
               true);

            this.processDeploymentSvc.SetStates(
                new List<string>
                {
                    foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
                },
                Enumerable.Empty<string>(),
                userToImpersonate);

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_WithError_LogsError()
        {
            var foundProcesses = new List<Entity> { GetProcess(Constants.Workflow.StateCodeInactive) };
            this.MockSetStatesProcesses(foundProcesses);
            var fault = new OrganizationServiceFault { Message = "Some error." };
            var response = new ExecuteMultipleResponse
            {
                Results = new ParameterCollection
                {
                    { "Responses", new ExecuteMultipleResponseItemCollection() },
                    { "IsFaulted", true },
                },
            };
            response.Responses.Add(new ExecuteMultipleResponseItem { Fault = fault });
            this.MockExecuteMultipleResponse(response);

            this.processDeploymentSvc.SetStates(
                new List<string>
                {
                    foundProcesses.First().GetAttributeValue<string>(Constants.Workflow.Fields.Name),
                },
                Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogError(It.Is<string>(s => s.Contains(fault.Message))));
        }

        private static Entity GetProcess(int stateCode)
        {
            return new Entity(Constants.Workflow.LogicalName, Guid.NewGuid())
            {
                Attributes =
                    {
                        {
                            Constants.Workflow.Fields.Name, "Process"
                        },
                        {
                            Constants.Workflow.Fields.StateCode, new OptionSetValue(stateCode)
                        },
                    },
            };
        }

        private void MockExecuteMultipleResponse(ExecuteMultipleResponse response = null, Expression<Func<ICrmServiceAdapter, ExecuteMultipleResponse>> expression = null, bool verifiable = false)
        {
            if (expression == null)
            {
                expression = svc => svc.ExecuteMultiple(
                    It.IsAny<IEnumerable<OrganizationRequest>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>());
            }

            if (response == null)
            {
                response = new ExecuteMultipleResponse();
            }

            var returnResult = this.crmServiceAdapterMock
                .Setup(expression)
                .Returns(response);

            if (verifiable)
            {
                returnResult.Verifiable();
            }
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
