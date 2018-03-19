using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Repository.Events;
using Microsoft.Xrm.Sdk;
using System;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmImportRepository : ICrmRepository
    {
        event EventHandler<AsyncImportUpdateEventArgs> RaiseImportUpdateEvent;

        ImportStatus CheckAsyncImportStatus(Guid asyncJobId);

        ImportStatus CheckImportStatus();

        void DeleteSolutionByName(string name);

        Entity GetSolutionByName(string uniqueName);

        ImportStatus ImportSolution(string solutionFilePath, bool publishWorkflows, bool convertToManaged, bool overwriteUnmanagedCustomizations, bool importAsync, bool waitForCompletion, int sleepInterval, int asyncWaitTimeout, bool useHoldingSolution = false);

        ImportStatus ApplySolutionUpgrade(string solutionName);

        void DeactivateProcess(Guid processId);

        void ActivateProcess(Guid processId);
    }
}