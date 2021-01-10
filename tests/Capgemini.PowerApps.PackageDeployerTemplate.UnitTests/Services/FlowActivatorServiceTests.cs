using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class FlowActivatorServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly FlowActivationService flowActivationService;

        public FlowActivatorServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            crmServiceAdapterMock.Setup(x => x.GetOrganizationService())
                .Returns(() => new Mock<IOrganizationService>().Object);

            flowActivationService = new FlowActivationService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Import_NullImportConfigs_LogsNoConfig()
        {
            flowActivationService.ActivateFlows(null, null);

            loggerMock.VerifyLog(x => x.LogInformation("No solutions to activate flows for"));
        }

        [Fact]
        public void Deactivate_When_A_Flow_Is_Found()
        {
            crmServiceAdapterMock
               .Setup(x => x.RetrieveMultiple(It.IsAny<QueryExpression>()))
               .Returns((QueryExpression query) =>
               {
                   var entityCollection = new EntityCollection
                   {
                       EntityName = query.EntityName
                   };

                   var entity = new Entity(query.EntityName, Guid.NewGuid());
                   entity["name"] = "test";
                   entityCollection.Entities.Add(entity);

                   return entityCollection;
               })
               .Verifiable();

            flowActivationService.ActivateFlows(new string[] { "test" }, new string[] { "solution1" });

            crmServiceAdapterMock.Verify(exec=> exec.RetrieveMultiple(It.IsAny<QueryExpression>()),Times.Exactly(3));
            crmServiceAdapterMock.Verify(exec => exec.UpdateStateAndStatusForEntity(It.IsAny<string>(),It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        }

        [Fact]
        public void Activate_When_No_Flows_To_Deactivate()
        {
            crmServiceAdapterMock
               .Setup(x => x.RetrieveMultiple(It.IsAny<QueryExpression>()))
               .Returns((QueryExpression query) =>
               {
                   var entityCollection = new EntityCollection
                   {
                       EntityName = query.EntityName
                   };

                   var entity = new Entity(query.EntityName, Guid.NewGuid());
                   entity["name"] = "test";
                   entityCollection.Entities.Add(entity);

                   return entityCollection;
               })
               .Verifiable();

            flowActivationService.ActivateFlows(null, new string[] { "solution1" });

            crmServiceAdapterMock.Verify(exec => exec.RetrieveMultiple(It.IsAny<QueryExpression>()), Times.Exactly(3));
            crmServiceAdapterMock.Verify(exec => exec.UpdateStateAndStatusForEntity("workflow", It.IsAny<Guid>(),1, 2), Times.Exactly(1));
        }
    }
}
