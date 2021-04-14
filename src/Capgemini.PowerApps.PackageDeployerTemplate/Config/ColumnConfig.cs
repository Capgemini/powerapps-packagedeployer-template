namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Auto-number seed configuration element.
    /// </summary>
    public class ColumnConfig
    {
        /// <summary>
        /// Gets or Sets the logical name of the column we want to configure.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the value to set the Auto-number seed to. Setting this will specify this as an auto-number column.
        /// </summary>
        [XmlAttribute("autonumberseedvalue")]

        public string AutonumberSeedAsText
        {
            get { return this.AutonumberSeedValue.HasValue ? this.AutonumberSeedValue.ToString() : null; }
            set { this.AutonumberSeedValue = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }

        /// <summary>
        /// Gets or sets a nullable int version of AutonumberSeedValue.
        /// </summary>
        [XmlIgnore]
        public int? AutonumberSeedValue { get; set; }
    }
}
