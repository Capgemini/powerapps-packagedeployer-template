namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// An en extended <see cref="IOrganizationService"/>.
    /// </summary>
    public interface ICrmServiceAdapter : IOrganizationService
    {
        /// <summary>
        /// Gets or sets the AAD object ID of the caller.
        /// </summary>
        Guid? CallerAADObjectId { get; set; }

        /// <summary>
        /// Imports a word template.
        /// </summary>
        /// <param name="fileInfo">Information about file.</param>
        /// <param name="entityLogicalName">The entity schema name the document is based off.</param>
        /// <param name="templateType">The template extension type.</param>
        /// <param name="filePath">The path to the word template.</param>
        void ImportWordTemplate(FileInfo fileInfo, string entityLogicalName, OptionSetValue templateType, string filePath);

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
        /// Execute multiple requests.
        /// </summary>
        /// <param name="requests">The requests.</param>
        /// <param name="continueOnError">Whether to continue on error.</param>
        /// <param name="returnResponses">Whether to return responses.</param>
        /// <returns>The <see cref="ExecuteMultipleResponse"/>.</returns>
        ExecuteMultipleResponse ExecuteMultiple(IEnumerable<OrganizationRequest> requests, bool continueOnError = true, bool returnResponses = true);

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
        /// Retrives the Azure AD object ID for a user by domain name (or null if not found).
        /// </summary>
        /// <param name="domainName">The domain name of the system user.</param>
        /// <returns>The Azure AD object ID (or null if not found).</returns>
        /// <exception cref="ArgumentException">Thrown when the specified user doesn't exist.</exception>
        Guid RetrieveAzureAdObjectIdByDomainName(string domainName);

        /// <summary>
        /// Executes a request as a particular user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="username">The user to impersonate.</param>
        /// <param name="fallbackToExistingUser">Whether to fallback to the authenticated user if the action fails as the specified user.</param>
        /// <typeparam name="TResponse">The type of response.</typeparam>
        /// <returns>The response.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified user doesn't exist and fallback is disabled.</exception>
        public TResponse Execute<TResponse>(OrganizationRequest request, string username, bool fallbackToExistingUser = true)
            where TResponse : OrganizationResponse;

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

        /// <summary>
        /// Retrieve solution component object IDs of a given type and solution.
        /// </summary>
        /// <param name="entityLogicalName">The unique name of the solution.</param>
        /// <returns> EntityTypeCode for a given entity type.</returns>
        string GetEntityTypeCode(string entityLogicalName);
    }
}
