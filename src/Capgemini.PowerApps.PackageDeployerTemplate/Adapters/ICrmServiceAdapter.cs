namespace Capgemini.PowerApps.PackageDeployerTemplate.Adapters
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

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
        /// <returns>The matching records.</returns>
        EntityCollection RetrieveMultipleByAttribute(string entity, string attribute, IEnumerable<object> values);

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
    }
}