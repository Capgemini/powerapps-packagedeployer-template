namespace Capgemini.PowerApps.PackageDeployerTemplate.Extensions
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

    /// <summary>
    /// Extensions for <see cref="IOrganizationService"/>.
    /// </summary>
    public static class IOrganizationServiceExtensions
    {
        /// <summary>
        /// Execute multiple requests.
        /// </summary>
        /// <param name="orgSvc">The <see cref="IOrganizationService"/>.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="continueOnError">Whether to continue on error.</param>
        /// <param name="returnResponses">Whether to return responses.</param>
        /// <returns>The <see cref="ExecuteMultipleResponse"/>.</returns>
        public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService orgSvc, IEnumerable<OrganizationRequest> requests, bool continueOnError = true, bool returnResponses = true)
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

            return (ExecuteMultipleResponse)orgSvc.Execute(executeMultipleRequest);
        }
    }
}