namespace Capgemini.Xrm.Deployment.Config
{
    public class SolutionImportSetting
    {
        public string SolutionName { get; set; }

        public int InstallOrder { get; set; }

        public bool DeleteOnly { get; set; }

        public bool OverwriteUnmanagedCustomizations { get; set; }

        public bool PublishWorkflows { get; set; }

        public bool ForceUpgrade { get; set; }

        public bool UseAsync { get; set; }
        
        public bool UseUpgradeAsync { get; set; } 
    }
}