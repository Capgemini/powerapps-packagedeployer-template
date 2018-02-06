using Capgemini.Xrm.Deployment.Core.Exceptions;
using Capgemini.Xrm.Deployment.Core.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Linq;

namespace Capgemini.Xrm.Deployment.Core
{
    public class CrmAccess
    {
        protected const string EntityImageField = "entityimage";

        #region Constructors

        public CrmAccess(IOrganizationService service)
        {
            this._serviceProxy = service;
        }

        #endregion Constructors

        #region Public class implementation

        public EntityCollection GetDataByQuery(QueryExpression query, int pageSize)
        {
            return GetDataByQuery(query, pageSize, null, null);
        }

        public Entity GetEntity(string entityName, Guid crmId, string[] columns)
        {
            var colSet = columns != null ? new ColumnSet(columns.Where(c => c != null).Distinct().ToArray()) : new ColumnSet(true);
            var entity = this.CurrentServiceProxy.Retrieve(entityName, crmId, colSet);
            return entity;
        }

        public EntityCollection GetEntitiesByColumn(string entityName, string columnName, object columnValue, string[] columnsToRetrieve = null, int pageSize = 0)
        {
            pageSize = GetPageSize(pageSize);

            var query = new QueryExpression(entityName);
            query.ColumnSet = columnsToRetrieve != null ? new ColumnSet(columnsToRetrieve) : new ColumnSet(true);

            if (!string.IsNullOrWhiteSpace(columnName) && columnValue != null)
                query.Criteria.AddCondition(columnName, ConditionOperator.Equal, columnValue);
            else if (!string.IsNullOrWhiteSpace(columnName) && columnValue == null)
                query.Criteria.AddCondition(columnName, ConditionOperator.Null);

            return GetDataByQuery(query, pageSize);
        }

        protected IOrganizationService _serviceProxy;

        public virtual IOrganizationService CurrentServiceProxy
        {
            get
            {
                return _serviceProxy;
            }
        }

        public void SetEntityStatus(Entity entity, int stateCode, int statusCode)
        {
            SetEntityStatus(entity.ToEntityReference(), stateCode, statusCode);
        }

        public void SetEntityStatus(EntityReference entityReference, int stateCode, int statusCode)
        {
            var request = new SetStateRequest
            {
                EntityMoniker = entityReference,
                State = new OptionSetValue(stateCode),
                Status = new OptionSetValue(statusCode)
            };

            var response = this.CurrentServiceProxy.Execute(request) as SetStateResponse;
        }

        public void SetEntityPicture(EntityReference entityReference, byte[] picture)
        {
            var ent = this.GetEntity(entityReference.LogicalName, entityReference.Id, new string[] { EntityImageField });

            if (ent == null)
                throw new ValidationException("Cannot retrieve entity:" + entityReference.LogicalName + ", id:" + entityReference.Id);

            ent[EntityImageField] = picture;

            this.CurrentServiceProxy.Update(ent);
        }

        public void SetEntityPicture(string filePath, char seperator)
        {
            var file = new FileInfo(filePath);

            var items = file.Name.Split(seperator);

            if (items == null || items.Count() < 2)
                throw new ValidationException(String.Format("Cannot extract entity name and id from filepath: {0}, using seperator: {1}", filePath, seperator));

            //Get enity name
            string entName = items[0];

            //Get entity id
            var entId = items[1].Remove(items[1].LastIndexOf('.'));

            Guid id;
            if (!Guid.TryParse(entId, out id))
                throw new ValidationException("Cannot parse id:" + entId);

            SetEntityPicture(new EntityReference(entName, id), File.ReadAllBytes(filePath));
        }

        public ConfigurationParameter GetConfigurationParameter(string key)
        {
            var retrievedParam = GetEntitiesByColumn("cap_configurationparameter", "cap_key", key, new string[] { "cap_value" }, 10);

            if (retrievedParam.Entities.Count == 0)
                throw new ConfigurationException("Configuration Parameter: " + key + " not found");

            var conf = new ConfigurationParameter();
            conf.Value = retrievedParam[0].GetAttributeValue<String>("cap_value");
            conf.Id = retrievedParam[0].Id;
            return conf;
        }

        public void ExecuteWorkflow(Guid workflowId, Guid entityId)
        {
            var request = new ExecuteWorkflowRequest
            {
                WorkflowId = workflowId,
                EntityId = entityId
            };

            this.CurrentServiceProxy.Execute(request);
        }

        public void ExecuteWorkflow(string confKey)
        {
            var conf = GetConfigurationParameter(confKey);
            ExecuteWorkflow(FindWorkflowByName(conf.Value), conf.Id);
        }

        public int RunPagedRetrieveMultiple(QueryExpression pagequery, Action<Entity> operation, int pageSize = 0)
        {
            pageSize = GetPageSize(pageSize);

            var result = GetDataByQuery(pagequery, pageSize, operation, null);
            return result.Entities.Count;
        }

