using System.Diagnostics;
using System.IO;
using System.Linq;
using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
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

        protected DataImporterService DataImporterService { get; private set; }
        protected ProcessActivatorService ProcessActivatorService { get; private set; }
        protected SlaActivatorService SlaActivatorService { get; private set; }
        protected WordTemplateImporterService WordTemplateImporterService { get; private set; }
        protected SdkStepsActivatorService SdkStepsActivatorService { get; private set; }

        protected ConfigDataStorage ConfigDataStorage;

        public override void InitializeCustomExtension()
        {
            this.PackageLog.Log($"Initializing {nameof(PackageTemplateBase)} extension.");

            this.ConfigDataStorage = ConfigDataStorage.Load(this.ImportConfigFilePath);

            var crmServiceAdapter = new CrmServiceAdapter(this.CrmSvc);

            this.DataImporterService = new DataImporterService(this.PackageLog, crmServiceAdapter);
            this.ProcessActivatorService = new ProcessActivatorService(this.PackageLog, crmServiceAdapter);
            this.SlaActivatorService = new SlaActivatorService(this.PackageLog, crmServiceAdapter);
            this.WordTemplateImporterService = new WordTemplateImporterService(this.PackageLog, crmServiceAdapter);
            this.SdkStepsActivatorService = new SdkStepsActivatorService(this.PackageLog, crmServiceAdapter);

            this.BeforeAnything();
        }

        public void BeforeAnything()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(BeforeAnything)} running...", TraceEventType.Information);

            if (this.ConfigDataStorage.ActivateDeactivateSLAs)
            {
                this.SlaActivatorService.Deactivate();
            }
            this.DataImporterService.Import(this.ConfigDataStorage.DataImports?.Where(c => c.ImportBeforeSolutions), this.PackageFolderPath);

            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(BeforeAnything)} completed.", TraceEventType.Information);
        }

        public override bool BeforeImportStage()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(BeforeImportStage)} running...", TraceEventType.Information);
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(BeforeImportStage)} completed.", TraceEventType.Information);
            return true;
        }

        public override bool AfterPrimaryImport()
        {
            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(AfterPrimaryImport)} running...", TraceEventType.Information);
            if (this.ConfigDataStorage.ActivateDeactivateSLAs) 
            {
                this.SlaActivatorService.Activate(this.ConfigDataStorage.DefaultSlas);
            }
            this.ProcessActivatorService.Deactivate(this.ConfigDataStorage.ProcessesToDeactivate);
            this.SdkStepsActivatorService.Deactivate(this.ConfigDataStorage.SdkStepsToDeactivate);
            this.DataImporterService.Import(this.ConfigDataStorage.DataImports?.Where(c => !c.ImportBeforeSolutions), this.PackageFolderPath);
            this.ProcessActivatorService.Activate(this.ConfigDataStorage.ProcessesToActivate);
            this.WordTemplateImporterService.ImportWordTemplates(this.ConfigDataStorage.WordTemplates, this.PackageFolderPath);

            this.PackageLog.Log($"{nameof(PackageTemplateBase)}.{nameof(AfterPrimaryImport)} completed.", TraceEventType.Information);
            return true;
        }
    }
}
