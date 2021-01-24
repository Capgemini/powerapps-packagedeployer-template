namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// A base Package Deployer template class that provides additional configurable deployment functionality.
    /// </summary>
    public abstract class PackageTemplateBase : ImportExtension
    {
        private ICrmServiceAdapter crmServiceAdapter;
        private ICrmServiceAdapter licensedCrmServiceAdapter;
        private ConfigDataStorage configDataStorage;
        private IList<string> processedSolutions;
        private TraceLoggerAdapter traceLoggerAdapter;
        private DataImporterService dataImporterService;
        private ProcessDeploymentService processDeploymentService;
        private SlaDeploymentService slaDeploymentService;
        private WordTemplateImporterService wordTemplateImporterService;
        private SdkStepDeploymentService sdkStepsDeploymentService;
        private FlowDeploymentService flowDeploymentService;

        /// <summary>
        /// Gets the path to the package folder.
        /// </summary>
        protected string PackageFolderPath => Path.Combine(this.CurrentPackageLocation, this.GetImportPackageFolderName);

        /// <summary>
        /// Gets the path to the ImportConfig.xml.
        /// </summary>
        protected string ImportConfigFilePath => Path.Combine(this.PackageFolderPath, "ImportConfig.xml");

        /// <summary>
        /// Gets the connection reference to connection name mappings.
        /// </summary>
        /// <returns>The connection reference to connection name mappings.</returns>
        protected IDictionary<string, string> ConnectionReferenceMappings => this.GetSettings(Constants.Settings.ConnectionReferencePrefix);

        /// <summary>
        /// Gets an extended <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/>.
        /// </summary>
        /// <value>
        /// An extended <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/>.
        /// </value>
        protected ICrmServiceAdapter CrmServiceAdapter
        {
            get
            {
                if (this.crmServiceAdapter == null)
                {
                    this.crmServiceAdapter = new CrmServiceAdapter(this.CrmSvc);
                }

                return this.crmServiceAdapter;
            }
        }

        /// <summary>
        /// Gets an extended <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/> authenticated as a licensed user (if configured).
        /// </summary>
        /// <value>
        /// An extended <see cref="Microsoft.Xrm.Sdk.IOrganizationService"/> authenticated as a licensed user (if configured).
        /// </value>
        protected ICrmServiceAdapter LicensedCrmServiceAdapter
        {
            get
            {
                if (this.licensedCrmServiceAdapter == null)
                {
                    var username = this.GetSetting<string>(Constants.Settings.LicensedUsername);
                    var password = this.GetSetting<string>(Constants.Settings.LicensedPassword);

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        this.licensedCrmServiceAdapter = new CrmServiceAdapter(
                            new CrmServiceClient(
                                $"AuthType=OAuth; Username={username}; Password={password}; Url={this.CrmSvc.ConnectedOrgPublishedEndpoints.First().Value}; AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97; LoginPrompt=Never"));
                    }
                    else
                    {
                        this.PackageLog.Log("Attempted to establish a licensed connection when no licensed credentials were configured", TraceEventType.Warning);
                    }
                }

                return this.licensedCrmServiceAdapter;
            }
        }

        /// <summary>
        /// Gets a list of solutions that have been processed (i.e. <see cref="PreSolutionImport(string, bool, bool, out bool, out bool)"/> has been ran for that solution.)
        /// </summary>
        protected IList<string> ProcessedSolutions
        {
            get
            {
                if (this.processedSolutions == null)
                {
                    this.processedSolutions = new List<string>();
                }

                return this.processedSolutions;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to date imports.
        /// </summary>
        protected DataImporterService DataImporterService
        {
            get
            {
                if (this.dataImporterService == null)
                {
                    this.dataImporterService = new DataImporterService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.dataImporterService;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to processes.
        /// </summary>
        protected ProcessDeploymentService ProcessDeploymentService
        {
            get
            {
                if (this.processDeploymentService == null)
                {
                    this.processDeploymentService = new ProcessDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.processDeploymentService;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to SLAs.
        /// </summary>
        protected SlaDeploymentService SlaDeploymentService
        {
            get
            {
                if (this.slaDeploymentService == null)
                {
                    this.slaDeploymentService = new SlaDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.slaDeploymentService;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to word templates.
        /// </summary>
        protected WordTemplateImporterService WordTemplateImporterService
        {
            get
            {
                if (this.wordTemplateImporterService == null)
                {
                    this.wordTemplateImporterService = new WordTemplateImporterService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.wordTemplateImporterService;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to SDK steps.
        /// </summary>
        protected SdkStepDeploymentService SdkStepDeploymentService
        {
            get
            {
                if (this.sdkStepsDeploymentService == null)
                {
                    this.sdkStepsDeploymentService = new SdkStepDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.sdkStepsDeploymentService;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to flows.
        /// </summary>
        protected FlowDeploymentService FlowDeploymentService
        {
            get
            {
                if (this.flowDeploymentService == null)
                {
                    this.flowDeploymentService = new FlowDeploymentService(this.TraceLoggerAdapter, this.LicensedCrmServiceAdapter ?? this.CrmServiceAdapter);
                }

                return this.flowDeploymentService;
            }
        }

        /// <summary>
        /// Gets provides access to the configdatastorage section of the ImportConfig.xml.
        /// </summary>
        protected ConfigDataStorage ConfigDataStorage
        {
            get
            {
                if (this.configDataStorage == null)
                {
                    this.configDataStorage = ConfigDataStorage.Load(this.ImportConfigFilePath);
                }

                return this.configDataStorage;
            }
        }

        private TraceLoggerAdapter TraceLoggerAdapter
        {
            get
            {
                if (this.traceLoggerAdapter == null)
                {
                    this.traceLoggerAdapter = new TraceLoggerAdapter(this.PackageLog);
                }

                return this.traceLoggerAdapter;
            }
        }

        /// <inheritdoc/>
        public override void InitializeCustomExtension()
        {
            this.ExecuteLifecycleEvent(nameof(this.InitializeCustomExtension), () => { });

            this.BeforePrimaryImport();
        }

        /// <summary>
        /// Called after plugin initialization but before any solution or data deployment.
        /// </summary>
        public virtual void BeforePrimaryImport()
        {
            this.ExecuteLifecycleEvent(nameof(this.BeforePrimaryImport), () =>
            {
                if (this.ConfigDataStorage.ActivateDeactivateSLAs)
                {
                    this.SlaDeploymentService.DeactivateAll();
                }

                this.DataImporterService.Import(
                    this.ConfigDataStorage.DataImports?.Where(c => c.ImportBeforeSolutions),
                    this.PackageFolderPath);
            });
        }

        /// <inheritdoc/>
        public override void PreSolutionImport(string solutionName, bool solutionOverwriteUnmanagedCustomizations, bool solutionPublishWorkflowsAndActivatePlugins, out bool overwriteUnmanagedCustomizations, out bool publishWorkflowsAndActivatePlugins)
        {
            base.PreSolutionImport(solutionName, solutionOverwriteUnmanagedCustomizations, solutionPublishWorkflowsAndActivatePlugins, out overwriteUnmanagedCustomizations, out publishWorkflowsAndActivatePlugins);

            this.ExecuteLifecycleEvent(nameof(this.PreSolutionImport), () =>
            {
                this.ProcessedSolutions.Add(solutionName);
            });
        }

        /// <inheritdoc/>
        public override bool BeforeImportStage()
        {
            this.ExecuteLifecycleEvent(nameof(this.BeforeImportStage), () => { });

            return true;
        }

        /// <inheritdoc/>
        public override bool AfterPrimaryImport()
        {
            this.ExecuteLifecycleEvent(nameof(this.AfterPrimaryImport), () =>
            {
                if (this.ConfigDataStorage.ActivateDeactivateSLAs)
                {
                    this.SlaDeploymentService.ActivateAll();
                    this.SlaDeploymentService.SetDefaultSlas(this.ConfigDataStorage.DefaultSlas);
                }

                this.ProcessDeploymentService.Deactivate(this.ConfigDataStorage.ProcessesToDeactivate);
                this.SdkStepDeploymentService.Deactivate(this.ConfigDataStorage.SdkStepsToDeactivate);
                this.DataImporterService.Import(this.ConfigDataStorage.DataImports?.Where(c => !c.ImportBeforeSolutions), this.PackageFolderPath);
                this.ProcessDeploymentService.Activate(this.ConfigDataStorage.ProcessesToActivate);
                this.WordTemplateImporterService.ImportWordTemplates(this.ConfigDataStorage.WordTemplates, this.PackageFolderPath);
                this.FlowDeploymentService.ConnectConnectionReferences(this.ConnectionReferenceMappings);
                this.FlowDeploymentService.ActivateFlows(this.ProcessedSolutions, this.ConfigDataStorage.FlowsToDeactivate);
            });

            return true;
        }

        /// <summary>
        /// Gets a setting either from runtime arguments or an environment variable (in that order of preference). Environment variables should be prefixed with 'PACKAGEDEPLOYER_SETTINGS_'.
        /// </summary>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The setting value (if found).</returns>
        protected T GetSetting<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be empty", nameof(key));
            }

            key = key.Trim();
            string value = null;

            if (this.RuntimeSettings != null && this.RuntimeSettings.ContainsKey(key))
            {
                var obj = this.RuntimeSettings[key];

                if (obj is T t)
                {
                    return t;
                }
                else if (obj is string s)
                {
                    value = s;
                }
            }

            if (value == null)
            {
                value = Environment.GetEnvironmentVariable($"PACKAGEDEPLOYER_SETTINGS_{key.ToUpperInvariant()}");
            }

            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }

            return default;
        }

        /// <summary>
        /// Gets a collection of settings either from runtime arguments or environment variable (in that order of preference) with a common prefix.
        /// </summary>
        /// <param name="prefix">The common prefix.</param>
        /// <returns>The setting value (if found).</returns>
        protected IDictionary<string, string> GetSettings(string prefix)
        {
            this.PackageLog.Log($"Getting {prefix} settings", TraceEventType.Verbose);

            var environmentVariables = Environment.GetEnvironmentVariables();
            var mappings = environmentVariables.Keys
                .Cast<string>()
                .Where(k => k.StartsWith($"{Constants.Settings.EnvironmentVariablePrefix}{prefix}_", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    k => k.Remove(0, Constants.Settings.EnvironmentVariablePrefix.Length + prefix.Length + 1).ToLower(),
                    v => environmentVariables[v].ToString());

            this.PackageLog.Log($"{mappings.Count} matching settings found in environment variables", TraceEventType.Verbose);

            if (this.RuntimeSettings == null)
            {
                return mappings;
            }

            var runtimeSettingMappings = this.RuntimeSettings
                .Where(s => s.Key.StartsWith($"{prefix}:"))
                .ToDictionary(s => s.Key.Remove(0, prefix.Length + 1), s => s.Value.ToString());

            this.PackageLog.Log($"{mappings.Count} matching settings found in runtime settings", TraceEventType.Verbose);

            foreach (var runtimeSettingsMapping in runtimeSettingMappings)
            {
                if (mappings.ContainsKey(runtimeSettingsMapping.Key))
                {
                    this.PackageLog.Log($"Overriding environment variable setting with runtime setting for {runtimeSettingsMapping.Key}.");
                }

                mappings[runtimeSettingsMapping.Key] = runtimeSettingsMapping.Value;
            }

            return mappings;
        }

        private void ExecuteLifecycleEvent(string eventName, Action eventAction)
        {
            this.LogLifecycleEventStart(eventName);
            eventAction.Invoke();
            this.LogLifecycleEventEnd(eventName);
        }

        private void LogLifecycleEventStart(string lifecycleEvent)
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{lifecycleEvent} running...", TraceEventType.Information);
        }

        private void LogLifecycleEventEnd(string lifecycleEvent)
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{lifecycleEvent} completed.", TraceEventType.Information);
        }
    }
}
