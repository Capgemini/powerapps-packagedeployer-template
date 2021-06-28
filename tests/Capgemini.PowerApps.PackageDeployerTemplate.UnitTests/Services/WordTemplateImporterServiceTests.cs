namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.IO;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
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
        public void ImportWordTemplate_Should_UpdateBindingsAsExpected()
        {
            var random = new Random();
            var targetEntityTypeCode = random.Next(0, 99999);

            var workTemplatesToImport = new string[] { TestUtilities.GetResourcePath("Word Document with Bindings.docx"), TestUtilities.GetResourcePath("Word Document with Bindings - 2.docx") };
            var packageFolderPath = "F:/fake_directory_to_templates/";

            this.crmServiceAdapterMock.Setup(x => x.GetEntityTypeCode(It.IsAny<string>())).Returns(targetEntityTypeCode.ToString());

            this.wordTemplateImporterService.Import(workTemplatesToImport, packageFolderPath);

            foreach (var workTemplate in workTemplatesToImport)
            {
                this.loggerMock.VerifyLog(
                    x => x.LogInformation($"{nameof(DocumentTemplateDeploymentService)}: Word template '{workTemplate}' successfully imported."), Times.Once);

                this.VerifyDataBindingUpdates(workTemplate, targetEntityTypeCode.ToString());
            }

            this.crmServiceAdapterMock.Verify(
                x => x.ImportWordTemplate(
                    It.IsAny<FileInfo>(),
                    It.IsAny<string>(),
                    It.Is<OptionSetValue>(i => i.Value.Equals(Constants.DocumentTemplate.DocumentTypeWord)),
                    It.IsAny<string>()),
                Times.Exactly(workTemplatesToImport.Length));
        }

        private void VerifyDataBindingUpdates(string documentPath, string entityTypeCode)
        {
            using (var doc = WordprocessingDocument.Open(documentPath, true, new OpenSettings { AutoSave = true }))
            {
                foreach (var binding in doc.MainDocumentPart.Document.Descendants<DataBinding>())
                {
                    Assert.Matches($"urn:microsoft-crm/document-template/.*/{entityTypeCode}/", binding.PrefixMappings.Value);
                }

                foreach (var repeatableBinding in doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Office2013.Word.DataBinding>())
                {
                    Assert.Matches($"urn:microsoft-crm/document-template/.*/{entityTypeCode}/", repeatableBinding.PrefixMappings.Value);
                }
            }

            this.crmServiceAdapterMock.Verify(
                x => x.ImportWordTemplate(
                    It.IsAny<FileInfo>(),
                    It.IsAny<string>(),
                    It.Is<OptionSetValue>(i => i.Value.Equals(Constants.DocumentTemplate.DocumentTypeWord)),
                    It.Is<string>(j => j.Equals(documentPath))),
                Times.Once);
        }
    }
}
