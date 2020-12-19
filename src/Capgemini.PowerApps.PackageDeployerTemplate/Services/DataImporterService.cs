using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Capgemini.DataMigration.Resiliency.Polly;
using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Capgemini.Xrm.DataMigration.Engine;
using Capgemini.Xrm.DataMigration.Repositories;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    public class DataImporterService
    {
        private readonly TraceLogger packageLog;
        private readonly LoggerAdapter loggerAdapter;
        private readonly EntityRepository entityRepository;

        public DataImporterService(TraceLogger packageLog, CrmServiceAdapter crmSvc)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.loggerAdapter = new LoggerAdapter(this.packageLog);

            var organisationService = crmSvc.GetOrganizationService() ?? throw new ArgumentNullException(nameof(crmSvc));
            this.entityRepository = new EntityRepository(organisationService, new ServiceRetryExecutor());
        }

        public void Import(IEnumerable<DataImportConfig> dataImportConfigs, string packageFolderPath)
        {
            if (dataImportConfigs is null || !dataImportConfigs.Any())
            {
                this.packageLog.Log("No imports have been configured.");

                return;
            }

            foreach (var dataImportConfig in dataImportConfigs)
            {
                this.Import(dataImportConfig, packageFolderPath);
            }
        }

        private void Import(DataImportConfig dataImportConfig, string packageFolderPath)
        {
            this.packageLog.Log($"Importing data at {dataImportConfig.DataFolderPath} using import config at {dataImportConfig.ImportConfigPath}.");

            new CrmFileDataImporter(
                this.loggerAdapter,
                this.entityRepository,
                GetImportConfig(dataImportConfig, packageFolderPath),
                new CancellationToken(false))
                .MigrateData();

            this.packageLog.Log($"Finished importing data at {dataImportConfig.DataFolderPath}.");
        }

        private static CrmImportConfig GetImportConfig(DataImportConfig dataImportConfig, string packageFolderPath)
        {
            var importConfig = CrmImportConfig.GetConfiguration(Path.Combine(packageFolderPath, dataImportConfig.ImportConfigPath));
            importConfig.JsonFolderPath = Path.Combine(packageFolderPath, dataImportConfig.DataFolderPath);
            return importConfig;
        }
    }
}
