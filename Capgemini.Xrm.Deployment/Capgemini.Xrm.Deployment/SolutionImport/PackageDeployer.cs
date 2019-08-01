using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Extensions;
using Capgemini.Xrm.Deployment.Repository;
using Capgemini.Xrm.Deployment.SolutionImport.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Capgemini.Xrm.Deployment.SolutionImport
{
    public class PackageDeployer
    {
        #region Private Fields

        private readonly List<SolutionPackage> _packages = new List<SolutionPackage>();
        private readonly ICrmImportRepository _importRepo;
        private readonly int _sleepIntervalMiliseconds = 1000;
        private readonly int _asyncTimeoutSeconds = 1200;
        private readonly bool _importAsync = true;
        private readonly IPackageDeployerConfig _configReader;
        private readonly bool _upgradeAsync;

        #endregion Private Fields

        #region Constructors

        public PackageDeployer(ICrmImportRepository importRepo, IPackageDeployerConfig configReader)
        {
            configReader.ThrowIfNull();
            _importRepo = importRepo;
            _configReader = configReader;
            _sleepIntervalMiliseconds = configReader.AsyncSleepIntervalMiliseconds;
            _asyncTimeoutSeconds = configReader.AsyncTimeoutSeconds;
            _importAsync = configReader.UseAsyncImport;
            _upgradeAsync = configReader.UseAsyncUpgrade;
            ReadConfiguration();
        }

        #endregion Constructors

        #region Public Events

        public event EventHandler<ImportUpdateEventArgs> RaiseImportUpdateEvent;

        protected virtual void OnRaiseImportUpdatEvent(ImportUpdateEventArgs e)
        {
            e.ThrowIfNull();
            EventHandler<ImportUpdateEventArgs> handler = RaiseImportUpdateEvent;

            if (handler != null)
            {
                e.EventTime = DateTime.Now;
                handler(this, e);
            }
        }

        #endregion Public Events

        #region Public Methods and Properties

        public ICrmImportRepository ImportRepository { get { return _importRepo; } }

        public IPackageDeployerConfig DeploymentConfiguration { get { return _configReader; } }

        public List<SolutionImporter> GetSolutionDetails
        {
            get
            {
                return _packages.Select(p => p.SolutionImporter).ToList();
            }
        }

        /// <summary>
        /// Install holding solutions only if global option not disabled and DeleteOnly flag not set up
        /// </summary>
        public void InstallHoldingSolutions()
        {

            foreach (var item in _packages)
            {
                bool useAsync = item.ImportSetting.UseAsync && _importAsync;

                if (!item.ImportSetting.DeleteOnly && !_configReader.DontUseHoldingSulutions)
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Holding Solution installation started UseAsync:{useAsync}, PublishWorkflows:{item.ImportSetting.PublishWorkflows}, OverwriteUnmanagedCustomizations {item.ImportSetting.OverwriteUnmanagedCustomizations}"
                    });

                    var result = item.SolutionImporter.ImportHoldingSolution(useAsync, true, _sleepIntervalMiliseconds, _asyncTimeoutSeconds, item.ImportSetting.PublishWorkflows, item.ImportSetting.OverwriteUnmanagedCustomizations);

                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Holding Solution installation finished, status:{result.ImportState}"
                    });
                }
                else
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Holding Solution is not required because DontUseHoldingSulutions:{_configReader.DontUseHoldingSulutions} and DeleteOnly:{item.ImportSetting.DeleteOnly}"
                    });
                }
            }
        }

        /// <summary>
        /// Delete Original solutions only if DeleteOnly set up or DontUseHoldingSolutions false
        /// </summary>
        public void DeleteOriginalSolutions()
        {
            var procList = _packages.ToList();
            procList.Reverse();

            foreach (var item in procList)
            {
                bool useAsync = item.ImportSetting.UseAsync && _upgradeAsync;

                if (!_configReader.DontUseHoldingSulutions || item.ImportSetting.DeleteOnly)
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Original Solution deletion started because DontUseHoldingSulutions:{_configReader.DontUseHoldingSulutions}, DeleteOnly:{item.ImportSetting.DeleteOnly}, UseAsync:{useAsync}"
                    });

                    var result = item.SolutionImporter.DeleteOriginalSolution(item.ImportSetting.DeleteOnly, useAsync, true, _sleepIntervalMiliseconds, _asyncTimeoutSeconds);

                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = result
                    });
                }
                else
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Original Solution deletion not required because DontUseHoldingSulutions:{_configReader.DontUseHoldingSulutions} and DeleteOnly:{item.ImportSetting.DeleteOnly}"
                    });
                }
            }
        }

        public void InstallNewSolutions()
        {
            foreach (var item in _packages)
            {
                bool useAsync = item.ImportSetting.UseAsync && _importAsync;

                OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                {
                    SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                    Message = $"Updated Solution installation started UseAsync:{useAsync}, PublishWorkflows:{item.ImportSetting.PublishWorkflows}, OverwriteUnmanagedCustomizations {item.ImportSetting.OverwriteUnmanagedCustomizations}"
                });

                var result = item.SolutionImporter.ImportUpdatedSolution(useAsync, true, _sleepIntervalMiliseconds, _asyncTimeoutSeconds, item.ImportSetting.PublishWorkflows, item.ImportSetting.OverwriteUnmanagedCustomizations);

                OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                {
                    SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                    Message = $"Updated Solution installation finished, status:{result.ImportState}"
                });

                //Extra to delete holding solution immediatelly
                if (!item.ImportSetting.DeleteOnly && !_configReader.DontUseHoldingSulutions)
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Holding Solution deletion started because DontUseHoldingSulutions:{_configReader.DontUseHoldingSulutions} and DeleteOnly:{item.ImportSetting.DeleteOnly}"
                    });

                    var result2 = item.SolutionImporter.DeleteHoldingSolution();

                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = result2
                    });
                }
                else
                {
                    OnRaiseImportUpdatEvent(new ImportUpdateEventArgs
                    {
                        SolutionDetails = item.SolutionImporter.GetSolutionDetails,
                        Message = $"Holding Solution deletion not required because DontUseHoldingSulutions:{_configReader.DontUseHoldingSulutions} and DeleteOnly:{item.ImportSetting.DeleteOnly}"
                    });
                }
            }
        }

        #endregion Public Methods and Properties

        #region Internall class implementation

        private void ReadConfiguration()
        {
            foreach (var item in _configReader.SolutionImportSettings)
            {
                var pkgConfig = new SolutionPackage
                {
                    ImportSetting = item
                };

                var fileManager = new SolutionFileManager(Path.Combine(_configReader.SolutionsFolder, item.SolutionName), item.ForceUpgrade);

                pkgConfig.SolutionImporter = new SolutionImporter(fileManager, _importRepo, _configReader.UseNewApi);

                _packages.Add(pkgConfig);
            }
        }

        #endregion Internall class implementation
    }
}