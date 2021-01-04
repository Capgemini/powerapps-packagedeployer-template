using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class ProcessActivatorServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly ProcessDeploymentService processActivatorService;

        public ProcessActivatorServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            processActivatorService = new ProcessDeploymentService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ProcessDeploymentService(null, crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ProcessDeploymentService(loggerMock.Object, null);
            });
        }

        [Fact]
        public void Activate_NullProcessesToActivate_LogsNoConfig()
        {
            processActivatorService.Activate(null);

            loggerMock.VerifyLog(x => x.LogInformation("No processes to activate have been configured."));
        }

        [Fact]
        public void Activate_EmptyProcessesToActivate_LogsNoConfig()
        {
            processActivatorService.Activate(Array.Empty<string>());

            loggerMock.VerifyLog(x => x.LogInformation("No processes to activate have been configured."));
        }

        [Fact]
        public void Activate_QueryForProcesses_IncludesAllNamesPassed()
        {
            var processesToActivate = new string[] { "process_one", "process_two" };

            crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            processActivatorService.Activate(processesToActivate);

            crmServiceAdapterMock.Verify(x => x.RetrieveMultiple(It.Is<QueryByAttribute>(
                q => processesToActivate.All(value => q.Values.Contains(value))
            )));
        }

        [Fact]
        public void Activate_FaultWhenSettingsStatus_LogErrors()
        {
            crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            processActivatorService.Activate(new string[] { "a_process_to_activate" });

            loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            loggerMock.VerifyLog(x => x.LogError("Error activating processes."));
            loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void Deactivate_NullProcessesToActivate_LogsNoConfig()
        {
            processActivatorService.Deactivate(null);

            loggerMock.VerifyLog(x => x.LogInformation("No processes to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_EmptyProcessesToActivate_LogsNoConfig()
        {
            processActivatorService.Deactivate(Array.Empty<string>());

            loggerMock.VerifyLog(x => x.LogInformation("No processes to deactivate have been configured."));
        }

        [Fact]
        public void Deactivate_QueryForProcesses_IncludesAllNamesPassed()
        {
            var processesToDeactivate = new string[] { "process_one", "process_two" };

            crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            processActivatorService.Deactivate(processesToDeactivate);

            crmServiceAdapterMock.Verify(x => x.RetrieveMultiple(It.Is<QueryByAttribute>(
                q => processesToDeactivate.All(value => q.Values.Contains(value))
            )));
        }

        [Fact]
        public void Deactivate_FaultWhenSettingsStatus_LogErrors()
        {
            crmServiceAdapterMock
                .Setup(x => x.RetrieveMultiple(It.IsAny<QueryByAttribute>()))
                .Returns((QueryByAttribute query) =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = query.EntityName
                    };
                    entityCollection.Entities.AddRange(query.Values.Select(value => new Entity(query.EntityName, Guid.NewGuid())));
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

            processActivatorService.Deactivate(new string[] { "a_process_to_deactivate" });

            loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            loggerMock.VerifyLog(x => x.LogError("Error deactivating processes."));
            loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }
    }
}

