using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    public interface ICrmServiceAdapter
    {
        IOrganizationService GetOrganizationService();
        void ImportWordTemplate(string filePath);
        EntityCollection QueryRecordsBySingleAttributeValue(string entity, string attribute, IEnumerable<object> values);
        EntityCollection RetrieveMultiple(QueryByAttribute query);
        ExecuteMultipleResponse SetRecordsStateInBatch(EntityCollection queryResponse, int statecode, int statuscode);
        void Update(Entity record);
    }
}