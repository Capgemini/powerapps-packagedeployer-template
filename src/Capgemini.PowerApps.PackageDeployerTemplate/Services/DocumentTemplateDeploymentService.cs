namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Deployment functionality related to document templates.
    /// </summary>
    public class DocumentTemplateDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTemplateDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public DocumentTemplateDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Imports the provided document templates. Only Word templates are currently supported.
        /// </summary>
        /// <param name="documentTemplates">The file names of the document templates.</param>
        /// <param name="packageFolderPath">The path of the package folder.</param>
        public void Import(IEnumerable<string> documentTemplates, string packageFolderPath)
        {
            if (documentTemplates is null || !documentTemplates.Any())
            {
                this.logger.LogInformation("No Word template to import.");
                return;
            }

            foreach (var docTemplate in documentTemplates)
            {
                try
                {
                    this.UpdateTemplateBindingAndImport(Path.Combine(packageFolderPath, docTemplate));
                    this.logger.LogInformation($"{nameof(DocumentTemplateDeploymentService)}: Word template '{docTemplate}' successfully imported.");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"{nameof(DocumentTemplateDeploymentService)}: Word template '{docTemplate}' failed to import.");
                }
            }
        }

        private static string FindInWordDocument(string filePath, string regexPattern)
        {
            using var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true });
            foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
            {
                using var sr = new StreamReader(customXmlPart.GetStream());
                var match = Regex.Match(sr.ReadToEnd(), regexPattern);

                if (match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }

            throw new PackageDeployerException("Unable to find entity logical name and type code in template.");
        }

        private static void SetEntity(string filePath, string logicalName, string typeCode)
        {
            var pattern = @"urn:microsoft-crm/document-template/.*/\d*/";
            var replace = $@"urn:microsoft-crm/document-template/{logicalName}/{typeCode}/";

            using var doc = WordprocessingDocument.Open(filePath, true, new OpenSettings { AutoSave = true });

            foreach (var binding in doc.MainDocumentPart.Document.Descendants<DataBinding>())
            {
                binding.PrefixMappings = Regex.Replace(binding.PrefixMappings, pattern, replace);
            }

            foreach (var wordBinding in doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Office2013.Word.DataBinding>())
            {
                wordBinding.PrefixMappings = Regex.Replace(wordBinding.PrefixMappings, pattern, replace);
            }

            foreach (var customXmlPart in doc.MainDocumentPart.CustomXmlParts)
            {
                using var sr = new StreamReader(customXmlPart.GetStream());
                var updatedXmlPart = Regex.Replace(sr.ReadToEnd(), pattern, replace);
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(updatedXmlPart));
                customXmlPart.FeedData(ms);
            }
        }

        private static string GetEntityLogicalName(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/(.*)/\d*/");
        }

        private static string GetEntityTypeCode(string filePath)
        {
            return FindInWordDocument(filePath, @"urn:microsoft-crm/document-template/.*/(\d*)/");
        }

        private void UpdateTemplateBindingAndImport(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? Constants.DocumentTemplate.DocumentTypeExcel : Constants.DocumentTemplate.DocumentTypeWord);

            if (templateType.Value != 2)
            {
                throw new NotSupportedException("Only Word templates (.docx) files are supported.");
            }

            var logicalName = GetEntityLogicalName(filePath);
            var targetEntityTypeCode = this.crmSvc.GetEntityTypeCode(logicalName);
            var entityTypeCode = GetEntityTypeCode(filePath);

            if (targetEntityTypeCode != entityTypeCode)
            {
                SetEntity(filePath, logicalName, targetEntityTypeCode);
            }

            this.crmSvc.ImportWordTemplate(fileInfo, logicalName, templateType, filePath);
        }
    }
}
