namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Process configuration element.
    /// </summary>
    public class ProcessConfig
    {
        /// <summary>
        /// Gets or sets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process is external to the current package. Deployment actions do not include external components unless this flag is set to true.
        /// </summary>
        /// <value>
        /// Whether the process is external to the current package.
        /// </value>
        /// <remarks>
        /// Avoid unless necessary. It is recommended that to minimse the side-effects of deploying your package to ensure the target environment's compatability with other packages.
        /// </remarks>
        [XmlAttribute("external")]
        public bool External { get; set; } = false;

        /// <summary>
        /// Gets or sets the desired state of the process after the deployment.
        /// </summary>
        /// <value>
        /// The desired state of the process after the deployment.
        /// </value>
        [XmlAttribute("state")]
        public DesiredState DesiredState { get; set; } = DesiredState.Active;
    }
}