using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.PackageDeployer.Core;
using Capgemini.Xrm.Deployment.PackageDeployer.Forms;
using Capgemini.Xrm.Deployment.PackageDeployer.UI.Properties;
using Capgemini.Xrm.Deployment.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Capgemini.Xrm.Deployment.PackageDeployer.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            IPackageDeployerConfig config = new PackageDeployerConfigReader(Settings.Default.PkgFolderPath);
            var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
       
            CrmImportRepository impRepo = new CrmImportRepository(crmAccess);
            SolutionImport.PackageDeployer packageDeployer = new SolutionImport.PackageDeployer(impRepo, config);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CapgeminiPackageDeployerForm(packageDeployer, impRepo, Settings.Default.ImportData, Settings.Default.ImportConfigSubfolder));
        }
    }
}
