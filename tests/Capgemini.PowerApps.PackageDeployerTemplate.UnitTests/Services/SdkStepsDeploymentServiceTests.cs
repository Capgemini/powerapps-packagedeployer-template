namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using Moq;
    using Xunit;

    public class SdkStepsDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SdkStepDeploymentService sdkStepsActivatorService;

        public SdkStepsDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.sdkStepsActivatorService = new SdkStepDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SdkStepDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SdkStepDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void Deactivate_NullSdkStepsToDeactivate_LogsNoConfig()
        {
            this.sdkStepsActivatorService.Deactivate(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No SDK steps to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_EmptySdkStepsToDeactivate_LogsNoConfig()
        {
            this.sdkStepsActivatorService.Deactivate(Array.Empty<string>());

            this.loggerMock.VerifyLog(x => x.LogInformation("No SDK steps to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_QueryForSdkSteps_IncludesAllNamesPassed()
        {
            var sdkStepsToDeactivate = new string[] { "sdk_step_one", "sdk_step_two" };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sdkmessageprocessingstep", "name", sdkStepsToDeactivate, null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sdkmessageprocessingstep",
                    };
                    entityCollection.Entities.AddRange(sdkStepsToDeactivate.Select(value => new Entity("sdkmessageprocessingstep", Guid.NewGuid())));
                    return entityCollection;
                })
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { },
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                });

            this.sdkStepsActivatorService.Deactivate(sdkStepsToDeactivate);

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_FaultWhenSettingsStatus_LogErrors()
        {
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sdkmessageprocessingstep", "name", It.IsAny<string[]>(), It.IsAny<ColumnSet>()))
                .Returns((string entity, string attribute, string[] values, ColumnSet columnSet) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sdkmessageprocessingstep",
                    };
                    entityCollection.Entities.AddRange(values.Select(value => new Entity("sdkmessageprocessingstep", Guid.NewGuid())));
                    return entityCollection;
                });

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem
                        {
                            Fault = new OrganizationServiceFault
                            {
                                Message = "Test fault response",
                            },
                        },
                        new ExecuteMultipleResponseItem { },
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    response.Results.Add("IsFaulted", true);
                    return response;
                });

            this.sdkStepsActivatorService.Deactivate(new string[] { "a_process_to_deactivate" });

            this.loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            this.loggerMock.VerifyLog(x => x.LogError("Error deactivating SDK Message Processing Steps."));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }
    }
}
