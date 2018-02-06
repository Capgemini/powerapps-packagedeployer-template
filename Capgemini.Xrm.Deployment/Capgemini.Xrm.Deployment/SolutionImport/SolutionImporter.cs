using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Core.Model;
using Capgemini.Xrm.Deployment.Repository;
using System;

namespace Capgemini.Xrm.Deployment.SolutionImport
{
    public class SolutionImporter
    {
        #region Private fields

        private readonly SolutionFileManager _solutionFileManager;
        private readonly ICrmImportRepository _importRepo;
        private readonly bool _useNewAPI;

        #endregion Private fields

        #region Constructors

        public SolutionImporter(SolutionFileManager solutionFileManager, ICrmImportRepository importRepo, bool useNewAPI)
        {
            _importRepo = importRepo;
            _solutionFileManager = solutionFileManager;
            _useNewAPI = useNewAPI;
        }

        #endregion Constructors

        #region Public Fields

        public Version InstalledVersion { get; set; }

        public Version InstalledHoldingVersion { get; set; }

        public Guid InstalledSolutionId { get; set; }

        public Guid InstalledHoldingSolutionId { get; set; }

        public SolutionDetails GetSolutionDetails
        {
            get { return _solutionFileManager.SolutionDetails; }
        }

        #endregion Public Fields

        #region Public Methods

        public ImportStatus ImportUpdatedSolution(bool importAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeoutSeconds, bool publishWorkflows, bool overwriteUnamanagedCust)
        {
            UpdateSolutionDetails();

            if (InstalledVersion >= GetSolutionDetails.SolutionVersion && !_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                return new ImportStatus
                {
                    ImportState = "Not Required",
                    ImportMessage = "Solution with version " + InstalledVersion + " is already installed"
                };
            }

            return _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);
        }

        public ImportStatus ImportHoldingSolution(bool importAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeoutSeconds, bool publishWorkflows, bool overwriteUnamanagedCust)
        {
            UpdateSolutionDetails();

            if (InstalledHoldingVersion == GetSolutionDetails.SolutionVersion)
            {
                return new ImportStatus
                {
                    ImportState = "Holding Solution Not Needed",
                    ImportMessage = "Holding Solution with version " + InstalledHoldingVersion + " is already installed"
                };
            }

            if (InstalledVersion == null)
            {
                var importResult = _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);

                return new ImportStatus
                {
                    ImportState = "Holding Solution Not Needed",
                    ImportMessage = "Holding Solution not required becasue it is the first installation, so the new solution has been installed"
                };
            }

            if (InstalledVersion >= GetSolutionDetails.SolutionVersion && !_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                return new ImportStatus
                {
                    ImportState = "Holding Solution Not Needed",
                    ImportMessage = "Solution with version " + InstalledVersion + " is already installed"
                };
            }

            if (!_useNewAPI)
            {
                _solutionFileManager.CreateHoldingSolutionFile();

                try
                {
                    return _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.HoldingSolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);
                }
                finally
                {
                    _solutionFileManager.DeleteHoldingSolutionFile();
                }
            }

            if (_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                throw new Exception("Force update cannot be used with built in holding solutions! Change useNewApi to False or disable force update");
            }

            return _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds, true);
        }

        public string DeleteOriginalSolution(bool noHolding)
        {
            UpdateSolutionDetails();

            if (InstalledVersion == null)
            {
                return "Original Solution is not installed, deletion not required";
            }

            if (InstalledVersion >= GetSolutionDetails.SolutionVersion && !_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                return "Original Solution is up to date, deletion not required";
            }

            if (!noHolding && InstalledHoldingVersion != GetSolutionDetails.SolutionVersion)
            {
                throw new Exception("No valid version of holding solution is installed, cannot process deletion!");
            }

            if (noHolding || !_useNewAPI)
            {
                _importRepo.DeleteSolutionByName(_solutionFileManager.SolutionDetails.SolutionName);

                return "Original Solution with version " + InstalledVersion + " has been deleted";
            }

            _importRepo.ApplySolutionUpgrade(_solutionFileManager.SolutionDetails.SolutionName);
            return "Solution with version " + InstalledVersion + " has been UPGRADED, new API used";
        }

        public string DeleteHoldingSolution()
        {
            if (_useNewAPI)
            {
                return "New API used, no holding solution to delete";
            }

            UpdateSolutionDetails();

            if (InstalledHoldingVersion == null)
            {
                return "Holding Solution is not installed, deletion not required";
            }

            if (InstalledVersion != GetSolutionDetails.SolutionVersion)
            {
                throw new Exception("No updated solution is installed, cannot process deletion!");
            }

            _importRepo.DeleteSolutionByName(_solutionFileManager.SolutionDetails.HoldingSolutionName);

            return "Holding Solution with version " + InstalledHoldingVersion + " has been deleted";
        }

        #endregion Public Methods

        #region Internal class implementation

        public void UpdateSolutionDetails()
        {
            InstalledVersion = null;
            InstalledSolutionId = Guid.Empty;
            var solution = _importRepo.GetSolutionByName(_solutionFileManager.SolutionDetails.SolutionName);
            if (solution != null)
            {
                InstalledVersion = new Version(solution.GetAttributeValue<string>("version"));
                InstalledSolutionId = solution.Id;
            }

            InstalledHoldingVersion = null;
            InstalledHoldingSolutionId = Guid.Empty;
            var holdSolution = _importRepo.GetSolutionByName(_solutionFileManager.SolutionDetails.HoldingSolutionName);
            if (holdSolution != null)
            {
                InstalledHoldingVersion = new Version(holdSolution.GetAttributeValue<string>("version"));
                InstalledHoldingSolutionId = holdSolution.Id;
            }
        }

        #endregion Internal class implementation
    }
}