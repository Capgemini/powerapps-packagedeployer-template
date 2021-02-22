namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    using System.Xml.Serialization;

    /// <summary>
    /// Mailbox status.
    /// </summary>
    public enum MailboxStatus
    {
        /// <summary>
        /// Not Run.
        /// </summary>
        NotRun = 0,

        /// <summary>
        /// Success.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Failure.
        /// </summary>
        Failure = 2,
    }

    /// <summary>
    /// EmailRouter Access Approval.
    /// </summary>
    public enum EmailRouterAccessApproval
    {
        /// <summary>
        /// Empty.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Approved.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Pending Approval.
        /// </summary>
        PendingApproval = 2,

        /// <summary>
        /// Rejected.
        /// </summary>
        Rejected = 3,
    }

    /// <summary>
    /// Mailbox configuration element.
    /// </summary>
    public class MailboxConfig
    {
        /// <summary>
        /// Gets or sets environment prefix.
        /// </summary>
        /// <value>
        /// Environment Prefix.
        /// </value>
        [XmlAttribute("environmentprefix")]
        public string EnvironmentPrefix { get; set; }

        /// <summary>
        /// Gets or sets source emailaddress.
        /// </summary>
        /// <value>
        /// Source Emailaddress.
        /// </value>
        [XmlAttribute("sourceemailaddress")]
        public string SourceEmailaddress { get; set; }

        /// <summary>
        /// Gets or sets target emailaddress.
        /// </summary>
        /// <value>
        /// Target Emailaddress.
        /// </value>
        [XmlAttribute("targetemailaddress")]
        public string TargetEmailaddress { get; set; }
    }
}
