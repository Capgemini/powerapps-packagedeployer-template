namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// The root element of the ImportConfig.xml.
    /// </summary>
    [XmlRoot("configdatastorage")]
    public class ConfigDataStorage
    {
        /// <summary>
        /// Gets or sets the configuration used by <see cref="PackageTemplateBase"/>.
        /// </summary>
        /// <value>
        /// The configuration used by <see cref="PackageTemplateBase"/>.
        /// </value>
        [XmlElement("templateconfig")]
        public TemplateConfig TemplateConfig { get; set; }
    }
}
