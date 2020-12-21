using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class WordTemplateImporterService
    {

        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        public WordTemplateImporterService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void ImportWordTemplates(IEnumerable<string> wordTemplates, string packageFolderPath)
        {
            foreach (var wordTemplate in wordTemplates)
            {
                this.crmSvc.ImportWordTemplate(Path.Combine(packageFolderPath, wordTemplate));
                logger.LogInformation($"{nameof(WordTemplateImporterService)}: Word Template imported - {wordTemplate}");
            }
        }

    }
}
