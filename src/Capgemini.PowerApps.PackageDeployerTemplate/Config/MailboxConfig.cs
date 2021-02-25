namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Mailbox configuration element.
    /// </summary>
    public class MailboxConfig
    {
        /// <summary>
        /// Gets or sets source emailaddress.
        /// </summary>
        /// <value>
        /// Source Emailaddress.
        /// </value>
        [XmlAttribute("sourceemailaddress")]
        public string SourceEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets target emailaddress.
        /// </summary>
        /// <value>
        /// Target Emailaddress.
        /// </value>
        [XmlAttribute("targetemailaddress")]
        public string TargetEmailAddress { get; set; }
    }
}
