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

    public class SdkStepsDeploymentServiceTests
    {
        private static readonly string[] Solutions = new string[] { "new_SolutionA", "new_SolutionB" };

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SdkStepDeploymentService sdkStepDeploymentSvc;

        public SdkStepsDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            this.sdkStepDeploymentSvc = new SdkStepDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Action constructWithNullLogger = () => new SdkStepDeploymentService(null, this.crmServiceAdapterMock.Object);

            constructWithNullLogger.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Action constructWithNullCrmSvc = () => new SdkStepDeploymentService(this.loggerMock.Object, null);

            constructWithNullCrmSvc.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetStatesBySolution_NullSolutionsArgument_LogsNoSolutions()
        {
            this.sdkStepDeploymentSvc.SetStatesBySolution(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No solutions were provided to activate SDK steps for."));
        }

        [Fact]
        public void SetStatesBySolution_SdkStepInComponentsToDeactivateList_DeactivatesSdkStep()
        {
            var solutionSdkSteps = new List<Entity> { GetSdkStep(Constants.SdkMessageProcessingStep.StateCodeActive) };
            this.MockBySolutionSdkSteps(solutionSdkSteps);
            this.MockExecuteMultipleResponse(
                null,
                svc => svc.ExecuteMultiple(
                It.Is<IEnumerable<OrganizationRequest>>(
                    reqs => reqs.Cast<SetStateRequest>().Any(
                        req =>
                        req.EntityMoniker.LogicalName == Constants.SdkMessageProcessingStep.LogicalName &&
                        req.EntityMoniker.Id == solutionSdkSteps.First().Id &&
                        req.State.Value == Constants.SdkMessageProcessingStep.StateCodeInactive &&
                        req.Status.Value == Constants.SdkMessageProcessingStep.StatusCodeInactive)),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<int?>()),
                true);

            this.sdkStepDeploymentSvc.SetStatesBySolution(
                Solutions,
                new List<string>
                {
                    solutionSdkSteps.First().GetAttributeValue<string>("name"),
                });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_ComponentsToActivateNull_ThrowsArgumentNullException()
        {
            Action setStatesWithoutComponentsToActivate = () => this.sdkStepDeploymentSvc.SetStates(null);

            setStatesWithoutComponentsToActivate.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_ComponentsToDeactivateNull_DoesNotThrow()
        {
            Action setStatesWithoutComponentsToDeactivate = () => this.sdkStepDeploymentSvc.SetStates(Enumerable.Empty<string>());

            setStatesWithoutComponentsToDeactivate.Should().NotThrow<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_NoSdkStepsPassed_LogsNoSdkStepsProvided()
        {
            this.sdkStepDeploymentSvc.SetStates(Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogInformation("No SDK steps were provided."));
        }

        [Fact]
        public void SetStates_NoSdkStepsFound_LogsNoSdkStepsFound()
        {
            this.MockSetStatesSdkSteps(new List<Entity>());

            this.sdkStepDeploymentSvc.SetStates(new List<string> { "Not found SDK step" });

            this.loggerMock.VerifyLog(l => l.LogInformation("No SDK steps were found with the names provided."));
        }

        [Fact]
        public void SetStates_FoundSdkStepCountDiffersFromProvidedNamesCount_LogsMismatchWarning()
        {
            var foundSdkSteps = new List<Entity> { GetSdkStep(Constants.SdkMessageProcessingStep.StateCodeActive) };
            this.MockSetStatesSdkSteps(foundSdkSteps);
            var sdkStepsToFind = new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
                "Not found SDK step",
            };

            this.sdkStepDeploymentSvc.SetStates(sdkStepsToFind);

            this.loggerMock.VerifyLog(l => l.LogWarning($"Found {foundSdkSteps.Count} deployed SDK steps but expected {sdkStepsToFind.Count}."));
        }

        [Fact]
        public void SetStates_SdkStepInComponentsToActivateFound_ActivatesSdkStep()
        {
            var foundSdkSteps = new List<Entity> { GetSdkStep(Constants.SdkMessageProcessingStep.StateCodeInactive) };
            this.MockSetStatesSdkSteps(foundSdkSteps);
            this.MockExecuteMultipleResponse();

            this.sdkStepDeploymentSvc.SetStates(new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
            });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_SdkStepInComponentsToDeactivateFound_DectivatesSdkStep()
        {
            var foundSdkSteps = new List<Entity> { GetSdkStep(Constants.SdkMessageProcessingStep.StateCodeActive) };
            this.MockSetStatesSdkSteps(foundSdkSteps);
            this.MockExecuteMultipleResponse(
                null,
                svc => svc.ExecuteMultiple(
                It.Is<IEnumerable<OrganizationRequest>>(
                    reqs => reqs.Cast<SetStateRequest>().Any(
                        req =>
                        req.EntityMoniker.LogicalName == Constants.SdkMessageProcessingStep.LogicalName &&
                        req.EntityMoniker.Id == foundSdkSteps.First().Id &&
                        req.State.Value == Constants.SdkMessageProcessingStep.StateCodeInactive &&
                        req.Status.Value == Constants.SdkMessageProcessingStep.StatusCodeInactive)),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<int?>()),
                true);

            this.sdkStepDeploymentSvc.SetStates(Enumerable.Empty<string>(), new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
            });

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetStates_WithError_LogsError()
        {
            var foundSdkSteps = new List<Entity> { GetSdkStep(Constants.SdkMessageProcessingStep.StateCodeInactive) };
            this.MockSetStatesSdkSteps(foundSdkSteps);
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

            this.sdkStepDeploymentSvc.SetStates(
                new List<string>
                {
                    foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
                },
                Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogError(It.Is<string>(s => s.Contains(fault.Message))));
        }

        private static Entity GetSdkStep(int stateCode)
        {
            return new Entity(Constants.SdkMessageProcessingStep.LogicalName, Guid.NewGuid())
            {
                Attributes =
                    {
                        {
                            Constants.SdkMessageProcessingStep.Fields.Name, "SdkStep"
                        },
                        {
                            Constants.SdkMessageProcessingStep.Fields.StateCode, new OptionSetValue(stateCode)
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
                    It.IsAny<bool>(),
                    It.IsAny<int?>());
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

        private void MockSetStatesSdkSteps(IList<Entity> sdkSteps)
        {
            this.crmServiceAdapterMock.Setup(
                svc => svc.RetrieveMultiple(
                    It.Is<QueryExpression>(
                        q =>
                        q.EntityName == Constants.SdkMessageProcessingStep.LogicalName &&
                        q.Criteria.Conditions.Any(
                            c =>
                            c.AttributeName == Constants.SdkMessageProcessingStep.Fields.Name &&
                            c.Operator == ConditionOperator.In))))
                .Returns(new EntityCollection(sdkSteps));
        }

        private void MockBySolutionSdkSteps(IList<Entity> sdkSteps)
        {
            this.crmServiceAdapterMock
                .Setup(c => c.RetrieveDeployedSolutionComponents(
                    Solutions,
                    Constants.SolutionComponent.ComponentTypeSdkStep,
                    Constants.SdkMessageProcessingStep.LogicalName,
                    It.IsAny<ColumnSet>()))
                .Returns(new EntityCollection(sdkSteps));
        }
    }
}
