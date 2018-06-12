using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Repository.Events;
using Microsoft.Xrm.Sdk;
using System;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmImportRepository : ICrmRepository
    {
        event EventHandler<AsyncImportUpdateEventArgs> RaiseImportUpdateEvent;

        ImportStatus CheckAsyncOperationStatus(Guid asyncJobId);

        ImportStatus CheckImportStatus();

        void DeleteSolutionByName(string name);

        Entity GetSolutionByName(string uniqueName);

        /// <summary>
        /// ImportSolution
        /// </summary>
        /// <param name="solutionFilePath"></param>
        /// <param name="publishWorkflows"></param>
        /// <param name="convertToManaged"></param>
        /// <param name="overwriteUnmanagedCustomizations"></param>
        /// <param name="importAsync"></param>
        /// <param name="waitForCompletion"></param>
        /// <param name="sleepInterval">In miliseconds</param>
        /// <param name="asyncWaitTimeout">In seconds</param>
        /// <param name="useHoldingSolution"></param>
        /// <returns>ImportStatus</returns>
        ImportStatus ImportSolution(string solutionFilePath, bool publishWorkflows, bool convertToManaged, bool overwriteUnmanagedCustomizations, bool importAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeout, bool useHoldingSolution = false);

        /// <summary>
        /// ApplySolutionUpgrade
        /// </summary>
        /// <param name="solutionName">Solution Name</param>
        /// <param name="importAsync">Import async</param>
        /// <param name="waitForCompletion">Wait for completion</param>
        /// <param name="sleepInterval">In miliseconds</param>
        /// <param name="asyncWaitTimeout">In seconds</param>
        /// <returns>ImportStatus</returns>
        ImportStatus ApplySolutionUpgrade(string solutionName,
            bool importAsync,
            bool waitForCompletion,
            int sleepInterval,
            int asyncWaitTimeout);

        void DeactivateProcess(Guid processId);

        void ActivateProcess(Guid processId);
    }
}