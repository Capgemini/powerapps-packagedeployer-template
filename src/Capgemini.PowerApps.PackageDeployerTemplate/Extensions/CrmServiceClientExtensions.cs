using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Capgemini.PowerApps.PackageDeployerTemplate.Config;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Extensions
{
    public static class CrmServiceClientExtensions
    {
        public static ExecuteMultipleResponse SetRecordStateByAttribute(this CrmServiceClient svcClient, string entity, int statecode, int statuscode, string attribute, IEnumerable<object> values)
        {
            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentException("You must provide an entity logical name.", nameof(entity));
            }

            if (string.IsNullOrEmpty(attribute))
            {
                throw new ArgumentException("You must provide an attribute.", nameof(attribute));
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var query = new QueryByAttribute(entity)
            {
                Attributes = { attribute },
                ColumnSet = new ColumnSet(false)
            };
            query.Values.AddRange(values);

            if (entity.Equals("workflow", StringComparison.OrdinalIgnoreCase))
            {
                query.AddAttributeValue("type", 1);
            }

            var retrieveMultipleResponse = svcClient.RetrieveMultiple(query);
            if (retrieveMultipleResponse.Entities.Count == 0)
            {
                return new ExecuteMultipleResponse();
            }

            var batchId = svcClient.CreateBatchOperationRequest($"Set state of {entity}", true, true);
            foreach (var matchingRecord in retrieveMultipleResponse.Entities)
            {
                svcClient.UpdateStateAndStatusForEntity(entity, matchingRecord.Id, statecode, statuscode, batchId);
            }

            return svcClient.ExecuteBatch(batchId);
        }

        public static void ImportWordTemplate(this CrmServiceClient svcClient, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? 1 : 2);

            if (templateType.Value != 2)
            {
                throw new NotSupportedException("Only Word templates (.docx) files are supported.");
            }

            var logicalName = WordTemplateUtilities.GetEntityLogicalName(filePath);
            var targetEntityTypeCode = svcClient.GetEntityTypeCode(logicalName);
            var entityTypeCode = WordTemplateUtilities.GetEntityTypeCode(filePath);

            if (targetEntityTypeCode != entityTypeCode)
            {
                WordTemplateUtilities.SetEntity(filePath, logicalName, targetEntityTypeCode);
            }

            var retrieveMultipleResponse = svcClient.RetrieveMultiple(
                new QueryByAttribute("documenttemplate") { Attributes = { "name" }, Values = { Path.GetFileNameWithoutExtension(fileInfo.Name) } });

            var documentTemplate = retrieveMultipleResponse.Entities.FirstOrDefault();
            if (documentTemplate == null)
            {
                documentTemplate = new Entity("documenttemplate");
                documentTemplate["name"] = Path.GetFileNameWithoutExtension(fileInfo.Name);
            }
            documentTemplate["associatedentitytypecode"] = logicalName;
            documentTemplate["documenttype"] = templateType;
            documentTemplate["content"] = Convert.ToBase64String(File.ReadAllBytes(filePath));

            if (documentTemplate.Id == Guid.Empty)
            {
                svcClient.Create(documentTemplate);
            }
            else
            {
                svcClient.Update(documentTemplate);
            }
        }

        /// <summary>
        /// Sets the connection on a flow.
        /// </summary>
        /// <param name="workflowId">The ID of the flow.</param>
        /// <param name="apiName">The API name (e.g. shared_sharepointonline).</param>
        /// <param name="connectionName">The connection name.</param>
        /// <param name="activate">Whether to activate after setting the connection.</param>
        public static void SetFlowConnection(this CrmServiceClient svcClient, FlowConfig connection)
        {
            //if (string.IsNullOrEmpty(connection.FlowSharedConnectionName))
            //{
            //    throw new ArgumentException("You must provide an API name.", nameof(connection.FlowSharedConnectionName));
            //}

            //var flow = svcClient.Retrieve("workflow", workflowId, new ColumnSet("clientdata", "statecode", "statuscode"));
           
            //flow["clientdata"] = this.GetClientDataWithConnectionName(flow.GetAttributeValue<string>("clientdata"), apiName, connectionName);

            //svcClient.Update(flow);

            //if (connection.ActivateFlow)
            //{
            //    if (!svcClient.UpdateStateAndStatusForEntity("workflow", flowId, 1, 2))
            //    {
            //        throw new InvalidOperationException($"Failed to activatate flow {flowId}.");
            //    }
            //}
        }

    }
}
