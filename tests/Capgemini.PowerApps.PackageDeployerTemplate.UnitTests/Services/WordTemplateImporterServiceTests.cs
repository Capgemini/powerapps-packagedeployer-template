using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    public class WordTemplateImporterServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly WordTemplateImporterService wordTemplateImporterService;

        public WordTemplateImporterServiceTests()
        {
            loggerMock = new Mock<ILogger>();
            crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();
            crmServiceAdapterMock.Setup(x => x.GetOrganizationService())
                .Returns(() => new Mock<IOrganizationService>().Object);

            wordTemplateImporterService = new WordTemplateImporterService(loggerMock.Object, crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new WordTemplateImporterService(null, crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new WordTemplateImporterService(loggerMock.Object, null);
            });
        }

        [Fact]
        public void ImportWordTemplates_NullTemplatesToImport_LogNoConfig()
        {
            wordTemplateImporterService.ImportWordTemplates(null, "");

            loggerMock.VerifyLog(x => x.LogInformation("No Work Template to import."));
        }

        [Fact]
        public void ImportWordTemplates_EmptyTemplatesToImport_LogNoConfig()
        {
            wordTemplateImporterService.ImportWordTemplates(Array.Empty<string>(), "");

            loggerMock.VerifyLog(x => x.LogInformation("No Work Template to import."));
        }

        [Fact]
        public void ImportWordTemplate_ValidListOfTemplates_CallImportWordTemplate()
        {
            var workTemplatesToImport = new string[] { "word_template_one", "word_template_two" };
            var packageFolderPath = "F:/fake_directory_to_templates/";

            wordTemplateImporterService.ImportWordTemplates(workTemplatesToImport, packageFolderPath);

            crmServiceAdapterMock.Verify(
                x => x.ImportWordTemplate(It.IsIn<string>(
                    workTemplatesToImport.Select(path => packageFolderPath + path))),
                Times.Exactly(workTemplatesToImport.Length));
        }

        [Fact]
        public void ImportWordTemplate_ValidListOfTemplates_LogsComplete()
        {
            var workTemplatesToImport = new string[] { "word_template_one", "word_template_two" };
            var packageFolderPath = "F:/fake_directory_to_templates/";

            wordTemplateImporterService.ImportWordTemplates(workTemplatesToImport, packageFolderPath);

            foreach (var workTemplate in workTemplatesToImport)
            {
                loggerMock.VerifyLog(
                    x => x.LogInformation($"{nameof(WordTemplateImporterService)}: Word Template imported - {workTemplate}"), Times.Once);
            }
        }
    }
}
