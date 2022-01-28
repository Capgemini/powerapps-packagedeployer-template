namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Deployment functionality related to SDK steps.
    /// </summary>
    public class SdkStepDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkStepDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public SdkStepDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Activates SDK steps in the given solutions unless otherwise specified.
        /// </summary>
        /// <param name="solutions">The solutions to activate SDK steps in.</param>
        /// <param name="componentsToDeactivate">The names of any SDK steps to deactivate.</param>
        public void SetStatesBySolution(IEnumerable<string> solutions, IEnumerable<string> componentsToDeactivate = null)
        {
            this.logger.LogInformation("Setting SDK steps states in solution(s).");

            if (solutions == null || !solutions.Any())
            {
                this.logger.LogInformation($"No solutions were provided to activate SDK steps for.");
                return;
            }

            var deployedSdkSteps = this.crmSvc.RetrieveDeployedSolutionComponents(
                solutions,
                Constants.SolutionComponent.ComponentTypeSdkStep,
                Constants.SdkMessageProcessingStep.LogicalName,
                new ColumnSet(
                    Constants.SdkMessageProcessingStep.Fields.Name,
                    Constants.SdkMessageProcessingStep.Fields.StateCode)).Entities;

            this.SetStates(deployedSdkSteps, componentsToDeactivate);
        }

        /// <summary>
        /// Sets the states of SDK steps.
        /// </summary>
        /// <param name="componentsToActivate">The SDK steps to activate.</param>
        /// <param name="componentsToDeactivate">The SDK steps to deactivate.</param>
        public void SetStates(IEnumerable<string> componentsToActivate, IEnumerable<string> componentsToDeactivate = null)
        {
            this.logger.LogInformation("Setting SDK steps states.");

            if (componentsToActivate is null)
            {
                throw new ArgumentNullException(nameof(componentsToActivate));
            }

            var allSdkSteps = componentsToDeactivate != null ? componentsToActivate.Concat(componentsToDeactivate) : componentsToActivate;
            if (!allSdkSteps.Any())
            {
                this.logger.LogInformation($"No SDK steps were provided.");
                return;
            }

            var sdkSteps = this.RetrieveSdkSteps(allSdkSteps).Entities;
            if (!sdkSteps.Any())
            {
                this.logger.LogInformation($"No SDK steps were found with the names provided.");
                return;
            }

            this.logger.LogInformation($"Found {sdkSteps.Count} matching SDK steps.");
            if (sdkSteps.Count != allSdkSteps.Count())
            {
                this.logger.LogWarning($"Found {sdkSteps.Count} deployed SDK steps but expected {allSdkSteps.Count()}.");
            }

            this.SetStates(sdkSteps, componentsToDeactivate);
        }

        private void SetStates(IEnumerable<Entity> sdkSteps, IEnumerable<string> sdkStepsToDeactivate = null)
        {
            if (sdkSteps is null)
            {
                return;
            }

            var requests = new List<OrganizationRequest>();
            foreach (var deployedSdkStep in sdkSteps)
            {
                var stateCode = new OptionSetValue(Constants.SdkMessageProcessingStep.StateCodeActive);
                var statusCode = new OptionSetValue(Constants.SdkMessageProcessingStep.StatusCodeActive);

                if (sdkStepsToDeactivate != null && sdkStepsToDeactivate.Contains(deployedSdkStep[Constants.SdkMessageProcessingStep.Fields.Name]))
                {
                    stateCode.Value = Constants.SdkMessageProcessingStep.StateCodeInactive;
                    statusCode.Value = Constants.SdkMessageProcessingStep.StatusCodeInactive;
                }

                if (stateCode.Value == deployedSdkStep.GetAttributeValue<OptionSetValue>(Constants.SdkMessageProcessingStep.Fields.StateCode).Value)
                {
                    this.logger.LogInformation($"SDK step {deployedSdkStep[Constants.SdkMessageProcessingStep.Fields.Name]} already has desired state. Skipping.");
                    continue;
                }

                this.logger.LogInformation($"Setting SDK step status for {deployedSdkStep[Constants.SdkMessageProcessingStep.Fields.Name]} with statecode {stateCode.Value} and statuscode {statusCode.Value}");
                requests.Add(
                    new SetStateRequest
                    {
                        EntityMoniker = deployedSdkStep.ToEntityReference(),
                        State = stateCode,
                        Status = statusCode,
                    });
            }

            if (!requests.Any())
            {
                return;
            }

            var executeMultipleRes = this.crmSvc.ExecuteMultiple(requests, true, true, 120 + (requests.Count * 10));

            if (executeMultipleRes.IsFaulted)
            {
                this.logger.LogError("Error(s) encountered when setting SDK step states.");
                foreach (var failedResponse in executeMultipleRes.Responses.Where(r => r.Fault != null))
                {
                    var failedRequest = (SetStateRequest)requests[failedResponse.RequestIndex];
                    this.logger.LogError($"Failed to set state for SDK step {failedRequest.EntityMoniker.Name} with the following error: {failedResponse.Fault.Message}.");
                }
            }
        }

        private EntityCollection RetrieveSdkSteps(IEnumerable<string> names)
        {
            var query = new QueryExpression(Constants.SdkMessageProcessingStep.LogicalName)
            {
                ColumnSet = new ColumnSet(
                    Constants.SdkMessageProcessingStep.Fields.Name,
                    Constants.SdkMessageProcessingStep.Fields.StateCode),
            };
            query.Criteria.AddCondition(Constants.SdkMessageProcessingStep.Fields.Name, ConditionOperator.In, names.ToArray<object>());

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} SDK steps matching the {names.Count()} provided names.");

            return results;
        }
    }
}
