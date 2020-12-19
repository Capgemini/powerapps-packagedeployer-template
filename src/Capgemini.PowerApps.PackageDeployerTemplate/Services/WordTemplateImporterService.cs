using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class WordTemplateImporterService
    {

        private readonly TraceLogger packageLog;
        private readonly CrmServiceAdapter crmSvc;

        public WordTemplateImporterService(TraceLogger packageLog, CrmServiceAdapter crmSvc)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        public void ImportWordTemplates(IEnumerable<string> wordTemplates, string packageFolderPath)
        {
            foreach (var wordTemplate in wordTemplates)
            {
                this.crmSvc.ImportWordTemplate(Path.Combine(packageFolderPath, wordTemplate));
            }
        }

    }
}
