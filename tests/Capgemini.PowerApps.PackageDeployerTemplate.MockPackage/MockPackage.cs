namespace Capgemini.PowerApps.PackageDeployerTemplate.MockPackage
{
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// A mock package used for testing. 
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class MockPackage : PackageTemplateBase
    {
        public override string GetLongNameOfImport => "Mock Package";

        public override string GetImportPackageDataFolderName => "PkgFolder";

        public override string GetImportPackageDescriptionText => "Mock Package";

        public override string GetNameOfImport(bool plural) => "Mock Package";

        public override bool AfterPrimaryImport()
        {
            this.CrmSvc.ImportSolutionToCrm(
                Path.Combine(this.PackageFolderPath, "Solutions", $"{Constants.Solutions.ActiveSolutionHistory}.zip"),
                out _);

            return base.AfterPrimaryImport();
        }
    }
}