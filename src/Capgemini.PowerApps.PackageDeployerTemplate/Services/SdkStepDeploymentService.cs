namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Capgemini.PowerApps.PackageDeployerTemplate.Extensions;
    using Microsoft.Extensions.Logging;

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
        /// Deactivates the provided SDK steps.
        /// </summary>
        /// <param name="sdkStepsToDeactivate">The names of the SDK steps to deactivate.</param>
        public void Deactivate(IEnumerable<string> sdkStepsToDeactivate)
        {
            if (sdkStepsToDeactivate is null || !sdkStepsToDeactivate.Any())
            {
                this.logger.LogInformation("No SDK steps to deactivate have been configured.");
                return;
            }

            var queryResponse = this.crmSvc.RetrieveMultipleByAttribute(Constants.SdkMessageProcessingStep.LogicalName, Constants.SdkMessageProcessingStep.Fields.Name, sdkStepsToDeactivate);
            var executeMultipleResponse = this.crmSvc.UpdateStateAndStatusForEntityInBatch(
                queryResponse,
                Constants.SdkMessageProcessingStep.StateCodeInactive,
                Constants.SdkMessageProcessingStep.StatusCodeInactive);

            if (executeMultipleResponse.IsFaulted)
            {
                this.logger.LogError($"Error deactivating SDK Message Processing Steps.");
                this.logger.LogExecuteMultipleErrors(executeMultipleResponse);
            }
        }
    }
}
