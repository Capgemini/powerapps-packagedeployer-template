namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Deployment functionality related to word templates.
    /// </summary>
    public class WordTemplateImporterService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="WordTemplateImporterService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public WordTemplateImporterService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Imports the provided word templates.
        /// </summary>
        /// <param name="wordTemplates">The file names of the word templates.</param>
        /// <param name="packageFolderPath">The path of the package folder.</param>
        public void ImportWordTemplates(IEnumerable<string> wordTemplates, string packageFolderPath)
        {
            if (wordTemplates is null || !wordTemplates.Any())
            {
                this.logger.LogInformation("No Work Template to import.");
                return;
            }

            foreach (var wordTemplate in wordTemplates)
            {
                this.crmSvc.ImportWordTemplate(Path.Combine(packageFolderPath, wordTemplate));
                this.logger.LogInformation($"{nameof(WordTemplateImporterService)}: Word Template imported - {wordTemplate}");
            }
        }
    }
}
