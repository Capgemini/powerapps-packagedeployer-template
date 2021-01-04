using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class DataImporterServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly DataImporterService dataImporterService;

        public DataImporterServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            crmServiceAdapterMock.Setup(x => x.GetOrganizationService())
                .Returns(() => new Mock<IOrganizationService>().Object);

            dataImporterService = new DataImporterService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DataImporterService(null, crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DataImporterService(loggerMock.Object, null);
            });
        }

        [Fact]
        public void Import_NullImportConfigs_LogsNoConfig()
        {
            dataImporterService.Import(null, "");

            loggerMock.VerifyLog(x => x.LogInformation("No imports have been configured."));
        }

        [Fact]
        public void Import_EmptyImportConfigs_LogsNoConfig()
        {
            dataImporterService.Import(new List<DataImportConfig>(), "");

            loggerMock.VerifyLog(x => x.LogInformation("No imports have been configured."));
        }

        // TODO: Test call to the Data Migrator package but `CrmFileDataImporter` is newed up in the service itself. 
        // My first idea is a factory but that means more that the calling code has to pass it. Is a fully DI containter 
        // an option here?
    }
}
