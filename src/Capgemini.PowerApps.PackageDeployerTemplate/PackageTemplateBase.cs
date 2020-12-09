using System.IO;
using System.Linq;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Capgemini.PowerApps.PackageDeployerTemplate.Services;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Capgemini.PowerApps.PackageDeployerTemplate
{
    public abstract class PackageTemplateBase : ImportExtension
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S1144:Unused private types or members should be removed", 
            Justification = "Required or Polly does not get copied when referenced via project reference (e.g. in the TestPackage project)")]
        private readonly Polly.Policy _policy;

        protected string PackageFolderPath
        {
            get => Path.Combine(this.CurrentPackageLocation, GetImportPackageFolderName);
        }

        protected string ImportConfigFilePath
        {
            get => Path.Combine(this.PackageFolderPath, "ImportConfig.xml");
        }

        protected DataImporterService DataImporter { get; private set; }
        protected ProcessActivatorService ProcessActivatorService { get; private set; }
        protected SlaActivatorService SlaActivatorService { get; private set; }
        protected WordTemplateImporterService WordTemplateImporterService { get; private set; }
        protected SdkStepsActivatorService SdkStepsActivatorService { get; private set; }

        protected ConfigDataStorage ConfigDataStorage;

        public override void InitializeCustomExtension()
        {
            this.PackageLog.Log($"Initializing {nameof(PackageTemplateBase)} extension.");

            this.ConfigDataStorage = ConfigDataStorage.Load(this.ImportConfigFilePath);

            this.DataImporter = new DataImporterService(this.PackageLog, this.CrmSvc);
            this.ProcessActivatorService = new ProcessActivatorService(this.PackageLog, this.CrmSvc);
            this.SlaActivatorService = new SlaActivatorService(this.PackageLog, this.CrmSvc);
            this.WordTemplateImporterService = new WordTemplateImporterService(this.PackageLog, this.CrmSvc);
            this.SdkStepsActivatorService = new SdkStepsActivatorService(this.PackageLog, this.CrmSvc);
        }

        public override void PreSolutionImport(string solutionName, bool solutionOverwriteUnmanagedCustomizations, bool solutionPublishWorkflowsAndActivatePlugins, out bool overwriteUnmanagedCustomizations, out bool publishWorkflowsAndActivatePlugins)
        {
            if (this.ConfigDataStorage.ActivateDeactivateSLAs)
            {
                this.SlaActivatorService.Deactivate();
            }
            this.DataImporter.ImportData(this.ConfigDataStorage.DataImports?.Where(c => c.ImportBeforeSolutions), this.PackageFolderPath);

            base.PreSolutionImport(solutionName, solutionOverwriteUnmanagedCustomizations, solutionPublishWorkflowsAndActivatePlugins, out overwriteUnmanagedCustomizations, out publishWorkflowsAndActivatePlugins);
        }

        public override bool BeforeImportStage()
        {
            return true;
        }

        public override bool AfterPrimaryImport()
        {
            if(this.ConfigDataStorage.ActivateDeactivateSLAs) 
            {
                this.SlaActivatorService.Activate(this.ConfigDataStorage.DefaultSlas);
            }
            this.ProcessActivatorService.Deactivate(this.ConfigDataStorage.ProcessesToDeactivate);
            this.SdkStepsActivatorService.Deactivate(this.ConfigDataStorage.SdkStepsToDeactivate);
            this.DataImporter.ImportData(this.ConfigDataStorage.DataImports?.Where(c => !c.ImportBeforeSolutions), this.PackageFolderPath);
            this.ProcessActivatorService.Activate(this.ConfigDataStorage.ProcessesToActivate);
            this.WordTemplateImporterService.ImportWordTemplates(this.ConfigDataStorage.WordTemplates, this.PackageFolderPath);

            return true;
        }
    }
}
