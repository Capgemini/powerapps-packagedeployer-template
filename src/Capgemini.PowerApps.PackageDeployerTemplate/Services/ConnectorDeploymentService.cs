namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Functionality related to deploying custom connectors.
    /// </summary>
    public class ConnectorDeploymentService
    {
        private readonly ILogger logger;
        private readonly ICrmServiceAdapter crmSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorDeploymentService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
        public ConnectorDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
        }

        /// <summary>
        /// Sets the scheme, basePath and host of custom connectors on the target Power Apps environment.
        /// </summary>
        /// <param name="baseUrls">A dictionary of names and baseUrls to set.</param>
        public void SetBaseUrls(IDictionary<string, string> baseUrls)
        {
            if (baseUrls is null || !baseUrls.Any())
            {
                this.logger.LogInformation("No custom connector base URLs have been configured.");
                return;
            }

            foreach (KeyValuePair<string, string> entry in baseUrls)
            {
                this.SetBaseUrl(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Sets the scheme, basePath and host of a custom connector on the target Power Apps environment.
        /// </summary>
        /// <param name="name">Custom Connector name (NOT display name).</param>
        /// <param name="baseUrl">New base URL.</param>
        public void SetBaseUrl(string name, string baseUrl)
        {
            this.logger.LogInformation($"Setting {name} custom connector base URL to {baseUrl}.");

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var validatedUrl))
            {
                this.logger.LogError($"The base URL '{baseUrl}' is not valid and the connector '{name}' won't be updated.");
                return;
            }

            var customConnector = this.GetCustomConnectorByName(name, new ColumnSet(Constants.Connector.Fields.OpenApiDefinition));
            if (customConnector is null)
            {
                this.logger.LogError($"Custom connector {name} not found on target instance.");
                return;
            }

            var existingOpenAPiDefinition = customConnector.GetAttributeValue<string>(Constants.Connector.Fields.OpenApiDefinition);
            var updatedOpenApiDefinition = UpdateApiDefinition(existingOpenAPiDefinition, validatedUrl);

            customConnector[Constants.Connector.Fields.OpenApiDefinition] = updatedOpenApiDefinition;
            this.crmSvc.Update(customConnector);
        }

        private static string UpdateApiDefinition(string currentDefinition, Uri baseUrl)
        {
            dynamic openapidefinition = JsonConvert.DeserializeObject<ExpandoObject>(currentDefinition, new ExpandoObjectConverter());

            openapidefinition.host = baseUrl.Host;
            openapidefinition.basePath = baseUrl.AbsolutePath;
            openapidefinition.schemes = new string[] { baseUrl.Scheme };

            return JsonConvert.SerializeObject(openapidefinition);
        }

        private Entity GetCustomConnectorByName(string name, ColumnSet columnSet)
        {
            return this.crmSvc.RetrieveMultipleByAttribute(
                Constants.Connector.LogicalName,
                Constants.Connector.Fields.Name,
                new string[] { name },
                columnSet).Entities.FirstOrDefault();
        }
    }
}
