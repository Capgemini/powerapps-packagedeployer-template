using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    public class FlowConnection
    {      
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}