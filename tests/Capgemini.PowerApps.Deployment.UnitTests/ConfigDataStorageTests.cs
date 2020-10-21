using Capgemini.PowerApps.Deployment.Config;
using Capgemini.PowerApps.Deployment.UnitTests;
using FluentAssertions;

using Xunit;

namespace Capgemini.PowerApps.Deployment.UnitTests
{
    public class ConfigDataStorageTests
    {
        private readonly ConfigDataStorage config;

        public ConfigDataStorageTests()
        {
            this.config = LoadConfig("ImportConfig.xml");
        }

        [Fact]
        public void Load_ConfigDataStoragePopulated_ConfigDataStorageIsDeserialised()
        {
            this.config.Should().NotBeNull();
        }

        [Fact]
        public void Load_ProcessesToDeactivatePopulated_ProcessesToDeactivateIsDeserialized()
        {
            this.config.ProcessesToDeactivate.Should().Contain("Case: Set Name");
        }

        [Fact]
        public void Load_SdkStepsToDeactivatePopulated_SdkStepsToDeactivateIsDeserialized()
        {
            this.config.SdkStepsToDeactivate.Should().Contain("Case: Publish Update of Case Reference to ASB");
        }

        [Fact]
        public void Load_DefaultSlasPopulated_DefaultSlasIsDeserialized()
        {
            this.config.DefaultSlas.Should().Contain("Standard Case SLA");
        }

        [Fact]
        public void Load_WordTemplatesPopulated_WordTemplatesIsDeserialized()
        {
            this.config.WordTemplates.Should().Contain("Case Survey");
        }

        [Fact]
        public void Load_DataImportsPopulated_DataImportsIsDeserialized()
        {
            var expected = new DataImportConfig
            {
                 DataFolderPath = "Data/Reference/PreDeploy/Extract",
                 ImportBeforeSolutions = true,
                 ImportConfigPath = "Data/Reference/PreDeploy/ImportConfig.json"
            };

            this.config.DataImports.Should().Contain(element =>
                element.DataFolderPath == expected.DataFolderPath &&
                element.ImportBeforeSolutions == expected.ImportBeforeSolutions &&
                element.ImportConfigPath == expected.ImportConfigPath);
        }

        [Fact]
        public void Load_ActivateDeactivateSLAsPopulated_ActivateDeactivateSLAsDeserialized()
        {
            this.config.ActivateDeactivateSLAs.Should().BeFalse();
        }

        private ConfigDataStorage LoadConfig(string path)
        {
            return ConfigDataStorage.Load(TestUtilities.GetResourcePath(path));
        }
    }
}
