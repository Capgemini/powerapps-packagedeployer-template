namespace Capgemini.PowerApps.PackageDeployerTemplate.Config
{
    /// <summary>
    /// Configuration for the desired state of components post-deployment.
    /// </summary>
    public enum DesiredState
    {
        /// <summary>
        /// The component should be active post-deployment.
        /// </summary>
        Active,

        /// <summary>
        /// The component should be inactive post-deployment.
        /// </summary>
        Inactive,
    }
}