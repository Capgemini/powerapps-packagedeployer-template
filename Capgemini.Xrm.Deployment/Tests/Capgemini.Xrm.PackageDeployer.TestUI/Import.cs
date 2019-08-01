using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.PackageDeployer.BusinessLogic;
using Capgemini.Xrm.Deployment.PackageDeployer.Core;
using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.Repository.Events;
using Capgemini.Xrm.Deployment.SolutionImport.Events;
using Capgemini.Xrm.DeploymentHelpers;
using Capgemini.Xrm.PackageDeployer.TestUI.Logging;
using Capgemini.Xrm.PackageDeployer.TestUI.Properties;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Capgemini.Xrm.PackageDeployer.TestUI
{
    public partial class Import : Form
    {
        private Deployment.SolutionImport.PackageDeployer _deployer;
        private readonly SynchronizationContext syncContext;

        public Import()
        {
            this.syncContext = SynchronizationContext.Current;
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            btStart.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    DisplayMessage("InstallHoldingSolutions ...");
                    _deployer.InstallHoldingSolutions();

                    DisplayMessage("DeleteOriginalSolutions ...");
                    _deployer.DeleteOriginalSolutions();

                    DisplayMessage("InstallNewSolutions ...");
                    _deployer.InstallNewSolutions();
                }
                catch (Exception ex)
                {
                    DisplayMessage("ERROR:" + ex);
                }
            });
            btStart.Enabled = true;
        }

        private async void Import_Load(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                DisplayMessage("Package Folder:" + Settings.Default.PkgFolderPath);

                var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
                DisplayMessage("CRM Url:" + crmAccess.ServiceClient.CrmConnectOrgUriActual);

                var impRepo = new CrmImportRepository(crmAccess);
                impRepo.RaiseImportUpdateEvent += ImpGateway_RaiseImportUpdateEvent;
                var configReader = new PackageDeployerConfigReader(Settings.Default.PkgFolderPath);

                _deployer = new Deployment.SolutionImport.PackageDeployer(impRepo, configReader);

                _deployer.RaiseImportUpdateEvent += _deployer_RaiseImportUpdateEvent;

                DisplayMessage("Solutions Found:");

                foreach (var item in _deployer.GetSolutionDetails)
                {
                    item.UpdateSolutionDetails();
                    DisplayMessage(string.Format("Solution:{0}, Version{1}, Installed Version{2}",
                        item.GetSolutionDetails.SolutionName,
                        item.GetSolutionDetails.SolutionVersion,
                        item.InstalledVersion == null ? "None" : item.InstalledVersion.ToString()));
                }
            });

            btStart.Enabled = true;
        }

        private void ImpGateway_RaiseImportUpdateEvent(object sender, AsyncImportUpdateEventArgs e)
        {
            DisplayMessage(string.Format("Import Update, State:{0}, StatusCode:{1}, Message{2}", e.ImportState, e.ImportStatusCode, e.Message));
        }

        private void _deployer_RaiseImportUpdateEvent(object sender, ImportUpdateEventArgs e)
        {
            DisplayMessage(string.Format("Import Solution:{0}, Message:{1}", e.SolutionDetails.SolutionName, e.Message));
        }

        private void DisplayMessage(string message)
        {
            this.syncContext.Send(p =>
            {
                tbMessage.AppendText(string.Format("{0} - {1}{2}", DateTime.Now, message, Environment.NewLine));
            }, null);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
            DisplayMessage("CRM Url:" + crmAccess.ServiceClient.CrmConnectOrgUriActual);
            MessageLogger logger = new MessageLogger(tbMessage, SynchronizationContext.Current);
            var configReader = new PackageDeployerConfigReader(Settings.Default.PkgFolderPath);
            ProcessesActivator proAct = new ProcessesActivator(configReader, logger, crmAccess);
            proAct.DeactivatePluginSteps();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
            DisplayMessage("CRM Url:" + crmAccess.ServiceClient.CrmConnectOrgUriActual);
            MessageLogger logger = new MessageLogger(tbMessage, SynchronizationContext.Current);

            var importFacade = new CapgeminiDataMigratorFacade(crmAccess.CurrentServiceProxy, logger);
            importFacade.MigrateDataPackages(Settings.Default.PkgFolderPath + "\\ImportConfig.xml", "");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageLogger.LogLevel = 3;
            var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
            DisplayMessage("CRM Url:" + crmAccess.ServiceClient.CrmConnectOrgUriActual);
            MessageLogger logger = new MessageLogger(tbMessage, SynchronizationContext.Current);

            var configReader = new PackageDeployerConfigReader(Settings.Default.PkgFolderPath);
            DeploymentActivities deplAct = new DeploymentActivities(configReader, logger, crmAccess);

            deplAct.ActivateAllSLAs();
            MessageLogger.LogLevel = 2;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageLogger.LogLevel = 3;
            var crmAccess = new CrmAccessClient(ConfigurationManager.ConnectionStrings["CrmDefaultConnection"], 60);
            DisplayMessage("CRM Url:" + crmAccess.ServiceClient.CrmConnectOrgUriActual);
            MessageLogger logger = new MessageLogger(tbMessage, SynchronizationContext.Current);
            var configReader = new PackageDeployerConfigReader(Settings.Default.PkgFolderPath);
            DeploymentActivities deplAct = new DeploymentActivities(configReader, logger, crmAccess);

            deplAct.DeactivateAllSLAs();
            MessageLogger.LogLevel = 2;
        }
    }
}