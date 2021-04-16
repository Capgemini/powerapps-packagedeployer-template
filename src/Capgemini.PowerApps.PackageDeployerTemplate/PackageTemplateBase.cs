namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// A base Package Deployer template class that provides additional configurable deployment functionality.
    /// </summary>
    public abstract class PackageTemplateBase : ImportExtension
    {
        #region private-props

        private ICrmServiceAdapter crmServiceAdapter;
        private string licensedUsername;
        private TemplateConfig templateConfig;
        private IList<string> processedSolutions;
        private TraceLoggerAdapter traceLoggerAdapter;
        private DataImporterService dataImporterSvc;
        private ProcessDeploymentService processSvc;
        private SlaDeploymentService slaDeploymentSvc;
        private DocumentTemplateDeploymentService documentTemplateSvc;
        private SdkStepDeploymentService sdkStepsSvc;
        private ConnectionReferenceDeploymentService connectionReferenceSvc;
        private TableColumnProcessingService autonumberSeedSettingSvc;
        private MailboxDeploymentService mailboxSvc;

        private EnvironmentVariableDeploymentService environmentVariableService;

        #endregion
        #region protected-props

        /// <summary>
        /// Gets a value indicating whether whether the deployment is running on Azure DevOps.
        /// </summary>
        protected static bool RunningOnAzureDevOps => bool.TryParse(Environment.GetEnvironmentVariable("TF_BUILD"), out var tfBuild) && tfBuild;

        /// <summary>
        /// Gets the path to the package folder.
        /// </summary>
        protected string PackageFolderPath => Path.Combine(this.CurrentPackageLocation, this.GetImportPackageFolderName);

        /// <summary>
        /// Gets the path to the ImportConfig.xml.
        /// </summary>
        protected string ImportConfigFilePath => Path.Combine(this.PackageFolderPath, "ImportConfig.xml");

        /// <summary>
        /// Gets the username of an (optional) licensed user to impersonate when connecting connection references or activating flows. Useful when deploying as an application user, as they can't own connections or activate flows.
        /// </summary>
        protected string LicensedUsername
        {
            get
            {
                if (string.IsNullOrEmpty(this.licensedUsername))
                {
                    this.licensedUsername = this.GetSetting<string>(Constants.Settings.LicensedUsername);
                }

                return this.licensedUsername;
            }
        }

        /// <summary>
        /// Gets the connection reference to connection name mappings.
        /// </summary>
        /// <returns>The connection reference to connection name mappings.</returns>
        protected IDictionary<string, string> ConnectionReferenceMappings => this.GetSettings(Constants.Settings.ConnectionReferencePrefix);

        /// <summary>
        /// Gets the mailbox mappings.
        /// </summary>
        /// <returns>The mailbox mappings.</returns>
        protected IDictionary<string, string> MailboxMappings => this.GetSettings(Constants.Settings.MailboxPrefix);

        /// <summary>
        /// Gets the PowerApps environment variables mappings.
        /// </summary>
        /// <returns>The Power App environment variables.</returns>
        protected IDictionary<string, string> PowerAppsEnvironmentVariables => this.GetSettings(Constants.Settings.PowerAppsEnvironmentVariablePrefix);

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
        /// Gets provides access to the templateconfig section of the ImportConfig.xml.
        /// </summary>
        protected TemplateConfig TemplateConfig
        {
            get
            {
                if (this.templateConfig == null)
                {
                    this.templateConfig = TemplateConfig.Load(this.ImportConfigFilePath);
                }

                return this.templateConfig;
            }
        }

        #endregion
        #region service-initialisers

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
                    this.crmServiceAdapter = new CrmServiceAdapter(this.CrmSvc, this.TraceLoggerAdapter);
                }

                return this.crmServiceAdapter;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to date imports.
        /// </summary>
        protected DataImporterService DataImporterService
        {
            get
            {
                if (this.dataImporterSvc == null)
                {
                    this.dataImporterSvc = new DataImporterService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.dataImporterSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to processes.
        /// </summary>
        protected ProcessDeploymentService ProcessDeploymentService
        {
            get
            {
                if (this.processSvc == null)
                {
                    this.processSvc = new ProcessDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.processSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to SLAs.
        /// </summary>
        protected SlaDeploymentService SlaDeploymentService
        {
            get
            {
                if (this.slaDeploymentSvc == null)
                {
                    this.slaDeploymentSvc = new SlaDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.slaDeploymentSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to word templates.
        /// </summary>
        protected DocumentTemplateDeploymentService DocumentTemplateSvc
        {
            get
            {
                if (this.documentTemplateSvc == null)
                {
                    this.documentTemplateSvc = new DocumentTemplateDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.documentTemplateSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to SDK steps.
        /// </summary>
        protected SdkStepDeploymentService SdkStepSvc
        {
            get
            {
                if (this.sdkStepsSvc == null)
                {
                    this.sdkStepsSvc = new SdkStepDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.sdkStepsSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to flows.
        /// </summary>
        protected ConnectionReferenceDeploymentService ConnectionReferenceSvc
        {
            get
            {
                if (this.connectionReferenceSvc == null)
                {
                    this.connectionReferenceSvc = new ConnectionReferenceDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.connectionReferenceSvc;
            }
        }

        /// <summary>
        /// Gets a service that provides functionality relating to setting autonumber seeds.
        /// </summary>
        protected TableColumnProcessingService AutonumberSeedSettingSvc
        {
            get
            {
                if (this.autonumberSeedSettingSvc == null)
                {
                    this.autonumberSeedSettingSvc = new TableColumnProcessingService(this.TraceLoggerAdapter, this.CrmServiceAdapter ?? this.CrmServiceAdapter);
                }

                return this.autonumberSeedSettingSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to mailboxes.
        /// </summary>
        protected MailboxDeploymentService MailboxSvc
        {
            get
            {
                if (this.mailboxSvc == null)
                {
                    this.mailboxSvc = new MailboxDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.mailboxSvc;
            }
        }

        /// <summary>
        /// Gets provides deployment functionality relating to environment variables.
        /// </summary>
        protected EnvironmentVariableDeploymentService EnvironmentVariablesSvc
        {
            get
            {
                if (this.environmentVariableService == null)
                {
                    this.environmentVariableService = new EnvironmentVariableDeploymentService(this.TraceLoggerAdapter, this.CrmServiceAdapter);
                }

                return this.environmentVariableService;
            }
        }

        /// <summary>
        /// Gets a <see cref="TraceLogger"/> adapter that provides additional functionality (e.g. for Azure Pipelines).
        /// </summary>
        protected TraceLoggerAdapter TraceLoggerAdapter
        {
            get
            {
                if (this.traceLoggerAdapter == null)
                {
                    this.traceLoggerAdapter = RunningOnAzureDevOps ? new AzureDevOpsTraceLoggerAdapter(this.PackageLog) : new TraceLoggerAdapter(this.PackageLog);
                }

                return this.traceLoggerAdapter;
            }
        }

        #endregion
        #region lifecycle-events

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
                if (this.TemplateConfig.ActivateDeactivateSLAs)
                {
                    this.SlaDeploymentService.DeactivateAll();
                }

                this.DataImporterService.Import(
                    this.TemplateConfig.PreDeployDataImports,
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
                if (this.TemplateConfig.ActivateDeactivateSLAs)
                {
                    this.SlaDeploymentService.ActivateAll();
                    this.SlaDeploymentService.SetDefaultSlas(this.TemplateConfig.DefaultSlas.Select(sla => sla.Name));
                }

                this.DataImporterService.Import(
                    this.TemplateConfig.PostDeployDataImports,
                    this.PackageFolderPath);

                this.EnvironmentVariablesSvc.SetEnvironmentVariables(this.PowerAppsEnvironmentVariables);

                this.SdkStepSvc.SetStatesBySolution(
                    this.ProcessedSolutions,
                    this.TemplateConfig.SdkStepsToDeactivate.Select(s => s.Name));

                if (this.TemplateConfig.SdkSteps.Any(p => p.External))
                {
                    this.SdkStepSvc.SetStates(
                        this.TemplateConfig.SdkStepsToActivate.Where(s => s.External).Select(s => s.Name),
                        this.TemplateConfig.SdkStepsToDeactivate.Where(s => s.External).Select(s => s.Name));
                }

                this.ConnectionReferenceSvc.ConnectConnectionReferences(this.ConnectionReferenceMappings, this.LicensedUsername);

                this.ProcessDeploymentService.SetStatesBySolution(
                    this.ProcessedSolutions,
                    this.TemplateConfig.ProcessesToDeactivate.Select(p => p.Name),
                    this.LicensedUsername);

                if (this.TemplateConfig.Processes.Any(p => p.External))
                {
                    this.ProcessDeploymentService.SetStates(
                        this.TemplateConfig.ProcessesToActivate.Where(p => p.External).Select(p => p.Name),
                        this.TemplateConfig.ProcessesToDeactivate.Where(p => p.External).Select(p => p.Name),
                        this.LicensedUsername);
                }

                this.DocumentTemplateSvc.Import(
                    this.TemplateConfig.DocumentTemplates.Select(d => d.Path),
                    this.PackageFolderPath);

                if (this.TemplateConfig.Tables != null && this.TemplateConfig.Tables.Any())
                {
                    this.AutonumberSeedSettingSvc.ProcessTables(this.TemplateConfig.Tables);
                }

                this.MailboxSvc.UpdateApproveAndEnableMailboxes(this.MailboxMappings);

                if (RunningOnAzureDevOps)
                {
                    this.LogTaskCompleteResult();
                }
            });

            return true;
        }

        #endregion
        #region settings-retrival

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
            this.TraceLoggerAdapter.LogDebug($"Getting {prefix} settings");

            var environmentVariables = Environment.GetEnvironmentVariables();
            var mappings = environmentVariables.Keys
                .Cast<string>()
                .Where(k => k.StartsWith($"{Constants.Settings.EnvironmentVariablePrefix}{prefix}_", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(
                    k => k.Remove(0, Constants.Settings.EnvironmentVariablePrefix.Length + prefix.Length + 1).ToLower(),
                    v => environmentVariables[v].ToString());

            this.TraceLoggerAdapter.LogDebug($"{mappings.Count} matching settings found in environment variables");

            if (this.RuntimeSettings == null)
            {
                return mappings;
            }

            var runtimeSettingMappings = this.RuntimeSettings
                .Where(s => s.Key.StartsWith($"{prefix}:"))
                .ToDictionary(s => s.Key.Remove(0, prefix.Length + 1), s => s.Value.ToString());

            this.TraceLoggerAdapter.LogDebug($"{mappings.Count} matching settings found in runtime settings");

            foreach (var runtimeSettingsMapping in runtimeSettingMappings)
            {
                if (mappings.ContainsKey(runtimeSettingsMapping.Key))
                {
                    this.TraceLoggerAdapter.LogInformation($"Overriding environment variable setting with runtime setting for {runtimeSettingsMapping.Key}.");
                }

                mappings[runtimeSettingsMapping.Key] = runtimeSettingsMapping.Value;
            }

            return mappings;
        }

        #endregion
        #region lifecycle-event-helpers

        private void ExecuteLifecycleEvent(string eventName, Action eventAction)
        {
            this.LogLifecycleEventStart(eventName);
            eventAction.Invoke();
            this.LogLifecycleEventEnd(eventName);
        }

        private void LogLifecycleEventStart(string lifecycleEvent)
        {
            this.TraceLoggerAdapter.LogInformation($"{nameof(PackageTemplateBase)}.{lifecycleEvent} running...");
        }

        private void LogLifecycleEventEnd(string lifecycleEvent)
        {
            this.TraceLoggerAdapter.LogInformation($"{nameof(PackageTemplateBase)}.{lifecycleEvent} completed.");
        }

        #endregion
        #region logging-helpers

        // Excluded as it would require our CI or PR validation pipelines to be partially succeeding or failing
        [ExcludeFromCodeCoverage]
        private void LogTaskCompleteResult()
        {
            if (this.TraceLoggerAdapter.Errors.Any())
            {
                Console.WriteLine("##vso[task.complete result=Failed;]DONE");
            }
            else if (this.TraceLoggerAdapter.Warnings.Any())
            {
                Console.WriteLine("##vso[task.complete result=SucceededWithIssues;]DONE");
            }
        }

        #endregion
    }
}
