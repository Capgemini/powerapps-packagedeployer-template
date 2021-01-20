namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Data import configuration element.
    /// </summary>
    public class DataImportConfig
    {
        /// <summary>
        /// Gets or sets the path to the folder containing the raw data files.
        /// </summary>
        [XmlAttribute("datafolderpath")]
        public string DataFolderPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the data import configuration file.
        /// </summary>
        [XmlAttribute("importconfigpath")]
        public string ImportConfigPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to import the data before solution import.
        /// </summary>
        [XmlAttribute("importbeforesolutions")]
        public bool ImportBeforeSolutions { get; set; }
    }
}