using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class SlaActivatorServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SlaActivatorService slaActivatorService;

        public SlaActivatorServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            crmServiceAdapterMock.Setup(x => x.GetOrganizationService())
                .Returns(() => new Mock<IOrganizationService>().Object);

            slaActivatorService = new SlaActivatorService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SlaActivatorService(null, crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SlaActivatorService(loggerMock.Object, null);
            });
        }

        [Fact]
        public void Activate_NullDefaultSlas_LogsNoConfig()
        {
            slaActivatorService.Activate(null);

            loggerMock.VerifyLog(x => x.LogInformation("No default SLAs have been configured."));
        }

        [Fact]
        public void Activate_EmptyDefaultSlas_LogsNoConfig()
        {
            slaActivatorService.Activate(Array.Empty<string>());

            loggerMock.VerifyLog(x => x.LogInformation("No default SLAs have been configured."));
        }

        [Fact]
        public void Activate_QueryForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                })
                .Verifiable();

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "name", It.IsAny<string[]>()))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.Add(new Entity("sla", Guid.NewGuid()));
                    return entityCollection;
                });

            slaActivatorService.Activate(new string[] { "default_sla_one" });

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Activate_SetActiveStateForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { }
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                })
                .Verifiable();

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "name", It.IsAny<string[]>()))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.Add(new Entity("sla", Guid.NewGuid()));
                    return entityCollection;
                });

            slaActivatorService.Activate(new string[] { "default_sla_one" });

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Activate_SetActiveStateForAllSlasFail_LogsErrors()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "name", It.IsAny<string[]>()))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.Add(new Entity("sla", Guid.NewGuid()));
                    return entityCollection;
                });

            slaActivatorService.Activate(new string[] { "default_sla_one" });

            loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            loggerMock.VerifyLog(x => x.LogError("Error activating SLAs."));
            loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void Activate_QueryDefaultSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            var defaultSlaNames = new string[] { "default_sla_one", "default_sla_two" };

            var defaultSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.IsAny<EntityCollection>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "name", defaultSlaNames))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(defaultSlas);
                    return entityCollection;
                }).Verifiable();

            slaActivatorService.Activate(defaultSlaNames);

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Activate_SetDefaultForDefaultSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            var defaultSlaNames = new string[] { "default_sla_one", "default_sla_two" };

            var defaultSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.IsAny<EntityCollection>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "name", defaultSlaNames))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(defaultSlas);
                    return entityCollection;
                });

            slaActivatorService.Activate(defaultSlaNames);

            crmServiceAdapterMock
                .Verify(x => x.Update(It.Is<Entity>(
                    entity => defaultSlas.Contains(entity) && entity.Attributes.Contains("isdefault") && (bool)entity.Attributes["isdefault"]
                )), Times.Exactly(defaultSlas.Count));
        }

        [Fact]
        public void Deactivate_QueryForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                })
                .Verifiable();

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            slaActivatorService.Deactivate();

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_SetDisabledStateForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { }
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                })
                .Verifiable();

            slaActivatorService.Deactivate();

            crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_SetDisabledStateForAllSlasFail_LogsErrors()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid())
            };

            crmServiceAdapterMock
                .Setup(x => x.QueryRecordsBySingleAttributeValue("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1)))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla"
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            crmServiceAdapterMock
                .Setup(x => x.SetRecordsStateInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            slaActivatorService.Deactivate();

            loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            loggerMock.VerifyLog(x => x.LogError("Error deactivating SLAs."));
            loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

    }
}
