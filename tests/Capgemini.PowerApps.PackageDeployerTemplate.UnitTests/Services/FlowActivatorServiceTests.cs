namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class FlowActivatorServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly FlowActivationService flowActivationService;

        public FlowActivatorServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.flowActivationService = new FlowActivationService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Import_NullImportConfigs_LogsNoConfig()
        {
            this.flowActivationService.ActivateFlows(null, null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No solutions to activate flows for"));
        }

        [Fact]
        public void Deactivate_When_A_Flow_Is_Found()
        {
            this.crmServiceAdapterMock
               .Setup(x => x.RetrieveMultiple(It.IsAny<QueryExpression>()))
               .Returns((QueryExpression query) =>
               {
                   var entityCollection = new EntityCollection
                   {
                       EntityName = query.EntityName,
                   };

                   var entity = new Entity(query.EntityName, Guid.NewGuid());
                   entity["name"] = "test";
                   entityCollection.Entities.Add(entity);

                   return entityCollection;
               })
               .Verifiable();

            this.flowActivationService.ActivateFlows(new string[] { "test" }, new string[] { "solution1" });

            this.crmServiceAdapterMock.Verify(exec => exec.RetrieveMultiple(It.IsAny<QueryExpression>()), Times.Exactly(3));
            this.crmServiceAdapterMock.Verify(exec => exec.UpdateStateAndStatusForEntity(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        }

        [Fact]
        public void Activate_When_No_Flows_To_Deactivate()
        {
            this.crmServiceAdapterMock
               .Setup(x => x.RetrieveMultiple(It.IsAny<QueryExpression>()))
               .Returns((QueryExpression query) =>
               {
                   var entityCollection = new EntityCollection
                   {
                       EntityName = query.EntityName,
                   };

                   var entity = new Entity(query.EntityName, Guid.NewGuid());
                   entity["name"] = "test";
                   entityCollection.Entities.Add(entity);

                   return entityCollection;
               })
               .Verifiable();

            this.flowActivationService.ActivateFlows(null, new string[] { "solution1" });

            this.crmServiceAdapterMock.Verify(exec => exec.RetrieveMultiple(It.IsAny<QueryExpression>()), Times.Exactly(3));
            this.crmServiceAdapterMock.Verify(exec => exec.UpdateStateAndStatusForEntity("workflow", It.IsAny<Guid>(), 1, 2), Times.Exactly(1));
        }
    }
}
