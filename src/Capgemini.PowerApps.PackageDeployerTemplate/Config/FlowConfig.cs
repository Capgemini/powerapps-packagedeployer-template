using System;
using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    public class FlowConfig
    {
        /// <summary>
        /// The name of the connection.
        /// </summary>
        [XmlAttribute("flowConnectionName")]
        public string FlowSharedConnectionName { get; set; }

        /// <summary>
        /// If true, Activates the flow.
        /// </summary>
        [XmlAttribute("activateFlow")]
        public bool ActivateFlow { get; set; } = true;

        public string EnvironmentVariablePrefix { get; set; } = "CAPGEMINI_PACKAGEDEPLOYER_CONNECTION_{0}";
        public Guid WorkFlowId { get; internal set; }
    }
}