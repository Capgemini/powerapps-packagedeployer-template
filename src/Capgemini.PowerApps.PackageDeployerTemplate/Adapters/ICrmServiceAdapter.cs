namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// An en extended <see cref="IOrganizationService"/>.
    /// </summary>
    public interface ICrmServiceAdapter : IOrganizationService
    {
        /// <summary>
        /// Imports a word template.
        /// </summary>
        /// <param name="filePath">The path to the word template.</param>
        void ImportWordTemplate(string filePath);

        /// <summary>
        /// Query for records based on a single field matching any of the given values.
        /// </summary>
        /// <param name="entity">The entity logical name.</param>
        /// <param name="attribute">The attribute logical name.</param>
        /// <param name="values">The values to match on.</param>
        /// <param name="columnSet">The columns to select.</param>
        /// <returns>The matching records.</returns>
        EntityCollection RetrieveMultipleByAttribute(string entity, string attribute, IEnumerable<object> values, ColumnSet columnSet = null);

        /// <summary>
        /// Sets the state of a number of records in batch.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="statecode">The state code.</param>
        /// <param name="statuscode">The status code.</param>
        /// <returns>An <see cref="ExecuteMultipleResponse"/>.</returns>
        ExecuteMultipleResponse UpdateStateAndStatusForEntityInBatch(EntityCollection records, int statecode, int statuscode);

        /// <summary>
        /// Updates the state and status for an entity.
        /// </summary>
        /// <param name="entityLogicalName">The entity logical name.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="statecode">The state code.</param>
        /// <param name="status">The status code.</param>
        /// <returns>True on success.</returns>
        bool UpdateStateAndStatusForEntity(string entityLogicalName, Guid entityId, int statecode, int status);

        /// <summary>
        /// Execute multiple requests.
        /// </summary>
        /// <param name="requests">The requests.</param>
        /// <param name="continueOnError">Whether to continue on error.</param>
        /// <param name="returnResponses">Whether to return responses.</param>
        /// <returns>The <see cref="ExecuteMultipleResponse"/>.</returns>
        ExecuteMultipleResponse ExecuteMultiple(IEnumerable<OrganizationRequest> requests, bool continueOnError = true, bool returnResponses = true);

        /// <summary>
        /// Retrieve solution component object IDs of a given type and solution.
        /// </summary>
        /// <param name="solutionName">The unique name of the solution.</param>
        /// <param name="componentType">The type of the components.</param>
        /// <returns>A collection of object IDs.</returns>
        IEnumerable<Guid> RetrieveSolutionComponentObjectIds(string solutionName, int componentType);

        /// <summary>
        /// Retrieve deployed component records for the given solution(s) and component type.
        /// </summary>
        /// <param name="solutions">The solutions to get component records for.</param>
        /// <param name="solutionComponentType">The component type.</param>
        /// <param name="componentLogicalName">The logical name of the entity associated with the component type.</param>
        /// <param name="columnSet">The columns to select.</param>
        /// <returns>A collection of the component entity records.</returns>
        EntityCollection RetrieveDeployedSolutionComponents(IEnumerable<string> solutions, int solutionComponentType, string componentLogicalName, ColumnSet columnSet = null);
    }
}