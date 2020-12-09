using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class WordTemplateImporterService
    {

        private readonly TraceLogger packageLog;
        private readonly CrmServiceClient crmSvc;

        public WordTemplateImporterService(TraceLogger packageLog, CrmServiceClient crmSvc)
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
