namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
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
                new ColumnSet(Constants.SdkMessageProcessingStep.Fields.Name)).Entities;

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

        private void SetStates(IEnumerable<Entity> sdkSteps, IEnumerable<string> sdkStepsToDeactivate)
        {
            if (sdkSteps is null)
            {
                return;
            }

            foreach (var deployedSdkStep in sdkSteps)
            {
                var stateCode = Constants.SdkMessageProcessingStep.StateCodeActive;
                var statusCode = Constants.SdkMessageProcessingStep.StatusCodeActive;

                if (sdkStepsToDeactivate != null && sdkStepsToDeactivate.Contains(deployedSdkStep[Constants.SdkMessageProcessingStep.Fields.Name]))
                {
                    stateCode = Constants.SdkMessageProcessingStep.StateCodeInactive;
                    statusCode = Constants.SdkMessageProcessingStep.StatusCodeInactive;
                }

                this.logger.LogInformation($"Setting SDK step status for {deployedSdkStep[Constants.SdkMessageProcessingStep.Fields.Name]} with statecode {stateCode} and statuscode {statusCode}");
                if (!this.crmSvc.UpdateStateAndStatusForEntity(Constants.SdkMessageProcessingStep.LogicalName, deployedSdkStep.Id, stateCode, statusCode))
                {
                    this.logger.LogError($"Status for SDK step {deployedSdkStep.Attributes[Constants.SdkMessageProcessingStep.Fields.Name]} could not be set.");
                }
            }
        }

        private EntityCollection RetrieveSdkSteps(IEnumerable<string> names)
        {
            var query = new QueryExpression(Constants.SdkMessageProcessingStep.LogicalName)
            {
                ColumnSet = new ColumnSet(false),
            };
            query.Criteria.AddCondition(Constants.SdkMessageProcessingStep.Fields.Name, ConditionOperator.In, names.ToArray<object>());

            var results = this.crmSvc.RetrieveMultiple(query);
            this.logger.LogInformation($"Found {results.Entities.Count} SDK steps matching the {names.Count()} provided names.");

            return results;
        }
    }
}
