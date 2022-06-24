namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class EnvironmentVariableDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;
        private readonly EnvironmentVariableDeploymentService environmentVariableDeploymentService;

        public EnvironmentVariableDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.environmentVariableDeploymentService = new EnvironmentVariableDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new EnvironmentVariableDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new EnvironmentVariableDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void SetEnvironmentVariables_NullConfig_LogNoConfig()
        {
            this.environmentVariableDeploymentService.SetEnvironmentVariables(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No environment variables have been configured."));
        }

        [Fact]
        public void SetEnvironmentVariables_DefinitionDoesNotExist_LogNoVariableExist()
        {
            var environmentVariableConfigs = new Dictionary<string, string>
            {
                { "variable1", "variable1_value" },
            };

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.EnvironmentVariableDefinition.LogicalName,
                Entities = { },
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.EnvironmentVariableDefinition.LogicalName,
                  Constants.EnvironmentVariableDefinition.Fields.SchemaName,
                  It.Is<IEnumerable<string>>(values => values.Contains(environmentVariableConfigs.ElementAt(0).Key)),
                  It.IsAny<ColumnSet>()))
                .Returns(entityCollection)
                .Verifiable();

            this.environmentVariableDeploymentService.SetEnvironmentVariables(environmentVariableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation($"Environment variable {environmentVariableConfigs.ElementAt(0).Key} not found on target instance."));
        }

        [Fact]
        public void SetEnvironmentVariables_NoExistingValue_CreatesNewValue()
        {
            var environmentVariableConfigs = new Dictionary<string, string>
            {
                { "variable1", "variable1_value" },
            };

            var definitionId = Guid.NewGuid();

            var definitionEntityCollection = new EntityCollection
            {
                EntityName = Constants.EnvironmentVariableDefinition.LogicalName,
                Entities =
                {
                    new Entity(Constants.EnvironmentVariableDefinition.LogicalName, definitionId),
                },
            };

            var valueEntityCollection = new EntityCollection
            {
                EntityName = Constants.EnvironmentVariableValue.LogicalName,
                Entities = { },
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.EnvironmentVariableDefinition.LogicalName,
                  Constants.EnvironmentVariableDefinition.Fields.SchemaName,
                  It.Is<IEnumerable<string>>(values => values.Contains(environmentVariableConfigs.ElementAt(0).Key)),
                  It.IsAny<ColumnSet>()))
                .Returns(definitionEntityCollection)
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.EnvironmentVariableValue.LogicalName,
                  Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId,
                  It.Is<IEnumerable<object>>(values => values.Contains(definitionId)),
                  It.IsAny<ColumnSet>()))
                .Returns(valueEntityCollection)
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.Create(
                    It.Is<Entity>(entity =>
                        entity.LogicalName == Constants.EnvironmentVariableValue.LogicalName &&
                        ((EntityReference)entity.Attributes[Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId]).Id == definitionId &&
                        (string)entity.Attributes[Constants.EnvironmentVariableValue.Fields.Value] == environmentVariableConfigs.ElementAt(0).Value)))
                .Returns(Guid.NewGuid())
                .Verifiable();

            this.environmentVariableDeploymentService.SetEnvironmentVariables(environmentVariableConfigs);

            this.crmServiceAdapterMock.VerifyAll();
        }

        [Fact]
        public void SetEnvironmentVariables_ExistingValue_UpdatesExistingValue()
        {
            var environmentVariableConfigs = new Dictionary<string, string>
            {
                { "variable1", "variable1_value" },
            };

            var definitionId = Guid.NewGuid();
            var valueId = Guid.NewGuid();

            var definitionEntityCollection = new EntityCollection
            {
                EntityName = Constants.EnvironmentVariableDefinition.LogicalName,
                Entities =
                {
                    new Entity(Constants.EnvironmentVariableDefinition.LogicalName, definitionId),
                },
            };

            var valueEntityCollection = new EntityCollection
            {
                EntityName = Constants.EnvironmentVariableValue.LogicalName,
                Entities =
                {
                    new Entity(Constants.EnvironmentVariableValue.LogicalName, valueId),
                },
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.EnvironmentVariableDefinition.LogicalName,
                  Constants.EnvironmentVariableDefinition.Fields.SchemaName,
                  It.Is<IEnumerable<string>>(values => values.Contains(environmentVariableConfigs.ElementAt(0).Key)),
                  It.IsAny<ColumnSet>()))
                .Returns(definitionEntityCollection)
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.EnvironmentVariableValue.LogicalName,
                  Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId,
                  It.Is<IEnumerable<object>>(values => values.Contains(definitionId)),
                  It.IsAny<ColumnSet>()))
                .Returns(valueEntityCollection)
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.Update(
                    It.Is<Entity>(entity =>
                        entity.LogicalName == Constants.EnvironmentVariableValue.LogicalName &&
                        entity.Id == valueId &&
                        (string)entity.Attributes[Constants.EnvironmentVariableValue.Fields.Value] == environmentVariableConfigs.ElementAt(0).Value)))
                .Verifiable();

            this.environmentVariableDeploymentService.SetEnvironmentVariables(environmentVariableConfigs);

            this.crmServiceAdapterMock.VerifyAll();
        }
    }
}
