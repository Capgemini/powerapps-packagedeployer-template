namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
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
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Functionality related to importing data.
    /// </summary>
    public class DataImporterService
    {
        private readonly ILogger logger;
        private readonly DataMigratorLoggerAdapter loggerAdapter;
        private readonly EntityRepository entityRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImporterService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public DataImporterService(ILogger logger, IOrganizationService crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loggerAdapter = new DataMigratorLoggerAdapter(this.logger);

            if (crmSvc is null)
            {
                throw new ArgumentNullException(nameof(crmSvc));
            }

            this.entityRepository = new EntityRepository(crmSvc, new ServiceRetryExecutor());
        }

        /// <summary>
        /// Import data.
        /// </summary>
        /// <param name="dataImportConfigs">The data import configs to use.</param>
        /// <param name="packageFolderPath">The path to the package folder.</param>
        public void Import(IEnumerable<DataImportConfig> dataImportConfigs, string packageFolderPath)
        {
            if (dataImportConfigs is null || !dataImportConfigs.Any())
            {
                this.logger.LogInformation("No imports have been configured.");

                return;
            }

            foreach (var dataImportConfig in dataImportConfigs)
            {
                this.Import(dataImportConfig, packageFolderPath);
            }
        }

        private static CrmImportConfig GetImportConfig(DataImportConfig dataImportConfig, string packageFolderPath)
        {
            var importConfig = CrmImportConfig.GetConfiguration(Path.Combine(packageFolderPath, dataImportConfig.ImportConfigPath));
            importConfig.JsonFolderPath = Path.Combine(packageFolderPath, dataImportConfig.DataFolderPath);
            return importConfig;
        }

        private void Import(DataImportConfig dataImportConfig, string packageFolderPath)
        {
            this.logger.LogInformation($"Importing data at {dataImportConfig.DataFolderPath} using import config at {dataImportConfig.ImportConfigPath}.");

            new CrmFileDataImporter(
                this.loggerAdapter,
                this.entityRepository,
                GetImportConfig(dataImportConfig, packageFolderPath),
                new CancellationToken(false))
                .MigrateData();

            this.logger.LogInformation($"Finished importing data at {dataImportConfig.DataFolderPath}.");
        }
    }
}
