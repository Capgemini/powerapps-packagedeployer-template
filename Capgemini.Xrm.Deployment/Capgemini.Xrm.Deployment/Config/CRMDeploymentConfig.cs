namespace Capgemini.Xrm.Deployment.Config
{
    public static class CRMDeploymentConfig
    {
        //Constructor to set default settings
        static CRMDeploymentConfig()
        {
            ConvertToManaged = false;
        }

        public static bool ConvertToManaged { get; set; }
    }
}