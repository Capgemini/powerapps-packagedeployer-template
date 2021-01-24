namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Moq;
    using Xunit;

    public class SlaDeploymentServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly SlaDeploymentService slaActivatorService;

        public SlaDeploymentServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.slaActivatorService = new SlaDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SlaDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new SlaDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void ActivateAll_QueryForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                })
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            this.slaActivatorService.ActivateAll();

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void ActivateAll_SetActiveStateForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { },
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                })
                .Verifiable();

            this.slaActivatorService.ActivateAll();

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void ActivateAll_SetActiveStateForAllSlasFail_LogsErrors()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 0), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(
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

            this.slaActivatorService.ActivateAll();

            this.loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            this.loggerMock.VerifyLog(x => x.LogError("Error activating SLAs."));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void Deactivate_QueryForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                })
                .Verifiable();

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(It.IsAny<EntityCollection>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
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

            this.slaActivatorService.DeactivateAll();

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_SetDisabledStateForAllSlas_Called()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(
                    It.Is<EntityCollection>(collection => allSlas.All(sla => collection.Entities.Contains(sla))),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns((EntityCollection records, int statecode, int statuscode) =>
                {
                    var responseItemCollection = new ExecuteMultipleResponseItemCollection
                    {
                        new ExecuteMultipleResponseItem { },
                        new ExecuteMultipleResponseItem { },
                    };

                    var response = new ExecuteMultipleResponse();
                    response.Results.Add("Responses", responseItemCollection);
                    return response;
                })
                .Verifiable();

            this.slaActivatorService.DeactivateAll();

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Deactivate_SetDisabledStateForAllSlasFail_LogsErrors()
        {
            var allSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "statecode", It.Is<object[]>(value => (int)value[0] == 1), null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(allSlas);
                    return entityCollection;
                });

            this.crmServiceAdapterMock
                .Setup(x => x.UpdateStateAndStatusForEntityInBatch(
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

            this.slaActivatorService.DeactivateAll();

            this.loggerMock.VerifyLog(x => x.LogError(It.IsAny<string>()), Times.Exactly(2));
            this.loggerMock.VerifyLog(x => x.LogError("Error deactivating SLAs."));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void SetDefaultSlas_NullDefaultSlas_LogsNoConfig()
        {
            this.slaActivatorService.SetDefaultSlas(null);

            this.loggerMock.VerifyLog(x => x.LogInformation("No default SLAs have been configured."));
        }

        [Fact]
        public void SetDefaultSlas_EmptyDefaultSlas_LogsNoConfig()
        {
            this.slaActivatorService.SetDefaultSlas(Array.Empty<string>());

            this.loggerMock.VerifyLog(x => x.LogInformation("No default SLAs have been configured."));
        }

        [Fact]
        public void SetDefaultSlas_QueryDefaultSlas_Called()
        {
            var defaultSlaNames = new string[] { "default_sla_one", "default_sla_two" };

            var defaultSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "name", defaultSlaNames, null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(defaultSlas);
                    return entityCollection;
                }).Verifiable();

            this.slaActivatorService.SetDefaultSlas(defaultSlaNames);

            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void SetDefaultSlas_SetDefaultForDefaultSlas_Called()
        {
            var defaultSlaNames = new string[] { "default_sla_one", "default_sla_two" };

            var defaultSlas = new List<Entity>
            {
                new Entity("sla", Guid.NewGuid()),
                new Entity("sla", Guid.NewGuid()),
            };

            this.crmServiceAdapterMock
                .Setup(x => x.RetrieveMultipleByAttribute("sla", "name", defaultSlaNames, null))
                .Returns(() =>
                {
                    var entityCollection = new EntityCollection
                    {
                        EntityName = "sla",
                    };
                    entityCollection.Entities.AddRange(defaultSlas);
                    return entityCollection;
                });

            this.slaActivatorService.SetDefaultSlas(defaultSlaNames);

            this.crmServiceAdapterMock
                .Verify(
                    x => x.Update(It.Is<Entity>(
                    entity => defaultSlas.Contains(entity) && entity.Attributes.Contains("isdefault") && (bool)entity.Attributes["isdefault"])), Times.Exactly(defaultSlas.Count));
        }
    }
}
