namespace Capgemini.PowerApps.PackageDeployerTemplate.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /** this class can be used in unit testing to locally debug OpenXml without the need to connect into any instance**/

    public class FakeCrmServiceAdapter : ICrmServiceAdapter
    {
        public Guid? CallerAADObjectId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public Guid Create(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(string entityName, Guid id)
        {
            throw new NotImplementedException();
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public TResponse Execute<TResponse>(OrganizationRequest request, string username, bool fallbackToExistingUser = true)
            where TResponse : OrganizationResponse
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }

        public ExecuteMultipleResponse ExecuteMultiple(IEnumerable<OrganizationRequest> requests, bool continueOnError = true, bool returnResponses = true)
        {
            throw new NotImplementedException();
        }

        public void ImportWordTemplate(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var templateType = new OptionSetValue(fileInfo.Extension.Equals("xlsx", StringComparison.OrdinalIgnoreCase) ? Constants.DocumentTemplate.DocumentTypeExcel : Constants.DocumentTemplate.DocumentTypeWord);

            if (templateType.Value != 2)
            {
                throw new NotSupportedException("Only Word templates (.docx) files are supported.");
            }

            var logicalName = WordTemplateUtilities.GetEntityLogicalName(filePath);
            var targetEntityTypeCode = "10348";
            var entityTypeCode = WordTemplateUtilities.GetEntityTypeCode(filePath);

            if (targetEntityTypeCode != entityTypeCode)
            {
                WordTemplateUtilities.SetEntity(filePath, logicalName, targetEntityTypeCode);
            }
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            throw new NotImplementedException();
        }

        public Guid RetrieveAzureAdObjectIdByDomainName(string domainName)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveDeployedSolutionComponents(IEnumerable<string> solutions, int solutionComponentType, string componentLogicalName, ColumnSet columnSet = null)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveMultipleByAttribute(string entity, string attribute, IEnumerable<object> values, ColumnSet columnSet = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> RetrieveSolutionComponentObjectIds(string solutionName, int componentType)
        {
            throw new NotImplementedException();
        }

        public void Update(Entity entity)
        {
            throw new NotImplementedException();
        }

        public bool UpdateStateAndStatusForEntity(string entityLogicalName, Guid entityId, int statecode, int status)
        {
            throw new NotImplementedException();
        }

        public ExecuteMultipleResponse UpdateStateAndStatusForEntityInBatch(EntityCollection records, int statecode, int statuscode)
        {
            throw new NotImplementedException();
        }
    }
}
