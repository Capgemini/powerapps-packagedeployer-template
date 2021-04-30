namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Auto-number seed configuration element.
    /// </summary>
    public class TableConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableConfig"/> class.
        /// </summary>
        public TableConfig()
        {
            this.Columns = Array.Empty<ColumnConfig>();
        }

        /// <summary>
        /// Gets or Sets the logical name of the table we want to configure.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the logical name of the Auto-number attribute we want to set the seed for.
        /// </summary>
        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public ColumnConfig[] Columns { get; set; }
    }
}
