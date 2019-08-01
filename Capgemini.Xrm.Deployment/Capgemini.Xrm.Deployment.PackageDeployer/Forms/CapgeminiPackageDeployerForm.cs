using Capgemini.Xrm.Deployment.Extensions;
using Capgemini.Xrm.Deployment.PackageDeployer.BusinessLogic;
using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.SolutionImport.Events;
using Capgemini.Xrm.DeploymentHelpers;
using Capgemini.Xrm.PackageDeployer.TestUI.Logging;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Forms
{
    public partial class CapgeminiPackageDeployerForm : Form
    {
        private SolutionImport.PackageDeployer _deployer;
        private readonly SynchronizationContext _syncContext;
        private readonly ICrmImportRepository _impRepo;
        private readonly string _configSubfolder = "";
        private readonly bool _importData;

        public Exception LastException { get; set; }
        public bool ImportFinished { get; set; }

        public CapgeminiPackageDeployerForm(SolutionImport.PackageDeployer packageDeployer, ICrmImportRepository impRepo, bool importData, string configSubfolder) : this(packageDeployer, impRepo)
        {
            _configSubfolder = configSubfolder;
            _importData = importData;
            var config = _deployer.DeploymentConfiguration;

            DisplayMessage($"SolutionsFolder={config.SolutionsFolder}");
            DisplayMessage($"UseHoldingSulutions={!config.DontUseHoldingSulutions}");
            DisplayMessage($"UseAsyncImport={config.UseAsyncImport}");
            DisplayMessage($"AsyncSleepIntervalMiliseconds={config.AsyncSleepIntervalMiliseconds}");
            DisplayMessage($"AsyncTimeoutSeconds={config.AsyncTimeoutSeconds}");
            DisplayMessage($"UseNewApi={config.UseNewApi}");
            DisplayMessage($"DisableSlaBeforeImport={config.DisableSlaBeforeImport}");
            DisplayMessage($"EnableSlaAfterImport={config.EnableSlaAfterImport}");
            DisplayMessage($"SkipPostDeploymentActions={config.SkipPostDeploymentActions}");

            DisplayMessage($"ImportData={_importData}");
            DisplayMessage($"ConfigSubfolder={_configSubfolder}");
     
        }

        public CapgeminiPackageDeployerForm(SolutionImport.PackageDeployer packageDeployer, ICrmImportRepository impRepo)
        {
            InitializeComponent();
            this._syncContext = SynchronizationContext.Current;
            this._deployer = packageDeployer;
            this._impRepo = impRepo;
        }

        private async void CapgeminiPackageDeployerForm_Load(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                _deployer.RaiseImportUpdateEvent += _deployer_RaiseImportUpdateEvent;
                _impRepo.RaiseImportUpdateEvent += _impGateway_RaiseImportUpdateEvent;

                DisplayMessage("Solutions Found:");

                foreach (var item in _deployer.GetSolutionDetails)
                {
                    item.UpdateSolutionDetails();
                    DisplayMessage($"Solution:{item.GetSolutionDetails.SolutionName}, Version:{item.GetSolutionDetails.SolutionVersion}, Installed Version:{(item.InstalledVersion == null ? "None" : item.InstalledVersion.ToString())}, Installed Holding Version:{(item.InstalledHoldingVersion == null ? "None" : item.InstalledHoldingVersion.ToString())}");
                }
            }).ConfigureAwait(false);

            btStart.Enabled = true;
        }

        private void _impGateway_RaiseImportUpdateEvent(object sender, Repository.Events.AsyncImportUpdateEventArgs e)
        {
            DisplayMessage($"AsyncImport: {e.Message}");
        }

        private void _deployer_RaiseImportUpdateEvent(object sender, ImportUpdateEventArgs e)
        {
            DisplayMessage($"Solution:{e.SolutionDetails.SolutionName}, ver:{e.SolutionDetails.SolutionVersion}, Message:{e.Message}");
        }

        private void DisplayMessage(string message)
        {
            this._syncContext.Send(p =>
            {
                tbMessage.AppendText($"{DateTime.Now} - {message}{Environment.NewLine}");
            }, null);
        }

        private async void btStart_Click(object sender, EventArgs e)
        {

            btStart.Enabled = false;
            var logger = new TextBoxLogger(this.tbMessage, this._syncContext);
            var deplActivities = new DeploymentActivities(_deployer.DeploymentConfiguration, logger, _impRepo.CurrentAccess);
            var procActivator = new ProcessesActivator(_deployer.DeploymentConfiguration, logger, _impRepo.CurrentAccess);
            var importFacade = new CapgeminiDataMigratorFacade(_impRepo.CurrentOrganizationService, logger);
            var config = _deployer.DeploymentConfiguration;
           

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    deplActivities.DeactivateAllSLAs();

                    ImportFinished = false;

                    LastException = null;
                    DisplayMessage("InstallHoldingSolutions ...");
                    _deployer.InstallHoldingSolutions();

                    DisplayMessage("DeleteOriginalSolutions ...");
                    _deployer.DeleteOriginalSolutions();

                    DisplayMessage("InstallNewSolutions and delete Holding Solutions ...");
                    _deployer.InstallNewSolutions();

                    if (!config.SkipPostDeploymentActions)
                    {
                        deplActivities.LoadTemplates();
                        procActivator.ActivateRequiredWorkflows();
                        procActivator.DeactivatePluginSteps();
                    }

                    deplActivities.ActivateAllSLAs();

                    if (_importData)
                        importFacade.MigrateDataPackages(_deployer.DeploymentConfiguration.SolutionConfigFilePath, _configSubfolder);
                }
                catch (Exception ex)
                {
                    DisplayMessage($"ERROR:{ex}");
                    LastException = ex;
                }
            }).ConfigureAwait(false);

            if (LastException == null)
            {
                MessageBox.Show("All solutions have been imported.");
                this.Close();
            }
            else
            {
                MessageBox.Show($"Import Error, You can try to fix the problem and try again, Error: {LastException}");
                btStart.Enabled = true;
            }
        }
    }
}