using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    public class FlowConfig
    {
        /// <summary>
        /// Use variables from the release pipeline. If not attempts to get them from Dynamics environment variables.
        /// </summary>
        [XmlAttribute("useReleaseVariable")]
        public bool PreferReleaseVariables { get; set; }

        /// <summary>
        /// The name of the connection.
        /// </summary>
        [XmlAttribute("flowConnectionName")]
        public string FlowSharedConnectionName { get; set; }

        /// <summary>
        /// The name of the connection.
        /// </summary>
        [XmlAttribute("environmentVariableName")]
        public string EnvironmentVariableName { get; set; }

        /// <summary>
        /// If true, Activates the flow.
        /// </summary>
        [XmlAttribute("activateFlow")]
        public bool ActivateFlow { get; set; }
               
   }
}