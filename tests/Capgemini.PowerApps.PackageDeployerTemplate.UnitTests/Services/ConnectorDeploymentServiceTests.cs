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

    public class ConnectorDeploymentServiceTests
    {
        private static string exampleOpenApiDefinition = "{\"swagger\":\"2.0\",\"info\":{\"title\":\"Default title\",\"description\":\"\",\"version\":\"1.0\"},\"host\":\"example.com\",\"basePath\":\"/\",\"schemes\":[\"https\"],\"consumes\":[],\"produces\":[],\"paths\":{\"/test\":{\"get\":{\"responses\":{\"default\":{\"description\":\"default\",\"schema\":{}}},\"summary\":\"Test\",\"description\":\"Test\",\"operationId\":\"Test\",\"parameters\":[]}}},\"definitions\":{},\"parameters\":{},\"responses\":{},\"securityDefinitions\":{},\"security\":[],\"tags\":[]}";

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;
        private readonly ConnectorDeploymentService connectorDeploymentService;

        public ConnectorDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.connectorDeploymentService = new ConnectorDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ConnectorDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ConnectorDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void SetBaseUrls_NullConfig_LogNoConfig()
        {
            this.connectorDeploymentService.SetBaseUrls(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No custom connector base URLs have been configured."));
        }

        [Fact]
        public void SetBaseUrls_InvalidBaseUrl_LogNoInvalidBaseUrl()
        {
            var baseUrlConfigs = new Dictionary<string, string>
            {
                { "connector1", "invalid_base_url" },
            };

            this.connectorDeploymentService.SetBaseUrls(baseUrlConfigs);
            this.loggerMock.VerifyLog(x => x.LogError($"The base URL '{baseUrlConfigs.ElementAt(0).Value}' is not valid and the connector '{baseUrlConfigs.ElementAt(0).Key}' won't be updated."));
        }

        [Fact]
        public void SetBaseUrls_DefinitionDoesNotExist_LogNoVariableExist()
        {
            var baseUrlConfigs = new Dictionary<string, string>
            {
                { "connector1", "https://example.com/api/" },
            };

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Connector.LogicalName,
                Entities = { },
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.Connector.LogicalName,
                  Constants.Connector.Fields.Name,
                  It.Is<IEnumerable<string>>(values => values.Contains(baseUrlConfigs.ElementAt(0).Key)),
                  It.IsAny<ColumnSet>()))
                .Returns(entityCollection)
                .Verifiable();

            this.connectorDeploymentService.SetBaseUrls(baseUrlConfigs);
            this.loggerMock.VerifyLog(x => x.LogError($"Custom connector {baseUrlConfigs.ElementAt(0).Key} not found on target instance."));
        }

        [Theory]
        [InlineData("https://example.com/api/")]
        [InlineData("https://example.com/")]
        [InlineData("https://example.com")]
        public void SetBaseUrls_UpdateApiDefinition(string baseUrl)
        {
            var baseUrlConfigs = new Dictionary<string, string>
            {
                { "connector1", baseUrl },
            };

            var connectorId = Guid.NewGuid();

            var entityCollection = new EntityCollection
            {
                EntityName = Constants.Connector.LogicalName,
                Entities =
                {
                    new Entity(Constants.Connector.LogicalName, connectorId)
                    {
                        Attributes =
                        {
                            {
                                Constants.Connector.Fields.OpenApiDefinition,
                                exampleOpenApiDefinition
                            },
                        },
                    },
                },
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute(
                  Constants.Connector.LogicalName,
                  Constants.Connector.Fields.Name,
                  It.Is<IEnumerable<string>>(values => values.Contains(baseUrlConfigs.ElementAt(0).Key)),
                  It.IsAny<ColumnSet>()))
                .Returns(entityCollection)
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.Update(
                    It.Is<Entity>(entity =>
                        entity.LogicalName == Constants.Connector.LogicalName &&
                        entity.Id == connectorId &&
                        VerifiyOpenApiDefinition((string)entity.Attributes[Constants.Connector.Fields.OpenApiDefinition], new Uri(baseUrlConfigs.ElementAt(0).Value)))))
                .Verifiable();

            this.connectorDeploymentService.SetBaseUrls(baseUrlConfigs);

            this.crmServiceAdapterMock.VerifyAll();
        }

        private static bool VerifiyOpenApiDefinition(string apiDefinition, Uri baseUrl)
        {
            return
                apiDefinition.Contains($"\"host\":\"{baseUrl.Host}\"") &&
                apiDefinition.Contains($"\"basePath\":\"{baseUrl.AbsolutePath}\"") &&
                apiDefinition.Contains($"\"schemes\":[\"{baseUrl.Scheme}\"]");
        }
    }
}
