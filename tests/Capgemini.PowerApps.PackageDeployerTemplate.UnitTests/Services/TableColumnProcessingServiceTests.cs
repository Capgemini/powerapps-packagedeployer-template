namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System.Collections.Generic;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
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
        public void Execute_Multiple_Not_Called_When_There_Are_No_Requests()
        {
            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[0];

            tableConfigs.Add(this.GetTableConfig("test_table", tableColumns));

            this.tableColumnProcessingService.ProcessTables(tableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("No requests for table columns were added."));
            this.crmServiceAdapterMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Execute_Multiple_Called_When_There_Are_Requests()
        {
            this.crmServiceAdapterMock
                .Setup(x => x.ExecuteMultiple(It.IsAny<List<OrganizationRequest>>(), It.IsAny<bool>(), It.IsAny<bool>()))
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
                this.GetAutonumberColumnConfig("test_autonumberone", 1000),
                this.GetAutonumberColumnConfig("test_autonumbertwo", 2000),
            };

            tableConfigs.Add(this.GetTableConfig("test_table", tableColumns));

            this.tableColumnProcessingService.ProcessTables(tableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("Executing requests for table columns."));
            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Log_Autonumber_Seed_Request_When_Generated()
        {

            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                this.GetAutonumberColumnConfig("test_autonumberone", 1000),
                this.GetAutonumberColumnConfig("test_autonumbertwo", 2000),
            };

            tableConfigs.Add(this.GetTableConfig("test_table", tableColumns));

            this.tableColumnProcessingService.ProcessTables(tableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("Adding auto-number seed request. Entity Name: Entity Name: test_table. Auto-number Attribute: test_autonumberone. Value: 1000"));
            this.loggerMock.VerifyLog(x => x.LogInformation("Adding auto-number seed request. Entity Name: Entity Name: test_table. Auto-number Attribute: test_autonumbertwo. Value: 2000"));
            this.crmServiceAdapterMock.Verify();
        }

        [Fact]
        public void Log_No_Autonumber_Seed_Requets_Added()
        {
            List<TableConfig> tableConfigs = new List<TableConfig>();
            ColumnConfig[] tableColumns = new ColumnConfig[]
            {
                this.GetAutonumberColumnConfig("test_name", null),
            };

            tableConfigs.Add(this.GetTableConfig("test_table", tableColumns));

            this.tableColumnProcessingService.ProcessTables(tableConfigs);
            this.loggerMock.VerifyLog(x => x.LogInformation("No requests for Auto-number seeds were added."));
        }

        private TableConfig GetTableConfig(string tableName, ColumnConfig[] columnConfigs)
        {
            return new TableConfig()
            {
                Name = tableName,
                Columns = columnConfigs,
            };
        }

        private ColumnConfig GetAutonumberColumnConfig(string name, int? value)
        {
            return new ColumnConfig()
            {
                Name = name,
                AutonumberSeedValue = value,
            };
        }
    }
}
