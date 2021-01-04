using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    public class FlowConnection
    {
        [XmlAttribute("activate")]
        public string Activate { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}