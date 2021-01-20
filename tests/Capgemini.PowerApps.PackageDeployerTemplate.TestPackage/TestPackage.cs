namespace Capgemini.PowerApps.PackageDeployerTemplate.TestPackage
{
    using System.ComponentModel.Composition;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class TestPackage : PackageTemplateBase
    {
        /// <summary>
        /// Gets folder Name for the Package data.
        /// </summary>
        public override string GetImportPackageDataFolderName
        {
            get
            {
                // WARNING this value directly correlates to the folder name in the Solution Explorer where the ImportConfig.xml and sub content is located.
                // Changing this name requires that you also change the correlating name in the Solution Explorer
                return "PkgFolder";
            }
        }

        /// <summary>
        /// Gets description of the package, used in the package selection UI.
        /// </summary>
        public override string GetImportPackageDescriptionText
        {
            get { return "Package Description"; }
        }

        /// <summary>
        /// Gets long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport
        {
            get { return "Package Long Name"; }
        }

        /// <summary>
        /// Name of the Import Package to Use.
        /// </summary>
        /// <param name="plural">if true, return plural version.</param>
        /// <returns>The package short name.</returns>
        public override string GetNameOfImport(bool plural)
        {
            return "Package Short Name";
        }
    }
}
