namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /*
    *    Title: Development Hub
    *    Author: Max Ewing
    *    Date: 15 April 2021
    *    Code version: v0.2.25
    *    Availability: https://github.com/ewingjm/development-hub/blob/v0.2.25/deploy/EnvironmentVariableDeploymentService.cs
    */

    /// <summary>
    /// Deployment functionality related to environment variables.
    /// </summary>
    public class EnvironmentVariableDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public EnvironmentVariableDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Sets environment variables on the target Power Apps environment.
        /// </summary>
        /// <param name="environmentVariables">A dictionary of keys and values to set.</param>
        public void SetEnvironmentVariables(IDictionary<string, string> environmentVariables)
        {
            if (environmentVariables is null || !environmentVariables.Any())
            {
                this.logger.LogInformation("No environment variables have been configured.");
                return;
            }

            foreach (KeyValuePair<string, string> entry in environmentVariables)
            {
                this.SetEnvironmentVariable(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Sets an environment variable on the target Power Apps environment.
        /// </summary>
        /// <param name="key">Environment variable key.</param>
        /// <param name="value">Environment variable value.</param>
        public void SetEnvironmentVariable(string key, string value)
        {
            this.logger.LogInformation($"Setting {key} environment variable to {value}.");

            var definition = this.GetDefinitionByKey(key, new ColumnSet(false));
            if (definition == null)
            {
                this.logger.LogError($"Environment variable {key} not found on target instance.");
                return;
            }

            var definitionReference = definition.ToEntityReference();
            this.logger.LogTrace($"Found environment variable on target instance: {definition.Id}");

            this.UpsertEnvironmentVariableValue(value, definitionReference);
        }

        private void UpsertEnvironmentVariableValue(string value, EntityReference definitionReference)
        {
            var existingValueRecord = this.GetValueByDefinitionId(definitionReference, new ColumnSet(Constants.EnvironmentVariableValue.Fields.Value));
            if (existingValueRecord != null)
            {
                existingValueRecord[Constants.EnvironmentVariableValue.Fields.Value] = value;
                this.crmSvc.Update(existingValueRecord);
            }
            else
            {
                this.SetValue(value, definitionReference);
            }
        }

        private void SetValue(string value, EntityReference definition)
        {
            var valueRecord = new Entity(Constants.EnvironmentVariableValue.LogicalName)
            {
                Attributes = new AttributeCollection
                            {
                                { Constants.EnvironmentVariableValue.Fields.Value, value },
                                { Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId, definition },
                            },
            };

            this.crmSvc.Create(valueRecord);
        }

        private Entity GetValueByDefinitionId(EntityReference definitionReference, ColumnSet columnSet)
        {
            return this.crmSvc.RetrieveMultipleByAttribute(
                Constants.EnvironmentVariableValue.LogicalName,
                Constants.EnvironmentVariableValue.Fields.EnvironmentVariableDefinitonId,
                new object[] { definitionReference.Id },
                columnSet).Entities.FirstOrDefault();
        }

        private Entity GetDefinitionByKey(string key, ColumnSet columnSet)
        {
            return this.crmSvc.RetrieveMultipleByAttribute(
                Constants.EnvironmentVariableDefinition.LogicalName,
                Constants.EnvironmentVariableDefinition.Fields.SchemaName,
                new string[] { key },
                columnSet).Entities.FirstOrDefault();
        }
    }
}