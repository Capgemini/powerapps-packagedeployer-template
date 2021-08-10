namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmServiceAdapter"/> class.
        /// </summary>
        /// <param name="crmSvc">The <see cref="CrmServiceClient"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CrmServiceAdapter(CrmServiceClient crmSvc, ILogger logger)
        {
            this.crmSvc = crmSvc;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Guid? CallerAADObjectId { get => this.crmSvc.CallerAADObjectId; set => this.crmSvc.CallerAADObjectId = value; }

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

            var query = new QueryExpression(entity)
            {
                ColumnSet = columnSet ?? new ColumnSet(false),
                Criteria = new FilterExpression()
                {
                    Conditions =
                    {
                        new ConditionExpression(attribute, ConditionOperator.In, values.ToArray()),
                    },
                },
            };

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
        public void ImportWordTemplate(FileInfo fileInfo, string entityLogicalName, OptionSetValue templateType, string filePath)
        {
            var retrieveMultipleResponse = this.crmSvc.RetrieveMultiple(
                new QueryByAttribute(Constants.DocumentTemplate.LogicalName) { Attributes = { Constants.DocumentTemplate.Fields.Name }, Values = { Path.GetFileNameWithoutExtension(fileInfo.Name) } });

            var documentTemplate = retrieveMultipleResponse.Entities.FirstOrDefault();
            if (documentTemplate == null)
            {
                documentTemplate = new Entity(Constants.DocumentTemplate.LogicalName);
                documentTemplate[Constants.DocumentTemplate.Fields.Name] = Path.GetFileNameWithoutExtension(fileInfo.Name);
            }

            documentTemplate[Constants.DocumentTemplate.Fields.AssociatedEntityTypeCode] = entityLogicalName;
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
        /// <exception cref="ArgumentException">Thrown if a user with the given domain name does not exist.</exception>
        public Guid RetrieveAzureAdObjectIdByDomainName(string domainName)
        {
            var systemUser = this.RetrieveMultipleByAttribute(
                Constants.SystemUser.LogicalName,
                Constants.SystemUser.Fields.DomainName,
                new string[] { domainName },
                new ColumnSet(Constants.SystemUser.Fields.AzureActiveDirectoryObjectId)).Entities.FirstOrDefault();

            if (systemUser == null)
            {
                throw new ArgumentException($"Unable to find a system user with a domain name of {domainName}");
            }

            return systemUser.GetAttributeValue<Guid>(Constants.SystemUser.Fields.AzureActiveDirectoryObjectId);
        }

        /// <inheritdoc/>
        public string GetEntityTypeCode(string entityLogicalName)
        {
            return this.crmSvc.GetEntityTypeCode(entityLogicalName);
        }

        /// <inheritdoc/>
        public TResponse Execute<TResponse>(OrganizationRequest request, string username, bool fallbackToExistingUser = true)
            where TResponse : OrganizationResponse
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (username is null)
            {
                throw new ArgumentNullException(nameof(username));
            }

            this.logger.LogDebug($"Executing {request.RequestName} as {username}.");

            var previousCallerObjectId = this.CallerAADObjectId;
            TResponse response = null;
            try
            {
                this.CallerAADObjectId = this.RetrieveAzureAdObjectIdByDomainName(username);
                response = (TResponse)this.Execute(request);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    this.logger.LogWarning($"Failed to execute {request.RequestName} as {username} as the user was not found.");
                }
                else
                {
                    this.logger.LogWarning(ex, $"Failed to execute {request.RequestName} as {username}. {ex.Message}");
                }

                if (!fallbackToExistingUser)
                {
                    throw;
                }
            }
            finally
            {
                this.CallerAADObjectId = previousCallerObjectId;
            }

            if (response != null)
            {
                return response;
            }

            this.logger.LogInformation($"Falling back to executing {request.RequestName} as {this.crmSvc.OAuthUserId}.");

            return (TResponse)this.Execute(request);
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
