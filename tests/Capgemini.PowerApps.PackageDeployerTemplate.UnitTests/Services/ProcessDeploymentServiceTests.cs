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

    public class ProcessDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly ProcessDeploymentService processActivatorService;

        public ProcessDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.processActivatorService = new ProcessDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ProcessDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ProcessDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void Activate_NullProcessesToActivate_LogsNoConfig()
        {
            this.processActivatorService.Activate(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No processes to activate have been configured."));
        }

        [Fact]
        public void Activate_EmptyProcessesToActivate_LogsNoConfig()
        {
            this.processActivatorService.Activate(Array.Empty<string>());

            this.loggerMock.VerifyLog(x => x.LogInformation("No processes to activate have been configured."));
        }

        [Fact]
        public void Activate_QueryForProcesses_IncludesAllNamesPassed()
        {
            var processesToActivate = new string[] { "process_one", "process_two" };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName,
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            this.processActivatorService.Activate(processesToActivate);

            this.crmServiceAdapterMock.Verify(x => x.RetrieveMultiple(It.Is<QueryByAttribute>(
                q => processesToActivate.All(value => q.Values.Contains(value)))));
        }

        [Fact]
        public void Activate_FaultWhenSettingsStatus_LogErrors()
        {
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName,
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            this.processActivatorService.Activate(new string[] { "a_process_to_activate" });

            this.loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            this.loggerMock.VerifyLog(x => x.LogError("Error activating processes."));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void Deactivate_NullProcessesToActivate_LogsNoConfig()
        {
            this.processActivatorService.Deactivate(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No processes to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_EmptyProcessesToActivate_LogsNoConfig()
        {
            this.processActivatorService.Deactivate(Array.Empty<string>());

            this.loggerMock.VerifyLog(x => x.LogInformation("No processes to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_QueryForProcesses_IncludesAllNamesPassed()
        {
            var processesToDeactivate = new string[] { "process_one", "process_two" };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName,
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            this.processActivatorService.Deactivate(processesToDeactivate);

            this.crmServiceAdapterMock.Verify(x => x.RetrieveMultiple(It.Is<QueryByAttribute>(
                q => processesToDeactivate.All(value => q.Values.Contains(value)))));
        }

        [Fact]
        public void Deactivate_FaultWhenSettingsStatus_LogErrors()
        {
            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName,
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            this.processActivatorService.Deactivate(new string[] { "a_process_to_deactivate" });

            this.loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            this.loggerMock.VerifyLog(x => x.LogError("Error deactivating processes."));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }
    }
}
