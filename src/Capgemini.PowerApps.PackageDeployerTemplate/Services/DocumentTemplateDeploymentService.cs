namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;

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
                this.crmSvc.ImportWordTemplate(Path.Combine(packageFolderPath, docTemplate));
                this.logger.LogInformation($"{nameof(DocumentTemplateDeploymentService)}: Word template imported - {docTemplate}");
            }
        }
    }
}
