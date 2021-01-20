namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// The root element of the ImportConfig.xml.
    /// </summary>
    [XmlRoot("configdatastorage")]
    public class ConfigDataStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigDataStorage"/> class.
        /// </summary>
        public ConfigDataStorage()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to force updates (rather than upgrades) when deploying a different solution major version.
        /// </summary>
        [XmlAttribute("useupdateformajorversions")]
        public bool UseUpdateForMajorVersions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to force updates (rather than upgrades) when deploying a different solution minor version.
        /// </summary>
        [XmlAttribute("useupdateforminorversions")]
        public bool UseUpdateForMinorVersions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to force updates (rather than upgrades) when deploying a different solution patch version.
        /// </summary>
        [XmlAttribute("useupdateforpatchversions")]
        public bool UseUpdateForPatchVersions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to activate/deactivate SLAs before/after deployment.
        /// </summary>
        [XmlAttribute("activatedeactivateslas")]
        public bool ActivateDeactivateSLAs { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of processes to deactivate after deployment.
        /// </summary>
        [XmlArray("processestodeactivate")]
        [XmlArrayItem("processtodeactivate")]
        public string[] ProcessesToDeactivate { get; set; }

        /// <summary>
        /// Gets or sets a list of processes to activate after deployment and data import.
        /// </summary>
        [XmlArray("processestoactivate")]
        [XmlArrayItem("processtoactivate")]
        public string[] ProcessesToActivate { get; set; }

        /// <summary>
        /// Gets or sets a list of SDK Message Processing Steps to deactivate after deployment.
        /// </summary>
        [XmlArray("sdkstepstodeactivate")]
        [XmlArrayItem("sdksteptodeactivate")]
        public string[] SdkStepsToDeactivate { get; set; }

        /// <summary>
        /// Gets or sets a list of SLAs to set as default.
        /// </summary>
        [XmlArray("defaultslas")]
        [XmlArrayItem("defaultsla")]
        public string[] DefaultSlas { get; set; }

        /// <summary>
        /// Gets or sets a list of word templates to deploy.
        /// </summary>
        [XmlArray("wordtemplates")]
        [XmlArrayItem("wordtemplate")]
        public string[] WordTemplates { get; set; }

        /// <summary>
        /// Gets or sets a list of data import configurations (data imported using Capgemini's data migrator tool).
        /// </summary>
        [XmlArray("dataimports")]
        [XmlArrayItem("dataimport")]
        public DataImportConfig[] DataImports { get; set; }

        /// <summary>
        /// Gets or sets a list of flows to be inactive post-deployment.
        /// </summary>
        [XmlArray("flowstodeactivate")]
        [XmlArrayItem("flowtodeactivate")]
        public string[] FlowsToDeactivate { get; set; }

        /// <summary>
        /// Load the import config from the specified path.
        /// </summary>
        /// <param name="importConfigPath">The path of the import config file.</param>
        /// <returns>A deserialized import config.</returns>
        public static ConfigDataStorage Load(string importConfigPath)
        {
            if (string.IsNullOrEmpty(importConfigPath))
            {
                throw new ArgumentException("A path must be provided.", nameof(importConfigPath));
            }

            using var fs = new FileStream(importConfigPath.ToString(), FileMode.Open);
            return (ConfigDataStorage)new XmlSerializer(typeof(ConfigDataStorage)).Deserialize(fs);
        }
    }
}
