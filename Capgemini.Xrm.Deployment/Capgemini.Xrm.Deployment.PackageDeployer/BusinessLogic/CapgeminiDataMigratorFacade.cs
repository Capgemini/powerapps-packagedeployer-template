using Capgemini.Xrm.DataMigration.Config;
using Capgemini.Xrm.DataMigration.Core;
using Capgemini.Xrm.DataMigration.Engine;
using Capgemini.Xrm.DataMigration.Repositories;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Capgemini.Xrm.Deployment.PackageDeployer.BusinessLogic
{
    public class CapgeminiDataMigratorFacade
    {
        private readonly ILogger _logger;
        private readonly IOrganizationService _service;

        public CapgeminiDataMigratorFacade(IOrganizationService service, ILogger packageLog)
        {
            _service = service;
            _logger = packageLog;
        }

        public void MigrateDataPackages(string importConfigPath, string configSubfolder)
        {
            _logger.Info("MigrateDataPackages importing started");

            string impConfigFolder = Path.GetDirectoryName(importConfigPath);

            var config = XDocument.Load(importConfigPath);

            Tuple<string, string>[] dataPackages = null;

            dataPackages = config?.Element("configdatastorage")
                ?.Element("capgeminiConfigurationMigration")
                ?.Elements("importPackage")
                .Select(a => new Tuple<string, string>(a.Attribute("extractedDataPath").Value, a.Attribute("importConfigFilePath").Value)).ToArray();

            if (dataPackages != null)
            {
                foreach (var item in dataPackages)
                {
                    try
                    {
                        string dataPath = Path.Combine(impConfigFolder, item.Item1);
                        string configPath = Path.Combine(impConfigFolder, item.Item2);

                        if (!string.IsNullOrWhiteSpace(configSubfolder))
                        {
                            configPath = Path.Combine(Path.GetDirectoryName(configPath), configSubfolder, Path.GetFileName(configPath));
                        }

                        ImportDataPackage(dataPath, configPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("MigrateDataPackages Error:" + ex);
                    }
                }
            }

            _logger.Info("MigrateDataPackages importing finished");
        }

        private void ImportDataPackage(string extractedDataPath, string configFilePath)
        {
            _logger.Info("Importing Started" + extractedDataPath);
            _logger.Info("Import Config Path set to " + configFilePath);

            EntityRepository entityRepo = new EntityRepository(_service);

            CrmImportConfig importConfig = CrmImportConfig.GetConfiguration(configFilePath);

            importConfig.JsonFolderPath = extractedDataPath;

            CrmFileDataImporter fileExporter = new CrmFileDataImporter(_logger, entityRepo, importConfig, new CancellationToken(false));

            fileExporter.MigrateData();

            _logger.Info("Importing Completed" + extractedDataPath);
        }
    }
}