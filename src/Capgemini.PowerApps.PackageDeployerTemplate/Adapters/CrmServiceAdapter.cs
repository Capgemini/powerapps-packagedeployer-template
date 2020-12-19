using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    public class CrmServiceAdapter
    {
        private readonly CrmServiceClient crmSvc;

        public CrmServiceAdapter(CrmServiceClient crmSvc)
        {
            this.crmSvc = crmSvc;
        }

        public ExecuteMultipleResponse SetRecordStateByAttribute(string entity, int statecode, int statuscode, string attribute, IEnumerable<object> values)
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

            var retrieveMultipleResponse = crmSvc.RetrieveMultiple(query);
            if (retrieveMultipleResponse.Entities.Count == 0)
            {
                return new ExecuteMultipleResponse();
            }

            var batchId = crmSvc.CreateBatchOperationRequest($"Set state of {entity}", true, true);
            foreach (var matchingRecord in retrieveMultipleResponse.Entities)
            {
                crmSvc.UpdateStateAndStatusForEntity(entity, matchingRecord.Id, statecode, statuscode, batchId);
            }

            return crmSvc.ExecuteBatch(batchId);
        }

        public void ImportWordTemplate(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? 1 : 2);

            if (templateType.Value != 2)
            {
                throw new NotSupportedException("Only Word templates (.docx) files are supported.");
            }

            var logicalName = WordTemplateUtilities.GetEntityLogicalName(filePath);
            var targetEntityTypeCode = crmSvc.GetEntityTypeCode(logicalName);
            var entityTypeCode = WordTemplateUtilities.GetEntityTypeCode(filePath);

            if (targetEntityTypeCode != entityTypeCode)
            {
                WordTemplateUtilities.SetEntity(filePath, logicalName, targetEntityTypeCode);
            }

            var retrieveMultipleResponse = crmSvc.RetrieveMultiple(
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
                crmSvc.Create(documentTemplate);
            }
            else
            {
                crmSvc.Update(documentTemplate);
            }
        }

        public IOrganizationService GetOrganizationService() => (IOrganizationService)crmSvc.OrganizationWebProxyClient ?? crmSvc.OrganizationServiceProxy;

        public EntityCollection RetrieveMultiple(QueryByAttribute query) => crmSvc.RetrieveMultiple(query);

        public void Update(Entity record) => crmSvc.Update(record);
    }
}
