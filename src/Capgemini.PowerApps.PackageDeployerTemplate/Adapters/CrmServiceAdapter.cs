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
        public ExecuteMultipleResponse ExecuteMultiple(IEnumerable<OrganizationRequest> requests, bool continueOnError = true, bool returnResponses = true)
        {
            var executeMultipleRequest = new ExecuteMultipleRequest
            {
                Requests = new OrganizationRequestCollection(),
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = continueOnError,
                    ReturnResponses = returnResponses,
                },
            };
            executeMultipleRequest.Requests.AddRange(requests);

            return (ExecuteMultipleResponse)this.crmSvc.Execute(executeMultipleRequest);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> RetrieveSolutionComponentObjectIds(string solutionName, int componentType)
        {
            var queryExpression = new QueryExpression(Constants.SolutionComponent.LogicalName)
            {
                ColumnSet = new ColumnSet(new string[] { Constants.SolutionComponent.Fields.ObjectId }),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            queryExpression.AddLink(
                Constants.Solution.LogicalName,
                Constants.SolutionComponent.Fields.SolutionId,
                Constants.Solution.Fields.SolutionId);
            queryExpression.Criteria.AddCondition(Constants.SolutionComponent.Fields.ComponentType, ConditionOperator.Equal, componentType);
            queryExpression.Criteria.AddCondition(Constants.Solution.LogicalName, Constants.Solution.Fields.UniqueName, ConditionOperator.Equal, solutionName);

            var results = this.crmSvc.RetrieveMultiple(queryExpression);

            return results.Entities.Select(e => e.GetAttributeValue<Guid>(Constants.SolutionComponent.Fields.ObjectId)).ToArray();
        }

        /// <inheritdoc/>
        public EntityCollection RetrieveMultipleByAttribute(string entity, string attribute, IEnumerable<object> values, ColumnSet columnSet = null)
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
                ColumnSet = columnSet ?? new ColumnSet(false),
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
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? Constants.DocumentTemplate.DocumentTypeExcel : Constants.DocumentTemplate.DocumentTypeWord);

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
                new QueryByAttribute(Constants.DocumentTemplate.LogicalName) { Attributes = { Constants.DocumentTemplate.Fields.Name }, Values = { Path.GetFileNameWithoutExtension(fileInfo.Name) } });

            var documentTemplate = retrieveMultipleResponse.Entities.FirstOrDefault();
            if (documentTemplate == null)
            {
                documentTemplate = new Entity(Constants.DocumentTemplate.LogicalName);
                documentTemplate[Constants.DocumentTemplate.Fields.Name] = Path.GetFileNameWithoutExtension(fileInfo.Name);
            }

            documentTemplate[Constants.DocumentTemplate.Fields.AssociatedEntityTypeCode] = logicalName;
            documentTemplate[Constants.DocumentTemplate.Fields.DocumentType] = templateType;
            documentTemplate[Constants.DocumentTemplate.Fields.Content] = Convert.ToBase64String(File.ReadAllBytes(filePath));

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
        public EntityCollection RetrieveDeployedSolutionComponents(IEnumerable<string> solutions, int solutionComponentType, string componentLogicalName, ColumnSet columnSet = null)
        {
            if (solutions is null)
            {
                throw new ArgumentNullException(nameof(solutions));
            }

            if (!solutions.Any())
            {
                return new EntityCollection();
            }

            var objectIds = solutions.SelectMany(s => this.RetrieveSolutionComponentObjectIds(s, solutionComponentType));
            if (!objectIds.Any())
            {
                return new EntityCollection();
            }

            var entityQuery = new QueryExpression(componentLogicalName)
            {
                ColumnSet = columnSet ?? new ColumnSet(false),
                Criteria = new FilterExpression(),
            };
            entityQuery.Criteria.AddCondition($"{componentLogicalName}id", ConditionOperator.In, objectIds.Cast<object>().ToArray());

            return this.RetrieveMultiple(entityQuery);
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
