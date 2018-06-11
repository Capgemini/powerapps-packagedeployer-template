using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Core.Model;
using Capgemini.Xrm.Deployment.Repository;
using System;
using System.Threading;

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
                    ImportState = "Not Required, already installed",
                    ImportMessage = "Solution with version " + InstalledVersion + " is already installed"
                };
            }

            return _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);
        }

        public ImportStatus ImportHoldingSolution(bool importAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeoutSeconds, bool publishWorkflows, bool overwriteUnamanagedCust)
        {
            UpdateSolutionDetails();

            if (InstalledHoldingVersion == GetSolutionDetails.SolutionVersion && !_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                return new ImportStatus
                {
                    ImportState = "Holding Solution Not Needed because already exists",
                    ImportMessage = "Holding Solution with version " + InstalledHoldingVersion + " is already installed"
                };
            }

            if (InstalledVersion == null)
            {
                var importResult = _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);

                importResult.ImportState += ", First Installation, Original solution has been installed instead";
                importResult.ImportMessage += ", Holding Solution not required becasue it is the first installation, so the new solution has been installed instead";

                return importResult;
            }

            if (InstalledVersion >= GetSolutionDetails.SolutionVersion && !_solutionFileManager.SolutionDetails.ForceUpdate)
            {
                return new ImportStatus
                {
                    ImportState = "Holding Solution Not Needed, newer version already installed",
                    ImportMessage = "Solution with version " + InstalledVersion + " is already installed"
                };
            }
            else
            {

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
                else
                {

                    if (_solutionFileManager.SolutionDetails.ForceUpdate)
                    {
                        throw new Exception("Force update cannot be used with built in holding solutions! Change useNewApi to False or disable force update");
                    }

                    try
                    {
                        return _importRepo.ImportSolution(_solutionFileManager.SolutionDetails.SolutionFilePath, publishWorkflows, CRMDeploymentConfig.ConvertToManaged, overwriteUnamanagedCust, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds, true);
                    }
                    catch (Exception exw)
                    {
                        if (GetSolutionDetails.SolutionName == "Nhsbt_Sessions_Workflows" && exw.ToString().Contains("The action was failed after 0 times of retry. InnerException is: Microsoft.Crm.BusinessEntities.CrmObjectNotFoundException: sdkmessageprocessingstep With Id"))
                        {
                            return new ImportStatus
                            {
                                ImportState = "success",
                                ImportMessage = $"Ignoring MSFT error : {exw.Message}",
                                ImportStatusCode = 30,
                                SolutionName = GetSolutionDetails.SolutionName,
                                ImportAsyncId = Guid.NewGuid(),
                                ImportId = Guid.NewGuid()
                            };
                        }

                        throw;
                    }
                   
                }
            }
        }

        public string DeleteOriginalSolution(bool noHolding, bool applyUpgradeAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeoutSeconds)
        {
            UpdateSolutionDetails();
            var currentVersion = InstalledVersion;

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
                try
                {
                    _importRepo.DeleteSolutionByName(_solutionFileManager.SolutionDetails.SolutionName);
                    return "Original Solution with version " + InstalledVersion + " has been deleted";
                }
                catch (TimeoutException ext)
                {
                    // Checking if operation invoked OK
                    UpdateSolutionDetails();
                    if (InstalledVersion == null)
                    {
                        Thread.Sleep(10000);
                        return $"Original Solution with version {currentVersion} has been deleted despite of timeout exception : {ext.Message}";
                    }

                    throw;
                }
            }
            else
            {
                try
                {
                    _importRepo.ApplySolutionUpgrade(_solutionFileManager.SolutionDetails.SolutionName, applyUpgradeAsync, waitForCompletion, sleepInterval, asyncWaitTimeoutSeconds);
                    return "Solution with version " + InstalledVersion + " has been UPGRADED, new API used";
                }
                catch (TimeoutException ext)
                {
                    // Checking if operation invoked OK
                    UpdateSolutionDetails();
                    if (InstalledHoldingVersion == null)
                    {
                        Thread.Sleep(10000);
                        return $"Solution Solution with version {currentVersion} has been updated despite of timeout exception : {ext.Message}";
                    }

                    throw;
                } 
            }
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

            try
            {
                _importRepo.DeleteSolutionByName(_solutionFileManager.SolutionDetails.HoldingSolutionName);
                return "Holding Solution with version " + InstalledHoldingVersion + " has been deleted";
            }
            catch (TimeoutException ext)
            {
                // Checking if operation invoked OK
                var currentVersion = InstalledHoldingVersion;
                UpdateSolutionDetails();
                if (InstalledHoldingVersion == null)
                {
                    Thread.Sleep(10000);
                    return $"Holding Solution with version {currentVersion} has been deleted despite of timeout exception : {ext.Message}";
                }

                throw;
            }
          
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