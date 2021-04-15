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
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class SdkStepsDeploymentServiceTests
    {
        private static readonly string[] Solutions = new string[] { "new_SolutionA", "new_SolutionB" };

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SdkStepDeploymentService sdkStepSvc;

        public SdkStepsDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.sdkStepSvc = new SdkStepDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
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
            this.sdkStepSvc.SetStatesBySolution(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No solutions were provided to activate SDK steps for."));
        }

        [Fact]
        public void SetStatesBySolution_SdkStepInComponentsToDeactivateList_DeactivatesSdkStep()
        {
            var sdkStepToDeactivateName = "SDK step to deactivate";
            var solutionSdkSteps = new List<Entity>
            {
                new Entity(Constants.SdkMessageProcessingStep.LogicalName)
                {
                    Id = Guid.NewGuid(),
                    Attributes =
                    {
                        { Constants.SdkMessageProcessingStep.Fields.Name, sdkStepToDeactivateName },
                    },
                },
            };
            this.MockBySolutionSdkSteps(solutionSdkSteps);

            this.sdkStepSvc.SetStatesBySolution(Solutions, new List<string> { sdkStepToDeactivateName });

            this.crmServiceAdapterMock.Verify(
                c => c.UpdateStateAndStatusForEntity(
                    Constants.SdkMessageProcessingStep.LogicalName,
                    solutionSdkSteps[0].Id,
                    Constants.SdkMessageProcessingStep.StateCodeInactive,
                    Constants.SdkMessageProcessingStep.StatusCodeInactive),
                Times.Once());
        }

        [Fact]
        public void SetStates_ComponentsToActivateNull_ThrowsArgumentNullException()
        {
            Action setStatesWithoutComponentsToActivate = () => this.sdkStepSvc.SetStates(null);

            setStatesWithoutComponentsToActivate.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_ComponentsToDeactivateNull_DoesNotThrow()
        {
            Action setStatesWithoutComponentsToDeactivate = () => this.sdkStepSvc.SetStates(Enumerable.Empty<string>());

            setStatesWithoutComponentsToDeactivate.Should().NotThrow<ArgumentNullException>();
        }

        [Fact]
        public void SetStates_NoSdkStepsPassed_LogsNoSdkStepsProvided()
        {
            this.sdkStepSvc.SetStates(Enumerable.Empty<string>());

            this.loggerMock.VerifyLog(l => l.LogInformation("No SDK steps were provided."));
        }

        [Fact]
        public void SetStates_NoSdkStepsFound_LogsNoSdkStepsFound()
        {
            this.MockSetStatesSdkSteps(new List<Entity>());

            this.sdkStepSvc.SetStates(new List<string> { "Not found SDK step" });

            this.loggerMock.VerifyLog(l => l.LogInformation("No SDK steps were found with the names provided."));
        }

        [Fact]
        public void SetStates_FoundSdkStepCountDiffersFromProvidedNamesCount_LogsMismatchWarning()
        {
            var foundSdkSteps = new List<Entity>
            {
                new Entity(Constants.SdkMessageProcessingStep.LogicalName) { Attributes = { { Constants.SdkMessageProcessingStep.Fields.Name, "Found SDK step" } } },
            };
            this.MockSetStatesSdkSteps(foundSdkSteps);
            var sdkStepsToFind = new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
                "Not found SDK step",
            };

            this.sdkStepSvc.SetStates(sdkStepsToFind);

            this.loggerMock.VerifyLog(l => l.LogWarning($"Found {foundSdkSteps.Count} deployed SDK steps but expected {sdkStepsToFind.Count}."));
        }

        [Fact]
        public void SetStates_SdkStepInComponentsToActivateFound_ActivatesSdkStep()
        {
            var foundSdkSteps = new List<Entity>
            {
                new Entity(Constants.SdkMessageProcessingStep.LogicalName) { Attributes = { { Constants.SdkMessageProcessingStep.Fields.Name, "Found SDK step" } } },
            };
            this.MockSetStatesSdkSteps(foundSdkSteps);

            this.sdkStepSvc.SetStates(new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
            });

            this.crmServiceAdapterMock.Verify(
                svc => svc.UpdateStateAndStatusForEntity(
                    Constants.SdkMessageProcessingStep.LogicalName,
                    foundSdkSteps.First().Id,
                    Constants.SdkMessageProcessingStep.StateCodeActive,
                    Constants.SdkMessageProcessingStep.StatusCodeActive),
                Times.Once());
        }

        [Fact]
        public void SetStates_SdkStepInComponentsToDeactivateFound_DectivatesSdkStep()
        {
            var foundSdkSteps = new List<Entity>
            {
                new Entity(Constants.SdkMessageProcessingStep.LogicalName) { Attributes = { { Constants.SdkMessageProcessingStep.Fields.Name, "Found SDK step" } } },
            };
            this.MockSetStatesSdkSteps(foundSdkSteps);

            this.sdkStepSvc.SetStates(Enumerable.Empty<string>(), new List<string>
            {
                foundSdkSteps.First().GetAttributeValue<string>(Constants.SdkMessageProcessingStep.Fields.Name),
            });

            this.crmServiceAdapterMock.Verify(
                svc => svc.UpdateStateAndStatusForEntity(
                    Constants.SdkMessageProcessingStep.LogicalName,
                    foundSdkSteps.First().Id,
                    Constants.SdkMessageProcessingStep.StateCodeInactive,
                    Constants.SdkMessageProcessingStep.StatusCodeInactive),
                Times.Once());
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
