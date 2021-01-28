namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class WordTemplateImporterServiceTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<ICrmServiceAdapter> crmServiceAdapterMock;

        private readonly DocumentTemplateDeploymentService wordTemplateImporterService;

        public WordTemplateImporterServiceTests()
        {
            this.loggerMock = new Mock<ILogger>();
            this.crmServiceAdapterMock = new Mock<ICrmServiceAdapter>();

            this.wordTemplateImporterService = new DocumentTemplateDeploymentService(this.loggerMock.Object, this.crmServiceAdapterMock.Object);
        }

        [Fact]
        public void Construct_NullPackageLog_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DocumentTemplateDeploymentService(null, this.crmServiceAdapterMock.Object);
            });
        }

        [Fact]
        public void Construct_NullCrmService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new DocumentTemplateDeploymentService(this.loggerMock.Object, null);
            });
        }

        [Fact]
        public void Import_NullTemplatesToImport_LogNoConfig()
        {
            this.wordTemplateImporterService.Import(null, string.Empty);

            this.loggerMock.VerifyLog(x => x.LogInformation("No Word template to import."));
        }

        [Fact]
        public void Import_EmptyTemplatesToImport_LogNoConfig()
        {
            this.wordTemplateImporterService.Import(Array.Empty<string>(), string.Empty);

            this.loggerMock.VerifyLog(x => x.LogInformation("No Word template to import."));
        }

        [Fact]
        public void ImportWordTemplate_ValidListOfTemplates_CallImportWordTemplate()
        {
            var workTemplatesToImport = new string[] { "word_template_one", "word_template_two" };
            var packageFolderPath = "F:/fake_directory_to_templates/";

            this.wordTemplateImporterService.Import(workTemplatesToImport, packageFolderPath);

            this.crmServiceAdapterMock.Verify(
                x => x.ImportWordTemplate(It.IsIn(
                    workTemplatesToImport.Select(path => packageFolderPath + path))),
                Times.Exactly(workTemplatesToImport.Length));
        }

        [Fact]
        public void ImportWordTemplate_ValidListOfTemplates_LogsComplete()
        {
            var workTemplatesToImport = new string[] { "word_template_one", "word_template_two" };
            var packageFolderPath = "F:/fake_directory_to_templates/";

            this.wordTemplateImporterService.Import(workTemplatesToImport, packageFolderPath);

            foreach (var workTemplate in workTemplatesToImport)
            {
                this.loggerMock.VerifyLog(
                    x => x.LogInformation($"{nameof(DocumentTemplateDeploymentService)}: Word Template imported - {workTemplate}"), Times.Once);
            }
        }
    }
}
