using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class SdkStepsActivatorServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SdkStepDeploymentService sdkStepsActivatorService;

        public SdkStepsActivatorServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            crmServiceAdapterMock.Setup(x => x.GetOrganizationService())
                .Returns(() => new Mock<IOrganizationService>().Object);

            sdkStepsActivatorService = new SdkStepDeploymentService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SdkStepDeploymentService(null, crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SdkStepDeploymentService(loggerMock.Object, null);
            });
        }


        [Fact]
        public void Deactivate_NullSdkStepsToDeactivate_LogsNoConfig()
        {
            sdkStepsActivatorService.Deactivate(null);

            loggerMock.VerifyLog(x => x.LogInformation("No SDK steps to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_EmptySdkStepsToDeactivate_LogsNoConfig()
        {
            sdkStepsActivatorService.Deactivate(Array.Empty<string>());

            loggerMock.VerifyLog(x => x.LogInformation("No SDK steps to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_QueryForSdkSteps_IncludesAllNamesPassed()
        {
            var sdkStepsToDeactivate = new string[] { "sdk_step_one", "sdk_step_two" };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sdkmessageprocessingstep", "name", sdkStepsToDeactivate))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sdkmessageprocessingstep"
                    };
                    entityCollection.Entities.AddRange(sdkStepsToDeactivate.Select(value => new Entity("sdkmessageprocessingstep", Guid.NewGuid())));
                    return entityCollection;
                })
                .Verifiable();

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { }
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                });

            sdkStepsActivatorService.Deactivate(sdkStepsToDeactivate);

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_FaultWhenSettingsStatus_LogErrors()
        {
            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sdkmessageprocessingstep", "name", It.IsAny<string[]>()))
                .Returns((string entity, string attribute, string[] values) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sdkmessageprocessingstep"
                    };
                    entityCollection.Entities.AddRange(values.Select(value => new Entity("sdkmessageprocessingstep", Guid.NewGuid())));
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem
                        {
                            Fault = new OrganizationServiceFault
                            {
                                Message = "Test fault response"
                            }
                        },
                        new ExecuteMultipleResponseItem { }
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    response.Results.Add("IsFaulted", true);
                    return response;
                });

            sdkStepsActivatorService.Deactivate(new string[] { "a_process_to_deactivate" });

            loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            loggerMock.VerifyLog(x => x.LogError("Error deactivating SDK Message Processing Steps."));
            loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

    }
}
