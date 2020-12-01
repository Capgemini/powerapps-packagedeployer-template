using System;
using System.IO;
using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    [XmlRoot("configdatastorage")]
    public class ConfigDataStorage
    {
        public ConfigDataStorage()
        {
        }

        /// <summary>
        /// Whether to force updates (rather than upgrades) when deploying a different solution major version.
        /// </summary>
        [XmlAttribute("useupdateformajorversions")]
        public bool UseUpdateForMajorVersions { get; set; }

        /// <summary>
        /// Whether to force updates (rather than upgrades) when deploying a different solution minor version.
        /// </summary>
        [XmlAttribute("useupdateforminorversions")]
        public bool UseUpdateForMinorVersions { get; set; }

        /// <summary>
        /// Whether to force updates (rather than upgrades) when deploying a different solution patch version.
        /// </summary>
        [XmlAttribute("useupdateforpatchversions")]
        public bool UseUpdateForPatchVersions { get; set; }

        /// <summary>
        /// Whether to activate/deactivate SLAs before/after deployment.
        /// </summary>
        [XmlAttribute("activatedeactivateslas")]
        public bool ActivateDeactivateSLAs { get; set; } = true;

        /// <summary>
        /// A list of processes to deactivate after deployment.
        /// </summary>
        [XmlArray("processestodeactivate")]
        [XmlArrayItem("processtodeactivate")]
        public string[] ProcessesToDeactivate { get; set; }

        /// <summary>
        /// A list of processes to activate after deployment and data import.
        /// </summary>
        [XmlArray("processestoactivate")]
        [XmlArrayItem("processtoactivate")]
        public string[] ProcessesToActivate { get; set; }

        /// <summary>
        /// A list of SDK Message Processing Steps to deactivate after deployment.
        /// </summary>
        [XmlArray("sdkstepstodeactivate")]
        [XmlArrayItem("sdksteptodeactivate")]
        public string[] SdkStepsToDeactivate { get; set; }

        /// <summary>
        /// A list of SLAs to set as default
        /// </summary>
        [XmlArray("defaultslas")]
        [XmlArrayItem("defaultsla")]
        public string[] DefaultSlas { get; set; }

        /// <summary>
        /// A list of word templates to deploy.
        /// </summary>
        [XmlArray("wordtemplates")]
        [XmlArrayItem("wordtemplate")]
        public string[] WordTemplates { get; set; }

        /// <summary>
        /// A list of data import configurations (data imported using Capgemini's data migrator tool).
        /// </summary>
        [XmlArray("dataimports")]
        [XmlArrayItem("dataimport")]
        public DataImportConfig[] DataImports { get; set; }

        /// <summary>
        /// Load an <see cref="ImportConfig"/> from the specified path.
        /// </summary>
        /// <param name="importConfigPath">The path of the import config file.</param>
        /// <returns>A deserialized <see cref="ImportConfig"/></returns>
        public static ConfigDataStorage Load(string importConfigPath)
        {
            if (string.IsNullOrEmpty(importConfigPath))
            {
                throw new ArgumentException("A path must be provided.", nameof(importConfigPath));
            }

            using (var fs = new FileStream(importConfigPath.ToString(), FileMode.Open))
            {
                return (ConfigDataStorage)new XmlSerializer(typeof(ConfigDataStorage)).Deserialize(fs);
            }
        }
    }
}
