namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;

    /// <summary>
    /// An extended <see cref="IOrganizationService"/> built on <see cref="CrmServiceClient"/>.
    /// </summary>
    public class CrmServiceAdapter : ICrmServiceAdapter, IDisposable
    {
        private readonly CrmServiceClient crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmServiceAdapter"/> class.
        /// </summary>
        /// <param name="crmSvc">The <see cref="CrmServiceClient"/>.</param>
        public CrmServiceAdapter(CrmServiceClient crmSvc)
        {
            this.crmSvc = crmSvc;
        }

        /// <inheritdoc/>
        public EntityCollection RetrieveMultipleByAttribute(string entity, string attribute, IEnumerable<object> values)
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
                ColumnSet = new ColumnSet(false),
            };
            query.Values.AddRange(values);

            return this.crmSvc.RetrieveMultiple(query);
        }

        /// <inheritdoc/>
        public ExecuteMultipleResponse UpdateStateAndStatusForEntityInBatch(EntityCollection records, int statecode, int statuscode)
        {
            if (records is null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            if (string.IsNullOrEmpty(records.EntityName))
            {
                throw new ArgumentException($"{nameof(EntityCollection.EntityName)} must have a value.", nameof(records));
            }

            if (records.Entities.Count == 0)
            {
                return new ExecuteMultipleResponse();
            }

            if (statecode < 0)
            {
                throw new ArgumentException("You must provide a value greater than or equal to 0.", nameof(statecode));
            }

            if (statuscode < 0)
            {
                throw new ArgumentException("You must provide a value greater than or equal to 0.", nameof(statuscode));
            }

            var batchId = this.crmSvc.CreateBatchOperationRequest($"Set state of {records.EntityName}", true, true);
            foreach (var matchingRecord in records.Entities)
            {
                this.crmSvc.UpdateStateAndStatusForEntity(records.EntityName, matchingRecord.Id, statecode, statuscode, batchId);
            }

            return this.crmSvc.ExecuteBatch(batchId);
        }

        /// <inheritdoc/>
        public void ImportWordTemplate(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? 1 : 2);

            if (templateType.Value != 2)
            {
                throw new NotSupportedException("Only Word templates (.docx) files are supported.");
            }

            var logicalName = WordTemplateUtilities.GetEntityLogicalName(filePath);
            var targetEntityTypeCode = this.crmSvc.GetEntityTypeCode(logicalName);
            var entityTypeCode = WordTemplateUtilities.GetEntityTypeCode(filePath);

            if (targetEntityTypeCode != entityTypeCode)
            {
                WordTemplateUtilities.SetEntity(filePath, logicalName, targetEntityTypeCode);
            }

            var retrieveMultipleResponse = this.crmSvc.RetrieveMultiple(
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
                this.crmSvc.Create(documentTemplate);
            }
            else
            {
                this.crmSvc.Update(documentTemplate);
            }
        }

        /// <inheritdoc/>
        public bool UpdateStateAndStatusForEntity(string entityLogicalName, Guid entityId, int statecode, int status) => this.crmSvc.UpdateStateAndStatusForEntity(entityLogicalName, entityId, statecode, status);

        /// <inheritdoc/>
        public void Update(Entity entity) => this.crmSvc.Update(entity);

        /// <inheritdoc/>
        public Guid Create(Entity entity) => this.crmSvc.Create(entity);

        /// <inheritdoc/>
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => this.crmSvc.Retrieve(entityName, id, columnSet);

        /// <inheritdoc/>
        public void Delete(string entityName, Guid id) => this.crmSvc.Delete(entityName, id);

        /// <inheritdoc/>
        public OrganizationResponse Execute(OrganizationRequest request) => this.crmSvc.Execute(request);

        /// <inheritdoc/>
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
            => this.crmSvc.Associate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc/>
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
            this.crmSvc.Disassociate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc/>
        public EntityCollection RetrieveMultiple(QueryBase query) => this.crmSvc.RetrieveMultiple(query);

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.crmSvc.Dispose();
        }
    }
}