        public int RunPagedRetrieveMultipleAnExecuteMultiple(QueryExpression pagequery, Func<Entity, OrganizationRequest> operation, int pageSize = 0)
        {
            pageSize = GetPageSize(pageSize);

            var result = GetDataByQuery(pagequery, pageSize, null, operation);
            return result.Entities.Count;
        }

        public int GetEntityCode(string entityName)
        {
            var code = GetEntityMetadata(entityName).ObjectTypeCode.Value;

            return code;
        }

        public EntityMetadata GetEntityMetadata(string entityName)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = entityName
            };

            RetrieveEntityResponse response = (RetrieveEntityResponse)_serviceProxy.Execute(request);

            return response.EntityMetadata;
        }

        #endregion Public class implementation

        #region Protected and Private Methods

        private int GetPageSize(int requestedPageSize)
        {
            if (requestedPageSize <= 0)
            {
                try
                {
                    if (!int.TryParse(GetConfigurationParameter(ConfigurationParameter.ExecuteMultipleRequestsKey).Value, out requestedPageSize))
                    {
                        requestedPageSize = 500;
                    }
                }
                catch
                {
                    requestedPageSize = 500;
                }
            }

            if (requestedPageSize <= 0)
                requestedPageSize = 500;

            return requestedPageSize;
        }

        private Guid FindWorkflowByName(string name)
        {
            var objQueryExpression = new QueryExpression("workflow");
            objQueryExpression.ColumnSet = new ColumnSet(true);
            objQueryExpression.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, name));
            objQueryExpression.Criteria.AddCondition(new ConditionExpression("parentworkflowid", ConditionOperator.Null));

            var entities = this.CurrentServiceProxy.RetrieveMultiple(objQueryExpression);

            if (!entities.Entities.Any())
            {
                throw new ConfigurationException(string.Format("Workflow {0} not found", name));
            }
            return entities.Entities[0].Id;
        }

        private EntityCollection GetDataByQuery(QueryExpression query, int pageSize, Action<Entity> actionToExecute, Func<Entity, OrganizationRequest> requestToExecute)
        {
            var allResults = new EntityCollection();

            query.PageInfo = new PagingInfo
            {
                Count = pageSize,
                PageNumber = 1,
                PagingCookie = null
            };

            while (true)
            {
                var pagedResults = this.CurrentServiceProxy.RetrieveMultiple(query);

                if (query.PageInfo.PageNumber == 1)
                    allResults = pagedResults;
                else
                    allResults.Entities.AddRange(pagedResults.Entities);

                if (actionToExecute != null)
                {
                    // Retrieve all records from the result set.
                    foreach (Entity res in pagedResults.Entities)
                    {
                        actionToExecute(res);
                    }
                }

                if (requestToExecute != null)
                {
                    var request = new ExecuteMultipleRequest();

                    foreach (Entity res in pagedResults.Entities)
                    {
                        request.Requests.Add(requestToExecute(res));
                    }

                    var response = (ExecuteMultipleResponse)CurrentServiceProxy.Execute(request);
                }

                if (pagedResults.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = pagedResults.PagingCookie;
                }
                else
                    break;
            }

            return allResults;
        }

        #endregion Protected and Private Methods
    }

    public abstract class CrmAccess<TCrmContext> : CrmAccess, IDisposable
        where TCrmContext : OrganizationServiceContext
    {
        #region Constructors

        protected CrmAccess(IOrganizationService service)
            : base(service)
        { }

        #endregion Constructors

        #region Public class implementation

        private TCrmContext _crmContext;

        public TCrmContext CurrentCrmContext
        {
            get
            {
                if (this._crmContext == null)
                {
                    this._crmContext = (TCrmContext)Activator.CreateInstance(typeof(TCrmContext), this.CurrentServiceProxy);
                }
                return this._crmContext;
            }
        }

        public void UpdateEntity(Entity entity)
        {
            SaveEntity(entity, true);
        }

        public void UpdateEntityDeffered(Entity entity)
        {
            SaveEntity(entity, false);
        }

        protected virtual void SaveEntity(Entity entity, bool doSave)
        {
            if (entity != null)
            {
                var attachedEntity = this.CurrentCrmContext.GetAttachedEntities().SingleOrDefault(p => p.Id == entity.Id);
                if (attachedEntity != null)
                    this.CurrentCrmContext.Detach(attachedEntity);

                this.CurrentCrmContext.Attach(entity);
                this.CurrentCrmContext.UpdateObject(entity);
                if (doSave) this.CurrentCrmContext.SaveChanges();
            }
        }

        #endregion Public class implementation

        #region IDisposable imlemenatation

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_crmContext != null)
                    _crmContext.Dispose();
            }
            //Dispose unmanaged resource if any
        }

        #endregion IDisposable imlemenatation
    }
}