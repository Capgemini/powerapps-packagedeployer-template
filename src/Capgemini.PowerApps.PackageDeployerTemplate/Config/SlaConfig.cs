namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// SLA configuration element.
    /// </summary>
    public class SlaConfig
    {
        /// <summary>
        /// Gets or sets SLA name.
        /// </summary>
        /// <value>
        /// SLA name.
        /// </value>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether the SLA should be set as default.
        /// </summary>
        /// <value>
        /// A value indicating whether whether the SLA should be set as default.
        /// </value>
        [XmlAttribute("isdefault")]
        public bool IsDefault { get; set; }
    }
}