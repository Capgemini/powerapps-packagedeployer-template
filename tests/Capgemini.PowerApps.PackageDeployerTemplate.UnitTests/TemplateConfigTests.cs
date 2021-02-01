namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests
{
    using Capgemini.PowerApps.PackageDeployerTemplate.Config;
    using FluentAssertions;
    using Xunit;

    public class TemplateConfigTests
    {
        private readonly TemplateConfig config;
        private readonly TemplateConfig defaultConfig;

        public TemplateConfigTests()
        {
            this.config = Load("ImportConfig.xml");
            this.defaultConfig = Load("EmptyImportConfig.xml");
        }

        [Fact]
        public void Load_TemplateConfigElementPresent_TemplateConfigIsDeserialised()
        {
            this.config.Should().NotBeNull();
        }

        [Fact]
        public void Load_TemplateConfigElementNotPresent_TemplateConfigIsDefaulted()
        {
            this.defaultConfig.Should().NotBeNull();
        }

        [Fact]
        public void Load_ProcessesElementNotPresent_ProcessesIsDefaulted()
        {
            this.defaultConfig.Processes.Should().NotBeNull();
        }

        [Fact]
        public void Load_Process_ReturnedInProcessesToDeactivate()
        {
            this.config.ProcessesToDeactivate.Should().Contain(p => p.Name == "This process should be deactivated");
        }

        [Fact]
        public void Load_ProcessWithStateAttributeSetToInactive_ReturnedInProcessesToDeactivate()
        {
            this.config.ProcessesToDeactivate.Should().Contain(p => p.Name == "This process should be deactivated");
        }

        [Fact]
        public void Load_ProcessWithStateAttributeNotSet_DefaultsToActive()
        {
            this.config.ProcessesToActivate.Should().Contain(p => p.Name == "This process should also be activated");
        }

        [Fact]
        public void Load_ProcessWithStateAttributeSetToActive_ReturnedInProcessesToActivate()
        {
            this.config.ProcessesToActivate.Should().Contain(p => p.Name == "This process should be activated");
        }

        [Fact]
        public void Load_SdkStepsElementNotPresent_SdkStepsIsDefaulted()
        {
            this.defaultConfig.SdkSteps.Should().NotBeNull();
        }

        [Fact]
        public void Load_SdkStep_ReturnedInSdkStepsToDeactivate()
        {
            this.config.SdkStepsToDeactivate.Should().Contain(p => p.Name == "This SDK step should be deactivated");
        }

        [Fact]
        public void Load_SdkStepWithStateAttributeSetToInactive_ReturnedInSdkStepesToDeactivate()
        {
            this.config.SdkStepsToDeactivate.Should().Contain(p => p.Name == "This SDK step should be deactivated");
        }

        [Fact]
        public void Load_SdkStepWithStateAttributeNotSet_DefaultsToActive()
        {
            this.config.SdkStepsToActivate.Should().Contain(p => p.Name == "This SDK step should also be activated");
        }

        [Fact]
        public void Load_SdkStepWithStateAttributeSetToActive_ReturnedInSdkStepesToActivate()
        {
            this.config.SdkStepsToActivate.Should().Contain(p => p.Name == "This SDK step should be activated");
        }

        [Fact]
        public void Load_SlasElementNotPresent_SlasIsDefaulted()
        {
            this.defaultConfig.Slas.Should().NotBeNull();
        }

        [Fact]
        public void Load_SlaIsPresent_SlaIsDeserialized()
        {
            this.config.Slas.Should().Contain(sla => sla.Name == "Standard Case SLA" && sla.IsDefault == true);
        }

        [Fact]
        public void Load_SlaHasDefaultSetToTrue_IsReturnedByDefaultSlas()
        {
            this.config.DefaultSlas.Should().Contain(sla => sla.Name == "Standard Case SLA");
        }

        [Fact]
        public void Load_DocumentTemplatesElementNotPresent_DocumentTemplatesIsDefaulted()
        {
            this.defaultConfig.DocumentTemplates.Should().NotBeNull();
        }

        [Fact]
        public void Load_DocumentTemplatesPopulated_DocumentTemplatesIsDeserialized()
        {
            this.config.DocumentTemplates.Should().Contain(d => d.Path == "Word Document.docx");
        }

        [Fact]
        public void Load_DataImportsElementNotPresent_DataImportsIsDefaulted()
        {
            this.defaultConfig.DataImports.Should().NotBeNull();
        }

        [Fact]
        public void Load_DataImportsPopulated_DataImportsIsDeserialized()
        {
            this.config.DataImports.Should().ContainEquivalentOf(new DataImportConfig
            {
                DataFolderPath = "Data/Reference/PreDeploy/Extract",
                ImportBeforeSolutions = true,
                ImportConfigPath = "Data/Reference/PreDeploy/ImportConfig.json",
            });
        }

        [Fact]
        public void Load_DataImportHasImportBeforeSolutionsSetToTrue_IsReturnedByPreDeployDataImports()
        {
            this.config.PreDeployDataImports.Should().Contain(p => p.DataFolderPath == "Data/Reference/PreDeploy/Extract");
        }

        [Fact]
        public void Load_DataImportHasImportBeforeSolutionsSetToFalse_IsReturnedByPostDeployDataImports()
        {
            this.config.PostDeployDataImports.Should().Contain(p => p.DataFolderPath == "Data/Reference/PostDeploy/Extract");
        }

        [Fact]
        public void Load_DataImportHasImportBeforeSolutionsNotSet_IsReturnedByPostDeployDataImports()
        {
            this.config.PostDeployDataImports.Should().Contain(p => p.DataFolderPath == "Data/Reference/DefaultedPostDeploy/Extract");
        }

        [Fact]
        public void Load_ActivateDeactivateSlasPopulated_ActivateDeactivateSlasDeserialized()
        {
            this.config.ActivateDeactivateSLAs.Should().BeTrue();
        }

        private static TemplateConfig Load(string path)
        {
            return TemplateConfig.Load(TestUtilities.GetResourcePath(path));
        }
    }
}
