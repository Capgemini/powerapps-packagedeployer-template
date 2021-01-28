namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Word template configuration element.
    /// </summary>
    public class DocumentTemplateConfig
    {
        /// <summary>
        /// Gets or sets path to the document template file.
        /// </summary>
        /// <value>
        /// Path to the document template file.
        /// </value>
        [XmlAttribute("path")]
        public string Path { get; set; }
    }
}