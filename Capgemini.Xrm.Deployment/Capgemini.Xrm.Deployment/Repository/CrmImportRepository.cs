using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Core.Model;
using Capgemini.Xrm.Deployment.Repository.Events;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmImportRepository : CrmRepository, ICrmImportRepository
    {
        #region Constructors and Private Fields

        private Guid? _requestId;

        public CrmImportRepository(CrmAccess crmAccess) : base(crmAccess)
        {
        }

        #endregion Constructors and Private Fields

        #region Public Events

        public event EventHandler<AsyncImportUpdateEventArgs> RaiseImportUpdateEvent;

        protected virtual void OnRaiseImportUpdatEvent(AsyncImportUpdateEventArgs e)
        {
            EventHandler<AsyncImportUpdateEventArgs> handler = RaiseImportUpdateEvent;

            if (handler != null)
            {
                e.EventTime = DateTime.Now;
                handler(this, e);
            }
        }

        #endregion Public Events

        #region ICrmImportRepository Interface Implementation

        public void DeleteSolutionByName(string name)
        {
            var queryImportedSolution = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet(new string[] { "solutionid", "friendlyname" }),
                Criteria = new FilterExpression()
            };

            queryImportedSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, name);

            var ImportedSolution = this.CurrentOrganizationService.RetrieveMultiple(queryImportedSolution).Entities.FirstOrDefault();

            if (ImportedSolution == null) throw new Exception("Solution with name " + name + " does not exist!");

            this.CurrentOrganizationService.Delete("solution", ImportedSolution.GetAttributeValue<Guid>("solutionid"));
        }

        public Entity GetSolutionByName(string uniqueName)
        {
            var getSolutionQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("version"),
                Criteria = new FilterExpression()
            };

            getSolutionQuery.Criteria.AddCondition("uniquename", ConditionOperator.Equal, uniqueName);

            var records = CurrentAccess.GetDataByQuery(getSolutionQuery, 10);

            return records.Entities.FirstOrDefault();
        }

        public ImportStatus CheckAsyncOperationStatus(Guid asyncJobId)
        {
            ImportStatus outResult;

            try
            {
                var asyncOperation = CurrentOrganizationService.Retrieve("asyncoperation", asyncJobId, new ColumnSet("asyncoperationid", "statuscode", "message"));

                int statusCode = asyncOperation.GetAttributeValue<OptionSetValue>("statuscode").Value;

                outResult = new ImportStatus
                {
                    ImportAsyncId = asyncJobId,
                    ImportState = statusCode.ToString(CultureInfo.InvariantCulture),
                    ImportMessage = asyncOperation.GetAttributeValue<string>("message"),
                    ImportId = _requestId.Value,
                    ImportStatusCode = statusCode
                };
            }
            catch (Exception ex)
            {
                outResult = new ImportStatus
                {
                    ImportAsyncId = asyncJobId,
                    ImportState = "Unknown",
                    ImportMessage = "Cannot get import status, error:" + ex,
                    ImportId = _requestId.Value,
                    ImportStatusCode = 0
                };
            }

            return outResult;
        }

        public ImportStatus CheckImportStatus()
        {
            var job = CurrentOrganizationService.Retrieve("importjob", _requestId.Value, new ColumnSet(new [] { "data", "solutionname" }));

            if (job == null) throw new Exception("Invalid importjob id");

            var reportData = job.GetAttributeValue<string>("data");

            var doc = new XmlDocument();
            doc.LoadXml(reportData);

            String ImportedSolutionName = doc.SelectSingleNode("//solutionManifest/UniqueName").InnerText;
            var resultNode = doc.SelectSingleNode("//solutionManifest/result/@result");

            String SolutionImportResult = resultNode != null ? resultNode.Value : "";

            var result = new ImportStatus
            {
                ImportId = _requestId.Value,
                ImportState = SolutionImportResult,
                SolutionName = ImportedSolutionName,
                ImportMessage = reportData
            };

            _requestId = null;

            return result;
        }

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
        public ImportStatus ImportSolution(string solutionFilePath,
                   bool publishWorkflows,
                   bool convertToManaged,
                   bool overwriteUnmanagedCustomizations,
                   bool importAsync,
                   bool waitForCompletion,
                   int sleepInterval,
                   int asyncWaitTimeout,
                   bool useHoldingSolution = false)
        {

            if (_requestId.HasValue) throw new Exception("Import already in progress");
            _requestId = Guid.NewGuid();

            var solutionBytes = File.ReadAllBytes(solutionFilePath);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = publishWorkflows,
                ConvertToManaged = convertToManaged,
                OverwriteUnmanagedCustomizations = overwriteUnmanagedCustomizations,
                ImportJobId = _requestId.Value,
                HoldingSolution = useHoldingSolution,
                SkipProductUpdateDependencies = true
            };

            try
            {
                var result = ExecuteImportOperation(importSolutionRequest, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeout);
                return result;
            }
            finally
            {
                _requestId = null;
            }
          

        }

        /// <summary>
        /// ApplySolutionUpgrade
        /// </summary>
        /// <param name="solutionName">Solution Name</param>
        /// <param name="importAsync">Import async</param>
        /// <param name="waitForCompletion">Wait for completion</param>
        /// <param name="sleepInterval">In miliseconds</param>
        /// <param name="asyncWaitTimeout">In seconds</param>
        /// <returns>ImportStatus</returns>
        public ImportStatus ApplySolutionUpgrade(string solutionName,
            bool importAsync,
            bool waitForCompletion,
            int sleepInterval,
            int asyncWaitTimeout)
        {
            if (_requestId.HasValue) throw new Exception("ANother operation already in progress");
            _requestId = Guid.NewGuid();

            var upgradeRequest = new DeleteAndPromoteRequest
            {
                UniqueName = solutionName,
                RequestId = _requestId
            };
            var result = new ImportStatus();

            try
            {
                if (!importAsync)
                {
                    var upgradeResponse = CurrentOrganizationService.Execute(upgradeRequest) as DeleteAndPromoteResponse;

                    result = new ImportStatus
                    {
                        ImportId = _requestId,
                        ImportMessage = $"Solution {upgradeResponse.SolutionId} has been promoted to the new version",
                        ImportState = "Promoted",
                        SolutionName = solutionName
                    };

                }
                else
                {
                    result = ExecuteAsyncOperation(upgradeRequest, waitForCompletion, sleepInterval, asyncWaitTimeout);
                    result.ImportMessage = $"Solution  has been promoted to the new version, Async:{result.ImportMessage}";
                    result.SolutionName = solutionName;
                }
            }
            finally
            {
                _requestId = null;
            }

            return result;
        }

        private ImportStatus ExecuteAsyncOperation(OrganizationRequest request,
                  bool waitForCompletion,
                  int sleepInterval,
                  int asyncWaitTimeout)
        {
            var asyncRequest = new ExecuteAsyncRequest
            {
                Request = request
            };

            var asyncResponse = CurrentOrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

            Guid? asyncJobId = asyncResponse.AsyncJobId;

            var watch = Stopwatch.StartNew();
            watch.Start();

            if (waitForCompletion)
            {
                var end = DateTime.Now.AddSeconds(asyncWaitTimeout);

                while (end >= DateTime.Now)
                {
                    Thread.Sleep(sleepInterval);

                    var asyncOperation = CheckAsyncOperationStatus(asyncJobId.Value);

                    AsyncOperationStatusEnum statusCode = (AsyncOperationStatusEnum)asyncOperation.ImportStatusCode;

                    switch (statusCode)
                    {
                        case AsyncOperationStatusEnum.Succeeded:
                            watch.Stop();
                            return asyncOperation;

                        case AsyncOperationStatusEnum.Pausing:
                        case AsyncOperationStatusEnum.Canceling:
                        case AsyncOperationStatusEnum.Failed:
                        case AsyncOperationStatusEnum.Canceled:
                            watch.Stop();
                            throw new Exception($"Async Operation Failed: {statusCode} {asyncOperation.ImportMessage}");

                        default:
                            OnRaiseImportUpdatEvent(new AsyncImportUpdateEventArgs
                            {
                                Message = $"{statusCode} {asyncOperation.ImportMessage}, Id:{asyncOperation.ImportAsyncId}, ElapsedTime:{watch.Elapsed.TotalSeconds}",
                                ImportState = asyncOperation.ImportState,
                                ImportStatusCode = asyncOperation.ImportStatusCode,
                                ImportId = asyncOperation.ImportAsyncId
                            });
                            break;
                    }
                }

                throw new TimeoutException($"Async Operation Timeout: {asyncWaitTimeout}");
            }

            watch.Stop();

            return CheckAsyncOperationStatus(asyncJobId.Value);

        }

        private ImportStatus ExecuteImportOperation(ImportSolutionRequest request,
                  bool importAsync,
                  bool waitForCompletion,
                  int sleepInterval,
                  int asyncWaitTimeout
                  )
        {

            ImportStatus result = null;

            if (importAsync)
            {
                var asyncResult = ExecuteAsyncOperation(request, waitForCompletion, sleepInterval, asyncWaitTimeout);
                result = CheckImportStatus();
                result.ImportAsyncId = asyncResult.ImportAsyncId;
                result.ImportMessage = $"{result.ImportMessage}, Async:{asyncResult.ImportMessage}";


            }
            else
            {
                CurrentOrganizationService.Execute(request);
                result = CheckImportStatus();
            }

            return result;
        }

       

        public void DeactivateProcess(Guid processId)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("workflow", processId),
                State = new OptionSetValue(0),
                Status = new OptionSetValue(1)
            };
            _crmAccess.CurrentServiceProxy.Execute(activateRequest);
        }

        public void ActivateProcess(Guid processId)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("workflow", processId),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(2)
            };
            _crmAccess.CurrentServiceProxy.Execute(activateRequest);
        }
    }

    #endregion ICrmImportRepository Interface Implementation
}