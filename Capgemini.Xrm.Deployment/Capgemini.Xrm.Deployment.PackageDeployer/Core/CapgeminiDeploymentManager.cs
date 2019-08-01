using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.PackageDeployer.Forms;
using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.SolutionImport.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Core
{
    public class CapgeminiDeploymentManager
    {
        private const string StartCustomDeploymentMessage = "Capgemini Package Deployer - Deployment Start";

        private readonly ILogger _logger;
        private readonly ICrmImportRepository _impGateway;
        private readonly SolutionImport.PackageDeployer _packageDeployer;
        private readonly Dispatcher _dispatcher;

        public CapgeminiDeploymentManager(ILogger logger, CrmAccess gatewayAccess, IPackageDeployerConfig configReader)
            : this(null, logger, gatewayAccess, configReader)
        {
        }

        public CapgeminiDeploymentManager(IPackageTemplate packageTemplate, ILogger logger, CrmAccess gatewayAccess, IPackageDeployerConfig configReader)
        {
            _dispatcher = packageTemplate != null ? packageTemplate.RootControlDispatcher : null;
            _logger = logger;

            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var packageFolder = Path.Combine(assemblyFolder, packageTemplate.GetImportPackageDataFolderName);

            _impGateway = new CrmImportRepository(gatewayAccess);
            _impGateway.RaiseImportUpdateEvent += ImpGateway_RaiseImportUpdateEvent;
            _packageDeployer = new SolutionImport.PackageDeployer(_impGateway, configReader);

            _packageDeployer.RaiseImportUpdateEvent += packageDeployer_RaiseImportUpdateEvent;
        }            

        public void StartCustomDeployment()
        {
            _logger.WriteLogMessage(StartCustomDeploymentMessage, TraceEventType.Start);

            if (_dispatcher != null)
            {
                StartDeploymentWithUI();
            }
            else
            {
                StartDeploymentWithoutUI();
            }
        }

        private void StartDeploymentWithUI()
        {
            _dispatcher.Invoke((Action)(() =>
            {
                var window = new CapgeminiPackageDeployerForm(_packageDeployer, _impGateway)
                {
                    StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
                };

                window.ShowDialog();

                if (window.LastException != null)
                {
                    _logger.WriteLogMessage("Capgemini Package Deployer Error", TraceEventType.Error, window.LastException);
                    Environment.Exit(0);
                }

                if (!window.ImportFinished)
                    Environment.Exit(0);
            }), DispatcherPriority.Normal, null);
        }

        private void StartDeploymentWithoutUI()
        {
            try
            {
                _logger.WriteLogMessage("Capgemini Package Deployer - InstallHoldingSolutions", TraceEventType.Start);
                _packageDeployer.InstallHoldingSolutions();
            }
            catch (Exception ex)
            {
                _logger.WriteLogMessage("InstallHoldingSolutions Error", TraceEventType.Error, ex);
                throw;
            }

            try
            {
                _logger.WriteLogMessage("Capgemini Package Deployer - DeleteOriginalSolutions", TraceEventType.Start);
                _packageDeployer.DeleteOriginalSolutions();
            }
            catch (Exception ex)
            {
                _logger.WriteLogMessage("DeleteOriginalSolutions Error", TraceEventType.Error, ex);
                throw;
            }

            try
            {
                _logger.WriteLogMessage("Capgemini Package Deployer - InstallNewSolutions and delete holding solutions", TraceEventType.Start);
                _packageDeployer.InstallNewSolutions();
            }
            catch (Exception ex)
            {
                _logger.WriteLogMessage("InstallNewSolutions Error", TraceEventType.Error, ex);
                throw;
            }
        }

        private void packageDeployer_RaiseImportUpdateEvent(object sender, ImportUpdateEventArgs e)
        {
            var message = $"Solution:{e.SolutionDetails.SolutionName}:{e.SolutionDetails.SolutionVersion} - {e.Message}";
            _logger.WriteLogMessage(message);
        }

        private void ImpGateway_RaiseImportUpdateEvent(object sender, Repository.Events.AsyncImportUpdateEventArgs e)
        {
            var message = $"AsyncImportUpdate:{ e.Message}";
            _logger.WriteLogMessage(message, TraceEventType.Verbose);
        }

    }
}