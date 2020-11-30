using System.Xml.Serialization;

namespace Capgemini.PowerApps.Deployment.Config
{
    public class DataImportConfig
    {
        /// <summary>
        /// The path to the folder containing the raw data files.
        /// </summary>
        [XmlAttribute("datafolderpath")]
        public string DataFolderPath { get; set; }

        /// <summary>
        /// The path to the data import configuration file.
        /// </summary>
        [XmlAttribute("importconfigpath")]
        public string ImportConfigPath { get; set; }

        /// <summary>
        /// Whether to import the data before solution import.
        /// </summary>
        [XmlAttribute("importbeforesolutions")]
        public bool ImportBeforeSolutions { get; set; }
    }
}