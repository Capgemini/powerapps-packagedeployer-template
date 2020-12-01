using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Capgemini.DataMigration.Resiliency.Polly;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
using Capgemini.Xrm.DataMigration.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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

        protected PackageTemplateBase()
        {
        }

        protected string ImportConfigFilePath
        {
            get
            {
                return Path.Combine(this.CurrentPackageLocation, GetImportPackageFolderName, "ImportConfig.xml");
            }
        }

        protected DataImporter DataImporter { get; private set; }

        protected ConfigDataStorage ConfigDataStorage;

        public override bool AfterPrimaryImport()
        {
            if(this.ConfigDataStorage.ActivateDeactivateSLAs) 
            {
                this.ActivateSlas(this.ConfigDataStorage.DefaultSlas);
            }
            this.DeactivateProcesses(this.ConfigDataStorage.ProcessesToDeactivate);
            this.DeactivateSdkMessageProcessingSteps(this.ConfigDataStorage.SdkStepsToDeactivate);
            this.ImportData(this.ConfigDataStorage.DataImports?.Where(c => !c.ImportBeforeSolutions));
            this.ActivateProcesses(this.ConfigDataStorage.ProcessesToActivate);
            this.ImportWordTemplates(this.ConfigDataStorage.WordTemplates);

            return true;
        }

        public override UserRequestedImportAction OverrideSolutionImportDecision(string solutionUniqueName, Version organizationVersion, Version packageSolutionVersion, Version inboundSolutionVersion, Version deployedSolutionVersion, ImportAction systemSelectedImportAction)
        {
            if (systemSelectedImportAction != ImportAction.Import)
            {
                return UserRequestedImportAction.Default;
            }

            if (packageSolutionVersion.Major > organizationVersion.Major)
            {
                return this.ConfigDataStorage.UseUpdateForMajorVersions ? UserRequestedImportAction.ForceUpdate : UserRequestedImportAction.Default;
            }
            else if (packageSolutionVersion.Minor > organizationVersion.Minor)
            {
                return this.ConfigDataStorage.UseUpdateForMinorVersions ? UserRequestedImportAction.ForceUpdate : UserRequestedImportAction.Default;
            }
            else if (packageSolutionVersion.Revision > organizationVersion.Revision)
            {
                return this.ConfigDataStorage.UseUpdateForPatchVersions ? UserRequestedImportAction.ForceUpdate : UserRequestedImportAction.Default;
            }

            return base.OverrideSolutionImportDecision(solutionUniqueName, organizationVersion, packageSolutionVersion, inboundSolutionVersion, deployedSolutionVersion, systemSelectedImportAction);
        }

        public override bool BeforeImportStage()
        {
            return true;
        }

        private void ImportWordTemplates(IEnumerable<string> wordTemplates)
        {
            foreach (var wordTemplate in wordTemplates)
            {
                this.CrmSvc.ImportWordTemplate(
                    Path.Combine(this.CurrentPackageLocation, this.GetImportPackageDataFolderName, wordTemplate));
            }
        }

        private void ActivateSlas(IEnumerable<string> defaultSlas = null)
        {
            var executeMultipleResponse = this.CrmSvc.SetRecordStateByAttribute("sla", 1, 2, "statecode", new object[] { 0 });
            if (executeMultipleResponse.IsFaulted)
            {
                this.PackageLog.Log($"Error activating SLAs.", TraceEventType.Error);
                this.PackageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }

            if (defaultSlas == null || defaultSlas?.Count() == 0)
            {
                return;
            }

            var defaultSlasQuery = new QueryByAttribute("sla") { ColumnSet = new ColumnSet(false) };
            foreach (var sla in defaultSlas)
            {
                defaultSlasQuery.AddAttributeValue("name", sla);
            }
            var retrieveMultipleResponse = this.CrmSvc.RetrieveMultiple(defaultSlasQuery);

            foreach (var defaultSla in retrieveMultipleResponse.Entities)
            {
                defaultSla["isdefault"] = true;
                this.CrmSvc.Update(defaultSla);
            }
        }

        private void DeactivateSdkMessageProcessingSteps(IEnumerable<string> sdkStepsToDeactivate)
        {
            if (sdkStepsToDeactivate is null || !sdkStepsToDeactivate.Any())
            {
                this.PackageLog.Log("No SDK steps to deactivate have been configured.");
                return;
            }

            var executeMultipleResponse = this.CrmSvc.SetRecordStateByAttribute("sdkmessageprocessingstep", 1, 2, "name", sdkStepsToDeactivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.PackageLog.Log($"Error deactivating SDK Message Processing Steps.", TraceEventType.Error);
                this.PackageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        private void DeactivateProcesses(IEnumerable<string> processesToDeactivate)
        {
            if (processesToDeactivate is null || !processesToDeactivate.Any())
            {
                this.PackageLog.Log("No processes to deactivate have been configured.");
                return;
            }

            var executeMultipleResponse = this.CrmSvc.SetRecordStateByAttribute("workflow", 0, 1, "name", processesToDeactivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.PackageLog.Log($"Error deactivating processes.", TraceEventType.Error);
                this.PackageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        private void ActivateProcesses(IEnumerable<string> processesToActivate)
        {
            if (processesToActivate is null || !processesToActivate.Any())
            {
                this.PackageLog.Log("No processes to activate have been configured.");
                return;
            }

            var executeMultipleResponse = this.CrmSvc.SetRecordStateByAttribute("workflow", 1, 2, "name", processesToActivate);
            if (executeMultipleResponse.IsFaulted)
            {
                this.PackageLog.Log($"Error activating processes.", TraceEventType.Error);
                this.PackageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        private void DeactivateSlas()
        {
            var executeMultipleResponse = this.CrmSvc.SetRecordStateByAttribute("sla", 0, 1, "statecode", new object[] { 1 });
            if (executeMultipleResponse.IsFaulted)
            {
                this.PackageLog.Log($"Error deactivating SLAs.", TraceEventType.Error);
                this.PackageLog.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }

        public override void InitializeCustomExtension()
        {
            this.PackageLog.Log($"Initializing {nameof(PackageTemplateBase)} extension.");

            this.ConfigDataStorage = ConfigDataStorage.Load(this.ImportConfigFilePath);

            this.DataImporter = new DataImporter(
                this.PackageLog,
                new EntityRepository(
                    (IOrganizationService)this.CrmSvc.OrganizationWebProxyClient ?? CrmSvc.OrganizationServiceProxy,
                    new ServiceRetryExecutor()));

            // Previously in the BeforeImportStage method but this does not run before solution import.
            if(this.ConfigDataStorage.ActivateDeactivateSLAs)
            {
                this.DeactivateSlas();
            }
            this.ImportData(this.ConfigDataStorage.DataImports?.Where(c => c.ImportBeforeSolutions));
        }

        private void ImportData(IEnumerable<DataImportConfig> dataImportConfigs)
        {
            if (dataImportConfigs is null || !dataImportConfigs.Any())
            {
                this.PackageLog.Log("No imports have been configured.");

                return;
            }

            foreach (var dataImportConfig in dataImportConfigs)
            {
                this.DataImporter.Import(dataImportConfig, Path.Combine(this.CurrentPackageLocation, this.GetImportPackageDataFolderName));
            }
        }
    }
}
