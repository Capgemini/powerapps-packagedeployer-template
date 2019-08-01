using Capgemini.Xrm.Deployment.Constants;
using Capgemini.Xrm.Deployment.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Capgemini.Xrm.Deployment.Config
{
    public class PackageDeployerConfigReader : IPackageDeployerConfig
    {
        public string SolutionsFolder { get; private set; }

        public string SolutionConfigFilePath { get; private set; }

        public bool SkipCustomDeployment { get; private set; }

        public bool SkipPostDeploymentActions { get; private set; }

        public List<SolutionImportSetting> SolutionImportSettings { get; private set; }

        public string CrmMigdataImportFile { get; private set; }

        public bool DontUseHoldingSulutions { get; private set; }

        public bool UseNewApi { get; private set; }

        public bool UseAsyncImport { get; private set; }

        public bool UseAsyncUpgrade { get; private set; }

        public int AsyncTimeoutSeconds { get; private set; }

        public int AsyncSleepIntervalMiliseconds { get; private set; }

        public bool EnableSlaAfterImport { get; private set; }

        public bool DisableSlaBeforeImport { get; private set; }

        public List<string> ExcludedWorkflows { get; private set; }

        public List<Tuple<string, string>> SdkStepsToExclude { get; private set; }

        public List<string> WordTemplates { get; private set; }

        public List<string> DefaultSLANames { get; private set; }

        private readonly string _pkgFolderPath;

        private const string ConfigFileName = "ImportConfig.xml";
        
        public PackageDeployerConfigReader(string pkgFolderPath)
        {
            SolutionsFolder = pkgFolderPath;
            _pkgFolderPath = pkgFolderPath;
            SolutionImportSettings = new List<SolutionImportSetting>();
            ReadConfiguration();
        }

        private void ReadConfiguration()
        {
            SolutionConfigFilePath = Path.Combine(_pkgFolderPath, ConfigFileName);

            if (!File.Exists(SolutionConfigFilePath))
            {
                throw new Exception(CommonConstants.PackageDeployerConfigNotFound);
            }

            var doc = new XmlDocument();
            doc.Load(SolutionConfigFilePath);

            var nodeMain = doc.SelectSingleNode("configdatastorage");

            SkipCustomDeployment = ReadBoolMainSettings(nodeMain, "skipCustomDeployment");
            DontUseHoldingSulutions = ReadBoolMainSettings(nodeMain, "dontUseHoldingSolution");
            SkipPostDeploymentActions = ReadBoolMainSettings(nodeMain, "skipPostDeploymentActions");
            UseNewApi = ReadBoolMainSettings(nodeMain, "useNewApi");
            UseAsyncImport = ReadBoolMainSettings(nodeMain, "useAsyncImport");
            UseAsyncUpgrade = ReadBoolMainSettings(nodeMain, "useAsyncUpgrade");
            
            AsyncTimeoutSeconds = ReadIntMainSettings(nodeMain, "asyncTimeoutSeconds", 3600);
            AsyncSleepIntervalMiliseconds = ReadIntMainSettings(nodeMain, "asyncSleepIntervalMiliseconds", 10000);
            EnableSlaAfterImport = ReadBoolMainSettings(nodeMain, "enableSlaAfterImport");
            DisableSlaBeforeImport = ReadBoolMainSettings(nodeMain, "disableSlaBeforeImport");

            CrmMigdataImportFile = nodeMain.Attributes["crmmigdataimportfile"] != null ? nodeMain.Attributes["crmmigdataimportfile"].Value : "";

            var node = doc.SelectSingleNode("configdatastorage/solutions");
            int order = 1;

            foreach (XmlNode item in node.ChildNodes)
            {
                var solImpSet = new SolutionImportSetting
                {
                    SolutionName = item.Attributes["solutionpackagefilename"].Value,
                    InstallOrder = order,
                    DeleteOnly = ReadBoolMainSettings(item, "deleteonly"),
                    OverwriteUnmanagedCustomizations = ReadBoolMainSettings(item, "overwriteunmanagedcustomizations", true),
                    PublishWorkflows = ReadBoolMainSettings(item, "publishworkflowsandactivateplugins", true),
                    ForceUpgrade = ReadBoolMainSettings(item, "forceUpgrade"),
                    UseAsync = ReadBoolMainSettings(item, "useAsync")
                };

                SolutionImportSettings.Add(solImpSet);

                order++;
            }

            var config = XDocument.Load(SolutionConfigFilePath);

            ExcludedWorkflows = config?.Element("configdatastorage")
                ?.Element("processesToExclude")
                ?.Elements("process")
                ?.Select(a => a.Attribute("name").Value).ToList();

            SdkStepsToExclude = config?.Element("configdatastorage")
                ?.Element("SDKStepsToExclude")
                ?.Elements("SDKStep")
                ?.Select(a => new Tuple<string, string>(a.Attribute("name").Value, a.Attribute("eventHandler").Value)).ToList();

            WordTemplates = config?.Element("configdatastorage")
                  ?.Element("wordtemplatestoimport")
                  ?.Elements("wordtemplate")
                  ?.Select(a => a.Attribute("name").Value).ToList();

            string defaultSLAs = GetStringSettingFromImportConfig(nodeMain, "defaultSlaNames");
            if (defaultSLAs != null)
                DefaultSLANames = defaultSLAs.Split(',').ToList();
        }

        private static bool ReadBoolMainSettings(XmlNode node, string settingName, bool defaultValue = false)
        {
            bool boolValue = defaultValue;
            if (node.Attributes[settingName] != null)
            {
                var result = bool.TryParse(node.Attributes[settingName].Value, out boolValue);
            }
            return boolValue;
        }

        private static  int ReadIntMainSettings(XmlNode node, string settingName, int defaultValue)
        {
            int intValue = defaultValue;
            if (node.Attributes[settingName] != null)
            {
                var result = int.TryParse(node.Attributes[settingName].Value, out intValue);
            }
            return intValue;
        }

        private static string GetStringSettingFromImportConfig(XmlNode node, string settingName)
        {
            return node?.Attributes[settingName]?.Value;
        }
    }
}