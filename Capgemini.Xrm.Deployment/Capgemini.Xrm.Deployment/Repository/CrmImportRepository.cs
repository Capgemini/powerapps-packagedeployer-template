using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Core.Model;
using Capgemini.Xrm.Deployment.Repository.Events;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmImportRepository : CrmRepository, ICrmImportRepository
    {
        #region Constructors and Private Fields

        private Guid? _importJobId;

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

        public ImportStatus CheckAsyncImportStatus(Guid asyncJobId)
        {
            ImportStatus outResult;

            try
            {
                var asyncOperation = CurrentOrganizationService.Retrieve("asyncoperation", asyncJobId, new ColumnSet("asyncoperationid", "statuscode", "message"));

                int statusCode = asyncOperation.GetAttributeValue<OptionSetValue>("statuscode").Value;

                outResult = new ImportStatus
                {
                    ImportAsyncId = asyncJobId,
                    ImportState = statusCode.ToString(),
                    ImportMessage = asyncOperation.GetAttributeValue<string>("message"),
                    ImportId = _importJobId.Value,
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
                    ImportId = _importJobId.Value,
                    ImportStatusCode = 0
                };
            }

            return outResult;
        }

        public ImportStatus CheckImportStatus()
        {
            var job = CurrentOrganizationService.Retrieve("importjob", _importJobId.Value, new ColumnSet(new System.String[] { "data", "solutionname" }));

            if (job == null) throw new Exception("Invalid importjob id");

            var reportData = job.GetAttributeValue<string>("data");

            var doc = new XmlDocument();
            doc.LoadXml(reportData);

            String ImportedSolutionName = doc.SelectSingleNode("//solutionManifest/UniqueName").InnerText;
            var resultNode = doc.SelectSingleNode("//solutionManifest/result/@result");

            String SolutionImportResult = resultNode != null ? resultNode.Value : "";

            var result = new ImportStatus
            {
                ImportId = _importJobId.Value,
                ImportState = SolutionImportResult,
                SolutionName = ImportedSolutionName,
                ImportMessage = reportData
            };

            _importJobId = null;

            return result;
        }

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

            if (_importJobId.HasValue) throw new Exception("Import already in progress");
            _importJobId = Guid.NewGuid();

            var solutionBytes = File.ReadAllBytes(solutionFilePath);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = publishWorkflows,
                ConvertToManaged = convertToManaged,
                OverwriteUnmanagedCustomizations = overwriteUnmanagedCustomizations,
                ImportJobId = _importJobId.Value,
                HoldingSolution = useHoldingSolution,
                SkipProductUpdateDependencies = true
            };

            var result = ExecuteImportOperation(importSolutionRequest, importAsync, waitForCompletion, sleepInterval, asyncWaitTimeout);
            return result;

        }

        public ImportStatus ApplySolutionUpgrade(string solutionName)
        {
            if (_importJobId.HasValue) throw new Exception("Import already in progress");
            _importJobId = Guid.NewGuid();

            var upgradeRequest = new DeleteAndPromoteRequest
            {
                UniqueName = solutionName,
                RequestId = _importJobId
            };

            var upgradeResponse = CurrentOrganizationService.Execute(upgradeRequest) as DeleteAndPromoteResponse;

            var result= new ImportStatus
            {
                ImportId = _importJobId,
                ImportMessage = "Solution has been promoted to the new version",
                ImportState = "Promoted",
                SolutionName = solutionName
            };

            _importJobId = null;

            return result;
        }

        private ImportStatus ExecuteImportOperation(OrganizationRequest request,
                  bool importAsync,
                  bool waitForCompletion,
                  int sleepInterval,
                  int asyncWaitTimeout
                  )
        {
           
            Guid? asyncJobId = null;
 
            var result = new ImportStatus
            {
                ImportId = _importJobId
            };

            if (importAsync)
            {
                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = request
                };

                var asyncResponse = CurrentOrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

                asyncJobId = asyncResponse.AsyncJobId;
                result.ImportAsyncId = asyncJobId;

                var watch = Stopwatch.StartNew();
                watch.Start();

                if (waitForCompletion)
                {
                    var end = DateTime.Now.AddSeconds(asyncWaitTimeout);

                    while (end >= DateTime.Now)
                    {
                        Thread.Sleep(sleepInterval);

                        var asyncOperation = CheckAsyncImportStatus(asyncJobId.Value);

                        AsyncOperationStatusEnum statusCode = (AsyncOperationStatusEnum)asyncOperation.ImportStatusCode;

                        switch (statusCode)
                        {
                            case AsyncOperationStatusEnum.Succeeded:
                                watch.Stop();
                                result = CheckImportStatus();
                                return result;

                            case AsyncOperationStatusEnum.Pausing:
                            case AsyncOperationStatusEnum.Canceling:
                            case AsyncOperationStatusEnum.Failed:
                            case AsyncOperationStatusEnum.Canceled:
                                watch.Stop();
                                throw new Exception(string.Format("Solution Import Failed: {0} {1}", statusCode, asyncOperation.ImportMessage));

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

                    throw new Exception(string.Format("Import Timeout: {0}", asyncWaitTimeout));
                }

                watch.Stop();

                result = CheckAsyncImportStatus(asyncJobId.Value);
                return result;
            }
            else
            {
                try
                {
                    var importSolutionResponse = CurrentOrganizationService.Execute(request);
                }
                catch (Exception)
                {
                    result = CheckImportStatus();
                    throw;
                }
            }

            result = CheckImportStatus();
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