using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Repository.Events;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
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

            OnRaiseImportUpdatEvent(new AsyncImportUpdateEventArgs
            {
                Message = outResult.ImportMessage,
                ImportState = outResult.ImportState,
                ImportStatusCode = outResult.ImportStatusCode
            });

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

            OnRaiseImportUpdatEvent(new AsyncImportUpdateEventArgs
            {
                Message = result.ImportMessage,
                ImportState = result.ImportState,
                ImportStatusCode = result.ImportStatusCode
            });

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

            Guid? asyncJobId = null;
            _importJobId = Guid.NewGuid();

            var result = new ImportStatus
            {
                ImportId = _importJobId
            };

            var solutionBytes = File.ReadAllBytes(solutionFilePath);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = solutionBytes,
                PublishWorkflows = publishWorkflows,
                ConvertToManaged = convertToManaged,
                OverwriteUnmanagedCustomizations = overwriteUnmanagedCustomizations,
                ImportJobId = _importJobId.Value,
                HoldingSolution = useHoldingSolution
            };

            if (importAsync)
            {
                var asyncRequest = new ExecuteAsyncRequest
                {
                    Request = importSolutionRequest
                };

                var asyncResponse = CurrentOrganizationService.Execute(asyncRequest) as ExecuteAsyncResponse;

                asyncJobId = asyncResponse.AsyncJobId;
                result.ImportAsyncId = asyncJobId;

                if (waitForCompletion)
                {
                    var end = DateTime.Now.AddSeconds(asyncWaitTimeout);

                    while (end >= DateTime.Now)
                    {
                        var asyncOperation = CheckAsyncImportStatus(asyncJobId.Value);

                        int statusCode = asyncOperation.ImportStatusCode;

                        switch (statusCode)
                        {
                            //Succeeded
                            case 30:
                                result = CheckImportStatus();
                                return result;
                            //Pausing //Canceling //Failed //Canceled
                            case 21:
                            case 22:
                            case 31:
                            case 32:
                                throw new Exception(string.Format("Solution Import Failed: {0} {1}", statusCode, asyncOperation.ImportMessage));
                        }

                        Thread.Sleep(sleepInterval);
                    }

                    throw new Exception(string.Format("Import Timeout: {0}", asyncWaitTimeout));
                }

                result = CheckAsyncImportStatus(asyncJobId.Value);
                return result;
            }
            else
            {
                try
                {
                    var importSolutionResponse = CurrentOrganizationService.Execute(importSolutionRequest) as ImportSolutionResponse;
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

        public void ApplySolutionUpgrade(string solutionName)
        {
            _importJobId = Guid.NewGuid();

            var upgradeRequest = new DeleteAndPromoteRequest
            {
                UniqueName = solutionName,
                RequestId = _importJobId
            };

            var upgradeResponse = CurrentOrganizationService.Execute(upgradeRequest) as DeleteAndPromoteResponse;
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