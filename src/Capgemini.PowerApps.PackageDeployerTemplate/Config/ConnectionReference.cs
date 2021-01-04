using System.Xml.Serialization;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    public class ConnectionReference
    {
        [XmlAttribute("connectionname")]
        public string ConnectionName { get; set; }

        [XmlAttribute("connectionreferencelogicalname")]
        public string ConnectionReferenceLogicalName { get; set; }       

    }
}