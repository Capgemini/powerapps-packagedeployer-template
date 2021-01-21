namespace Capgemini.PowerApps.PackageDeployerTemplate.MockPackage
{
    /// <summary>
    /// A mock package used for testing. 
    /// </summary>
    public class MockPackage : PackageTemplateBase
    {
        public override string GetLongNameOfImport => "Mock Package";

        public override string GetImportPackageDataFolderName => "PkgFolder";

        public override string GetImportPackageDescriptionText => "Mock Package";

        public override string GetNameOfImport(bool plural) => "Mock Package";
    }
}