using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.IntegrationTests.Core;
using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.SolutionImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Capgemini.Xrm.Deployment.IntegrationTests.ImportTests
{
    [TestClass]
    public class PackageDeployerTest : IntegrationTestBase
    {
        [TestMethod]
        public void DeploymentTest()
        {
            var repo = new CrmImportRepository(this.CrmAccessLogged);

            string folderPath = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            string pkgPath = Path.Combine(folderPath, @"TestFiles\PkgFolderMin");

            var configReader = new PackageDeployerConfigReader(pkgPath);

            var pd = new PackageDeployer(repo, 5000, 3600, true, configReader);

            pd.RaiseImportUpdateEvent += Pd_RaiseImportUpdateEvent;
            pd.InstallHoldingSolutions();
            pd.DeleteOriginalSolutions();
            pd.InstallNewSolutions();
        }

        private void Pd_RaiseImportUpdateEvent(object sender, Deployment.SolutionImport.Events.ImportUpdateEventArgs e)
        {
            Debug.WriteLine(string.Format("{0} - {1} - {2}", e.EventTime, e.Message, e.SolutionDetails.ToString()));
        }
    }
}