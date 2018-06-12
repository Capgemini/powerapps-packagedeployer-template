using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.PackageDeployer.BusinessLogic;
using Capgemini.Xrm.Deployment.PackageDeployer.Core;
using Capgemini.Xrm.DeploymentHelpers;
using Microsoft.Uii.Common.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Capgemini.Xrm.Deployment.PackageDeployer
{
    /// <summary>
    /// Import package starter frame.
    /// </summary>
    public class CapgeminiPackageTemplate : ImportExtension, IPackageTemplate
    {
        private Logger _logger;
        private CrmAccess _gatewayAcces;
        private IOrganizationService _orgService;
        private PackageDeployerConfigReader _config;
        private DeploymentActivities _deplActivities;
        private ProcessesActivator _procActivator;

        private string _impConfigSubfolder = "";
        private bool _skipPostDeploymentActions = false;
        private int _maxTimeout = 30;

        /// <summary>
        /// Called When the package is initialized.
        /// </summary>
        public override void InitializeCustomExtension()
        {
            if (RuntimeSettings != null)
            {
                PackageLog.Log(string.Format("Runtime Settings populated.  Count = {0}", RuntimeSettings.Count));
                foreach (var setting in RuntimeSettings)
                {
                    PackageLog.Log(string.Format("Key={0} | Value={1}", setting.Key, setting.Value));
                }

                // Check to see if skip checks is present.
                if (RuntimeSettings.ContainsKey("SkipChecks"))
                {
                    bool bSkipChecks = false;
                    if (bool.TryParse((string)RuntimeSettings["SkipChecks"], out bSkipChecks))
                        OverrideDataImportSafetyChecks = bSkipChecks;
                }

                if (RuntimeSettings.ContainsKey("ImpConfigSubfolder"))
                {
                    _impConfigSubfolder = (string)RuntimeSettings["ImpConfigSubfolder"];
                }

                if (RuntimeSettings.ContainsKey("SkipPostDeploymentActions"))
                {
                    bool bSkipPostDeployment = false;
                    if (bool.TryParse((string)RuntimeSettings["SkipPostDeploymentActions"], out bSkipPostDeployment))
                        _skipPostDeploymentActions = bSkipPostDeployment;

                    PackageLog.Log($"SkipPostDeploymentActions set up to {_skipPostDeploymentActions}");
                }

                if (RuntimeSettings.ContainsKey("MaxCrmConnectionTimeOutMinutes"))
                {
                    int maxTimeout = 30;
                    if (int.TryParse((string)RuntimeSettings["MaxCrmConnectionTimeOutMinutes"], out maxTimeout))
                        _maxTimeout = maxTimeout;

                    PackageLog.Log($"Max TimeOut set up to {_maxTimeout}");
                }
            }
            else
                PackageLog.Log("Runtime Settings not populated");

            _config = new PackageDeployerConfigReader(GetImportPackageFolderPath);

            SetConfiguredTimeout();

            if (this.CrmSvc.OrganizationServiceProxy != null)
                _orgService = this.CrmSvc.OrganizationServiceProxy;
            else if (this.CrmSvc.OrganizationWebProxyClient != null)
                _orgService = this.CrmSvc.OrganizationWebProxyClient;

            _logger = new Logger(this);
            _logger.WriteLogMessage("InitializeCustomExtension", TraceEventType.Start);
            _gatewayAcces = new CrmAccess(_orgService, _maxTimeout);

            _deplActivities = new DeploymentActivities(_config, _logger, _gatewayAcces);
            _procActivator = new ProcessesActivator(_config, _logger, _gatewayAcces);

            try
            {
                PackageLog.Log("Deactivating SLA");
                _deplActivities.DeactivateAllSLAs();
            }
            catch (Exception ex)
            {
                _logger.WriteLogMessage("Error deactivating SLAs", TraceEventType.Error, ex);
            }

            if (!_config.SkipCustomDeployment)
            {
                var deplMgr = new CapgeminiDeploymentManager(this, _logger, _gatewayAcces, _config);
                deplMgr.StartCustomDeployment();
                _logger.WriteLogMessage("InitializeCustomExtension", TraceEventType.Stop);
            }
        }

        /// <summary>
        /// Called Before Import Completes.
        /// </summary>
        /// <returns></returns>
        public override bool BeforeImportStage()
        {
            return true; // do nothing here.
        }

        /// <summary>
        /// Called for each UII record imported into the system
        /// This is UII Specific and is not generally used by Package Developers
        /// </summary>
        /// <param name="app">App Record</param>
        /// <returns></returns>
        public override ApplicationRecord BeforeApplicationRecordImport(ApplicationRecord app)
        {
            return app;  // do nothing here.
        }

        /// <summary>
        /// Called during a solution upgrade while both solutions are present in the target CRM instance.
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring.
        /// </summary>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }

        /// <summary>
        /// Called after Import completes.
        /// </summary>
        /// <returns></returns>
        public override bool AfterPrimaryImport()
        {
            if (!_skipPostDeploymentActions && !_config.SkipPostDeploymentActions)
            {
                try
                {
                    var importFacade = new CapgeminiDataMigratorFacade(this._gatewayAcces.CurrentServiceProxy, this._logger);
                    importFacade.MigrateDataPackages(_config.SolutionConfigFilePath, _impConfigSubfolder);
                }
                catch (Exception ex)
                {
                    _logger.WriteLogMessage("Error Importing Data", TraceEventType.Error, ex);
                }

                _logger.WriteLogMessage("Capgemini Data Migration", TraceEventType.Stop);

                try
                {
                    _deplActivities.LoadTemplates();
                }
                catch (Exception ex)
                {
                    _logger.WriteLogMessage("Error Loading Word Templates", TraceEventType.Error, ex);
                }

                try
                {
                    _deplActivities.ActivateAllSLAs();
                }
                catch (Exception ex)
                {
                    _logger.WriteLogMessage("Error activating SLAs", TraceEventType.Error, ex);
                }

                try
                {
                    _procActivator.ActivateRequiredWorkflows();
                }
                catch (Exception ex)
                {
                    _logger.WriteLogMessage("Error Activating/Deactivating Processes", TraceEventType.Error, ex);
                }

                try
                {
                    _procActivator.DeactivatePluginSteps();
                }
                catch (Exception ex)
                {
                    _logger.WriteLogMessage("Error Deactivating Plugin Steps", TraceEventType.Error, ex);
                }
            }
            else
            {
                _logger.WriteLogMessage("Skipping all post deployment actions");
            }

            return true;
        }

        #region Properties

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        /// <returns></returns>
        public override string GetNameOfImport(bool plural)
        {
            return "Capgemini PackageDeployer Deployment";
        }

        /// <summary>
        /// Folder Name for the Package data.
        /// </summary>
        public override string GetImportPackageDataFolderName
        {
            get
            {
                // WARNING this value directly correlates to the folder name in the Solution Explorer where the ImportConfig.xml and sub content is located.
                // Changing this name requires that you also change the correlating name in the Solution Explorer
                return "PkgFolder";
            }
        }

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText
        {
            get { return "CRM solutions to be deployed"; }
        }

        /// <summary>
        /// Long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport
        {
            get { return "CRM solutions to be deployed"; }
        }

        #endregion Properties

        public string GetImportPackageFolderPath
        {
            get
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var packageFolder = Path.Combine(assemblyFolder, GetImportPackageDataFolderName);
                return packageFolder;
            }
        }

        #region IPackageTemplate Interface Implementtion

        public new TraceLogger PackageLog
        {
            get
            {
                return base.PackageLog;
            }
            set
            {
                base.PackageLog = value;
            }
        }

        public new void CreateProgressItem(string message)
        {
            base.CreateProgressItem(message);
        }

        public new void RaiseFailEvent(string message, Exception ex)
        {
            base.RaiseFailEvent(message, ex);
        }

        public new void RaiseUpdateEvent(string message, ProgressPanelItemStatus panelStatus)
        {
            base.RaiseUpdateEvent(message, panelStatus);
        }

        #endregion IPackageTemplate Interface Implementtion

        private void SetConfiguredTimeout()
        {
            int MaxCrmConnectionTimeOutMinutes = _maxTimeout;
            try
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains("MaxCrmConnectionTimeOutMinutes"))
                {
                    int.TryParse(ConfigurationManager.AppSettings["MaxCrmConnectionTimeOutMinutes"], out MaxCrmConnectionTimeOutMinutes);
                }
            }
            catch (Exception)
            {
                MaxCrmConnectionTimeOutMinutes = _maxTimeout;
            }

            if (this.CrmSvc.OrganizationServiceProxy != null)
                this.CrmSvc.OrganizationServiceProxy.Timeout = TimeSpan.FromMinutes(MaxCrmConnectionTimeOutMinutes);
            else if (this.CrmSvc.OrganizationWebProxyClient != null)
                this.CrmSvc.OrganizationWebProxyClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(MaxCrmConnectionTimeOutMinutes);
        }
    }
}