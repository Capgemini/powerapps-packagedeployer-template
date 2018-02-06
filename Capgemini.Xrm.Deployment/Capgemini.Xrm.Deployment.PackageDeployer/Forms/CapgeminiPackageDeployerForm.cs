using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.SolutionImport.Events;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Forms
{
    public partial class CapgeminiPackageDeployerForm : Form
    {
        private SolutionImport.PackageDeployer _deployer;
        private SynchronizationContext _syncContext;
        private ICrmImportRepository _impGateway;
        public Exception LastException { get; set; }
        public bool ImportFinished { get; set; }

        public CapgeminiPackageDeployerForm(SolutionImport.PackageDeployer packageDeployer, ICrmImportRepository impGateway)
        {
            InitializeComponent();
            this._syncContext = SynchronizationContext.Current;
            this._deployer = packageDeployer;
            this._impGateway = impGateway;
        }

        private async void CapgeminiPackageDeployerForm_Load(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                _deployer.RaiseImportUpdateEvent += _deployer_RaiseImportUpdateEvent;

                DisplayMessage("Solutions Found:");

                foreach (var item in _deployer.GetSolutionDetails)
                {
                    item.UpdateSolutionDetails();
                    DisplayMessage(string.Format("Solution:{0}, Version:{1}, Installed Version:{2}, Installed Holding Version:{3}",
                        item.GetSolutionDetails.SolutionName,
                        item.GetSolutionDetails.SolutionVersion,
                        item.InstalledVersion == null ? "None" : item.InstalledVersion.ToString(),
                        item.InstalledHoldingVersion == null ? "None" : item.InstalledHoldingVersion.ToString()));
                }
            });

            btStart.Enabled = true;
        }

        private void _deployer_RaiseImportUpdateEvent(object sender, ImportUpdateEventArgs e)
        {
            DisplayMessage(string.Format("Solution:{0}, Message:{1}", e.SolutionDetails.SolutionName, e.Message));
        }

        private void DisplayMessage(string message)
        {
            this._syncContext.Send(p =>
            {
                tbMessage.AppendText(string.Format("{0} - {1}{2}", DateTime.Now, message, Environment.NewLine));
            }, null);
        }

        private async void btStart_Click(object sender, EventArgs e)
        {
            btStart.Enabled = false;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    ImportFinished = false;

                    LastException = null;
                    DisplayMessage("InstallHoldingSolutions ...");
                    _deployer.InstallHoldingSolutions();

                    DisplayMessage("DeleteOriginalSolutions ...");
                    _deployer.DeleteOriginalSolutions();

                    DisplayMessage("InstallNewSolutions ...");
                    _deployer.InstallNewSolutions();

                    ImportFinished = true;
                }
                catch (Exception ex)
                {
                    DisplayMessage("ERROR:" + ex);
                    LastException = ex;
                }
            });

            if (LastException == null)
            {
                MessageBox.Show("All solutions have been imported.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Import Error, You can try to fix the problem and try again, Error: " + LastException);
                btStart.Enabled = true;
            }
        }
    }
}