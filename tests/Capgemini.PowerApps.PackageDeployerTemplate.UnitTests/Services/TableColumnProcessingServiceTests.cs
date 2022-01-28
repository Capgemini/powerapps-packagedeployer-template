namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Moq;
    using Xunit;

    public class TableColumnProcessingServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly TableColumnProcessingService tableColumnProcessingService;

        public TableColumnProcessingServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.tableColumnProcessingService = new TableColumnProcessingService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void ExecuteMultiple_NoRequests_IsNotCalled()
        {
            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = Array.Empty<ColumnConfig>();

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            this.tableColumnProcessingService.ProcessTables(tableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("No requests for table columns were added."));
            this.crmServiceAdapterMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ExecuteMultiple_Requests_IsCalled()
        {
            // Arrange
            this.crmServiceAdapterMock
                .Setup(x => x.ExecuteMultiple(It.IsAny<List<OrganizationRequest>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int?>()))
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

            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                GetAutonumberColumnConfig("test_autonumberone", 1000),
                GetAutonumberColumnConfig("test_autonumbertwo", 2000),
            };

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            // Act
            this.tableColumnProcessingService.ProcessTables(tableConfigs);

            // Assert
            this.loggerMock.VerifyLog(x => x.LogInformation("Executing requests for table columns."));
            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Log_AutonumbersProcessed_AutonumberSeedRequestAdded()
        {
            // Arrage
            this.crmServiceAdapterMock
            .Setup(x => x.ExecuteMultiple(It.IsAny<List<OrganizationRequest>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int?>()))
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

            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                GetAutonumberColumnConfig("test_autonumberone", 1000),
                GetAutonumberColumnConfig("test_autonumbertwo", 2000),
            };

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            // Act
            this.tableColumnProcessingService.ProcessTables(tableConfigs);

            // Assert
            this.loggerMock.VerifyLog(x => x.LogInformation("Adding auto-number seed request. Entity Name: test_table. Auto-number Attribute: test_autonumberone. Value: 1000"));
            this.loggerMock.VerifyLog(x => x.LogInformation("Adding auto-number seed request. Entity Name: test_table. Auto-number Attribute: test_autonumbertwo. Value: 2000"));
            this.crmServiceAdapterMock.Verify(svc => svc.ExecuteMultiple(It.IsAny<List<OrganizationRequest>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int?>()));
        }

        [Fact]
        public void ExecuteMultiple_Errors_LogErrors()
        {
            this.crmServiceAdapterMock
            .Setup(x => x.ExecuteMultiple(It.IsAny<List<OrganizationRequest>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int?>()))
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

            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                GetAutonumberColumnConfig("test_autonumberone", 1000),
                GetAutonumberColumnConfig("test_autonumbertwo", 2000),
            };

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            // Act
            this.tableColumnProcessingService.ProcessTables(tableConfigs);

            // Assert
            this.loggerMock.VerifyLog(x => x.LogError("Error processing requests for table columns"));
            this.loggerMock.VerifyLog(x => x.LogError("Test fault response"));
        }

        [Fact]
        public void Log_AutonumberSeedAlreadySet_AutoNumberSeedRequestNotAdded()
        {
            // Arrage
            this.crmServiceAdapterMock
            .Setup(x => x.Execute(It.IsAny<GetAutoNumberSeedRequest>()))
            .Returns(() =>
            {
                return new GetAutoNumberSeedResponse { Results = { { "AutoNumberSeedValue", 1000L } } };
            });

            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                GetAutonumberColumnConfig("test_autonumberone", 1000),
            };

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            // Act
            this.tableColumnProcessingService.ProcessTables(tableConfigs);

            // Assert
            this.loggerMock.VerifyLog(x => x.LogInformation("Auto-number seed test_autonumberone for test_table is already set to value: 1000"));
            this.loggerMock.VerifyLog(x => x.LogInformation("No requests for Auto-number seeds were added."));
            this.crmServiceAdapterMock.Verify(svc => svc.Execute(It.IsAny<GetAutoNumberSeedRequest>()));
            this.crmServiceAdapterMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Log_NoAutonumbersProcessed_NoRequestsAdded()
        {
            // Arrange
            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                GetAutonumberColumnConfig("test_name", null),
            };

            tableConfigs.Add(GetTableConfig("test_table", tableColumns));

            // Act
            this.tableColumnProcessingService.ProcessTables(tableConfigs);

            // Assert
            this.loggerMock.VerifyLog(x => x.LogInformation("No requests for Auto-number seeds were added."));
        }

        private static TableConfig GetTableConfig(string tableName, ColumnConfig[] columnConfigs)
        {
            return new TableConfig()
            {
                Name = tableName,
                Columns = columnConfigs,
            };
        }

        private static ColumnConfig GetAutonumberColumnConfig(string name, int? value)
        {
            string stringValue = value.ToString();
            return new ColumnConfig()
            {
                Name = name,
                AutonumberSeedValue = value,
                AutonumberSeedAsText = stringValue,
            };
        }
    }
}
