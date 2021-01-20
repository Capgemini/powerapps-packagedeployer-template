namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using Capgemini.PowerApps.PackageDeployerTemplate.Services;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// A base Package Deployer template class that provides additional configurable deployment functionality.
    /// </summary>
    public abstract class PackageTemplateBase : ImportExtension
    {
        /// <summary>
        /// Gets the path to the package folder.
        /// </summary>
        protected string PackageFolderPath
        {
            get => Path.Combine(this.CurrentPackageLocation, this.GetImportPackageFolderName);
        }

        /// <summary>
        /// Gets the path to the ImportConfig.xml.
        /// </summary>
        protected string ImportConfigFilePath
        {
            get => Path.Combine(this.PackageFolderPath, "ImportConfig.xml");
        }

        /// <summary>
        /// Gets a list of solutions that have been processed (i.e. <see cref="PreSolutionImport(string, bool, bool, out bool, out bool)"/> has been ran for that solution.)
        /// </summary>
        protected List<string> ProcessedSolutions { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to date imports.
        /// </summary>
        protected DataImporterService DataImporterService { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to processes.
        /// </summary>
        protected ProcessDeploymentService ProcessDeploymentService { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to SLAs.
        /// </summary>
        protected SlaDeploymentService SlaDeploymentService { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to word templates.
        /// </summary>
        protected WordTemplateImporterService WordTemplateImporterService { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to SDK steps.
        /// </summary>
        protected SdkStepDeploymentService SdkStepsDeploymentService { get; private set; }

        /// <summary>
        /// Gets provides deployment functionality relating to flows.
        /// </summary>
        protected FlowActivationService FlowActivationService { get; private set; }

        /// <summary>
        /// Gets provides access to the configdatastorage section of the ImportConfig.xml.
        /// </summary>
        protected ConfigDataStorage ConfigDataStorage { get; private set; }

        /// <inheritdoc/>
        public override void InitializeCustomExtension()
        {
            this.ProcessedSolutions = new List<string>();

            this.PackageLog.Log($"Initializing {nameof(PackageTemplateBase)} extension.");

            this.ConfigDataStorage = ConfigDataStorage.Load(this.ImportConfigFilePath);

            var crmServiceAdapter = new CrmServiceAdapter(this.CrmSvc);
            var logger = new TraceLoggerAdapter(this.PackageLog);

            this.DataImporterService = new DataImporterService(logger, crmServiceAdapter);
            this.ProcessDeploymentService = new ProcessDeploymentService(logger, crmServiceAdapter);
            this.SlaDeploymentService = new SlaDeploymentService(logger, crmServiceAdapter);
            this.WordTemplateImporterService = new WordTemplateImporterService(logger, crmServiceAdapter);
            this.SdkStepsDeploymentService = new SdkStepDeploymentService(logger, crmServiceAdapter);
            this.FlowActivationService = new FlowActivationService(logger, crmServiceAdapter);

            this.BeforeAnything();
        }

        /// <summary>
        /// Called after plugin initialization but before any solution deployment.
        /// </summary>
        public virtual void BeforeAnything()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.BeforeAnything)} running...", TraceEventType.Information);

            if (this.ConfigDataStorage.ActivateDeactivateSLAs)
            {
                this.SlaDeploymentService.DeactivateAll();
            }

            this.DataImporterService.Import(this.ConfigDataStorage.DataImports?.Where(c => c.ImportBeforeSolutions), this.PackageFolderPath);

            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.BeforeAnything)} completed.", TraceEventType.Information);
        }

        /// <inheritdoc/>
        public override void PreSolutionImport(string solutionName, bool solutionOverwriteUnmanagedCustomizations, bool solutionPublishWorkflowsAndActivatePlugins, out bool overwriteUnmanagedCustomizations, out bool publishWorkflowsAndActivatePlugins)
        {
            this.ProcessedSolutions.Add(solutionName);
            base.PreSolutionImport(solutionName, solutionOverwriteUnmanagedCustomizations, solutionPublishWorkflowsAndActivatePlugins, out overwriteUnmanagedCustomizations, out publishWorkflowsAndActivatePlugins);
        }

        /// <inheritdoc/>
        public override bool BeforeImportStage()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.BeforeImportStage)} running...", TraceEventType.Information);
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.BeforeImportStage)} completed.", TraceEventType.Information);
            return true;
        }

        /// <inheritdoc/>
        public override bool AfterPrimaryImport()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.AfterPrimaryImport)} running...", TraceEventType.Information);
            if (this.ConfigDataStorage.ActivateDeactivateSLAs)
            {
                this.SlaDeploymentService.ActivateAll();
                this.SlaDeploymentService.SetDefaultSlas(this.ConfigDataStorage.DefaultSlas);
            }

            this.ProcessDeploymentService.Deactivate(this.ConfigDataStorage.ProcessesToDeactivate);
            this.SdkStepsDeploymentService.Deactivate(this.ConfigDataStorage.SdkStepsToDeactivate);
            this.DataImporterService.Import(this.ConfigDataStorage.DataImports?.Where(c => !c.ImportBeforeSolutions), this.PackageFolderPath);
            this.ProcessDeploymentService.Activate(this.ConfigDataStorage.ProcessesToActivate);
            this.WordTemplateImporterService.ImportWordTemplates(this.ConfigDataStorage.WordTemplates, this.PackageFolderPath);
            this.FlowActivationService.ActivateFlows(this.ConfigDataStorage.FlowsToDeactivate, this.ProcessedSolutions);

            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(this.AfterPrimaryImport)} completed.", TraceEventType.Information);
            return true;
        }
    }
}
