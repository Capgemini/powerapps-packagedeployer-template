using System;
using System.IO;
using System.Threading;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Capgemini.Xrm.DataMigration.CrmStore.Config;
using Capgemini.Xrm.DataMigration.Engine;
using Capgemini.Xrm.DataMigration.Repositories;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    public class DataImporter
    {
        private readonly TraceLogger packageLog;
        private readonly EntityRepository entityRepository;
        private readonly LoggerAdapter loggerAdapter;

        public DataImporter(TraceLogger packageLog, EntityRepository entityRepository)
        {
            this.packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
            this.entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
            this.loggerAdapter = new LoggerAdapter(this.packageLog);
        }

        public void Import(DataImportConfig dataImportConfig, string packageFolderPath)
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
