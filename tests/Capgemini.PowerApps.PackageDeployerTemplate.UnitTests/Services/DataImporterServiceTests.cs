namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class DataImporterServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly DataImporterService dataImporterService;

        public DataImporterServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.dataImporterService = new DataImporterService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DataImporterService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DataImporterService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void Import_NullImportConfigs_LogsNoConfig()
        {
            this.dataImporterService.Import(null, string.Empty);

            this.loggerMock.VerifyLog(x => x.LogInformation("No imports have been configured."));
        }

        [Fact]
        public void Import_EmptyImportConfigs_LogsNoConfig()
        {
            this.dataImporterService.Import(new List<DataImportConfig>(), string.Empty);

            this.loggerMock.VerifyLog(x => x.LogInformation("No imports have been configured."));
        }

        // TODO: Test call to the Data Migrator package but `CrmFileDataImporter` is newed up in the service itself.
        // My first idea is a factory but that means more that the calling code has to pass it. Is a fully DI containter
        // an option here?
    }
}
